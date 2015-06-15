using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Cirqus.TypeScript.Model
{
    class TypeDef
    {
        protected const string indent = "    ";

        protected readonly List<PropertyDef> _properties = new List<PropertyDef>();
        protected readonly TypeDef _baseType;

        public TypeDef(QualifiedClassName name, TypeType typeType, Type type)
            : this(name, typeType, null, type)
        {
        }

        public TypeDef(QualifiedClassName name, TypeType typeType, TypeDef baseType, Type type)
        {
            _baseType = baseType;
            Name = name;
            TypeType = typeType;
            Type = type;
        }

        public TypeDef(TypeDef source) : this(source.Name, source.TypeType, source._baseType, source.Type)
        {
        }

        public QualifiedClassName Name { get; private set; }

        public IEnumerable<PropertyDef> Properties
        {
            get { return _properties; }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1} properties)", Name, _properties.Count);
        }

        public void AddProperty(PropertyDef propertyDef)
        {
            _properties.Add(propertyDef);
        }

        public virtual string FullyQualifiedTsTypeName
        {
            get { return string.Format("{0}.{1}", Name.Ns, Name.Name); }
        }

        public TypeType TypeType
        {
            get;
            private set;
        }

        public bool Optional { get; set; }
        public Type Type { get; protected set; }

        public string AssemblyQualifiedName
        {
            get
            {
                if (Type == null)
                {
                    return null;
                }
                
                if (Type.AssemblyQualifiedName == null)
                {
                    return null;
                }

                return string.Join(",", Type.AssemblyQualifiedName.Split(',').Take(2));
            }
        }

        public virtual string GetCode(ProxyGeneratorContext context)
        {
            const string left = indent + indent;

            return string.Format(
@"export module {0} {{
    export class {1}{2} {{
{3}
    }}
}}", 
   Name.Ns, Name.Name, GetExtensionText(), 
   left + string.Join(Environment.NewLine + left, GetTypedProperties().Select(EndOfStatement)));
        }

        string GetExtensionText()
        {
            if (_baseType == null) return "";

            return string.Format(" implements {0}", _baseType.FullyQualifiedTsTypeName);
        }

        protected virtual IEnumerable<string> GetTypedProperties()
        {
            return Properties.Select(p =>
                string.Format("{0}{1}: {2}",
                    ToCamelCase(p.Name),
                    p.Type.Optional ? "?" : "",
                    p.Type.FullyQualifiedTsTypeName));
        }

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (!char.IsUpper(s[0]))
                return s;

            var chars = s.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                var hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                    break;

                chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
            }

            return new string(chars);
        }

        protected static string EndOfStatement(string x)
        {
            return string.Format("{0};", x);
        }
    }
}