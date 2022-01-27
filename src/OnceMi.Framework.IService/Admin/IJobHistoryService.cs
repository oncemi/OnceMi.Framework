using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IJobHistoryService : IBaseService<JobHistory, long>
    {
        Task<IPageResponse<JobHistoryItemResponse>> Query(JobHistoryPageRequest request);
    }
}
