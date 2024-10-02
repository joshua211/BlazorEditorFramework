using BlazorEditorFramework.Core.Editor.Extensions;

namespace BlazorEditorFramework.Core.Editor.Render;

public record DecorationCol(
    Type ViewType,
    int From,
    int To,
    string Tag,
    string ClassString,
    DecorationOrientation Orientation,
    Dictionary<string, object> Params)
    : Col(From, To, Tag);