using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor;

public class Node : IRange
{
    public Node(int from, int to, string nodeType)
    {
        From = from;
        To = to;
        NodeType = nodeType;
    }

    public string NodeType { get; set; }

    public int Length => To - From;

    public int From { get; set; }
    public int To { get; set; }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, To, NodeType);
    }

    public override bool Equals(object? obj)
    {
        return obj is Node node &&
               From == node.From &&
               To == node.To &&
               NodeType == node.NodeType;
    }

    public override string ToString()
    {
        return $"Node: {NodeType} ({From}, {To})";
    }
}