using System;
using System.Collections.Generic;
using System.Linq;

namespace Cirqus.TypeScript.Model
{
    class TypeDef
    {
        readonly TypeDef _baseType;
        readonly List<PropertyDef> _properties = new List<PropertyDef>();

        public TypeDef(QualifiedClassName name, TypeType typeType)
            : this(name, typeType, null, null)
        {
            Name = name;
            TypeType = typeType;
        }

        public TypeDef(QualifiedClassName name, TypeType typeType, TypeDef baseType, Type type)
        {
            _baseType = baseType;
            Name = name;
            TypeType = typeType;
            Type = type;
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
            return string.Format(@"export module {0} {{
    export class {1}{2} {{
{3}
    }}
}}", Name.Ns, Name.Name, GetExtensionText(), string.Join(Environment.NewLine, FormatProperties()));
        }

        string GetExtensionText()
        {
            if (_baseType == null) return "";

            return string.Format(" implements {0}", _baseType.FullyQualifiedTsTypeName);
        }

        IEnumerable<string> FormatProperties()
        {
            const string indentation = "        ";

            if (TypeType == TypeType.Command)
            {
                yield return string.Format("{0}$commandType = \"{1}\";", indentation, AssemblyQualifiedName);
                yield return string.Format("{0}$commandName = \"{1}\";", indentation, Name.Name);
            }

            foreach (var p in Properties)
            {
                yield return string.Format("{0}{1}{2}: {3};",
                    indentation,
                    p.Name,
                    p.Type.Optional ? "?" : "",
                    p.Type.FullyQualifiedTsTypeName);
            }
        }
    }
}