using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Processes;
using Fs.Processes.JobObjects;

namespace SampleApp.Samples
{
    public class JobLimitSample : ISampleAsync
    {
        private const int ERROR_NOT_ENOUGH_QUOTA = 1816;

        public async Task RunAsync ()
        {
            using (var jobObject = new JobObject())
            {
                // setup some limits, we do this before starting any processes, but it's not
                // required..

                var jobLimits = new JobLimits();
                jobLimits.CpuRate = new CpuRateLimit(10.0m, false);
                jobLimits.Options = JobOptions.TerminateProcessesWhenJobClosed;
                jobLimits.ActiveProcesses = 3;
                jobObject.SetLimits(jobLimits);

                // setup a few event handlers..
                jobObject.ProcessAdded += ( s, e ) => Console.WriteLine($"Process {e.ID} added.");
                jobObject.ProcessExited += ( s, e ) => Console.WriteLine($"Process {e.ID} exited.");
                jobObject.ProcessLimitExceeded += ( s, e ) => Console.WriteLine("Process limit exceeded.");
                jobObject.CpuRateLimitExceeded += ( s, e ) => Console.WriteLine("CPU rate limit exceeded.");

                // configure CPU rate notification..
                var jobNotifications = new JobNotifications();
                jobNotifications.CpuRate = new RateControl(RateControlInterval.Short, RateControlTolerance.Low);

                // start some ping processes...
                var cpiPing = new CreateProcessInfo
                {
                    FileName = "ping.exe",
                    ArgumentsList = { "8.8.8.8", "-t" }
                };

                // create ping processes, directly through the JobObject, this will create the process
                // and associate the process with the JobObject ..
                for (int iIndex = 0; iIndex < 10; iIndex++)
                {
                    try
                    {
                        using (var process = jobObject.CreateProcess(cpiPing))
                            Console.WriteLine($"Created Process {process.Id}");
                    }
                    catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == ERROR_NOT_ENOUGH_QUOTA)
                    {
                        // ERROR_NOT_ENOUGH_QUOTA happens if the process cannot be assigned to the job object
                        // because of the active process limit..

                        Console.WriteLine("JobObject.CreateProcess failed, due to process limit.");
                    }
                }

                Console.WriteLine("[enter] to terminate active processes.");
                Console.ReadLine();
                jobObject.Kill();

                // wait for JobObject to become idle..
                await jobObject.Idle;
            }

        }
    }
}
