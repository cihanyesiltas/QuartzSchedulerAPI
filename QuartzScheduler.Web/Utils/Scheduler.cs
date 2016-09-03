using System.Web.Configuration;
using QuartzScheduler.Infrastructure.Interfaces;
using QuartzScheduler.Infrastructure.Models;
using QuartzScheduler.Infrastructure.Utils;

namespace QuartzScheduler.Web.Utils
{
    public sealed class Scheduler
    {
        private static volatile IJobScheduler _instance;
        private static object syncRoot = new object();

        private Scheduler() { }

        public static IJobScheduler Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (syncRoot)
                {
                    if (_instance == null)
                        _instance = JobSchedulerFactory.CreateJobScheduler(new Connection
                        {
                            SchedulerName = WebConfigurationManager.AppSettings["Quartz.SchedulerName"],
                            ServerName = WebConfigurationManager.AppSettings["Quartz.ServerName"],
                            Port = int.Parse(WebConfigurationManager.AppSettings["Quartz.Port"])
                        });
                }

                return _instance;
            }
        }
    }
}