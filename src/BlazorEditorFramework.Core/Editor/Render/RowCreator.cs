using System.Diagnostics;
using BlazorEditorFramework.Core.Editor.Extensions;
using BlazorEditorFramework.Core.Editor.Helper;
using BlazorEditorFramework.Core.Editor.Render.Abstractions;
using Range = BlazorEditorFramework.Core.Editor.Helper.Range;

namespace BlazorEditorFramework.Core.Editor.Render;

public class RowCreator : IRowCreator
{
    // TODO we dont need to calculate EVERY row, only the ones that have changed and shift the rest
    public IReadOnlyCollection<Row> CalculateRows(IReadOnlyList<DocumentLine> lines, IReadOnlyCollection<Node> nodes,
        EditorState state)
    {
        var watch = Stopwatch.StartNew();
        var result = new List<Row>();

        foreach (var line in lines)
        {
            var content = line.Content;
            var cols = CalculateCols(content, line.From, line.To, nodes, state);

            cols.Sort(CompareCols);


            var row = new Row(content, line.From, line.To, cols);
            result.Add(row);
        }

        watch.Stop();
        Log.Logger.Debug("Calculated rows in {WatchElapsedMilliseconds}ms", watch.ElapsedMilliseconds);

        return result;
    }

    private int CompareCols(Col col1, Col col2)
    {
        // First, compare by the 'From' and 'To' values to check for overlap
        var isOverlap = col1.From < col2.To && col2.From < col1.To;

        // If there's no overlap, order based on 'From' values
        if (!isOverlap)
        {
            var fromComparison = col1.From.CompareTo(col2.From);
            return fromComparison != 0 ? fromComparison : col1.To.CompareTo(col2.To);
        }

        // If overlap exists, apply the custom sorting logic
        if (col1 is DecorationCol decorationCol1 && col2 is TextCol)
        {
            // If DecorationOrientation is Before, decorationCol should come first
            if (decorationCol1.Orientation == DecorationOrientation.Before)
            {
                return -1; // decorationCol comes before TextCol
            }
            else
            {
                return 1; // decorationCol comes after TextCol
            }
        }
        else if (col1 is TextCol && col2 is DecorationCol decorationCol2)
        {
            // If DecorationOrientation is Before, decorationCol should come first
            if (decorationCol2.Orientation == DecorationOrientation.Before)
            {
                return 1; // TextCol comes after decorationCol
            }
            else
            {
                return -1; // TextCol comes before decorationCol
            }
        }

        // If both are of the same type or neither is a DecorationCol, keep them as-is
        return 0;
    }

