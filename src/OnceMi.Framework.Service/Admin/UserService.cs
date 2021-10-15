using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Dto.Request.Admin.User;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Enum;
using OnceMi.Framework.Util.Http;
using OnceMi.Framework.Util.Images;
using OnceMi.Framework.Util.Security;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IUpLoadFileService _upLoadFileService;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository
            , ILogger<UserService> logger
            , IIdGeneratorService idGenerator
            , IUpLoadFileService upLoadFileService
            , IHttpContextAccessor accessor
            , IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _upLoadFileService = upLoadFileService ?? throw new ArgumentNullException(nameof(upLoadFileService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accessor = accessor;
        }

        public List<ISelectResponse<string>> GetUserStatus()
        {
            List<EnumModel> enumModels = EnumUtil.EnumToList<UserStatus>();
            if (enumModels == null || enumModels.Count == 0)
            {
                return new List<ISelectResponse<string>>();
            }
            List<ISelectResponse<string>> result = enumModels
                .OrderBy(p => p.Value)
                .Select(p => new ISelectResponse<string>()
                {
                    Name = p.Description,
                    Value = p.Name,
                })
                .ToList();
            return result;
        }

        public List<ISelectResponse<string>> GetUserGender()
        {
            List<EnumModel> enumModels = EnumUtil.EnumToList<UserGender>();
            if (enumModels == null || enumModels.Count == 0)
            {
                return new List<ISelectResponse<string>>();
            }
            List<ISelectResponse<string>> result = enumModels
                .OrderBy(p => p.Value)
                .Select(p => new ISelectResponse<string>()
                {
                    Name = p.Description,
                    Value = p.Name,
                })
                .ToList();
            return result;
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
            if (request.OrderByModels.Count == 0)
            {
                request.OrderBy = new string[] { $"{nameof(Users.Id)},desc" };
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

        public async Task<Users> Query(string inputInfo, bool isQueryEnabled = false)
        {
            Expression<Func<Users, bool>> exp = p => !p.IsDeleted && (p.Id.ToString() == inputInfo || p.UserName == inputInfo);
            if (isQueryEnabled)
            {
                exp = exp.And(p => p.Status == UserStatus.Enable);
            }

            //查询用户
            Users user = await _repository.Where(exp)
                .IncludeMany(u => u.Roles)
                .IncludeMany(u => u.Organizes)
                .NoTracking()
                .ToOneAsync();
            if (user == null)
                return null;
            return user;
        }

        [Transaction]
        public async Task<UserItemResponse> Insert(CreateUserRequest request)
        {
            if (!IsUserName(request.UserName))
            {
                throw new BusException(ResultCodeConstant.USER_NAME_INVALID, "用户名只能由数字和字母组成");
            }
            Users user = _mapper.Map<Users>(request);
            if (user == null)
            {
                throw new Exception($"Map '{nameof(CreateUserRequest)}' DTO to '{nameof(user)}' entity failed.");
            }
            //判断用户是否存在
            if (await _repository.Select.AnyAsync(p => p.UserName == request.UserName && !p.IsDeleted))
            {
                throw new BusException(ResultCodeConstant.USER_NAME_EXISTS, $"用户名“{request.UserName}”已存在");
            }
            //判断电话是否重复
            if (!string.IsNullOrEmpty(request.PhoneNumber)
                && await _repository.Select.AnyAsync(p => p.PhoneNumber == request.PhoneNumber && p.PhoneNumberConfirmed && !p.IsDeleted))
            {
                throw new BusException(ResultCodeConstant.USER_PHONE_EXISTS, $"当前电话号码已被注册");
            }
            //判断邮箱是否被注册
            if (!string.IsNullOrEmpty(request.Email)
                && await _repository.Select.AnyAsync(p => p.Email == request.Email && p.EmailConfirmed && !p.IsDeleted))
            {
                throw new BusException(ResultCodeConstant.USER_EMAIL_EXISTS, $"当前邮箱已被注册");
            }
            //创建信息
            user.Id = _idGenerator.NewId();
            user.CreatedTime = DateTime.Now;
            user.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            //创建密码
            if (request.PasswordHashed)
                user.Password = user.CreatePassword(request.Password);
            else
                user.Password = user.CreatePassword(Encrypt.SHA256(request.Password));
            //保存头像
            UploadFileInfo fileInfo = null;
            if (!string.IsNullOrEmpty(request.Avatar))
            {
                using (Image headerImage = ImageBase64Converter.Base64ToImage(request.Avatar))
                {
                    using (var saveStream = new MemoryStream())
                    {
                        headerImage.Save(saveStream, ImageFormat.Png);
                        fileInfo = await _upLoadFileService.Upload(saveStream, $"{user.Id}.png", (user.CreatedUserId == null ? user.Id : user.CreatedUserId.Value), FileAccessMode.PublicRead);
                        if (fileInfo != null && !string.IsNullOrEmpty(fileInfo.Url))
                        {
                            user.Avatar = fileInfo.Url;
                        }
                    }
                }
            }
            try
            {
                await _repository.InsertAsync(user);
                await UpdateUserRoles(user.Id, request.UserRoles);
                await UpdateUserOrganizes(user.Id, request.UserOrganizes);

                return _mapper.Map<UserItemResponse>(user);
            }
            catch
            {
                //如果保存用户信息失败，且上传头像已经完成，那么删除上传的头像信息
                if (fileInfo != null)
                {
                    await _upLoadFileService.DeleteFile(fileInfo);
                }
                throw;
            }
        }

        [Transaction]
        public async Task Update(UpdateUserRequest request)
        {
            if (!IsUserName(request.UserName))
            {
                throw new BusException(ResultCodeConstant.USER_NAME_INVALID, "用户名只能由数字和字母组成");
            }
            //判断用户是否存在
            Users user = await _repository.Where(p => p.Id == request.Id && !p.IsDeleted).FirstAsync();
            if (user == null)
            {
                throw new BusException(ResultCodeConstant.USER_UPDATE_NOT_EXISTS, $"修改的用户不存在");
            }
            //判断用户是否存在
            if (await _repository.Select.AnyAsync(p => p.UserName == request.UserName && !p.IsDeleted && p.Id != request.Id))
            {
                throw new BusException(ResultCodeConstant.USER_NAME_EXISTS, $"用户名“{request.UserName}”已存在");
            }
            //判断电话是否重复
            if (!string.IsNullOrEmpty(request.PhoneNumber)
                && await _repository.Select.AnyAsync(p => p.PhoneNumber == request.PhoneNumber && p.PhoneNumberConfirmed && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(ResultCodeConstant.USER_PHONE_EXISTS, $"当前电话号码已被注册");
            }
            //判断邮箱是否被注册
            if (!string.IsNullOrEmpty(request.Email)
                && await _repository.Select.AnyAsync(p => p.Email == request.Email && p.EmailConfirmed && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(ResultCodeConstant.USER_EMAIL_EXISTS, $"当前邮箱已被注册");
            }
            //map会清理掉password
            string oldPwd = user.Password;
            string oldAvatar = user.Avatar;
            user = request.MapTo(user);
            if (!string.IsNullOrEmpty(request.Password))
            {
                //更新密码
                if (request.PasswordHashed)
                    user.Password = user.CreatePassword(request.Password);
                else
                    user.Password = user.CreatePassword(Encrypt.SHA256(request.Password));
            }
            else
            {
                user.Password = oldPwd;
            }
            user.UpdatedTime = DateTime.Now;
            user.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;

            //保存头像
            UploadFileInfo fileInfo = null;
            if (!string.IsNullOrEmpty(request.Avatar)
                && !request.Avatar.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                using (Image headerImage = ImageBase64Converter.Base64ToImage(request.Avatar))
                {
                    using (var saveStream = new MemoryStream())
                    {
                        headerImage.Save(saveStream, ImageFormat.Png);
                        fileInfo = await _upLoadFileService.Upload(saveStream, $"{user.Id}.png", (user.CreatedUserId == null ? user.Id : user.CreatedUserId.Value), FileAccessMode.PublicRead);
                        if (fileInfo != null && !string.IsNullOrEmpty(fileInfo.Url))
                        {
                            user.Avatar = fileInfo.Url;
                        }
                    }
                }
            }
            try
            {
                await _repository.UpdateAsync(user);
                await UpdateUserRoles(user.Id, request.UserRoles);
                await UpdateUserOrganizes(user.Id, request.UserOrganizes);
            }
            catch
            {
                //如果保存用户信息失败，且上传头像已经完成，那么删除上传的头像信息
                if (fileInfo != null)
                {
                    await _upLoadFileService.DeleteFile(fileInfo);
                }
                throw;
            }
            //删除旧的头像文件
            if ((fileInfo != null || fileInfo == null && string.IsNullOrEmpty(request.Avatar))
                && !string.IsNullOrEmpty(oldAvatar)
                && oldAvatar.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var oldAvatarKey = ParamConvertUtils<UserAvatarUrlParamModel>.StringConvertEntity(oldAvatar, true);
                    if (oldAvatarKey != null && !string.IsNullOrEmpty(oldAvatarKey.Key))
                    {
                        UploadFileInfo uploadFileInfo = _upLoadFileService.DecodeFileKey(oldAvatarKey.Key);
                        await _upLoadFileService.DeleteFile(uploadFileInfo);
                    }
                }
                catch { }
            }
        }

        public async Task UpdateStatus(UpdateUserStatusRequest request)
        {
            //判断用户是否存在
            Users user = await _repository.Where(p => p.Id == request.Id && !p.IsDeleted).FirstAsync();
            if (user == null)
            {
                throw new BusException(ResultCodeConstant.USER_UPDATE_NOT_EXISTS, $"用户不存在！");
            }
            if (user.Status != request.Status)
            {
                user.UpdatedTime = DateTime.Now;
                user.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
                user.Status = request.Status;
                await _repository.UpdateAsync(user);
            }
        }

        public async Task UpdatePassword(UpdateUserPasswordRequest request)
        {
            Users user = await _repository.Where(p => p.Id == request.Id && !p.IsDeleted).ToOneAsync();
            if (user == null)
            {
                throw new BusException(ResultCodeConstant.USER_UPDATE_NOT_EXISTS, $"用户不存在");
            }
            if (false)
            //if (!user.AuthenticatePassword(request.OldPassword))
            {
                throw new BusException(ResultCodeConstant.USER_OLD_PWD_INVALID, $"旧密码错误");
            }
            string pwd = user.CreatePassword(request.Password);
            await _repository.Where(p => p.Id == request.Id)
                .ToUpdate()
                .Set(p => p.Password, pwd)
                .Set(p => p.UpdatedUserId, _accessor?.HttpContext?.User?.GetSubject().id)
                .ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 软删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [Transaction]
        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(ResultCodeConstant.USER_DELETE_NOT_EXISTS, "没有要删除的条目");
            }
            await _repository.Where(p => ids.Contains(p.Id))
                .ToUpdate()
                .Set(p => p.IsDeleted, true)
                .Set(p => p.Status, UserStatus.Disable)
                .Set(p => p.UpdatedUserId, _accessor?.HttpContext?.User?.GetSubject().id)
                .ExecuteAffrowsAsync();
        }

        public byte[] GetAvatar(string name, int size = 250)
        {
            if (!string.IsNullOrEmpty(name) && name.Length > 2)
            {
                name = name.Substring(0, 2);
            }
            if (size < 30 || size > 1500)
            {
                size = 120;
            }
            var bytes = RandomAvatarBuilder
                .Build(size)
                .SetPadding(4)
                .FixedSeed(!string.IsNullOrEmpty(name), name)
                .ToBytes();
            return bytes;
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
                .ExecuteAffrowsAsync();
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
                .ExecuteAffrowsAsync();
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

        private bool IsUserName(string userName)
        {
            //用户名:英文和数字
            Regex checkUserName = new Regex("^[A-Za-z0-9]+$");
            var resultName = checkUserName.Match(userName);
            if (!resultName.Success)
            {
                return false;
            }
            return true;
        }
    }
}
