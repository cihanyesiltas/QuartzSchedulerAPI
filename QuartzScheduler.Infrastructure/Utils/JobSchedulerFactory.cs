using QuartzScheduler.Infrastructure.Interfaces;
using QuartzScheduler.Infrastructure.Models;

namespace QuartzScheduler.Infrastructure.Utils
{
    public static class JobSchedulerFactory
    {
        public static IJobScheduler CreateJobScheduler(Connection connection)
        {
            return new JobScheduler(connection);
        }
    }
}
