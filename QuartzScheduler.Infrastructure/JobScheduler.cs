using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using QuartzScheduler.Infrastructure.Interfaces;
using QuartzScheduler.Infrastructure.Models;
using QuartzScheduler.Infrastructure.Utils;

namespace QuartzScheduler.Infrastructure
{
    public class JobScheduler : IJobScheduler
    {
        private IScheduler _scheduler;

        public JobScheduler(Connection connection)
        {
            InitializeScheduler(connection.ServerName, connection.Port, connection.SchedulerName);
        }

        public void Start()
        {
            if (!_scheduler.IsStarted)
                _scheduler.Start();
        }

        public void Shutdown(bool waitForJobsToComplete)
        {
            if (!_scheduler.IsShutdown)
                _scheduler.Shutdown(waitForJobsToComplete);
        }

        public void StandBy()
        {
            if (!_scheduler.InStandbyMode)
                _scheduler.Standby();
        }

        /// <summary>
        /// Pause all triggers of jobs
        /// </summary>
        public void PauseAll()
        {
            _scheduler.PauseAll();
        }

        /// <summary>
        /// Resume all triggers of jobs
        /// </summary>
        public void ResumeAll()
        {
            _scheduler.ResumeAll();
        }

        public List<Job> GetAllJobs()
        {
            var jobs = new List<Job>();
            var jobGroups = _scheduler.GetJobGroupNames();
            foreach (string group in jobGroups)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupEquals(group);
                var jobKeys = _scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys)
                {
                    var job = new Job
                    {
                        Name = jobKey.Name,
                        GroupName = group,
                        Triggers = new List<Trigger>()
                    };
                    var triggers = _scheduler.GetTriggersOfJob(jobKey);
                    foreach (var trigger in triggers)
                    {
                        var cronTrigger = trigger as ICronTrigger;
                        var nextFireTime = trigger.GetNextFireTimeUtc();
                        var previousFireTime = trigger.GetPreviousFireTimeUtc();
                        job.Triggers.Add(new Trigger
                        {
                            Name = trigger.Key.Name,
                            GroupName = trigger.Key.Group,
                            Type = trigger.GetType().Name,
                            State = _scheduler.GetTriggerState(trigger.Key).ToString(),
                            NextFireTime = nextFireTime.HasValue
                                ? TimeZone.CurrentTimeZone.ToLocalTime(nextFireTime.Value.DateTime)
                                : DateTime.MinValue,
                            PreviousFireTime = previousFireTime.HasValue
                                ? TimeZone.CurrentTimeZone.ToLocalTime(previousFireTime.Value.DateTime)
                                : DateTime.MinValue,
                            CronExpression = cronTrigger?.CronExpressionString,
                            DataMap = trigger.JobDataMap
                        });
                    }
                    jobs.Add(job);
                }
            }
            return jobs;
        }

        public IScheduler GetScheduler()
        {
            return _scheduler;
        }

        /// <summary>
        ///  Remove the indicated trigger from the scheduler. If job is not durable, 
        ///  then job will be also deleted.
        /// </summary>
        /// <returns></returns>
        public bool UnscheduleJob(string triggerName, string triggerGroup)
        {
            return _scheduler.UnscheduleJob(new TriggerKey(triggerName, triggerGroup));
        }

        /// <summary>
        /// Delete specified job and all of its triggers. Does not stop running trigger.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        public bool DeleteJob(string jobName, string jobGroup)
        {
            return _scheduler.DeleteJob(new JobKey(jobName, jobGroup));
        }

        /// <summary>
        /// Pause job's all triggers. Does not stop running trigger.
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="groupName"></param>
        public void PauseJob(string jobName, string groupName)
        {
            _scheduler.PauseJob(new JobKey(jobName, groupName));
        }

        public void ResumeJob(string jobName, string groupName)
        {
            _scheduler.ResumeJob(new JobKey(jobName, groupName));
        }

        public void TriggerJob(string jobName, string groupName)
        {
            _scheduler.TriggerJob(new JobKey(jobName, groupName));
        }

        public bool IsJobExist(string jobName, string groupName)
        {
            return _scheduler.CheckExists(new JobKey(jobName, groupName));
        }

        public void ResumeTrigger(string triggerName, string groupName)
        {
            _scheduler.ResumeTrigger(new TriggerKey(triggerName, groupName));
        }

        public void PauseTrigger(string triggerName, string groupName)
        {
            _scheduler.PauseTrigger(new TriggerKey(triggerName, groupName));
        }

        public void AddJob(string jobName, string jobGroup, string triggerName, string triggerGroup, string cronExpression, Type jobType, bool isDurable, IDictionary<string, object> jobProperties)
        {
            if (IsJobExist(jobName, jobGroup))
                throw new ArgumentException(string.Format("Job name {0} Group Name {1} already exists", jobName, jobGroup));

            if (!JobUtil.IsValidCronExpression(cronExpression))
                throw new ArgumentException(string.Format("Invalid Expression {0}", cronExpression));

            var jobDetail = JobBuilder.Create()
                .OfType(jobType)
                .WithIdentity(jobName, jobGroup)
                .StoreDurably(isDurable)
                .RequestRecovery(true)
                .Build();

            //build trigger by cron expression
            var triggerBuilder = TriggerBuilder.Create()
                .WithIdentity(triggerName, triggerGroup)
                .WithCronSchedule(cronExpression);

            if (jobProperties != null && jobProperties.Count > 0)
                triggerBuilder.UsingJobData(new JobDataMap(jobProperties));

            var trigger = triggerBuilder.Build();

            _scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void ScheduleJob(string jobName, string groupName, string cronExpression)
        {
            if (!IsJobExist(jobName, groupName))
                throw new ArgumentException(string.Format("Job name {0} Group Name {1} does not exist", jobName, groupName));

            if (!JobUtil.IsValidCronExpression(cronExpression))
                throw new ArgumentException(string.Format("Invalid Expression {0}", cronExpression));

            var jobDetail = _scheduler.GetJobDetail(new JobKey(jobName, groupName));

            //build trigger by cron expression
            var trigger = TriggerBuilder.Create()
                .WithIdentity(jobName, groupName)
                .WithCronSchedule(cronExpression)
                .ForJob(jobDetail)
                .Build();

            _scheduler.ScheduleJob(trigger);
        }

        public SchedulerInformation GetMetaData()
        {
            var metaData = _scheduler.GetMetaData();
            return new SchedulerInformation
            {
                SchedulerName = metaData.SchedulerName,
                InStandbyMode = metaData.InStandbyMode,
                Shutdown = metaData.Shutdown,
                JobStoreSupportsPersistence = metaData.JobStoreSupportsPersistence,
                NumberOfJobsExecuted = metaData.NumberOfJobsExecuted,
                RunningSince = metaData.RunningSince,
                SchedulerInstanceId = metaData.SchedulerInstanceId,
                SchedulerRemote = metaData.SchedulerRemote,
                SchedulerType = metaData.SchedulerType,
                Started = metaData.Started,
                ThreadPoolSize = metaData.ThreadPoolSize,
                ThreadPoolType = metaData.ThreadPoolType
            };
        }

        public List<Job> GetCurrentlyExecutingJobs()
        {
            var executingJobs = _scheduler.GetCurrentlyExecutingJobs();

            return (from job in executingJobs
                    let nextFireTime = job.Trigger.GetNextFireTimeUtc()
                    let previousFireTime = job.Trigger.GetPreviousFireTimeUtc()
                    select new Job
                    {
                        Name = job.JobDetail.Key.Name,
                        GroupName = job.JobDetail.Key.Group,
                        Triggers = new List<Trigger>
                    {
                        new Trigger
                        {
                            GroupName = job.Trigger.Key.Group,
                            Name = job.Trigger.Key.Name,
                            NextFireTime = nextFireTime.HasValue
                                    ? TimeZone.CurrentTimeZone.ToLocalTime(nextFireTime.Value.DateTime)
                                    : DateTime.MinValue,
                            PreviousFireTime = previousFireTime.HasValue
                                    ? TimeZone.CurrentTimeZone.ToLocalTime(previousFireTime.Value.DateTime)
                                    : DateTime.MinValue
                        }
                    }
                    }).ToList();
        }

        #region Private Methods

        private void InitializeScheduler(string server, int port, string scheduler)
        {
            string address = string.Format("tcp://{0}:{1}/{2}", server, port, scheduler);
            var schedulerFactory = new StdSchedulerFactory(GetProperties(address));
            _scheduler = schedulerFactory.GetScheduler();
        }

        private NameValueCollection GetProperties(string address)
        {
            var properties = new NameValueCollection
            {
                ["quartz.scheduler.instanceName"] = ConfigurationManager.AppSettings["quartz.scheduler.instanceName"],
                ["quartz.scheduler.proxy"] = ConfigurationManager.AppSettings["quartz.scheduler.proxy"],
                ["quartz.threadPool.threadCount"] = ConfigurationManager.AppSettings["quartz.threadPool.threadCount"],
                ["quartz.scheduler.proxy.address"] = address
            };
            return properties;
        }

        #endregion
    }
}
