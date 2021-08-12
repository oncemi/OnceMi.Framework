using OnceMi.Framework.Model.Common;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public interface IJobNoticeService
    {
        Task Send(long jobId, JobExcuteResult result);
    }
}
