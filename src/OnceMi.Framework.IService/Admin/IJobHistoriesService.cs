using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IJobHistoriesService : IBaseService<JobHistories, long>
    {
        Task<IPageResponse<JobHistoryItemResponse>> Query(JobHistoryPageRequest request);
    }
}
