using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusyApp
{
    class Program
    {
        private static int Value = 10;
        private static int NumberOfTasks = Environment.ProcessorCount;
        private static TimeSpan ProcessTime = TimeSpan.FromSeconds(30);

        private static Task BusyTask ( CancellationToken cancellationToken )
        {
            while (true)
            {
                for (int iIndex = 0; iIndex < 10000; iIndex++)
                    Value = (Value * 1) + 5;

                Value = 0;
                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            return Task.CompletedTask;
        }

        private static void Run ( CancellationToken cancellationToken )
        {
            List<Task> tasks = new List<Task>();

            for (int iIndex = 0; iIndex < NumberOfTasks; iIndex++)
                tasks.Add(Task.Run(() => BusyTask(cancellationToken)));

            // wait for each task to complete..
            foreach (var task in tasks)
                task.Wait();
        }

        private static void HandleArgument ( string argument )
        {
            // ignore arguments we don't understand..
            if (!argument.StartsWith("--"))
                return;

            var argumentParts = argument.Split(new char[] { '=' });
            if (argumentParts.Length != 2)
                return;

            if (argumentParts[0].Equals("--time", StringComparison.OrdinalIgnoreCase))
                ProcessTime = TimeSpan.FromSeconds(uint.Parse(argumentParts[1], NumberStyles.None, CultureInfo.InvariantCulture));
            else if (argumentParts[0].Equals("--tasks", StringComparison.OrdinalIgnoreCase))
                NumberOfTasks = (int)(uint.Parse(argumentParts[1], NumberStyles.None, CultureInfo.InvariantCulture));
        }

        static void Main ( string[] args )
        {
            foreach (var argument in args)
                HandleArgument(argument);

            using (var tokenSource = new CancellationTokenSource(ProcessTime))
                Run(tokenSource.Token);
        }
    }
}
