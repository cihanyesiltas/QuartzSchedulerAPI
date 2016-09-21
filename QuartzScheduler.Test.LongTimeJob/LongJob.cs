using System;
using System.IO;
using Quartz;

namespace QuartzScheduler.Test.LongTimeJob
{
    [DisallowConcurrentExecution]
    public class LongJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            File.AppendAllText(@"C:\Temp\LongJobFile.txt", "Job Started " +DateTime.Now.ToString("O") + Environment.NewLine);
            long j = 0;
            for (int i = 0; i < 1000000; i++)
            {
                for (int k = 0; k < 1000000; k++)
                {
                    j++;
                }
            }
            File.AppendAllText(@"C:\Temp\LongJobFile.txt", "Job Finished " + DateTime.Now.ToString("O") + Environment.NewLine);
        }
    }
}
