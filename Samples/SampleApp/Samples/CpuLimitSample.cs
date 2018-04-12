using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Processes;
using Fs.Processes.JobObjects;

namespace SampleApp.Samples
{
    public class CpuLimitSample: ISampleAsync
    {
        public async Task RunAsync ()
        {
            // setup a soft CPU rate limit of 10% and notification for when it is
            // exceeded..

            var jobLimits = new JobLimits();
            var jobNotifications = new JobNotifications();

            jobLimits.CpuRate = new CpuRateLimit(10.0m, false);
            jobNotifications.CpuRate = new RateControl(RateControlInterval.Short, RateControlTolerance.Low);

            using (var jobObject = new JobObject(jobLimits, jobNotifications))
            {
                // assume our working directory is the output path for the project, and that we're in
                // debug configuration..

                jobObject.CpuRateLimitExceeded += ( s, e ) =>
                {
                    Console.WriteLine("CPU rate exceeded");
                };

                using (var process = jobObject.CreateProcess(new CreateProcessInfo
                {
                    FileName = @"..\..\..\BusyApp\bin\Debug\BusyApp.exe"
                }))
                {
                    await process.Exited;
                }
            }
        }
    }
}
