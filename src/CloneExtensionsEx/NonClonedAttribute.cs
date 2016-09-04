using System;

namespace CloneExtensionsEx
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NonClonedAttribute : Attribute
    {
    }
}
