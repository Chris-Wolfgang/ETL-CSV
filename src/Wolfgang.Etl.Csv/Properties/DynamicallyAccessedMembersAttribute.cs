#if !NET5_0_OR_GREATER

using System.ComponentModel;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Polyfill of <see cref="DynamicallyAccessedMembersAttribute"/> for target
/// frameworks where the type is internal (net462, netstandard2.0, netstandard2.1).
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
[AttributeUsage
(
    AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Field
        | AttributeTargets.Interface
        | AttributeTargets.Parameter
        | AttributeTargets.GenericParameter
        | AttributeTargets.ReturnValue,
    Inherited = false
)]
internal sealed class DynamicallyAccessedMembersAttribute : Attribute
{
    public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
    {
        MemberTypes = memberTypes;
    }

    public DynamicallyAccessedMemberTypes MemberTypes { get; }
}

#endif
