using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Helper;

public static class DocumentHelper
{
    /// <summary>
    /// Binary search a list of document lines to find the line containing the given position.
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static DocumentLine? GetLineContainingPosition(this IReadOnlyList<DocumentLine> lines, int position)
    {
        int left = 0;
        int right = lines.Count - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            DocumentLine line = lines[mid];

            if (position >= line.From && position <= line.To)
            {
                return line; // Position is within this line's range
            }
            else if (position < line.From)
            {
                right = mid - 1; // Search the left half
            }
            else
            {
                left = mid + 1; // Search the right half
            }
        }

        return null; // If not found
    }

    /// <summary>
    /// Get all nodes that intersect with the given range.
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static IEnumerable<Node> GetMatchingNodes(
        this IReadOnlyCollection<Node> nodes, int from, int to)
    {
        var range = new Range(from, to);
        return nodes.Where(node => node.Intersects(range));
    }

    public static bool Intersects(this IRange firstRange, IRange secondRange)
    {
        return IntersectsFirst(firstRange, secondRange) || IntersectsFirst(secondRange, firstRange);
    }

    public static bool IntersectsFirst(this IRange firstRange, IRange secondRange)
    {
        return firstRange.From >= secondRange.From && firstRange.From <= secondRange.To ||
               firstRange.To >= secondRange.From && firstRange.To <= secondRange.To;
    }

    /// <summary>
    /// Splits a given document into a list of lines, without keeping the delimiters.
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public static IReadOnlyCollection<DocumentLine> GetLines(this string document)
    {
        var lines = document.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        var documentLines = new List<DocumentLine>();
        var from = 0;
        var to = 0;
        var lineIndex = 0;

        foreach (var line in lines)
        {
            to = from + line.Length;
            to = to < from ? from : to;
            var documentLine = new DocumentLine(from, to, lineIndex, line);
            documentLines.Add(documentLine);
            from = to + 1;
            lineIndex++;
        }

        return documentLines;
    }
}