    private List<Col> CalculateCols(string content, int from, int to, IReadOnlyCollection<Node> rowNodes,
        EditorState state)
    {
        var list = new List<Col>();

        if (content.Length == 0)
        {
            list.Add(new TextCol(string.Empty, from, from, string.Empty, string.Empty));
            return list;
        }

        var nodes = rowNodes.GetMatchingNodes(from, to).ToList();
        var currentRange = new Range(from, to);
        var decorations = nodes.SelectMany(state.GetDecorations)
            .Where(d => currentRange.Intersects(d))
            .ToList();

        int documentCursor = from;
        int index = 0;

        Col? currentCol = null;

        List<Node> activeNodes = new();

        // Combine nodes and decorations into events for iteration
        var events = new List<(int position, bool isNode, bool isStart, object item)>();

        // Add node start and end events
        foreach (var node in nodes)
        {
            events.Add((node.From, true, true, node));
            events.Add((node.To, true, false, node));
        }

        // Add decoration start and end events
        foreach (var deco in decorations)
        {
            events.Add((deco.From, false, true, deco));
            events.Add((deco.To, false, false, deco));
        }

        // Sorting rules:
        // 1. Sort by position
        // 2. Start events before end events
        // 3. Node start before decoration start if position equals
        events.Sort((a, b) =>
        {
            // 1. Sort by position
            int posComp = a.position.CompareTo(b.position);
            if (posComp != 0) return posComp;

            // 2. End events before start events
            if (a.isStart && b.isStart == false) return 1;
            if (a.isStart == false && b.isStart) return -1;

            // 3. Node start before decoration start if position equals
            if (a.isNode && b.isNode == false) return -1;
            if (a.isNode == false && b.isNode) return 1;

            return 0;
        });

        // Process through sorted events
        foreach (var (position, isNode, isStart, item) in events)
        {
            var actualPosition = position > content.Length ? content.Length : position;
            // Handle start of nodes
            if (isStart)
            {
                if (isNode)
                {
                    activeNodes.Add((Node)item);
                }
            }

            if (isStart && isNode == false)
            {
                var deco = (Decoration)item;
                var actualFrom = deco.From < from ? from : deco.From;
                var actualTo = deco.To > to ? to : deco.To;

                if (deco is NoDisplayDecoration)
                {
                    index += deco.To - deco.From;
                    documentCursor += deco.To - deco.From;
                    continue;
                }

                var newCol = new DecorationCol(
                    deco.ViewType,
                    actualFrom,
                    actualTo,
                    string.Join(' ', activeNodes.Select(n => n.NodeType).Distinct().Append(deco.ViewType.Name)),
                    string.Join(' ', activeNodes.Select(state.GetClassString).Distinct()),
                    deco.Orientation,
                    deco.Parameter
                );

                AddOrMergeColumn(newCol, ref currentCol, list);

                // only advance document cursor if the decoration is overlapping
                if (deco.Orientation is DecorationOrientation.Overlap)
                {
                    // Move index and document cursor to the end of the decoration to avoid overlaps
                    index += deco.To - deco.From;
                    documentCursor += deco.To - deco.From;
                }
            }
            else if (isStart == false && isNode)
            {
                // Handle the node end event to capture remaining parts
                var node = (Node)item;
                var actualFrom = node.From < from ? from : node.From;
                var actualTo = node.To > to ? to : node.To;

                if (documentCursor <= actualFrom)
                {
                    var x = index;
                    var y = (index + node.To - node.From);
                    y = y > content.Length ? content.Length : y;

                    var newCol = new TextCol(
                        content[x..y],
                        actualFrom,
                        actualTo,
                        string.Join(' ', activeNodes.Select(n => n.NodeType).Distinct()),
                        string.Join(' ', activeNodes.Select(state.GetClassString).Distinct())
                    );

                    AddOrMergeColumn(newCol, ref currentCol, list);
                    documentCursor = actualTo;
                    index += newCol.Length;
                }
            }

            // handle end of nodes
            if (isStart == false)
            {
                if (isNode)
                {
                    activeNodes.Remove((Node)item);
                }
            }
        }

        // Add the last remaining column if it exists
        if (currentCol is not null)
        {
            list.Add(currentCol);
        }

        if (index < content.Length)
        {
            var newCol = new TextCol(
                content[index..],
                documentCursor,
                to,
                string.Empty,
                string.Empty
            );

            list.Add(newCol);
        }

        return list;
    }

    private void AddOrMergeColumn(Col newCol, ref Col? currentCol, List<Col> list)
    {
        if (currentCol is not null && currentCol.Tag == newCol.Tag)
        {
            // Extend the current column if tags match
            if (currentCol is TextCol currentTextCol && newCol is TextCol newTextCol)
            {
                currentCol = currentTextCol with
                {
                    To = newCol.To, Content = currentTextCol.Content + newTextCol.Content
                };
            }
            else if (currentCol is DecorationCol currentDecoCol && newCol is DecorationCol newDecoCol)
            {
                currentCol = currentDecoCol with { To = newCol.To, ViewType = newDecoCol.ViewType };
            }
        }
        else
        {
            // Add the current column to the list if tags differ, and start a new one
            if (currentCol is not null)
            {
                list.Add(currentCol);
            }

            currentCol = newCol;
        }
    }
}