namespace LiteTorrent.Domain;

public record Action
(
    int TreeIndex,
    int IndexInTree,
    Hash Hash
);

public class MerkelTree
{
    private readonly List<Hash[]> trees = new();
    private readonly Hash[] rootTree;
    private readonly List<int> leafCounts = new();
    public readonly Hash RootHash; 

    private readonly Queue<Action> addQueue = new();

    public MerkelTree(int count)
    {
        while (count != 0)
        {
            var leafCount = (int)Math.Pow(2, (int)Math.Log2(count));
            leafCounts.Add(leafCount);
            trees.Add(new Hash[2 * leafCount - 1]);
            count -= leafCount;
        }

        rootTree = new Hash[trees.Count * 2 - 1];
    }

    public MerkelTree(int count, Hash rootHash) : this(count)
    {
        RootHash = rootHash;
    }

    public MerkelTree(List<Hash[]> trees, Hash[] rootTree, Hash rootHash)
    {
        RootHash = rootHash;
        this.trees = trees;
        this.rootTree = rootTree;
    }

    public MerkelTree(Hash[] pieces) : this(pieces.Length)
    {
    }
    

    public bool TryAdd(int index, Hash itemHash, Hash[] path)
    {
        var (leafIndex, treeIndex) = GetIndexes(index);

        var currIndex = leafIndex + trees[treeIndex].Length - leafCounts[treeIndex];
        var rootHash = ComputeTreeHash(itemHash, treeIndex, currIndex, 0, path);

        if (rootHash != RootHash)
            return false;
        while (addQueue.Count != 0)
        {
            var act = addQueue.Dequeue();
            if (act.TreeIndex == -1)
                rootTree[act.IndexInTree] = act.Hash;
            else trees[act.TreeIndex][act.IndexInTree] = act.Hash;
        }

        return true;
    }

    private (int leafIndex, int treeIndex) GetIndexes(int index)
    {
        var leafIndex = index;
        var treeIndex = 0;
        for (; leafIndex > leafCounts[treeIndex]; ++treeIndex)
        {
            leafIndex -= leafCounts[treeIndex];
        }

        return (leafIndex, treeIndex);
    }

    private Hash ComputeTreeHash(Hash lastHash, int arrayIndex, int index, int pathIndex, Hash[] path)
    {
        addQueue.Enqueue(new Action(arrayIndex, index, lastHash));
        if (index == 0)
        {
            var treeIndex = arrayIndex * 2 + (arrayIndex == trees.Count - 1 ? 0 : 1);
            return ComputeRootHash(lastHash, treeIndex, pathIndex, path);
        }
        
        if (index % 2 == 0)
        {
            addQueue.Enqueue(new Action(arrayIndex, index - 1, path[pathIndex]));
            var left = path[pathIndex];
            return ComputeTreeHash(left.Concat(lastHash), arrayIndex,(index - 1) / 2, ++pathIndex, path);
        }
        addQueue.Enqueue(new Action(arrayIndex, index + 1, path[pathIndex]));
        var right = path[pathIndex];
        return ComputeTreeHash(lastHash.Concat(right), arrayIndex,(index - 1) / 2, ++pathIndex, path);
    }

    private Hash ComputeRootHash(Hash lastHash, int treeIndex, int pathIndex, Hash[] path)
    {
        addQueue.Enqueue(new Action(-1, treeIndex, lastHash));
        if (treeIndex == 0)
        {
            return lastHash;
        }
        if (treeIndex % 2 == 0)
        {
            addQueue.Enqueue(new Action(-1, treeIndex - 1, path[pathIndex]));
            var left = path[pathIndex];
            return ComputeRootHash(left.Concat(lastHash), treeIndex - 2, ++pathIndex, path);
        }
        addQueue.Enqueue(new Action(-1, treeIndex + 1, path[pathIndex]));
        var right = path[pathIndex];
        return ComputeRootHash(lastHash.Concat(right), treeIndex - 1, ++pathIndex, path);
    }

    private Hash BuildAllTree(Hash[] pieces)
    {
        var treeIndex = 0;
        for (var i = 0; i < trees.Count; i++)
        {
            var (_, leafIndex) = GetIndexes(treeIndex);
            for (var j = leafIndex; j < leafIndex + leafCounts[i]; ++j)
            {
                trees[i][j] = pieces[treeIndex++];
            }
        }
        
    }

    private Hash BuildTree(int treeIndex, int index)
    {
        if (treeIndex % 2 != 0 || treeIndex == trees.Count - 1)
    }
}