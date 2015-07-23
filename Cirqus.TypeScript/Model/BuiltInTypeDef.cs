using System;

namespace Cirqus.TypeScript.Model
{
    class BuiltInTypeDef : TypeDef
    {
        readonly string _code;
        readonly string _fullyQualifiedTsTypeName;

        public BuiltInTypeDef(BuiltInTypeDef source)
            : this(source.Type, source._code, source._fullyQualifiedTsTypeName)
        {
        }

        public BuiltInTypeDef(Type type, string code, string fullyQualifiedTsTypeName) 
            : base(new QualifiedClassName(type), CirqusType.Primitive, null, type)
        {
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