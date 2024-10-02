using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Helper;

public class Range : IRange
{
    public Range(int from, int to)
    {
        From = from;
        To = to;
    }

    public int From { get; }
    public int To { get; }
}