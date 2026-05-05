#if !NET5_0_OR_GREATER

using System.ComponentModel;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Polyfill of <see cref="RequiresUnreferencedCodeAttribute"/> for target
/// frameworks where the type is internal.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
internal sealed class RequiresUnreferencedCodeAttribute : Attribute
{
    public RequiresUnreferencedCodeAttribute(string message)
    {
        Message = message;
    }

    public string Message { get; }

    public string? Url { get; set; }
}

#endif
