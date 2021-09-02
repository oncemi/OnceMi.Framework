using OnceMi.Framework.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface ICacheService : IBaseService
    {
        public List<CacheKeyItemResponse> GetCacheKeys(string queryString);

        public DeleteCachesResponse DeleteCaches(DeleteCachesRequest request);
    }
}
