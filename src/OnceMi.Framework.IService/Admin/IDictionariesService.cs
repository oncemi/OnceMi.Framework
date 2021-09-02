using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IDictionariesService : IBaseService<Dictionaries, long>
    {
        ValueTask<int> QueryNextSortValue(long? parentId);

        Task<DictionaryItemResponse> Query(DictionaryDetailRequest request);

        Task<IPageResponse<DictionaryItemResponse>> Query(IPageRequest request);

        Task<DictionaryItemResponse> Insert(CreateDictionaryRequest request);

        Task Update(UpdateDictionaryRequest request);

        Task Delete(List<long> ids);
    }
}
