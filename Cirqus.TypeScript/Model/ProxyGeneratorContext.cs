using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Cirqus.TypeScript.Config;
using d60.Cirqus.Commands;
using d60.Cirqus.Numbers;
using d60.Cirqus.Views.ViewManagers;

namespace Cirqus.TypeScript.Model
{
    internal class ProxyGeneratorContext
    {
        readonly Configuration _configuration;
        readonly Dictionary<Type, TypeDef> _types = new Dictionary<Type, TypeDef>();
        bool _generateDictionaryDefinition;

        public ProxyGeneratorContext(IEnumerable<Type> types, Configuration configuration)
        {
            _configuration = configuration;

            AddBuiltInType(typeof(bool), "", "boolean");

            AddBuiltInType(typeof(short), "", "number");
            AddBuiltInType(typeof(int), "", "number");
            AddBuiltInType(typeof(long), "", "number");

            AddBuiltInType(typeof(float), "", "number");
            AddBuiltInType(typeof(double), "", "number");
            AddBuiltInType(typeof(decimal), "", "number");

            AddBuiltInType(typeof(string), "", "string");
            AddBuiltInType(typeof(DateTime), "", "Date");

            AddBuiltInType(typeof(object), "", "any");

            AddBuiltInType(typeof(Command), @"export interface Command {
    Meta?: any;
}", "Command");
            AddBuiltInType(typeof(Metadata), "", "any");
            AddBuiltInType(typeof(Guid), "export interface Guid {}", "Guid");

            foreach (var type in types)
            {
                GetOrCreateTypeDef(type);
            }
        }

        BuiltInTypeDef AddBuiltInType(Type type, string code, string fullyQualifiedTsTypeName)
        {
            var name = GetQualifiedClassName(type);
            var typeDef = new BuiltInTypeDef(name, type, code, fullyQualifiedTsTypeName);
            _types.Add(type, typeDef);

            return typeDef;
        }

        TypeDef GetOrCreateTypeDef(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            return GetExistingTypeDefOrNull(type)
                   ?? CreateSpecialTypeDefOrNull(type)
                   ?? CreateTypeDef(type);
        }

        public static Type GetClosedGenericInterfaceFromImplementation(Type implementation, Type openGenericInterface)
        {
            if (implementation.IsGenericType && implementation.GetGenericTypeDefinition() == openGenericInterface)
                return implementation;

            if (!openGenericInterface.IsInterface)
                throw new NotSupportedException(string.Format("Method only supports interfaces, the supplied type was not an interface: {0}", openGenericInterface.FullName));

            return implementation.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == openGenericInterface);
        }

        TypeDef CreateSpecialTypeDefOrNull(Type type)
        {
            Console.WriteLine("Generating type {0}", type);

            TypeDef typeDef = null;
            
            if (typeof (IDictionary).IsAssignableFrom(type))
            {
                var dictionaryType = GetClosedGenericInterfaceFromImplementation(type, typeof (IDictionary<,>));
                if (dictionaryType == null)
                    throw new NotSupportedException("Only dictionaries that implement IDictionary<TKey, TValue> is supported");

                var keyType = dictionaryType.GetGenericArguments()[0];
                var valueType = dictionaryType.GetGenericArguments()[1];
                var keyDef = GetOrCreateTypeDef(keyType);
                var valueDef = GetOrCreateTypeDef(valueType);

                if (keyDef.FullyQualifiedTsTypeName != "string")
                {
                    throw new NotSupportedException(
                        string.Format("Only dictionaries with string key is supported. Key: {0}, TypeScript key: {1}", 
                        keyType, 
                        keyDef.FullyQualifiedTsTypeName));
                }

                _generateDictionaryDefinition = true;

                typeDef = new BuiltInTypeDef(new QualifiedClassName("", "Dictionary"), type, "", string.Format("Dictionary<{0}>", valueDef.FullyQualifiedTsTypeName));
                _types.Add(type, typeDef);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsArray && type.GetArrayRank() == 1)
                {
                    var elementType = type.GetElementType();

                    var typeDefOfContainedType = GetOrCreateTypeDef(elementType);

                    typeDef = AddBuiltInType(type, "", string.Format("{0}[]", typeDefOfContainedType.FullyQualifiedTsTypeName));
                }
                else if (!type.IsGenericType)
                {
                    typeDef = AddBuiltInType(type, "", "any[]");
                }
                else if (type.IsGenericType && type.GetGenericArguments().Length == 1)
                {
                    var elementType = type.GetGenericArguments()[0];

                    if (!elementType.IsGenericParameter)
                    {
                        var typeDefOfContainedType = GetOrCreateTypeDef(elementType);
                        typeDef = AddBuiltInType(type, "", string.Format("{0}[]", typeDefOfContainedType.FullyQualifiedTsTypeName));
                    }
                    else
                    {
                        typeDef = AddBuiltInType(type, "", string.Format("{0}[]", elementType.Name));
                    }

                }
            }
            else if (type.IsEnum)
            {
                var enumTypeDef = new EnumDef(GetQualifiedClassName(type), CirqusType.Other, type);
                _types[type] = enumTypeDef;
                return enumTypeDef;
            }

