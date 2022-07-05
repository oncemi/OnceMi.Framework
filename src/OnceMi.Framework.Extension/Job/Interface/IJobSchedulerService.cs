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
        Task Add(Entity.Admin.Job job);

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Delete(Entity.Admin.Job job);

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Pause(Entity.Admin.Job job);

        /// <summary>
        /// 恢复任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Resume(Entity.Admin.Job job);

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Stop(Entity.Admin.Job job);

        /// <summary>
        /// 立即执行任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task Trigger(Entity.Admin.Job job);

        /// <summary>
        /// 作业是否存在
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        Task<bool> Exists(Entity.Admin.Job job);
    }
}
