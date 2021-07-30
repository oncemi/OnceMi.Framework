using AutoMapper;
using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.User;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class ApisService : BaseService<Apis, long>, IApisService
    {
        private readonly IApisRepository _repository;
        private readonly ILogger<ApisService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly ISwaggerProvider _swagger;
        private readonly SwaggerGeneratorOptions _options;
        private readonly IMapper _mapper;
        private readonly RedisClient _redisClient;

        public ApisService(IApisRepository repository
            , ILogger<ApisService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , ISwaggerProvider swagger
            , SwaggerGeneratorOptions options
            , IMapper mapper
            , RedisClient redisClient) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _swagger = swagger ?? throw new ArgumentNullException(nameof(swagger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _redisClient = redisClient ?? throw new ArgumentNullException(nameof(redisClient));
            _accessor = accessor;
        }

        public async Task<List<ISelectResponse<string>>> QueryApiVersions()
        {
            List<ISelectResponse<string>> result = _repository.Select.GroupBy(p => p.Version).Select(p => new ISelectResponse<string>
            {
                Value = p.Key,
                Name = p.Key
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<IPageResponse<ApiItemResponse>> Query(QueryApiPageRequest request, bool onlyQueryEnabled = false)
        {
            IPageResponse<ApiItemResponse> response = new IPageResponse<ApiItemResponse>();
            bool isSearchQuery = false;
            Expression<Func<Apis, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                isSearchQuery = true;
                exp = exp.And(p => p.Name.Contains(request.Search) || p.Path.Contains(request.Search));
            }
            if (!string.IsNullOrEmpty(request.ApiVersion))
            {
                isSearchQuery = true;
                exp = exp.And(p => p.Version == request.ApiVersion);
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
            List<Apis> allParentApis = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByModels)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allParentApis == null || allParentApis.Count == 0)
            {
                return new IPageResponse<ApiItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<ApiItemResponse>(),
                };
            }
            if (isSearchQuery)
            {
                List<Apis> removeApis = new List<Apis>();
                foreach (var item in allParentApis)
                {
                    GetQueryApiChild(allParentApis, item, removeApis);
                }
                if (removeApis.Count > 0)
                {
                    foreach (var item in removeApis)
                    {
                        allParentApis.Remove(item);
                    }
                }
            }
            else
            {
                Expression<Func<Apis, bool>> allQueryExp = p => !p.IsDeleted && p.ParentId != null;
                if (onlyQueryEnabled)
                {
                    allQueryExp = allQueryExp.And(p => p.IsEnabled);
                }
                List<Apis> allApis = await _repository
                    .Where(allQueryExp)
                    .NoTracking()
                    .ToListAsync();
                foreach (var item in allParentApis)
                {
                    GetQueryApiChild(allApis, item);
                }
            }
            return new IPageResponse<ApiItemResponse>()
            {
                Page = request.Page,
                Size = allParentApis.Count,
                Count = count,
                PageData = _mapper.Map<List<ApiItemResponse>>(allParentApis),
            };
        }

        public async Task<ApiItemResponse> Query(long id)
        {
            List<Apis> allApis = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            Apis queryApi = allApis.Where(p => p.Id == id).FirstOrDefault();
            if (queryApi == null)
                return null;

            GetQueryApiChild(allApis, queryApi);
            ApiItemResponse result = _mapper.Map<ApiItemResponse>(queryApi);
            return result;
        }

        public async Task<ApiItemResponse> Insert(CreateApiRequest request)
        {
            Apis api = _mapper.Map<Apis>(request);
            if (api == null)
            {
                throw new Exception($"Map '{nameof(CreateApiRequest)}' DTO to '{nameof(Apis)}' entity failed.");
            }
            if ((api.ParentId != null && api.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == api.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, "父条目不存在");
            }
            if ((api.ParentId != null && api.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == api.ParentId && p.Version == request.Version))
            {
                throw new BusException(-1, "添加的API版本必须和父节点版本相同");
            }
            if (await _repository.Select.AnyAsync(p => p.Path == api.Path && p.Version == p.Version && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前添加的路径‘{api.Path}’的Api已存在");
            }
            api.Code = $"{api.Path.Trim('/').Replace('/', ':')}:{api.Method}".ToLower();
            api.ParentId = api.ParentId == 0 ? null : api.ParentId;
            api.OperationId = api.Path.Replace("/", "").ToUpper() + "-" + (string.IsNullOrEmpty(api.Method) ? "Controller" : api.Method);
            api.Id = _idGenerator.NewId();
            api.CreateMethod = ApiCreateMethod.Manual;
            api.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            api.CreatedTime = DateTime.Now;
            api.CreateMethod = ApiCreateMethod.Manual;

            var result = await _repository.InsertAsync(api);
            return _mapper.Map<ApiItemResponse>(result);
        }

        public async Task Update(UpdateApiRequest request)
        {
            Apis api = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (api == null)
            {
                throw new BusException(-1, "修改的目不存在");
            }
            if ((request.ParentId != null && request.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, "父条目不存在");
            }
            if ((api.ParentId != null && api.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == api.ParentId && p.Version == request.Version))
            {
                throw new BusException(-1, "添加的API版本必须和父节点版本相同");
            }
            if (await _repository.Select.AnyAsync(p => p.Path == request.Path && p.Version == p.Version && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前修改路径‘{request.Path}’的Api已存在");
            }

            api = request.MapTo(api);
            api.ParentId = request.ParentId == 0 ? null : request.ParentId;
            api.OperationId = api.Path.Replace("/", "").ToUpper() + "-" + (string.IsNullOrEmpty(api.Method) ? "Controller" : api.Method);
            api.Code = $"{api.Path.Trim('/').Replace('/', ':')}:{api.Method}".ToLower();
            api.UpdatedTime = DateTime.Now;
            api.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            await _repository.UpdateAsync(api);
        }

        public async Task AutoResolve()
        {
            List<ApiDocInfo> apis = ResolveApi();
            //按照Controller分组
            List<Apis> newParentApis = apis.GroupBy(p => new { p.Controller, p.ControllerName, p.Version })
                .Select(p => new Apis()
                {
                    Id = _idGenerator.NewId(),
                    OperationId = p.Key.Controller + "-Controller",
                    Path = p.Key.Controller,
                    Name = p.Key.ControllerName,
                    Code = null,
                    Version = p.Key.Version,
                    CreatedTime = DateTime.Now,
                    CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                    Method = null,
                    IsEnabled = true,
                    CreateMethod = ApiCreateMethod.AutoSync,
                }).ToList();

            List<Apis> newApis = apis
                .Select(p => new Apis()
                {
                    Id = _idGenerator.NewId(),
                    ParentId = newParentApis.Where(x => x.Path == p.Controller).FirstOrDefault()?.Id,
                    OperationId = p.OperationId,
                    Version = p.Version,
                    Path = p.Path,
                    Name = p.Name,
                    Code = $"{p.Path.Trim('/').Replace('/', ':')}:{p.Method}".ToLower(),
                    Description = p.Description,
                    Method = p.Method,
                    Parameters = p.Parameters == null ? null : string.Join(',', p.Parameters),
                    CreatedTime = DateTime.Now,
                    CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id,
                    IsEnabled = true,
                    CreateMethod = ApiCreateMethod.AutoSync
                })
                .ToList();
            newApis.AddRange(newParentApis);
            //获取当前已经存在的
            List<Apis> oldApis = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            //比较差异
            DifferApis(oldApis, newApis);
            //再次搜素是否有遗漏的
            DeleteApis(oldApis);
            //排序后写入数据库
            oldApis = oldApis
                .OrderBy(p => p.OperationId)
                .ThenBy(p => p.Path)
                .ToList();
            //删除系统自动同步的
            await _repository.DeleteAsync(p => 1 == 1);
            await _repository.InsertAsync(oldApis);

            _logger.LogInformation($"成功解析到{oldApis.Count}个Api，并已存入数据库！");
        }

        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(-1, "没有要删除的条目");
            }
            List<Apis> allApis = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            if (allApis == null || allApis.Count == 0)
            {
                return;
            }
            List<long> delIds = new List<long>();
            foreach (var item in ids)
            {
                SearchDelApis(allApis, item, delIds);
            }
            if (delIds == null || delIds.Count == 0)
            {
                return;
            }
            List<long> menuIds = await GetMenuIncludeApis(delIds);
            List<long> permissionIds = await GetPermissionIncludeMenus(menuIds);

            using (var uow = _repository.Orm.CreateUnitOfWork())
            {
                try
                {
                    if (delIds != null)
                        uow.Orm.Delete<Apis>().Where(p => delIds.Contains(p.Id)).ExecuteAffrows();
                    if (menuIds != null)
                        uow.Orm.Delete<Views>().Where(p => menuIds.Contains(p.Id)).ExecuteAffrows();
                    if (permissionIds != null)
                        uow.Orm.Delete<RolePermissions>().Where(p => permissionIds.Contains(p.Id)).ExecuteAffrows();

                    uow.Commit();
                }
                catch (Exception ex)
                {
                    uow.Rollback();
                    _logger.LogError(ex, $"Delete apis failed, {ex.Message}");
                    throw new BusException(-1, $"Delete apis failed, {ex.Message}");
                }
            }
            //清除菜单缓存
            _redisClient.Del(AdminCacheKey.SystemMenusKey);
        }

        private void GetQueryApiChild(List<Apis> source, Apis api, List<Apis> removeApis = null)
        {
            var childs = source.Where(p => p.ParentId == api.Id).ToList();
            if (childs == null || childs.Count == 0)
            {
                return;
            }
            api.Children = childs;
            if (removeApis != null)
            {
                removeApis.AddRange(childs);
            }
            foreach (var item in api.Children)
            {
                GetQueryApiChild(source, item);
            }
        }

        /// <summary>
        /// 获取要删除的菜单
        /// </summary>
        /// <param name="delIds"></param>
        /// <returns></returns>
        private async Task<List<long>> GetMenuIncludeApis(List<long> delIds)
        {
            if (delIds == null || delIds.Count == 0)
                return null;
            List<Menus> allMenus = await _repository.Orm.Select<Menus>()
                .Where(p => p.ApiId != null)
                .NoTracking()
                .ToListAsync();
            if (allMenus == null || allMenus.Count == 0)
                return null;
            //获取要删除的menuid
            List<long> delParentMenuIds = allMenus.Where(p => delIds.Contains(p.ApiId.Value))
                .Select(p => p.Id)
                .ToList();
            if (delParentMenuIds == null || delParentMenuIds.Count == 0)
                return null;
            List<long> delMenuIds = new List<long>();
            foreach (var item in delParentMenuIds)
            {
                SearchDelMenus(allMenus, item, delMenuIds);
            }
            return delMenuIds;
        }

        /// <summary>
        /// 获取要删除的菜单权限
        /// </summary>
        /// <param name="delIds"></param>
        /// <returns></returns>
        private async Task<List<long>> GetPermissionIncludeMenus(List<long> delIds)
        {
            if (delIds == null || delIds.Count == 0)
                return null;
            List<RolePermissions> allPermissions = await _repository.Orm.Select<RolePermissions>()
                .Where(p => delIds.Contains(p.MenuId))
                .NoTracking()
                .ToListAsync();
            if (allPermissions == null || allPermissions.Count == 0)
                return null;
            return allPermissions.Select(p => p.Id).ToList();
        }

        /// <summary>
        /// 搜素要删除的父节点和子节点
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <param name="dest"></param>
        private void SearchDelApis(List<Apis> source, long id, List<long> dest)
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
            List<Apis> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelApis(source, citem.Id, dest);
            }
        }

        private void SearchDelMenus(List<Menus> source, long id, List<long> dest)
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
            List<Menus> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelMenus(source, citem.Id, dest);
            }
        }

        #region Resolver
        private void DifferApis(List<Apis> oldApis, List<Apis> newApis)
        {
            List<Apis> tempApis = new List<Apis>();
            //查找需要新增的
            foreach (var newItem in newApis)
            {
                bool isFind = false;
                foreach (var oldItem in oldApis)
                {
                    if (oldItem.Path == newItem.Path
                        && oldItem.Version == newItem.Version
                        && oldItem.OperationId == newItem.OperationId)
                    {
                        isFind = true;
                        break;
                    }
                }
                if (!isFind)
                {
                    tempApis.Add(newItem);
                }
            }
            //新增
            if (tempApis.Count > 0)
            {
                foreach (var item in tempApis)
                {
                    InsertApis(oldApis, newApis, item);
                }
                tempApis.Clear();
            }

            //查找需要删除和修改的
            foreach (var oldItem in oldApis)
            {
                bool isFind = false;
                foreach (var newItem in newApis)
                {
                    if (oldItem.Path == newItem.Path
                        && oldItem.Version == newItem.Version
                        && oldItem.OperationId == newItem.OperationId)
                    {
                        isFind = true;
                        //需要修改的直接修改
                        if (oldItem.Name != newItem.Name
                        || oldItem.Code != newItem.Code
                        || oldItem.Description != newItem.Description
                        || oldItem.Method != newItem.Method
                        || oldItem.Parameters != newItem.Parameters)
                        {
                            oldItem.Name = newItem.Name;
                            oldItem.Code = newItem.Code;
                            oldItem.Description = newItem.Description;
                            oldItem.Method = newItem.Method;
                            oldItem.Parameters = newItem.Parameters;
                            oldItem.UpdatedTime = DateTime.Now;
                            oldItem.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
                        }
                        break;
                    }
                }
                if (!isFind)
                {
                    tempApis.Add(oldItem);
                }
                if (isFind
                    && oldItem.ParentId != null
                    && !oldApis.Any(p => p.Id == oldItem.ParentId)
                    && !tempApis.Contains(oldItem))
                {
                    tempApis.Add(oldItem);
                }
            }
            //删除
            if (tempApis.Count != 0)
            {
                foreach (var item in tempApis)
                {
                    DeleteApis(oldApis, item);
                }
                tempApis.Clear();
            }

        }

        private List<ApiDocInfo> ResolveApi()
        {
            IDictionary<string, OpenApiInfo> docsInfos = _options.SwaggerDocs;
            if (docsInfos == null || docsInfos.Count == 0)
            {
                throw new BusException(-1, "Can not find open api option from SwaggerGeneratorOptions.");
            }
            List<ApiDocInfo> result = new List<ApiDocInfo>();
            foreach (var item in docsInfos)
            {
                OpenApiDocument docs = _swagger.GetSwagger(item.Key);
                if (docs == null)
                    continue;
                if (docs.Paths == null || docs.Paths.Count == 0)
                    continue;
                foreach (var pathItem in docs.Paths)
                {
                    var operations = pathItem.Value?.Operations;
                    if (operations == null || operations.Count == 0)
                    {
                        continue;
                    }
                    foreach (var operationItem in operations)
                    {
                        ApiDocInfo itemInfo = new ApiDocInfo()
                        {
                            Controller = GetItemController(operationItem.Value?.OperationId, operationItem.Value?.Tags),
                            Path = pathItem.Key,
                            Name = operationItem.Value?.Summary,
                            OperationId = operationItem.Value?.OperationId,
                            Version = docs?.Info.Version,
                            Description = operationItem.Value?.Description,
                            Method = operationItem.Key.ToString(),
                            Parameters = operationItem.Value?.Parameters?.Select(p => p?.Name).ToList()
                        };
                        if (!string.IsNullOrEmpty(itemInfo.Controller)
                            && docs.Tags != null
                            && docs.Tags.Count > 0)
                        {
                            var tagItem = docs.Tags.Where(p => p.Name.Equals(itemInfo.Controller, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            if (tagItem != null)
                            {
                                itemInfo.ControllerName = tagItem.Description;
                            }
                        }
                        if (string.IsNullOrEmpty(itemInfo.ControllerName))
                        {
                            itemInfo.ControllerName = itemInfo.Controller;
                        }
                        if (string.IsNullOrEmpty(itemInfo.Name))
                        {
                            itemInfo.Name = itemInfo.Path;
                        }
                        result.Add(itemInfo);
                    }
                }
            }
            return result;
        }

        private string GetItemController(string operationId, IList<OpenApiTag> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(operationId))
            {
                return tags.FirstOrDefault()?.Name;
            }
            string operationNode = operationId.Split('-')[0];
            OpenApiTag tag = tags.Where(p => p.Name.Equals(operationNode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (tag == null)
            {
                return tags.FirstOrDefault()?.Name;
            }
            else
            {
                return tag.Name;
            }
        }

        /// <summary>
        /// 递归删除父节点及其子节点
        /// </summary>
        /// <param name="source"></param>
        /// <param name="api"></param>
        private void DeleteApis(List<Apis> source, Apis api)
        {
            if (source.Any(p => p.Id == api.Id))
            {
                source.Remove(api);
            }
            //搜索是否需要删除子节点
            if (source.Any(p => p.ParentId == api.Id))
            {
                List<Apis> temp = source.Where(p => p.ParentId == api.Id).ToList();
                foreach (var item in temp)
                {
                    DeleteApis(source, item);
                }
            }
        }

        private void DeleteApis(List<Apis> source)
        {
            List<Apis> delApis = new List<Apis>();
            foreach (var item in source)
            {
                if (item.ParentId != null && item.ParentId > 0)
                {
                    if (!source.Any(p => p.Id == item.ParentId))
                    {
                        delApis.Add(item);
                    }
                }
            }
            if (delApis.Count > 0)
            {
                foreach (var item in delApis)
                {
                    source.Remove(item);
                }
            }
        }

        private void InsertApis(List<Apis> oldSource, List<Apis> newSource, Apis api)
        {
            if (api.ParentId == null || api.ParentId == 0)
            {
                if (!oldSource.Contains(api)) oldSource.Add(api);
            }
            else
            {
                if (oldSource.Any(p => p.Id == api.ParentId))
                {
                    if (!oldSource.Contains(api)) oldSource.Add(api);
                }
                else
                {
                    var newParentApi = newSource.Where(p => p.Id == api.ParentId).SingleOrDefault();
                    if (newParentApi == null)
                        throw new BusException(-1, $"未找到当前Api(Id:{api.Id},Pid:{api.ParentId})的父节点。");
                    var oldParentApi = oldSource
                        .Where(p => p.Path == newParentApi.Path && p.Version == newParentApi.Version && p.OperationId == newParentApi.OperationId)
                        .SingleOrDefault();
                    if (oldParentApi != null)
                    {
                        api.ParentId = oldParentApi.Id;
                    }
                    if (!oldSource.Contains(api)) oldSource.Add(api);
                }
            }
            //搜索是否需要插入子节点
            if (newSource.Any(p => p.ParentId == api.Id))
            {
                List<Apis> temp = newSource.Where(p => p.ParentId == api.Id).ToList();
                foreach (var item in temp)
                {
                    InsertApis(oldSource, newSource, item);
                }
            }
        }

        #endregion
    }
}