            return typeDef;
        }

        TypeDef GetExistingTypeDefOrNull(Type type)
        {
            return _types.ContainsKey(type) ? _types[type] : null;
        }

        QualifiedClassName GetQualifiedClassName(Type type)
        {
            var ns = type.Namespace +
                     (type.IsNested
                         ? "." + type.DeclaringType.Name
                         : "");

            foreach (var alias in _configuration.NamespaceAliases)
            {
                if (ns.StartsWith(alias.Item1))
                {
                    ns = alias.Item2 + ns.Remove(0, alias.Item1.Length);
                }
            }

            string name;
            if (type.IsGenericTypeDefinition)
            {
                name = $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(x => x.Name))}>";
            }
            else if (type.IsGenericType)
            {
                name = string.Format("{0}<{1}>",
                    type.Name.Split('`')[0],
                    string.Join(", ", type.GetGenericArguments().Select(x =>
                    {
                        // if it's the "T" from a open parent generic class
                        if (x.IsGenericParameter)
                            return x.Name;

                        var arg = GetOrCreateTypeDef(x);

                        return arg.FullyQualifiedTsTypeName;
                    })));

                // make the open generic type too
                GetOrCreateTypeDef(type.GetGenericTypeDefinition());
            }
            else if (type.IsGenericParameter)
            {
                name = type.Name;
                ns = null;
            }
            else
            {
                name = type.Name;
            }

            return new QualifiedClassName(ns, name);
        }

        TypeDef CreateTypeDef(Type type)
        {
            Console.WriteLine("Generating type {0}", type);

            var qualifiedClassName = GetQualifiedClassName(type);

            var builtInTypeUsageConfigurations = _configuration
                .BuiltInTypeUsages
                .Where(x => x.IsForType(type))
                .ToList();

            if (builtInTypeUsageConfigurations.Any())
            {
                if (builtInTypeUsageConfigurations.Count > 1)
                    throw new PrettyException(string.Format("Found multiple built-in-type configurations for type {0}", type));

                var builtInTypeUsageConfiguration = _configuration.BuiltInTypeUsages.Single(x => x.IsForType(type));

                var existingTypes = _types.Values
                    .OfType<BuiltInTypeDef>()
                    .Where(x => x.FullyQualifiedTsTypeName == builtInTypeUsageConfiguration.TsType)
                    .ToList();

                if (existingTypes.Any())
                    return existingTypes.First();

                return AddBuiltInType(type, "", builtInTypeUsageConfiguration.TsType);
            }

            var typeDef = IsCommand(type)
                ? new CommandTypeDef(qualifiedClassName, GetTypeFor(typeof (Command)), type)
                : IsView(type)
                    ? new TypeDef(qualifiedClassName, CirqusType.View)
                    : new TypeDef(qualifiedClassName, CirqusType.Other);

            if ((type.IsGenericType && !type.IsGenericTypeDefinition) || type.IsGenericParameter)
            {
                typeDef.NoEmit = true;
            }

            _types[type] = typeDef;

            var ignoredProperties = _configuration
                .IgnoredProperties
                .Where(x => x.IsForType(type))
                .Select(x => x.PropertyName)
                .ToList();

            var properties = GetAllProperties(type).Where(x => !ignoredProperties.Contains(x.Name));
            foreach (var property in properties)
            {
                Console.WriteLine("Generating property {0}", property.Name);

                var propertyDef =
                    new PropertyDef(
                        GetOrCreateTypeDef(property.PropertyType),
                        property.Name);

                if (typeDef.CirqusType == CirqusType.Command && propertyDef.Name == "Meta") continue;

                typeDef.AddProperty(propertyDef);
            }

            return typeDef;
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        public string GetSystemDefinitions()
        {
            var builder = new StringBuilder();

            if (_generateDictionaryDefinition)
            {
                builder.Append(@"export interface Dictionary<T>
{
    [key: string]: T;
}");
            }

            return builder.ToString();
        }

        public string GetDefinitions(params CirqusType[] cirqusType)
        {
            var builder = new StringBuilder();

            var typeGroups = _types.Values
                .Where(x => cirqusType.Contains(x.CirqusType) && !x.NoEmit)
                .GroupBy(t => t.CirqusType)
                .OrderBy(g => g.Key)
                .ToList();

            builder.AppendLine("namespace Api");
            builder.AppendLine("{");
            builder.AppendLine();

            foreach (var typeGroup in typeGroups)
            {
                builder.AppendLine($@"/* {FormatTypeType(typeGroup.Key)} */");

                foreach (var type in typeGroup)
                {
                    var code = type.GetCode(this);
                    if (string.IsNullOrWhiteSpace(code)) continue;

                    builder.AppendLine(code);

                    builder.AppendLine();
                }

                builder.AppendLine();
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        string FormatTypeType(CirqusType cirqusType)
        {
            switch (cirqusType)
            {
                case CirqusType.Command:
                    return "Domain commands";

                case CirqusType.View:
                    return "Domain views";

                case CirqusType.Other:
                    return "Domain primitives";

                case CirqusType.Primitive:
                    return "Built-in primitives";

                default:
                    return cirqusType.ToString();
            }
        }

        public TypeDef GetTypeFor(Type type)
        {
            var typeDef = GetExistingTypeDefOrNull(type);

            if (typeDef == null)
            {
                throw new ArgumentException(String.Format("Could not find type for {0}!", type));
            }

            return typeDef;
        }

        public static bool IsCommand(Type type)
        {
            return !type.IsInterface &&
                   !type.IsAbstract &&
                   typeof (Command).IsAssignableFrom(type);
        }

        public static bool IsView(Type type)
        {
            return !type.IsInterface &&
                   !type.IsAbstract &&
                   typeof(IViewInstance).IsAssignableFrom(type);
        }
    }
}