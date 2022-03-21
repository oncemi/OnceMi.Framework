using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnceMi.Framework.Extension.Job;
using Quartz;
using System;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterQuartz
    {
        public static IServiceCollection AddQuartz(this IServiceCollection services)
        {
            int appId = -1;
            using (var provider = services.BuildServiceProvider())
            {
                string appIdStr = provider.GetRequiredService<IConfiguration>().GetValue<string>("AppSettings:AppId");
                if (string.IsNullOrEmpty(appIdStr) || !int.TryParse(appIdStr, out appId) || appId <= 0)
                {
                    throw new Exception("Can not get app id from app setting.");
                }
            }
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = false; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });

            services.AddQuartz(q =>
            {
                q.SchedulerId = $"FrameworkScheduler#{appId}";

                q.UseMicrosoftDependencyInjectionJobFactory();

                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 10;
                });
                // auto-interrupt long-running job
                q.UseJobAutoInterrupt(options =>
                {
                    // this is the default
                    options.DefaultMaxRunTime = TimeSpan.FromMinutes(60);
                });
                // convert time zones using converter that can handle Windows/Linux differences
                q.UseTimeZoneConverter();
            });
            //作业通知服务
            services.AddScoped<IJobNoticeService, JobNoticeService>();
            //注册JobScheduler
            services.AddScoped<IJobSchedulerService, JobSchedulerService>();
            //add base job
            services.AddTransient<HttpExcuteJob>();
            // Quartz.Extensions.Hosting allows you to fire background service that handles scheduler lifecycle
            services.AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });

            return services;
        }
    }
}
