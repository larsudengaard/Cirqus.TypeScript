using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using d60.Cirqus.Commands;
using d60.Cirqus.Numbers;
using d60.Cirqus.Views.ViewManagers;

namespace Cirqus.TypeScript.Model
{
    class ProxyGeneratorContext
    {
        readonly Configuration.Configuration _configuration;
        readonly Dictionary<Type, TypeDef> _types = new Dictionary<Type, TypeDef>();
        bool _generateDictionaryDefinition;

        public ProxyGeneratorContext(IEnumerable<Type> types, Configuration.Configuration configuration)
        {
            _configuration = configuration;

            AddBuiltInType(new BuiltInTypeDef(typeof(bool), "", "boolean"));

            AddBuiltInType(new BuiltInTypeDef(typeof(short), "", "number"));
            AddBuiltInType(new BuiltInTypeDef(typeof(int), "", "number"));
            AddBuiltInType(new BuiltInTypeDef(typeof(long), "", "number"));

            AddBuiltInType(new BuiltInTypeDef(typeof(float), "", "number"));
            AddBuiltInType(new BuiltInTypeDef(typeof(double), "", "number"));
            AddBuiltInType(new BuiltInTypeDef(typeof(decimal), "", "number"));

            AddBuiltInType(new BuiltInTypeDef(typeof(string), "", "string"));
            AddBuiltInType(new BuiltInTypeDef(typeof(DateTime), "", "Date"));

            AddBuiltInType(new BuiltInTypeDef(typeof(object), "", "any"));

            AddBuiltInType(new BuiltInTypeDef(typeof(Command), @"export interface Command {
    Meta?: any;
}", "Command"));
            AddBuiltInType(new BuiltInTypeDef(typeof(Metadata), "", "any") { Optional = true });
            AddBuiltInType(new BuiltInTypeDef(typeof(Guid), "export interface Guid {}", "Guid"));

            foreach (var type in types)
            {
                AddTypeDefFor(type);
            }
        }

        void AddBuiltInType(BuiltInTypeDef builtInTypeDef)
        {
            _types.Add(builtInTypeDef.Type, builtInTypeDef);
        }

        public void AddTypeDefFor(Type type)
        {
            var qualifiedClassName = new QualifiedClassName(type);

            GetOrCreateTypeDef(qualifiedClassName, type);
        }

        TypeDef GetOrCreateTypeDef(QualifiedClassName qualifiedClassName, Type type)
        {
            return GetExistingTypeDefOrNull(type)
                   ?? CreateSpecialTypeDefOrNull(type)
                   ?? CreateTypeDef(qualifiedClassName, type);
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
            BuiltInTypeDef typeDef = null;

            if (typeof (IDictionary).IsAssignableFrom(type))
            {
                var dictionaryType = GetClosedGenericInterfaceFromImplementation(type, typeof (IDictionary<,>));
                if (dictionaryType == null)
                    throw new NotSupportedException("Only dictionaries that implement IDictionary<TKey, TValue> is supported");

                var keyType = dictionaryType.GetGenericArguments()[0];
                var valueType = dictionaryType.GetGenericArguments()[1];
                var keyDef = GetOrCreateTypeDef(new QualifiedClassName(keyType), keyType);
                var valueDef = GetOrCreateTypeDef(new QualifiedClassName(valueType), valueType);

                if (keyDef.FullyQualifiedTsTypeName != "string")
                {
                    throw new NotSupportedException(
                        string.Format("Only dictionaries with string key is supported. Key: {0}, TypeScript key: {1}", 
                        keyType, 
                        keyDef.FullyQualifiedTsTypeName));
                }

                _generateDictionaryDefinition = true;
                typeDef = new BuiltInTypeDef(type, "", string.Format("Dictionary<{0}>", valueDef.FullyQualifiedTsTypeName));
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsArray && type.GetArrayRank() == 1)
                {
                    var elementType = type.GetElementType();

                    var typeDefOfContainedType = GetOrCreateTypeDef(new QualifiedClassName(elementType), elementType);

                    typeDef = new BuiltInTypeDef(type, "", string.Format("{0}[]", typeDefOfContainedType.FullyQualifiedTsTypeName));
                }
                else if (!type.IsGenericType)
                {
                    typeDef = new BuiltInTypeDef(type, "", "any[]");
                }
                else if (type.IsGenericType && type.GetGenericArguments().Length == 1)
                {
                    var elementType = type.GetGenericArguments()[0];

                    var typeDefOfContainedType = GetOrCreateTypeDef(new QualifiedClassName(elementType), elementType);

                    typeDef = new BuiltInTypeDef(type, "", string.Format("{0}[]", typeDefOfContainedType.FullyQualifiedTsTypeName));
                }
            }

            if (typeDef != null)
            {
                _types.Add(type, typeDef);
            }

            return typeDef;
        }

