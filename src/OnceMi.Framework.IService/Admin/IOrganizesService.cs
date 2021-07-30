using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IOrganizesService : IBaseService
    {
        Task<List<ISelectResponse<int>>> QueryOrganizeTypes();

        Task<OrganizeItemResponse> Query(long id);

        Task<IPageResponse<OrganizeItemResponse>> Query(OrganizePageRequest request, bool onlyQueryEnabled = false);

        Task<OrganizeItemResponse> Insert(CreateOrganizeRequest request);

        Task Update(UpdateOrganizeRequest request);

        Task Delete(List<long> ids);
    }
}
