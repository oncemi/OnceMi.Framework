using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IViewService : IBaseService<View, long>
    {
        Task<ViewItemResponse> Query(long id);

        Task<IPageResponse<ViewItemResponse>> Query(IPageRequest request, bool onlyQueryEnabled = false);

        Task<ViewItemResponse> Insert(CreateViewRequest request);

        Task Update(UpdateViewRequest request);

        Task Delete(List<long> ids);
    }
}
