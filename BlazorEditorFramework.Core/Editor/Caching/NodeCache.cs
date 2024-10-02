using BlazorEditorFramework.Core.Editor.Abstractions;
using BlazorEditorFramework.Core.Editor.Caching.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Caching;

public class NodeCache : INodeCache
{
    private IntervalTree<CacheEntry> intervalTree;

    public NodeCache()
    {
        intervalTree = new IntervalTree<CacheEntry>();
    }

    public void AddOrReplaceNodes(IEnumerable<Node> nodes)
    {
        var nodeList = nodes.ToList();
        if (nodeList.Count() == 0)
            return;

        DateTime now = DateTime.Now;
        DateTime expiry = now.AddSeconds(60);

        foreach (var node in nodeList)
        {
            var interval = new Interval(node.From, node.To);
            var entry = new CacheEntry(node, interval, expiry);

            RemoveNodeIfExists(interval);
            AddNodeToIntervalTree(entry, interval);
        }
    }

    public void Clear()
    {
        intervalTree = new IntervalTree<CacheEntry>();
    }

    public IReadOnlyCollection<Node> GetNodes(int from, int to)
    {
        var queryInterval = new Interval(from, to);
        var nodes = intervalTree.Search(queryInterval).Select(entry => entry.Node).ToList();

        return nodes;
    }

    public void RemoveRange(IRange range) => RemoveRange(range.From, range.To);

    public void RemoveRange(int from, int to)
    {
        var interval = new Interval(from, to);
        var searchResult = intervalTree.Search(interval);
        Log.Debug("Found {SearchResultCount} existing entries", searchResult.Count);
        foreach (var existingEntry in searchResult)
        {
            intervalTree.Remove(existingEntry.Interval, existingEntry);
        }
    }

    public int ShiftNodes(int from, int shift)
    {
        // Use the optimized shift method in the interval tree
        intervalTree.Shift(from, shift);

        // Return the number of nodes shifted
        // For performance, we assume a count based on the affected range, if needed
        int affectedCount = intervalTree.Search(new Interval(from, int.MaxValue)).Count;

        return affectedCount;
    }

    private void AddNodeToIntervalTree(CacheEntry entry, Interval interval)
    {
        intervalTree.Insert(interval, entry);
    }


    private void RemoveNodeIfExists(Interval interval)
    {
        // Search for existing nodes in the same range
        var searchResult = intervalTree.Search(interval);
        var existingEntries = searchResult.Where(e => e.Interval.Equals(interval)).ToList();
        foreach (var existingEntry in existingEntries)
        {
            intervalTree.Remove(existingEntry.Interval, existingEntry);
        }
    }
}

internal record CacheEntry(Node Node, Interval Interval, DateTime ExpiresAt);