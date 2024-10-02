using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Render;

public abstract record Col(int From, int To, string Tag) : IRange
{
    public int Length => To - From;
}