        TypeDef GetExistingTypeDefOrNull(Type type)
        {
            return _types.ContainsKey(type) ? _types[type] : null;
        }

        TypeDef CreateTypeDef(QualifiedClassName qualifiedClassName, Type type)
        {
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

                var builtInTypeDef = new BuiltInTypeDef(type, "", builtInTypeUsageConfiguration.TsType);
                AddBuiltInType(builtInTypeDef);
                return builtInTypeDef;
            }

            var typeDef = IsCommand(type)
                ? new TypeDef(qualifiedClassName, TypeType.Command, GetTypeFor(typeof (Command)), type)
                : IsView(type)
                    ? new TypeDef(qualifiedClassName, TypeType.View)
                    : new TypeDef(qualifiedClassName, TypeType.Other);

            _types[type] = typeDef;

            var ignoredProperties = _configuration
                .IgnoredProperties
                .Where(x => x.IsForType(type))
                .Select(x => x.PropertyName)
                .ToList();

            var properties = GetAllProperties(type).Where(x => !ignoredProperties.Contains(x.Name));
            foreach (var property in properties)
            {
                var propertyDef =
                    new PropertyDef(
                        GetOrCreateTypeDef(new QualifiedClassName(property.PropertyType), property.PropertyType),
                        property.Name);

                if (typeDef.TypeType == TypeType.Command && propertyDef.Name == "Meta") continue;

                typeDef.AddProperty(propertyDef);
            }

            return typeDef;
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        public string GetCommandProcessorDefinitation()
        {
            var builder = new StringBuilder();

            builder.AppendLine(@"export class CommandProcessor {
    private processCommandCallback: (command: Command) => void;

    constructor(processCommandCallback: (command: Command) => void) {
        this.processCommandCallback = processCommandCallback;
    }
");

            foreach (
                var commandType in
                    _types.Values.Where(t => t.TypeType == TypeType.Command).OrderBy(t => t.FullyQualifiedTsTypeName))
            {
                builder.AppendLine(string.Format(@"    public {0}(command: {1}) : void {{
        (<any>command).$type = ""{2}"";
        (<any>command).$name = ""{3}"";
        this.invokeCallback(command);
    }}", ToCamelCase(commandType), commandType.FullyQualifiedTsTypeName, commandType.AssemblyQualifiedName, commandType.Name.Name));

                builder.AppendLine();
            }


            builder.AppendLine(@"    private invokeCallback(command: Command) : void {
        try {
            this.processCommandCallback(command);
        } catch(error) {
            console.log(""Command processing error"", error);
        }
    }
}");

            if (_generateDictionaryDefinition)
            {
                builder.Append(@"export interface Dictionary<T>
{
    [key: string]: T;
}");
            }

            return builder.ToString();
        }

        static string ToCamelCase(TypeDef commandType)
        {
            var name = commandType.Name.Name;

            return char.ToLower(name[0]) + name.Substring(1);
        }

        public string GetDefinitions(params TypeType[] typeTypes)
        {
            var builder = new StringBuilder();

            var typeGroups = _types.Values
                .Where(x => typeTypes.Contains(x.TypeType))
                .GroupBy(t => t.TypeType)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var typeGroup in typeGroups)
            {
                builder.AppendLine(string.Format(@"/* {0} */", FormatTypeType(typeGroup.Key)));

                foreach (var type in typeGroup)
                {
                    var code = type.GetCode(this);
                    if (string.IsNullOrWhiteSpace(code)) continue;

                    builder.AppendLine(code);
                    builder.AppendLine();
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        string FormatTypeType(TypeType typeType)
        {
            switch (typeType)
            {
                case TypeType.Command:
                    return "Domain commands";

                case TypeType.View:
                    return "Domain views";

                case TypeType.Other:
                    return "Domain primitives";

                case TypeType.Primitive:
                    return "Built-in primitives";

                default:
                    return typeType.ToString();
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