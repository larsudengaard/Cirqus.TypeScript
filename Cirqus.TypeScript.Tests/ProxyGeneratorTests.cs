using System;
using System.Collections.Generic;
using System.Linq;
using Cirqus.TypeScript.Config;
using Cirqus.TypeScript.Generation;
using Cirqus.TypeScript.Model;
using Xunit;

namespace Cirqus.TypeScript.Tests
{
    public class ProxyGeneratorTests
    {
        public class ConfiguratorThatIncludesCustomTypes : TypeScriptConfigurator
        {
            public ConfiguratorThatIncludesCustomTypes()
            {
                Include<CustomClass>();
                Include<CustomEnum>();
            } 
        }

        [Fact]
        public void EmitsIncludedClasses()
        {
            var results = new ProxyGenerator().GetProxyGenerationResults(
                new[] { typeof (CustomClass)}, 
                new ConfiguratorThatIncludesCustomTypes().Configure());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests.ProxyGeneratorTests {
    export interface CustomClass {
        number: number;
    }
}
", results.Last().Code);
        }

        [Fact]
        public void EmitsIncludedEnums()
        {
            var results = new ProxyGenerator().GetProxyGenerationResults(
                new[] { typeof(CustomClass) },
                new ConfiguratorThatIncludesCustomTypes().Configure());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests.ProxyGeneratorTests {
    export enum CustomEnum {
        None = 1
    }
}
", results.Last().Code);
        }

        public class CustomClass
        {
            public int Number { get; set; }
        }


        public enum CustomEnum
        {
            None = 1
        }
    }
}