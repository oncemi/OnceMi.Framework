using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IJobsService : IBaseService<Jobs, long>
    {
        /// <summary>
        /// 查询job by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<Jobs> QueryJobById(long Id);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IPageResponse<JobItemResponse>> Query(IJobPageRequest request);

        /// <summary>
        /// 查询初始化作业
        /// </summary>
        /// <returns></returns>
        Task<List<Jobs>> QueryInitJobs();

        Task<Jobs> Insert(CreateJobRequest request);

        Task Update(UpdateJobRequest request);

        /// <summary>
        /// 更新作业状态
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="status"></param>
        /// <param name="isSaveToDb"></param>
        /// <returns></returns>
        Task UpdateJobStatus(long JobId, JobStatus status, bool isSaveToDb = false);

        /// <summary>
        /// 更新作业状态
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="status"></param>
        /// <param name="isSaveToDb"></param>
        /// <returns></returns>
        Task UpdateJobStatus(long jobId
            , JobStatus status
            , int? fireCount
            , DateTime? fireTime
            , DateTime? nextFireTime
            , bool isSaveToDb = false);

        /// <summary>
        /// 设置到期作业为停止状态
        /// </summary>
        /// <returns></returns>
        Task UpdateEndTimeJob();

        /// <summary>
        /// 禁用/启用任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="isDisable"></param>
        /// <returns></returns>
        Task DisableJob(long jobId, bool isDisable);

        Task Delete(List<long> ids);
    }
}
