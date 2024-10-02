using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor;

public record DocumentLine(int From, int To, int Index, string Content) : IRange
{
    public bool Contains(Selection selection)
    {
        // TODO this is wrong
        return selection.From >= From && selection.To <= To;
    }
}