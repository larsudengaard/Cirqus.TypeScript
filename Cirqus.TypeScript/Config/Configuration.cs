using System;
using System.Collections;
using System.Collections.Generic;

namespace Cirqus.TypeScript.Config
{
    public class Configuration
    {
        public List<Type> Types { get; private set; }
        public List<IgnoredPropertyConfiguration> IgnoredProperties { get; private set; }
        public List<BuiltInTypeUsageConfiguration> BuiltInTypeUsages { get; private set; }
        public List<Tuple<string, string>>  NamespaceAliases { get; private set; }

        internal Configuration()
        {
            Types = new List<Type>();
            IgnoredProperties = new List<IgnoredPropertyConfiguration>();
            BuiltInTypeUsages = new List<BuiltInTypeUsageConfiguration>();
            NamespaceAliases = new List<Tuple<string, string>>();
        }

        public class BuiltInTypeUsageConfiguration
        {
            readonly Func<Type, bool> _predicate;

            public string TsType { get; private set; }

            public BuiltInTypeUsageConfiguration(Func<Type, bool> predicate, string tsType)
            {
                TsType = tsType;
                _predicate = predicate;
            }

            public bool IsForType(Type type)
            {
                return _predicate(type);
            }
        }

        public class IgnoredPropertyConfiguration
        {
            public Type DeclaringType { get; private set; }
            public string PropertyName { get; private set; }

            public IgnoredPropertyConfiguration(Type declaringType, string propertyName)
            {
                DeclaringType = declaringType;
                PropertyName = propertyName;
            }

            public bool IsForType(Type type)
            {
                return DeclaringType.IsAssignableFrom(type);
            }
        }
    }
}