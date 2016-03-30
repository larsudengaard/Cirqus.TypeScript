using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cirqus.TypeScript.Config;
using Cirqus.TypeScript.Model;

namespace Cirqus.TypeScript.Generation
{
    class ProxyGenerator
    {
        public List<ProxyGenerationResult> Generate(string sourceDll)
        {
            var assembly = LoadAssembly(sourceDll);

            var configurators = assembly.GetTypes().Where(x => typeof(TypeScriptConfigurator).IsAssignableFrom(x)).ToList();
            if (configurators.Count > 1)
            {
                throw new PrettyException("Found multiple configurations in command assembly, only one is supported.");
            }

            var configuration = new Configuration();
            if (configurators.Count == 1)
            {
                var configuratorType = configurators.Single();
                var configurator = (TypeScriptConfigurator)Activator.CreateInstance(configuratorType);
                configuration = configurator.Configure();
            }

            return GetProxyGenerationResults(configuration).ToList();
        }

        public IEnumerable<ProxyGenerationResult> GetProxyGenerationResults(Configuration configuration)
        {
            var types = configuration.Types;

            Console.WriteLine("Found {0} types", types.Count);

            var context = new ProxyGeneratorContext(types, configuration);

            var apiCode = context.GetDefinitions(CirqusType.Command, CirqusType.View, CirqusType.Other, CirqusType.Primitive);
            var systemCode = context.GetSystemDefinitions();

            yield return new ProxyGenerationResult(apiCode + systemCode);
        }

        Assembly LoadAssembly(string filePath)
        {
            try
            {
                Console.WriteLine("Loading DLL {0}", filePath);
                return Assembly.LoadFrom(Path.GetFullPath(filePath));
            }
            catch (BadImageFormatException exception)
            {
                throw new BadImageFormatException(string.Format("Could not load {0}", filePath), exception);
            }
        }
    }
}