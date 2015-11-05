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
            var allTypes = GetTypes(assembly);

            var configurators = allTypes.Where(x => typeof(TypeScriptConfigurator).IsAssignableFrom(x)).ToList();
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

            return GetProxyGenerationResults(allTypes, configuration).ToList();
        }

        public IEnumerable<ProxyGenerationResult> GetProxyGenerationResults(Type[] allTypes, Configuration configuration)
        {
            var commandTypes = allTypes.Where(ProxyGeneratorContext.IsCommand).ToList();
            var viewTypes = allTypes.Where(ProxyGeneratorContext.IsView).ToList();
            var additionalTypes = configuration.AdditionalTypes;

            Console.WriteLine("Found {0} command types", commandTypes.Count);
            Console.WriteLine("Found {0} view types", viewTypes.Count);
            Console.WriteLine("Found {0} additional types", additionalTypes.Count);

            var context = new ProxyGeneratorContext(commandTypes.Concat(viewTypes).Concat(additionalTypes), configuration);

            var apiCode = context.GetDefinitions(CirqusType.Command, CirqusType.View, CirqusType.Other, CirqusType.Primitive);
            var systemCode = context.GetSystemDefinitions();

            yield return new ProxyGenerationResult(apiCode + systemCode);
        }

        Type[] GetTypes(Assembly assembly)
        {
            try
            {
                var types = assembly.GetTypes();

                return types;
            }
            catch (ReflectionTypeLoadException exception)
            {
                var loaderExceptions = exception.LoaderExceptions;

                var message = string.Format(@"Could not load types from {0} - got the following loader exceptions: {1}", assembly, string.Join(Environment.NewLine, loaderExceptions.Select(e => e.ToString())));

                throw new ApplicationException(message);
            }
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