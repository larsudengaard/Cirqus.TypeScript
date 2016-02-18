using System;
using System.Collections.Generic;
using Cirqus.TypeScript.Config;
using Cirqus.TypeScript.Model;
using d60.Cirqus.Commands;
using Xunit;

namespace Cirqus.TypeScript.Tests
{
    public class ProxyGeneratorContextTests
    {
        [Fact]
        public void CommandIsExportedAsClassAndMetaDataIsIgnored()
        {
            var context = new ProxyGeneratorContext(new[] {typeof (CommandClass)}, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export class CommandClass implements Command {

        constructor(args: {prop: number}) {
            this.prop = args.prop;
        }

        $commandType = ""Cirqus.TypeScript.Tests.CommandClass, Cirqus.TypeScript.Tests"";
        $commandName = ""CommandClass"";
        prop: number;
    }
}
", context.GetDefinitions(CirqusType.Command));

        }

        [Fact]
        public void IgnoresMarkedProperties()
        {
            var context = new ProxyGeneratorContext(new[] { typeof(ClassWithIgnoredProperty) }, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export interface ClassWithIgnoredProperty {
        
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitOpenGenericTypes()
        {
            var context = new ProxyGeneratorContext(new[] {typeof (GenericClass<>)}, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export interface GenericClass<T> {
        
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitOpenGenericTypesWithMultipleParams()
        {
            var context = new ProxyGeneratorContext(new[] {typeof (GenericClass<,>)}, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export interface GenericClass<T, U> {
        
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void IgnoredPropertiesArea()
        {
            
        }

        [Fact]
        public void EmitGenericTypesClosedWithTypeArgument()
        {
            var context = new ProxyGeneratorContext(new[] {typeof (MotherOfGenericClass<>)}, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export interface MotherOfGenericClass<T> {
        type: Cirqus.TypeScript.Tests.GenericClass<T>;
    }
}

export module Cirqus.TypeScript.Tests {
    export interface GenericClass<T> {
        
    }
}
", context.GetDefinitions(CirqusType.Other));
        }


        [Fact]
        public void EmitEnumerableGenericTypesClosedWithTypeArgument()
        {
            var context = new ProxyGeneratorContext(new[] { typeof(MotherOfEnumerableOf<>) }, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export interface MotherOfEnumerableOf<T> {
        type: T[];
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitClosedGenericAsFreeType()
        {
            var context = new ProxyGeneratorContext(new[] {typeof (GenericClass<int>)}, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export interface GenericClass<T> {
        
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitClosedGenericAsUsedType()
        {
            var context = new ProxyGeneratorContext(new[] {typeof (FatherOfGenericClass)}, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export interface FatherOfGenericClass {
        type: Cirqus.TypeScript.Tests.GenericClass<number>;
    }
}

export module Cirqus.TypeScript.Tests {
    export interface GenericClass<T> {
        
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void CanAliasNamespaces()
        {
            var context = new ProxyGeneratorContext(new[] { typeof(SomeClass) }, new Configuration()
            {
                NamespaceAliases =
                {
                    Tuple.Create("Cirqus.TypeScript", "HAT")
                }
            });

            Assert.Contains(
@"export module HAT.Tests {
    export interface SomeClass {
        
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitOpenGenericTypesWithTypeArgumentProperty()
        {
            var context = new ProxyGeneratorContext(new[] {typeof (GenericClassWithTypeArgumentProperty<>)},
                new Configuration());

            Assert.Contains(
                @"
export module Cirqus.TypeScript.Tests {
    export interface GenericClassWithTypeArgumentProperty<T> {
        item: T;
    }
}
", context.GetDefinitions(CirqusType.Other));

            Assert.DoesNotContain(@"export class T", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitsEnumWithExplicitValues()
        {
            var context = new ProxyGeneratorContext(new[] { typeof(EnumWithExplicitValues) }, new Configuration());

            Assert.Contains(
@"
export module Cirqus.TypeScript.Tests {
    export enum EnumWithExplicitValues {
        None = 2,
        Value1 = 4,
        Value2 = 6
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitsEnumWithImplicitValues()
        {
            var context = new ProxyGeneratorContext(new[] { typeof(Enum) }, new Configuration());

            Assert.Contains(
@"
export module Cirqus.TypeScript.Tests {
    export enum Enum {
        None = 0,
        Value1 = 1,
        Value2 = 2
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitsEnumProperty()
        {
            var context = new ProxyGeneratorContext(new[] { typeof(ClassWithEnum) }, new Configuration());

            Assert.Contains(
@"
export module Cirqus.TypeScript.Tests {
    export interface ClassWithEnum {
        enum: Cirqus.TypeScript.Tests.Enum;
    }
}

export module Cirqus.TypeScript.Tests {
    export enum Enum {
        None = 0,
        Value1 = 1,
        Value2 = 2
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitsEnumFromNullableProperty()
        {
            var context = new ProxyGeneratorContext(new[] { typeof(ClassWithEnum) }, new Configuration());

            Assert.Contains(
@"
export module Cirqus.TypeScript.Tests {
    export interface ClassWithEnum {
        enum: Cirqus.TypeScript.Tests.Enum;
    }
}

export module Cirqus.TypeScript.Tests {
    export enum Enum {
        None = 0,
        Value1 = 1,
        Value2 = 2
    }
}
", context.GetDefinitions(CirqusType.Other));
        }
    }

    public class ClassWithEnum
    {
        public Enum Enum { get; set; }
    }

    public class ClassWithNullableEnum
    {
        public Enum? Enum { get; set; }
    }

    public enum Enum
    {
        None,
        Value1,
        Value2
    }

    public enum EnumWithExplicitValues
    {
        None = 2,
        Value1 = 4,
        Value2 = 6
    }

    public class CommandClass : Command
    {
        public int Prop { get; set; }
    }

    public class GenericClass<T> { }
    public class GenericClass<T, U> { }

    public class MotherOfGenericClass<T>
    {
        public GenericClass<T> Type { get; set; }
    }

    public class GenericClassWithTypeArgumentProperty<T>
    {
        public T Item { get; set; }
    }

    public class MotherOfEnumerableOf<T>
    {
        public IEnumerable<T> Type { get; set; }
    }

    public class FatherOfGenericClass
    {
        public GenericClass<int> Type { get; set; }
    }

    public class SomeClass
    {

    }

    public class ClassWithIgnoredProperty
    {
        [TypeScriptIgnore]
        public int NobodyLikesMe { get; set; }
    }
}
