using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor;

public record Selection(int From, int To) : IRange
{
    public bool IsCollapsed => From == To;

    public int Length
    {
        get
        {
            var norm = Normalize();
            return norm.To - norm.From;
        }
    }

    public Selection Normalize() => From > To ? new Selection(To, From) : this;
}