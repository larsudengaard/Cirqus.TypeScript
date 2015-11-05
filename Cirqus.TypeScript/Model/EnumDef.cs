using System;
using System.Collections.Generic;
using System.Linq;

namespace Cirqus.TypeScript.Model
{
    class EnumDef : TypeDef
    {
        public EnumDef(QualifiedClassName name, CirqusType cirqusType, Type type) : base(name, cirqusType, null, type)
        {
        }


        public override string GetCode(ProxyGeneratorContext context)
        {
            const string left = indent + indent;
            var enums = Type.GetEnumValues().Cast<object>().Select(x => x + " = " + ((int)x).ToString());

            return string.Format(
@"export module {0} {{
    export enum {1} {{
{2}
    }}
}}",
   Name.Ns, Name.Name,
   left + string.Join("," + Environment.NewLine + left, enums));
        }
    }
}