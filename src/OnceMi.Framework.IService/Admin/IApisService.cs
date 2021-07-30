﻿using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IApisService : IBaseService<Apis, long>
    {
        /// <summary>
        /// 查询Api的版本
        /// </summary>
        /// <returns></returns>
        Task<List<ISelectResponse<string>>> QueryApiVersions();

        /// <summary>
        /// 自动解析当前项目的API
        /// </summary>
        /// <returns></returns>
        Task AutoResolve();

        Task<ApiItemResponse> Query(long id);

        Task<IPageResponse<ApiItemResponse>> Query(QueryApiPageRequest request, bool onlyQueryEnabled = false);

        Task<ApiItemResponse> Insert(CreateApiRequest request);

        Task Update(UpdateApiRequest request);

        Task Delete(List<long> ids);
    }
}