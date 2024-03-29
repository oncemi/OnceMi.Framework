﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exceptions;
using OnceMi.Framework.Util.Enum;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class OrganizeService : IOrganizeService
    {
        private readonly IOrganizeRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<OrganizeService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;

        public OrganizeService(IOrganizeRepository repository
            , ILogger<OrganizeService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IUserRepository userRepository
            , IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accessor = accessor;
        }

        /// <summary>
        /// 获取组织类型
        /// </summary>
        /// <returns></returns>
        public List<ISelectResponse<int>> QueryOrganizeTypes()
        {
            List<EnumModel> enumModels = EnumUtil.EnumToList<OrganizeType>();
            if (enumModels == null || enumModels.Count == 0)
            {
                return new List<ISelectResponse<int>>();
            }
            List<ISelectResponse<int>> result = enumModels
                .OrderBy(p => p.Value)
                .Select(p => new ISelectResponse<int>()
                {
                    Name = p.Description,
                    Value = p.Value,
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// ORGANIZE
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IPageResponse<OrganizeItemResponse>> Query(OrganizePageRequest request, bool onlyQueryEnabled = false)
        {
            IPageResponse<OrganizeItemResponse> response = new IPageResponse<OrganizeItemResponse>();
            bool isSearchQuery = false;
            Expression<Func<Organize, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                isSearchQuery = true;
                exp = exp.And(p => p.Name.Contains(request.Search)
                || p.Code.Contains(request.Search));
            }
            if (request.OrganizeType != null && request.OrganizeType != 0)
            {
                isSearchQuery = true;
                exp = exp.And(p => p.OrganizeType == request.OrganizeType);
            }
            if (!isSearchQuery)
            {
                exp = exp.And(p => p.ParentId == null);
            }
            if (onlyQueryEnabled)
            {
                exp = exp.And(p => p.IsEnabled);
            }
            //get count
            long count = await _repository.Where(exp).CountAsync();
            List<Organize> allParents = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByParams)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allParents == null || allParents.Count == 0)
            {
                return new IPageResponse<OrganizeItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<OrganizeItemResponse>(),
                };
            }

            //设置用户
            List<UserInfo> allUser = await _userRepository.Where(p => !p.IsDeleted).ToListAsync();
            await SetLeaders(allParents, allUser);
            if (isSearchQuery)
            {
                List<Organize> removeOrganizes = new List<Organize>();
                foreach (var item in allParents)
                {
                    GetQueryOrganizeChild(allParents, item, removeOrganizes);
                }
                if (removeOrganizes.Count > 0)
                {
                    foreach (var item in removeOrganizes)
                    {
                        allParents.Remove(item);
                    }
                }
            }
            else
            {
                Expression<Func<Organize, bool>> allQueryExp = p => !p.IsDeleted && p.ParentId != null;
                if (onlyQueryEnabled)
                {
                    allQueryExp = allQueryExp.And(p => p.IsEnabled);
                }
                List<Organize> allOrganizes = await _repository
                    .Where(allQueryExp)
                    .NoTracking()
                    .ToListAsync();
                await SetLeaders(allOrganizes, allUser);
                foreach (var item in allParents)
                {
                    GetQueryOrganizeChild(allOrganizes, item);
                }
            }
            return new IPageResponse<OrganizeItemResponse>()
            {
                Page = request.Page,
                Size = allParents.Count,
                Count = count,
                PageData = _mapper.Map<List<OrganizeItemResponse>>(allParents),
            };
        }

        public async Task<OrganizeItemResponse> Query(long id)
        {
            List<Organize> allOrganizes = await _repository
                .Where(p => !p.IsDeleted)
                .IncludeMany(p => p.DepartLeaders.Where(t => t.OrganizeId == p.Id))
                .IncludeMany(p => p.HeadLeaders.Where(t => t.OrganizeId == p.Id))
                .NoTracking()
                .ToListAsync();
            //设置用户
            List<UserInfo> allUser = await _userRepository.Where(p => !p.IsDeleted).ToListAsync();
            await SetLeaders(allOrganizes, allUser);
            Organize queryOrganize = allOrganizes.Where(p => p.Id == id).FirstOrDefault();
            if (queryOrganize == null)
                return null;

            GetQueryOrganizeChild(allOrganizes, queryOrganize);
            OrganizeItemResponse result = _mapper.Map<OrganizeItemResponse>(queryOrganize);
            return result;
        }

        [Transaction]
        [CleanCache(CacheType.MemoryCache, GlobalCacheConstant.Key.RolePermissionsKey)]
        public async Task<OrganizeItemResponse> Insert(CreateOrganizeRequest request)
        {
            Organize organize = _mapper.Map<Organize>(request);
            if (organize == null)
            {
                throw new Exception($"Map '{nameof(CreateOrganizeRequest)}' DTO to '{nameof(Organize)}' entity failed.");
            }
            if ((organize.ParentId != null && organize.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == organize.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ORG_PARENTS_NOT_EXISTS, "父条目不存在");
            }
            organize.ParentId = organize.ParentId == 0 ? null : organize.ParentId;
            organize.Id = _idGenerator.CreateId();
            organize.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            organize.CreatedTime = DateTime.Now;
            List<OrganizeManager> managers = await GenerateOrganizeManagers(request, organize.Id);

            if (managers.Count > 0)
            {
                await _repository.Orm.Insert(managers).ExecuteAffrowsAsync();
            }
            //保存
            var result = await _repository.InsertAsync(organize);

            return _mapper.Map<OrganizeItemResponse>(result);
        }

        [Transaction]
        [CleanCache(CacheType.MemoryCache, GlobalCacheConstant.Key.RolePermissionsKey)]
        public async Task Update(UpdateOrganizeRequest request)
        {
            Organize organize = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (organize == null)
            {
                throw new BusException(ResultCode.ORG_UPDATE_NOT_EXISTS, "修改的条目不存在");
            }
            if ((request.ParentId != null && request.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ORG_PARENTS_NOT_EXISTS, "父条目不存在");
            }
            if (request.ParentId != null && request.ParentId != 0 && request.Id == request.ParentId)
            {
                throw new BusException(ResultCode.ORG_PARENT_CANNOT_SELF, "父条目不能为本身");
            }
            List<OrganizeManager> managers = await GenerateOrganizeManagers(request);
            //将请求Map到要修改的对象
            organize = request.MapTo(organize);
            organize.ParentId = request.ParentId == 0 ? null : request.ParentId;
            organize.UpdatedTime = DateTime.Now;
            organize.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;

            //删除之前的OrganizeManagers
            await _repository.Orm.Select<OrganizeManager>()
                .Where(p => p.OrganizeId == organize.Id)
                .ToDelete()
                .ExecuteAffrowsAsync();
            //写入新的
            if (managers.Count > 0)
            {
                await _repository.Orm.Insert(managers).ExecuteAffrowsAsync();
            }
            //保存
            await _repository.UpdateAsync(organize);
        }

        /// <summary>
        /// 删除
        /// 1、软删除
        /// 2、子节点下有未删除用户或用户组禁止删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [Transaction]
        [CleanCache(CacheType.MemoryCache, GlobalCacheConstant.Key.RolePermissionsKey)]
        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(ResultCode.ORG_DELETE_NOT_EXISTS, "没有要删除的条目");
            }
            List<Organize> allOrganizes = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            if (allOrganizes == null || allOrganizes.Count == 0)
            {
                return;
            }
            List<long> delIds = new List<long>();
            foreach (var item in ids)
            {
                SearchDelOrganizes(allOrganizes, item, delIds);
            }
            if (delIds == null || delIds.Count == 0)
            {
                return;
            }
            bool canNotDelete = true;
            //判断角色
            canNotDelete = await _repository.Orm.Select<Role>().AnyAsync(p => delIds.Contains(p.OrganizeId) && !p.IsDeleted);
            if (canNotDelete)
            {
                throw new BusException(ResultCode.ORG_HAS_ROLES, "删除失败，当前组织机构下包含未删除的角色组");
            }
            canNotDelete = await _repository.Orm.Select<UserInfo>().AnyAsync(p => p.Organizes.AsSelect().Any(q => delIds.Contains(q.Id)) && !p.IsDeleted);
            if (canNotDelete)
            {
                throw new BusException(ResultCode.ORG_HAS_USERS, "删除失败，当前组织机构下包含启用的用户");
            }
            if (delIds != null)
            {
                await _repository.Select.Where(p => delIds.Contains(p.Id))
                    .ToUpdate()
                    .Set(p => p.IsDeleted, true)
                    .Set(p => p.IsEnabled, false)
                    .Set(p => p.UpdatedUserId, _accessor?.HttpContext?.User?.GetSubject().id)
                    .ExecuteAffrowsAsync();
            }
        }

        /// <summary>
        /// 搜素要删除的父节点和子节点
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <param name="dest"></param>
        private void SearchDelOrganizes(List<Organize> source, long id, List<long> dest)
        {
            var item = source.Where(p => p.Id == id).FirstOrDefault();
            if (item == null)
            {
                return;
            }
            if (!dest.Contains(item.Id))
            {
                dest.Add(item.Id);
            }
            List<Organize> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelOrganizes(source, citem.Id, dest);
            }
        }

        private void GetQueryOrganizeChild(List<Organize> source, Organize organize, List<Organize> removeOrganizes = null)
        {
            var childs = source.Where(p => p.ParentId == organize.Id).ToList();
            if (childs == null || childs.Count == 0)
            {
                return;
            }
            organize.Children = childs;
            if (removeOrganizes != null)
            {
                removeOrganizes.AddRange(childs);
            }
            foreach (var item in organize.Children)
            {
                GetQueryOrganizeChild(source, item);
            }
        }

        /// <summary>
        /// 设置Organizes的DepartLeaders和HeadLeaders
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="organizes"></param>
        /// <param name="allUser"></param>
        private async Task SetLeaders(List<Organize> organizes, List<UserInfo> allUser)
        {
            if (allUser == null || allUser.Count == 0)
                return;
            if (organizes == null || organizes.Count == 0)
                return;

            List<long> allOrganizeIds = organizes.Select(p => p.Id).ToList();
            List<OrganizeManager> managers = await _repository.Orm.Select<OrganizeManager>()
                .Where(p => allOrganizeIds.Contains(p.OrganizeId))
                .ToListAsync();

            foreach (var item in organizes)
            {
                item.DepartLeaders = managers.Where(p => p.OrganizeId == item.Id && p.ManagerType == OrganizeManagerType.DepartLeader).ToList();
                if (item.DepartLeaders != null && item.DepartLeaders.Count > 0)
                {
                    foreach (var depatLeaderItem in item.DepartLeaders)
                    {
                        depatLeaderItem.User = allUser.FirstOrDefault(p => p.Id == depatLeaderItem.UserId);
                    }
                }
                item.HeadLeaders = managers.Where(p => p.OrganizeId == item.Id && p.ManagerType == OrganizeManagerType.HeadLeader).ToList();
                if (item.HeadLeaders != null && item.HeadLeaders.Count > 0)
                {
                    foreach (var headLeaderItem in item.HeadLeaders)
                    {
                        headLeaderItem.User = allUser.FirstOrDefault(p => p.Id == headLeaderItem.UserId);
                    }
                }
            }
        }

        private async Task<List<OrganizeManager>> GenerateOrganizeManagers(UpdateOrganizeRequest request)
        {
            return await GenerateOrganizeManagers(new CreateOrganizeRequest()
            {
                ParentId = request.ParentId,
                Name = request.Name,
                Code = request.Code,
                OrganizeType = request.OrganizeType,
                IsEnabled = request.IsEnabled,
                DepartLeaders = request.DepartLeaders,
                HeadLeaders = request.HeadLeaders,
            }, request.Id);
        }

        private async Task<List<OrganizeManager>> GenerateOrganizeManagers(CreateOrganizeRequest request, long id)
        {
            //保存分管领导信息
            List<OrganizeManager> managers = new List<OrganizeManager>();
            if (request.DepartLeaders != null && request.DepartLeaders.Count > 0)
            {
                List<UserInfo> departLeaderUsers = await _userRepository.Where(p => request.DepartLeaders.Contains(p.Id) && !p.IsDeleted).ToListAsync();
                if (request.DepartLeaders.Count != departLeaderUsers.Count)
                {
                    throw new BusException(ResultCode.ORG_SELECT_DL_ERROR, "所选部门负责人不正确");
                }
                if (departLeaderUsers != null && departLeaderUsers.Count > 0)
                {
                    var departLeaders = request.DepartLeaders.Select(p => new OrganizeManager()
                    {
                        Id = _idGenerator.CreateId(),
                        UserId = departLeaderUsers.SingleOrDefault(q => q.Id == p).Id,
                        OrganizeId = id,
                        ManagerType = OrganizeManagerType.DepartLeader,
                        CreatedTime = DateTime.Now,
                        CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                    }).ToList();
                    managers.AddRange(departLeaders);
                }
            }
            if (request.HeadLeaders != null && request.HeadLeaders.Count > 0)
            {
                List<UserInfo> hesderLeaderUsers = await _userRepository.Where(p => request.HeadLeaders.Contains(p.Id) && !p.IsDeleted).ToListAsync();
                if (request.HeadLeaders.Count != hesderLeaderUsers.Count)
                {
                    throw new BusException(ResultCode.ORG_SELECT_HL_ERROR, "所选分管领导不正确");
                }
                if (hesderLeaderUsers != null && hesderLeaderUsers.Count > 0)
                {
                    var headLeaders = request.HeadLeaders.Select(p => new OrganizeManager()
                    {
                        Id = _idGenerator.CreateId(),
                        UserId = hesderLeaderUsers.SingleOrDefault(q => q.Id == p).Id,
                        OrganizeId = id,
                        ManagerType = OrganizeManagerType.HeadLeader,
                        CreatedTime = DateTime.Now,
                        CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                    }).ToList();
                    managers.AddRange(headLeaders);
                }
            }
            return managers;
        }
    }
}
