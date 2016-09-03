using System.IO;
using Quartz;

namespace QuartzScheduler.Infrastructure
{
   public class JobListener : IJobListener
    {

        public string Name
        {
            get { return "MyJobListener"; }
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            File.AppendAllText(@"C:\Users\cihan.yesiltas\Desktop\test.txt", "Vetoed  ");
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            File.AppendAllText(@"C:\Users\cihan.yesiltas\Desktop\test.txt", "ToBeExecuted  ");
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            File.AppendAllText(@"C:\Users\cihan.yesiltas\Desktop\test.txt", "Executed  ");
        }
    }
}
