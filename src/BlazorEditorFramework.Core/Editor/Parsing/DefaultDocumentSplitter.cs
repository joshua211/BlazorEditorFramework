using System.Text;
using BlazorEditorFramework.Core.Editor.Parsing.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Parsing;

/// <summary>
/// Splits a documents into parts. Each empty line is considered a new part.
/// </summary>
public class DefaultDocumentSplitter : IDocumentSplitter
{
    public List<DocumentPart> SplitDocument(string document)
    {
        var lines = document.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var parts = new List<DocumentPart>();
        var linesOfPart = new List<DocumentLine>();
        var partContent = new StringBuilder();

        var from = 0;
        var to = 0;
        var lineIndex = 0;
        var partIndex = 0;

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            to = from + line.Length;
            to = to < from ? from : to;
            var documentLine = new DocumentLine(from, to, lineIndex, line);
            partContent.Append(line);
            partContent.Append('\n');
            linesOfPart.Add(documentLine);
            from = to + 1;
            lineIndex++;

            if (string.IsNullOrWhiteSpace(line))
            {
                parts.Add(new DocumentPart(partContent.ToString(), linesOfPart, partIndex));
                partIndex++;
                partContent.Clear();
                linesOfPart.Clear();
            }
        }

        if (linesOfPart.Count > 0)
        {
            parts.Add(new DocumentPart(partContent.ToString(), linesOfPart, partIndex));
        }

        return parts;
    }
}