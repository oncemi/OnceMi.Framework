using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IJobsService : IBaseService<Jobs, long>
    {
        Task<Jobs> QueryJobById(long Id);

        Task Update(Jobs job, bool isSaveToDb = false);

        Task<JobItemResponse> Insert(CreateJobRequest request);
    }
}
