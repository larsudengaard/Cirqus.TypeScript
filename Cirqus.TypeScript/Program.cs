using System;
using System.IO;
using System.Linq;
using Cirqus.TypeScript.Generation;

namespace Cirqus.TypeScript
{
    class Program
    {
        static int Main(string [] args)
        {
            try
            {
                Console.WriteLine(@"-----------------------------------------------------------------------------
             d60 Cirqus TypeScript client code generator
-----------------------------------------------------------------------------");

                Run(args);

                return 0;
            }
            catch (PrettyException exception)
            {
                Console.WriteLine(exception.Message);

                return 1;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Unhandled exception: {0}", exception.Message);
                
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
                Console.WriteLine("Creating directory {0}", destinationDirectory);
                Directory.CreateDirectory(destinationDirectory);
            }

            var proxyGenerator = new ProxyGenerator(sourceDll);
            
            var results = proxyGenerator.Generate().ToList();

            Console.WriteLine("Writing files");
            foreach (var result in results)
            {
                result.WriteTo(destinationDirectory);
            }
        }
    }
}
