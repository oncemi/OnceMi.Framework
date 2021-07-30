using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Security;
using OnceMi.Framework.Util.User;
using OnceMi.IdentityServer4.User;
using OnceMi.IdentityServer4.User.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _repository;
        private readonly ILogger<UsersService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public UsersService(IUsersRepository repository
            , ILogger<UsersService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IMapper mapper
            , IMemoryCache cache)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accessor = accessor;
        }

        public async Task<List<ISelectResponse<long>>> GetUserSelectList(string query)
        {
            Expression<Func<Users, bool>> exp = p => !p.IsDeleted && p.Status == UserStatus.Enable;
            if (!string.IsNullOrEmpty(query))
            {
                exp.And(p => p.UserName.Contains(query) || p.NickName.Contains(query));
            }
            var userList = await _repository.Where(exp).OrderBy(p => p.NickName).ToListAsync();
            if (userList == null)
            {
                return new List<ISelectResponse<long>>();
            }
            return userList.Select(p => new ISelectResponse<long>()
            {
                Value = p.Id,
                Name = p.NickName,
            }).ToList();
        }

        public async Task<IPageResponse<UserItemResponse>> Query(QueryUserPageRequest request, bool onlyQueryEnabled = false)
        {
            IPageResponse<UserItemResponse> response = new IPageResponse<UserItemResponse>();
            Expression<Func<Users, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                exp = exp.And(p => p.UserName.Contains(request.Search)
                    || p.NickName.Contains(request.Search)
                    || p.Address.Contains(request.Search)
                    || p.PhoneNumber.Contains(request.Search)
                    || p.Email.Contains(request.Search));
            }
            if (request.Status != null && request.Status != 0 && !onlyQueryEnabled)
            {
                exp = exp.And(p => p.Status == request.Status);
            }
            if (request.RoleId != null && request.RoleId > 0)
            {
                exp = exp.And(p => p.Roles.AsSelect().Any(t => t.Id == request.RoleId.Value && !t.IsDeleted && t.IsEnabled));
            }
            if (request.OrganizeId != null && request.OrganizeId != 0)
            {
                //查询全部的组织
                List<Organizes> allOrganizes = await _repository.Orm
                    .Select<Organizes>()
                    .Where(p => !p.IsDeleted && p.IsEnabled)
                    .ToListAsync();
                List<long> organizeWithChildIds = new List<long>();
                BuildOrganizeWithChildList(allOrganizes, request.OrganizeId.Value, organizeWithChildIds);
                exp = exp.And(p => p.Organizes.AsSelect().Any(t => organizeWithChildIds.Contains(t.Id)));
            }
            if (onlyQueryEnabled)
            {
                exp = exp.And(p => p.Status == UserStatus.Enable);
            }
            //get count
            long count = await _repository.Where(exp).CountAsync();
            List<Users> allUsers = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByModels)
                .IncludeMany(p => p.Roles)
                .IncludeMany(p => p.Organizes)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allUsers == null || allUsers.Count == 0)
            {
                return new IPageResponse<UserItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<UserItemResponse>(),
                };
            }
            return new IPageResponse<UserItemResponse>()
            {
                Page = request.Page,
                Size = allUsers.Count,
                Count = count,
                PageData = _mapper.Map<List<UserItemResponse>>(allUsers),
            };
        }

        public async Task<UserItemResponse> Query(long id)
        {
            var response = await _cache.GetOrCreateAsync(AdminCacheKey.GetUserInfoKey(id), async (entry) =>
            {
                //设置滑动过期策略，1小时候过期
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(60));
                //查询用户
                Users user = await _repository.Where(p => p.Id == id && !p.IsDeleted)
                    .LeftJoin(u => u.CreateUser.Id == u.CreatedUserId)
                    .IncludeMany(u => u.Roles)
                    .IncludeMany(u => u.Organizes)
                    .FirstAsync();
                if (user == null)
                    return null;
                return _mapper.Map<UserItemResponse>(user);
            });
            return response;
        }

        [Transaction]
        public async Task<UserItemResponse> Insert(CreateUserRequest request)
        {
            Users user = _mapper.Map<Users>(request);
            if (user == null)
            {
                throw new Exception($"Map '{nameof(CreateUserRequest)}' DTO to '{nameof(user)}' entity failed.");
            }
            //判断用户是否存在
            if (await _repository.Select.AnyAsync(p => p.UserName == request.UserName && !p.IsDeleted))
            {
                throw new BusException(-1, $"用户名“{request.UserName}”已存在！");
            }
            //判断电话是否重复
            if (!string.IsNullOrEmpty(request.PhoneNumber)
                && await _repository.Select.AnyAsync(p => p.PhoneNumber == request.PhoneNumber && p.PhoneNumberConfirmed && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前电话号码已被注册！");
            }
            //判断邮箱是否被注册
            if (!string.IsNullOrEmpty(request.Email)
                && await _repository.Select.AnyAsync(p => p.Email == request.Email && p.EmailConfirmed && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前邮箱已被注册！");
            }
            //创建信息
            user.Id = _idGenerator.NewId();
            user.CreatedTime = DateTime.Now;
            user.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            //创建密码
            if (request.PasswordHashed)
                user.Password = user.Create(request.Password);
            else
                user.Password = user.Create(Encrypt.SHA256(request.Password));

            await _repository.InsertAsync(user);
            await UpdateUserRoles(user.Id, request.UserRoles);
            await UpdateUserOrganizes(user.Id, request.UserOrganizes);

            return _mapper.Map<UserItemResponse>(user);
        }

        [Transaction]
        public async Task Update(UpdateUserRequest request)
        {
            //判断用户是否存在
            Users user = await _repository.Where(p => p.Id == request.Id && !p.IsDeleted).FirstAsync();
            if (user == null)
            {
                throw new BusException(-1, $"修改的用户不存在");
            }
            //判断用户是否存在
            if (await _repository.Select.AnyAsync(p => p.UserName == request.UserName && !p.IsDeleted && p.Id != request.Id))
            {
                throw new BusException(-1, $"用户名“{request.UserName}”已存在！");
            }
            //判断电话是否重复
            if (!string.IsNullOrEmpty(request.PhoneNumber)
                && await _repository.Select.AnyAsync(p => p.PhoneNumber == request.PhoneNumber && p.PhoneNumberConfirmed && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前电话号码已被注册！");
            }
            //判断邮箱是否被注册
            if (!string.IsNullOrEmpty(request.Email)
                && await _repository.Select.AnyAsync(p => p.Email == request.Email && p.EmailConfirmed && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前邮箱已被注册！");
            }
            //map会清理掉password
            string oldPwd = user.Password;
            user = request.MapTo(user);
            if (!string.IsNullOrEmpty(request.Password))
            {
                //更新密码
                if (request.PasswordHashed)
                    user.Password = user.Create(request.Password);
                else
                    user.Password = user.Create(Encrypt.SHA256(request.Password));
            }
            else
            {
                user.Password = oldPwd;
            }
            user.UpdatedTime = DateTime.Now;
            user.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;

            await _repository.UpdateAsync(user);
            await UpdateUserRoles(user.Id, request.UserRoles);
            await UpdateUserOrganizes(user.Id, request.UserOrganizes);

            //移除当前用户信息缓存
            _cache.Remove(AdminCacheKey.GetUserInfoKey(user.Id));
        }

        public async Task UpdateUserStatus(UpdateUserStatusRequest request)
        {
            //判断用户是否存在
            Users user = await _repository.Where(p => p.Id == request.Id && !p.IsDeleted).FirstAsync();
            if (user == null)
            {
                throw new BusException(-1, $"用户不存在！");
            }
            if (user.Status != request.Status)
            {
                user.UpdatedTime = DateTime.Now;
                user.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
                user.Status = request.Status;
                await _repository.UpdateAsync(user);
                //移除当前用户信息缓存
                _cache.Remove(AdminCacheKey.GetUserInfoKey(user.Id));
            }
        }

        /// <summary>
        /// 软删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(-1, "没有要删除的条目");
            }
            await _repository.Where(p => ids.Contains(p.Id))
                .ToUpdate()
                .Set(p => p.IsDeleted, true)
                .Set(p => p.Status, UserStatus.Disable)
                .Set(p => p.UpdatedUserId, _accessor?.HttpContext?.User?.GetSubject().id)
                .ExecuteAffrowsAsync();
            foreach (var item in ids)
            {
                //移除当前用户信息缓存
                _cache.Remove(AdminCacheKey.GetUserInfoKey(item));
            }
        }

        public Task<byte[]> GetAvatar(string name, int size = 100)
        {
            if (!string.IsNullOrEmpty(name) && name.Length > 2)
            {
                name = name.Substring(0, 2);
            }
            if (size < 30 || size > 1500)
            {
                size = 80;
            }
            var bytes = RandomAvatarBuilder.Build(size).SetPadding(4).FixedSeed(!string.IsNullOrEmpty(name), name).ToBytes();
            return Task.FromResult(bytes);
        }

        private void BuildOrganizeWithChildList(List<Organizes> allOrganizes, long parentId, List<long> dest)
        {
            if (dest == null)
            {
                dest = new List<long>();
            }
            dest.Add(parentId);
            List<long> childIds = allOrganizes.Where(p => p.ParentId == parentId)?.Select(p => p.Id).ToList();
            if (childIds != null && childIds.Count > 0)
            {
                foreach (var item in childIds)
                {
                    BuildOrganizeWithChildList(allOrganizes, item, dest);
                }
            }
        }

        /// <summary>
        /// 更新用户角色
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        private async Task UpdateUserRoles(long userId, List<long> roles)
        {
            if (roles == null || roles.Count == 0)
            {
                return;
            }
            roles = roles.Distinct().ToList();
            await _repository.Orm.Select<UserRole>()
                .Where(p => p.UserId == userId)
                .ToDelete()
                .ExecuteDeletedAsync();
            List<UserRole> newUserRoles = roles.Select(p => new UserRole()
            {
                Id = _idGenerator.NewId(),
                UserId = userId,
                RoleId = p,
                CreatedTime = DateTime.Now,
                CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id
            })
                .ToList();
            await _repository.Orm.Insert(newUserRoles).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 更新用户组织机构
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="organizes"></param>
        /// <returns></returns>
        private async Task UpdateUserOrganizes(long userId, List<long> organizes)
        {
            if (organizes == null || organizes.Count == 0)
            {
                return;
            }
            organizes = organizes.Distinct().ToList();
            await _repository.Orm.Select<UserOrganize>()
                .Where(p => p.UserId == userId)
                .ToDelete()
                .ExecuteDeletedAsync();
            List<UserOrganize> newUserOrganizes = organizes.Select(p => new UserOrganize()
            {
                Id = _idGenerator.NewId(),
                UserId = userId,
                OrganizeId = p,
                CreatedTime = DateTime.Now,
                CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id
            })
                .ToList();
            await _repository.Orm.Insert(newUserOrganizes).ExecuteAffrowsAsync();
        }
    }
}
