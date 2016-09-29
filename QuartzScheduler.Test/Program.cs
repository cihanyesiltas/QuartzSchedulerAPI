using QuartzScheduler.Infrastructure.Interfaces;
using QuartzScheduler.Infrastructure.Models;
using QuartzScheduler.Infrastructure.Utils;

namespace QuartzScheduler.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = new Connection
            {
                Port = 555,
                SchedulerName = "QuartzScheduler",
                ServerName = "localhost"
            };

            IJobScheduler jobScheduler = JobSchedulerFactory.CreateJobScheduler(conn);

           // var lis=jobScheduler.GetCurrentlyExecutingJobs();
            var metadata = jobScheduler.GetMetaData();
            jobScheduler.GetAllJobs();
            jobScheduler.ScheduleJob("LongJob", "LongJob", "0 0/1 * 1/1 * ? *");
            jobScheduler.DeleteJob("LongJob", "LongJob");

            var type = JobUtil.Create("QuartzScheduler.Test.LongTimeJob", "QuartzScheduler.Test.LongTimeJob.LongJob");

            jobScheduler.PauseJob("SampleJob", "SampleJob");
            jobScheduler.UnscheduleJob("SampleJob", "SampleJob");
            jobScheduler.ScheduleJob("SampleJob", "SampleJob", "0/10 * * * * ?");

            //jobScheduler.AddJob("SampleJob", "SampleJob", "0 0/1 * 1/1 * ? *", type, true, jobData);

            //jobScheduler.UnscheduleJob("SampleJob", "SampleJob");
            //jobScheduler.ScheduleJob("SampleJob", "SampleJob", "10 * * * * ?");

            //jobScheduler.PauseTrigger("SampleJob", "SampleJob");
            //jobScheduler.ResumeTrigger("SampleJob", "SampleJob");

            //jobScheduler.Shutdown(true);

            //jobScheduler.TriggerJob("SampleJob", "SampleJob");

            var list = jobScheduler.GetAllJobs();
        }
    }
}
