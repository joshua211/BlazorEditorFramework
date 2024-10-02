using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Caching.Abstractions;

public interface INodeCache
{
    void AddOrReplaceNodes(IEnumerable<Node> nodes);

    void Clear();

    IReadOnlyCollection<Node> GetNodes(int from, int to);

    void RemoveRange(int from, int to);
    void RemoveRange(IRange range);

    /// <summary>
    /// Updates all nodes in the cache and shifts them by the given amount.
    /// </summary>
    /// <param name="from">The start after which all nodes are shifted</param>
    /// <param name="shift">The amount of shift</param>
    /// <returns>The number of nodes affected</returns>
    int ShiftNodes(int from, int shift);
}