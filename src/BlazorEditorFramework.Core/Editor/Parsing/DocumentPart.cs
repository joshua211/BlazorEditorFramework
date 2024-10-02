using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Parsing;

public class DocumentPart : IRange
{
    public DocumentPart(string content, IReadOnlyList<DocumentLine> lines, int index)
    {
        From = lines.First().From;
        To = lines.Last().To;
        Content = content;
        Lines = lines.ToList();
        Index = index;
    }

    public string Content { get; private set; }
    public IReadOnlyList<DocumentLine> Lines { get; private set; }
    public int Index { get; private set; }

    public int From { get; private set; }
    public int To { get; private set; }
}