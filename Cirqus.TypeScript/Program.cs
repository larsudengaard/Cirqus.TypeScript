using System;
using System.IO;
using System.Linq;
using Cirqus.TypeScript.Generation;
using Serilog;
using Serilog.Events;

namespace Cirqus.TypeScript
{
    class Program
    {
        static int Main(string [] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole(LogEventLevel.Information)
                .CreateLogger();

            try
            {
                Log.Logger.Information(@"-----------------------------------------------------------------------------
             d60 Cirqus TypeScript client code generator
-----------------------------------------------------------------------------");

                Run(args);

                return 0;
            }
            catch (PrettyException exception)
            {
                Log.Logger.Information(exception, exception.Message);

                return 1;
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception, "Unhandled exception");

                return 2;
            }
        }

        static void Run(string[] args)
        {
            if (args.Length != 2)
            {
                throw new PrettyException(@"Please call the tool like this:

    d60.Cirqus.TsClient <path-to-DLL> <output-directory>

where <path-to-DLL> should point to an assembly containing all of your commands,
and <output-directory> should be the directory in which you want the generated
'commands.ts','commandProcessor.ts', 'views.ts' and 'common.ts' to be put.");
            }

            var sourceDll = args[0];
            var destinationDirectory = args[1];

            if (!File.Exists(sourceDll))
            {
                throw new FileNotFoundException(string.Format("Could not find source DLL {0}", sourceDll));
            }

            if (!Directory.Exists(destinationDirectory))
            {
                Log.Logger.Information("Creating directory {0}", destinationDirectory);
                Directory.CreateDirectory(destinationDirectory);
            }

            var proxyGenerator = new ProxyGenerator(sourceDll);
            
            var results = proxyGenerator.Generate().ToList();

            Log.Logger.Information("Writing files");
            foreach (var result in results)
            {
                result.WriteTo(destinationDirectory);
            }
        }
    }
}
