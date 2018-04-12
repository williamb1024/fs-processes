using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Processes;

namespace SampleApp.Samples
{
    public class RedirectSample: ISampleAsync
    {
        public async Task RunAsync ()
        {
            var processInfo = new CreateProcessInfo
            {
                FileName = "ping.exe",
                ArgumentsList = {"8.8.8.8"},
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = new Process(processInfo))
            {
                process.OutputDataReceived += ( s, d ) =>
                {
                    // Data is null when EOF is read
                    if (d.Data != null)
                    {
                        Console.Write("StdOut: ");
                        Console.WriteLine(d.Data);
                    }
                };

                process.ErrorDataReceived += ( s, d ) =>
                {
                    // Data is null when EOF is read
                    if (d.Data != null)
                    {
                        Console.Write("StdErr: ");
                        Console.WriteLine(d.Data);
                    }
                };

                // passing 'false' to these methods tells the async reader to call the event handlers
                // every time data is received, rather than waiting for a CR, LF or CRLF pair terminator.

                var outTask = process.BeginReadingStandardOutputAsync(false);
                var errTask = process.BeginReadingStandardErrorAsync(false);

                // wait for the process to exit..
                await process.Exited;

                // the outTasks and errTask are completed when EOF is read..
                await outTask;
                await errTask;
            }
        }
    }
}
