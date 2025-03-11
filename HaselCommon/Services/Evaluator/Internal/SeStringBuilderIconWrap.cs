using Lumina.Text;

namespace HaselCommon.Services.Evaluator.Internal;

/// <summary>
/// Wraps payloads in an open and close icon, for example the Auto Translation open/close brackets.
/// </summary>
internal readonly struct SeStringBuilderIconWrap : IDisposable
{
    private readonly SeStringBuilder _builder;
    private readonly uint _iconClose;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeStringBuilderIconWrap"/> struct.<br/>
    /// Appends an icon macro with <paramref name="iconOpen"/> on creation, and an icon macro with
    /// <paramref name="iconClose"/> on disposal.
    /// </summary>
    /// <param name="builder">The builder to use.</param>
    /// <param name="iconOpen">The open icon id.</param>
    /// <param name="iconClose">The close icon id.</param>
    public SeStringBuilderIconWrap(SeStringBuilder builder, uint iconOpen, uint iconClose)
    {
        _builder = builder;
        _iconClose = iconClose;
        _builder.AppendIcon(iconOpen);
    }

    /// <inheritdoc/>
    public void Dispose() => _builder.AppendIcon(_iconClose);
}
