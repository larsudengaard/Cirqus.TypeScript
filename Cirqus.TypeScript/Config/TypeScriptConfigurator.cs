using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cirqus.TypeScript.Config
{
    public abstract class TypeScriptConfigurator
    {
        private readonly Configuration _configuration;

        protected TypeScriptConfigurator()
        {
            _configuration = new Configuration();
        }

        public Configuration Configure()
        {
            return _configuration;
        }

        public void IgnoreProperty<T>(Expression<Func<T, object>> propertyExpression)
        {
            var propertyInfo = GetPropertyInfo(propertyExpression);
            _configuration.IgnoredProperties.Add(new Configuration.IgnoredPropertyConfiguration(typeof (T), propertyInfo.Name));
        }

        public void UseBuiltInType(Func<Type, bool> predicate, string tsType)
        {
            _configuration.BuiltInTypeUsages.Add(new Configuration.BuiltInTypeUsageConfiguration(predicate, tsType));
        }

        public void AliasNamespace(string @namespace, string alias)
        {
            _configuration.NamespaceAliases.Add(Tuple.Create(@namespace, alias));
        }

        static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyExpression)
        {
            Type type = typeof (TSource);

            var member = propertyExpression.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException($"Expression '{propertyExpression}' refers to a method, not a property.");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{propertyExpression}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException($"Expression '{propertyExpression}' refers to a property that is not from type {type}.");

            return propInfo;
        }
    }
}