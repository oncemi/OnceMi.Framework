
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public interface IJobSchedulerService
    {
        Task Init();

        /// <summary>
        /// 添加（开始）任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Add(Jobs job);

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Delete(Jobs job);

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Pause(Jobs job);

        /// <summary>
        /// 恢复任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Resume(Jobs job);

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Stop(Jobs job);

        /// <summary>
        /// 立即执行任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Trigger(Jobs job);

        /// <summary>
        /// 作业是否存在
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task<bool> Exists(Jobs job);
    }
}
