using System;
using System.Collections.Generic;
using Cirqus.TypeScript.Config;
using Cirqus.TypeScript.Model;
using Xunit;

namespace Cirqus.TypeScript.Tests
{
    public class ProxyGeneratorContextTests
    {
        [Fact]
        public void EmitOpenGenericTypes()
        {
            var context = new ProxyGeneratorContext(new[] {typeof (GenericClass<>)}, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export class GenericClass<T> {
        
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
    export class GenericClass<T, U> {
        
    }
}
", context.GetDefinitions(CirqusType.Other));
        }

        [Fact]
        public void EmitGenericTypesClosedWithTypeArgument()
        {
            var context = new ProxyGeneratorContext(new[] {typeof (MotherOfGenericClass<>)}, new Configuration());

            Assert.Contains(
@"export module Cirqus.TypeScript.Tests {
    export class MotherOfGenericClass<T> {
        type: Cirqus.TypeScript.Tests.GenericClass<T>;
    }
}

export module Cirqus.TypeScript.Tests {
    export class GenericClass<T> {
        
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
    export class MotherOfEnumerableOf<T> {
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
    export class GenericClass<T> {
        
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
    export class FatherOfGenericClass {
        type: Cirqus.TypeScript.Tests.GenericClass<number>;
    }
}

export module Cirqus.TypeScript.Tests {
    export class GenericClass<T> {
        
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
    export class SomeClass {
        
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
    export class GenericClassWithTypeArgumentProperty<T> {
        item: T;
    }
}
", context.GetDefinitions(CirqusType.Other));

            Assert.DoesNotContain(@"export class T", context.GetDefinitions(CirqusType.Other));
        }


        [Fact]
        public void EmitsEnum()
        {
            var context = new ProxyGeneratorContext(new[] { typeof(Enum) }, new Configuration());

            Assert.Contains(
@"
export module Cirqus.TypeScript.Tests {
    export enum Enum {
        None,
        Value1,
        Value2,
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
    export class ClassWithEnum {
        enum: Cirqus.TypeScript.Tests.Enum;
    }
}

export module Cirqus.TypeScript.Tests {
    export enum Enum {
        None,
        Value1,
        Value2,
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
    export class ClassWithEnum {
        enum: Cirqus.TypeScript.Tests.Enum;
    }
}

export module Cirqus.TypeScript.Tests {
    export enum Enum {
        None,
        Value1,
        Value2,
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
}
