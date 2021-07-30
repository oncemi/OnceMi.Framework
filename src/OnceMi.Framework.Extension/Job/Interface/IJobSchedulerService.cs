
using OnceMi.Framework.Model.Dto;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public interface IJobSchedulerService
    {
        Task Init();

        Task Add(JobItemResponse job);
    }
}
