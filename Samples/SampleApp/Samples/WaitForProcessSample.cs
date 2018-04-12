using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fs.Processes;

namespace SampleApp.Samples
{
    public class WaitForProcessSample: ISampleAsync
    {
        public async Task RunAsync ()
        {
            var processInfo = new CreateProcessInfo
            {
                FileName = "ping.exe",
                ArgumentsList = {"8.8.8.8"}
            };

            using (var process = new Process(processInfo))
            {
                await process.Exited;
            }
        }
    }
}
