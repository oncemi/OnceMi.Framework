using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;

namespace OnceMi.Framework.IService.Admin
{
    public interface ICacheService : IBaseService
    {
        /// <summary>
        /// 获取缓存key
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public List<CacheKeyItemResponse> GetCacheKeys(string queryString);

        /// <summary>
        /// 清理缓存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DeleteCachesResponse DeleteCaches(DeleteCachesRequest request);
    }
}
