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
    public Hash RootHash { get; private set; }

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
        BuildAllTree(pieces);
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
    
    public IEnumerable<Hash> GetPath(int index)
    {
        var (leafIndex, treeIndex) = GetIndexes(index);
        var currIndex = leafIndex + trees[treeIndex].Length - leafCounts[treeIndex];
        var itemHash = trees[treeIndex][currIndex];
        return GetTreePath(itemHash, treeIndex, currIndex);
    }

    private (int leafIndex, int treeIndex) GetIndexes(int index)
    {
        var leafIndex = index;
        var treeIndex = 0;
        for (; leafIndex >= leafCounts[treeIndex]; ++treeIndex)
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
            var treeIndex = arrayIndex * 2 + (arrayIndex == rootTree.Length - 1 ? 0 : 1);
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
    
    private IEnumerable<Hash> GetTreePath(Hash lastHash, int arrayIndex, int index)
    {
        if (index == 0)
        {
            var treeIndex = arrayIndex * 2 + (arrayIndex == rootTree.Length - 1 ? 0 : 1);
            foreach (var hash in  GetRootPath(lastHash, treeIndex))
            {
                yield return hash;
            }
            yield break;
        }
        if (index % 2 == 0)
        {
            var left = trees[arrayIndex][index - 1];
            yield return left;
            foreach (var hash in  GetTreePath(left.Concat(lastHash), arrayIndex,(index - 1) / 2))
            {
                yield return hash;
            }
            yield break;
        }
        var right = trees[arrayIndex][index + 1];
        yield return right;
        foreach (var hash in  GetTreePath(lastHash.Concat(right), arrayIndex,(index - 1) / 2))
        {
            yield return hash;
        }
    }

    private IEnumerable<Hash> GetRootPath(Hash lastHash, int treeIndex)
    {
        if (treeIndex == 0)
        {
           
            yield break;
        }
        if (treeIndex % 2 == 0)
        {
            var left = rootTree[treeIndex - 1];
            yield return left;
            foreach (var hash in  GetRootPath(left.Concat(lastHash), treeIndex - 2))
            {
                yield return hash;
            }
            yield break;
        }
        var right = rootTree[treeIndex + 1];
        yield return right;
        foreach (var hash in  GetRootPath(lastHash.Concat(right), treeIndex - 1))
        {
            yield return hash;
        }
    }

    private Hash BuildAllTree(Hash[] pieces)
    {
        var pieceIndex = 0;
        for (var i = 0; i < trees.Count; i++)
        {
            var (leafIndex, treeIndex) = GetIndexes(pieceIndex);
            var curIndex = leafIndex + trees[treeIndex].Length - leafCounts[treeIndex];
            for (var j = curIndex; j < curIndex + leafCounts[i]; ++j)
            {
                trees[i][j] = pieces[pieceIndex++];
            }
        }
        RootHash = BuildTree(-1, 0);
        return RootHash;
    }

    private Hash BuildTree(int treeIndex, int index)
    {
        if (treeIndex == -1)
        {
            if (index % 2 != 0 || index == rootTree.Length - 1)
            {
                var calcHash =  BuildTree(index / 2, 0);
                rootTree[index] = calcHash;
                return calcHash;
            }
            var left = BuildTree(-1, index + 1);
            var right = BuildTree(-1, index + 2);
            var calcHash2 = left.Concat(right);
            rootTree[index] = calcHash2;
            return calcHash2;
        }
        var leafStart = trees[treeIndex].Length - leafCounts[treeIndex];
        if (index >= leafStart) {
            return trees[treeIndex][index];
        }
        var leftTreeHash = BuildTree(treeIndex, index * 2 + 1);
        var rightTreeHash = BuildTree(treeIndex, index * 2 + 2);
        var calcHash3 = leftTreeHash.Concat(rightTreeHash);
        trees[treeIndex][index] = calcHash3;
        return calcHash3;
    }
}