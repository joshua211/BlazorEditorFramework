namespace BlazorEditorFramework.Core.Editor.Caching;

public class Interval
{
    public Interval(int start, int end)
    {
        Start = start;
        End = end;
    }

    public int Start { get; set; }
    public int End { get; set; }

    public override string ToString()
    {
        return $"{Start} - {End}";
    }

    public bool Intersects(Interval other)
    {
        return Start <= other.End && End >= other.Start;
    }
}

public class IntervalNode<T>
{
    public IntervalNode(Interval interval, T value)
    {
        Interval = interval;
        Value = value;
        Max = interval.End;
        Lazy = 0; // Initialize lazy field to 0
    }

    public Interval Interval { get; set; }
    public T Value { get; set; }
    public int Max { get; private set; }
    public IntervalNode<T>? Left { get; set; }
    public IntervalNode<T>? Right { get; set; }
    public int Lazy { get; set; } // Lazy propagation field

    public void UpdateMax()
    {
        Max = Math.Max(Interval.End, Math.Max(Left?.Max ?? int.MinValue, Right?.Max ?? int.MinValue));
    }

    public void ApplyLazy()
    {
        if (Lazy != 0)
        {
            // Apply the pending shift to the interval
            Interval = new Interval(Interval.Start + Lazy, Interval.End + Lazy);

            // Update the Node's From and To fields
            if (Value is CacheEntry cacheEntry)
            {
                cacheEntry.Node.From += Lazy;
                cacheEntry.Node.To += Lazy;
                cacheEntry.Interval.Start += Lazy;
                cacheEntry.Interval.End += Lazy;
            }

            // Reset lazy value
            Lazy = 0;
        }
    }
}

public class IntervalTree<T>
{
    private IntervalNode<T>? root;

    // Helper function to apply lazy propagation
    private void ApplyLazy(IntervalNode<T>? node)
    {
        node?.ApplyLazy();
    }

    public void Insert(Interval interval, T value)
    {
        root = Insert(root, interval, value);
    }

    private IntervalNode<T> Insert(IntervalNode<T>? node, Interval interval, T value)
    {
        if (node == null)
        {
            return new IntervalNode<T>(interval, value);
        }

        ApplyLazy(node);

        if (interval.Start < node.Interval.Start)
        {
            node.Left = Insert(node.Left, interval, value);
        }
        else
        {
            node.Right = Insert(node.Right, interval, value);
        }

        node.UpdateMax();
        return node;
    }

    public List<T> Search(Interval query)
    {
        var results = new List<T>();
        Search(root, query, results);
        return results;
    }

    private void Search(IntervalNode<T>? node, Interval query, List<T> results)
    {
        if (node == null)
        {
            return;
        }

        ApplyLazy(node);

        if (node.Interval.Intersects(query))
        {
            results.Add(node.Value);
        }

        if (node.Left != null && node.Left.Max >= query.Start)
        {
            Search(node.Left, query, results);
        }

        if (node.Right != null && node.Interval.Start <= query.End)
        {
            Search(node.Right, query, results);
        }
    }

    public void Remove(Interval interval, T value)
    {
        root = Remove(root, interval, value);
    }

    private IntervalNode<T>? Remove(IntervalNode<T>? node, Interval interval, T value)
    {
        if (node == null)
        {
            return null;
        }

        ApplyLazy(node);

        if (interval.Start < node.Interval.Start)
        {
            node.Left = Remove(node.Left, interval, value);
        }
        else if (interval.Start > node.Interval.Start)
        {
            node.Right = Remove(node.Right, interval, value);
        }
        else if (value == null || EqualityComparer<T>.Default.Equals(node.Value, value))
        {
            if (node.Left == null)
            {
                return node.Right;
            }

            if (node.Right == null)
            {
                return node.Left;
            }

            var minNode = GetMin(node.Right);
            node.Interval = minNode.Interval;
            node.Value = minNode.Value;
            node.Right = Remove(node.Right, minNode.Interval, minNode.Value);
        }
        else
        {
            node.Right = Remove(node.Right, interval, value);
        }

        node.UpdateMax();
        return node;
    }

    private IntervalNode<T> GetMin(IntervalNode<T> node)
    {
        while (node.Left != null)
        {
            node = node.Left;
        }

        return node;
    }

    public void Clear()
    {
        root = null;
    }

    // New method for applying shift lazily with restructuring
    public void Shift(int from, int shift)
    {
        root = Shift(root, from, shift);
    }

    private IntervalNode<T>? Shift(IntervalNode<T>? node, int from, int shift)
    {
        if (node == null)
        {
            return null;
        }

        ApplyLazy(node);

        if (node.Interval.Start >= from)
        {
            node.Lazy += shift;
            ApplyLazy(node); // Apply immediately for current node
        }

        if (node.Left != null && node.Left.Max >= from)
        {
            node.Left = Shift(node.Left, from, shift);
        }

        if (node.Right != null && node.Right.Max >= from)
        {
            node.Right = Shift(node.Right, from, shift);
        }

        // Check and fix the tree structure
        node = FixTree(node);

        return node;
    }

    private IntervalNode<T>? FixTree(IntervalNode<T>? node)
    {
        if (node == null) return null;

        // Fix left subtree if necessary
        if (node.Left != null && node.Left.Interval.Start > node.Interval.Start)
        {
            node = RotateRight(node);
        }

        // Fix right subtree if necessary
        if (node?.Right != null && node.Right.Interval.Start < node.Interval.Start)
        {
            node = RotateLeft(node);
        }

        // Update max values after potential rotations
        node?.UpdateMax();

        return node;
    }

    private IntervalNode<T>? RotateLeft(IntervalNode<T> node)
    {
        var newRoot = node.Right;
        node.Right = newRoot?.Left;

        if (newRoot is not null)
            newRoot.Left = node;
        node.UpdateMax();
        newRoot?.UpdateMax();

        return newRoot;
    }

    private IntervalNode<T>? RotateRight(IntervalNode<T> node)
    {
        var newRoot = node.Left;
        node.Left = newRoot?.Right;
        if (newRoot is not null)
            newRoot.Right = node;
        node.UpdateMax();
        newRoot?.UpdateMax();

        return newRoot;
    }
}