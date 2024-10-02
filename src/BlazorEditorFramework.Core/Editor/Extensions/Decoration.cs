using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Extensions;

public record Decoration(
    int From,
    int To,
    Type ViewType,
    DecorationOrientation Orientation,
    Dictionary<string, object> Parameter) : IRange;

public record NoDisplayDecoration(int From, int To)
    : Decoration(From, To, null!, DecorationOrientation.Overlap, new Dictionary<string, object>());

public enum DecorationOrientation
{
    /// <summary>
    /// Replaces the existing node
    /// </summary>
    Overlap,

    /// <summary>
    /// Displays the decoration before the original node
    /// </summary>
    Before,

    /// <summary>
    /// Displays the decoration after the original node
    /// </summary>
    After
}