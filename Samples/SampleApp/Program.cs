using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        private static IEnumerable<Type> GetSamples ()
        {
            return typeof(Program)
                .Assembly
                .DefinedTypes
                .Where(t => t.ImplementedInterfaces
                    .Any(i => (i == typeof(ISample)) || (i == typeof(ISampleAsync)))
                );
        }

        private static IEnumerable<object> GetSamples ( string[] sampleNames )
        {
            var canonicalNames = sampleNames.
                Select(s => s.EndsWith("Sample", StringComparison.OrdinalIgnoreCase) ? s : s + "Sample")
                .ToArray();

            // new up each of the requested sample types..
            return GetSamples()
                .Join(canonicalNames,
                      inner => inner.Name,
                      outer => outer,
                      ( outer, inner ) => outer)
                .Select(t => Activator.CreateInstance(t));
        }

        private static void ShowSamples ()
        {
            Console.WriteLine("Samples:");
            foreach (var sampleType in GetSamples())
                Console.WriteLine($"  {sampleType.Name}");

            Console.WriteLine();
        }

        static async Task Main ( string[] args )
        {
            try
            {
                if ((args == null) || (args.Length == 0))
                {
                    ShowSamples();
                    return;
                }

                foreach (var sampleInstance in GetSamples(args))
                {
                    Console.WriteLine($"{sampleInstance.GetType().FullName}:");
                    if (sampleInstance is ISampleAsync)
                        await ((ISampleAsync)sampleInstance).RunAsync();
                    else if (sampleInstance is ISample)
                        ((ISample)sampleInstance).Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled Exception:");
                Console.WriteLine(ex);
            }
            finally
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("Done");
                    Console.ReadLine();
                }
            }
        }
    }
}
