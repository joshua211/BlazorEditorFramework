using System.Text;

namespace BlazorEditorFramework.Core.Editor.Render;

public class Row
{
    public Row(string content, int from, int to, List<Col> cols)
    {
        Content = content;
        From = from;
        To = to;
        Cols = cols;
    }

    public string Content { get; private set; }
    public int From { get; private set; }
    public int To { get; private set; }

    public IReadOnlyCollection<Col> Cols { get; private set; }


    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"ROW {From}-{To}: ");
        foreach (var col in Cols)
        {
            sb.Append($"{col.From}-{col.To} {col.Tag} | ");
        }

        return sb.ToString();
    }
}