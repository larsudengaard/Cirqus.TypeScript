using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;

namespace Cirqus.TypeScript.Model
{
    class CommandTypeDef : TypeDef
    {
        public CommandTypeDef(QualifiedClassName name, TypeDef baseType, Type type) : base(name, TypeType.Command, baseType, type) {}

        public override string GetCode(ProxyGeneratorContext context)
        {
            const string left = indent + indent;

            return string.Format(
@"export module {0} {{
    export class {1}{2} {{
{3}
{4}
    }}
}}",
   Name.Ns, Name.Name, GetExtensionText(), GetCtor(), 
   left + string.Join(Environment.NewLine + left, GetTypedProperties().Select(EndOfStatement)));
        }

        string GetCtor()
        {
            const string left = indent + indent;

            return string.Format(@"
{0}constructor(args: {{{1}}}) {{
{2}
{0}}}
", 
 left, string.Join(", ", base.GetTypedProperties()), 
 left + indent + string.Join(Environment.NewLine + left + indent, FormatInit()));
        }

        IEnumerable<string> FormatInit()
        {
            return _properties.Select(property => 
                string.Format("this.{0} = args.{0};", ToCamelCase(property.Name)));
        }

        string GetExtensionText()
        {
            if (_baseType == null) return "";

            return string.Format(" implements {0}", _baseType.FullyQualifiedTsTypeName);
        }

        protected override IEnumerable<string> GetTypedProperties()
        {
            yield return string.Format("$commandType = \"{0}\"", AssemblyQualifiedName);
            yield return string.Format("$commandName = \"{0}\"", Name.Name);

            foreach (var property in base.GetTypedProperties())
            {
                yield return property;
            }
        }
    }
}