using System;

namespace Cirqus.TypeScript.Model
{
    class BuiltInTypeDef : TypeDef
    {
        readonly string _code;
        readonly string _fullyQualifiedTsTypeName;

        public BuiltInTypeDef(Type type, string code, string fullyQualifiedTsTypeName) 
            : base(new QualifiedClassName(type), TypeType.Primitive)
        {
            Type = type;
            _code = code;
            _fullyQualifiedTsTypeName = fullyQualifiedTsTypeName;
        }

        public override string FullyQualifiedTsTypeName
        {
            get { return _fullyQualifiedTsTypeName; }
        }

        public override string GetCode(ProxyGeneratorContext context)
        {
            return _code;
        }
    }
}