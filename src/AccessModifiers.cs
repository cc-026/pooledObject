using System;

namespace cc_026.Pool
{
    public enum EAccessModifier
    {
        Public,
        Protected,
        Internal,
        ProtectedInternal,
        Private,
        PrivateProtected,
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class AccessModifiersAttribute: Attribute
    {
        public AccessModifiersAttribute(EAccessModifier modifier)
        {
            _accessModifier = modifier;
        }

        private EAccessModifier _accessModifier;
    }
}