using System.Diagnostics;
using System.Text;
using FluentAssertions;
using LiteTorrent.Core;

namespace LiteTorrent.Domain.Tests;

public class Tests
{
    private MerkleTree fullMerkleTree = null!;
    private MerkleTree uncompletedMerkleTree = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var hashes = new[] { "1", "2", "3" }
            .Select(str => Hash.CreateFromRaw(Encoding.UTF8.GetBytes(str)))
            .ToArray();

        fullMerkleTree = new MerkleTree(hashes);
    }

    [SetUp]
    public void SetUp()
    {
        uncompletedMerkleTree = new MerkleTree(fullMerkleTree.PieceCount, fullMerkleTree.RootHash);
    }

    [Test]
    public void TryAdd_WithWrongPieceHash_ReturnsFalse()
    {
        var isAdded = uncompletedMerkleTree.TryAdd(
            1, 
            fullMerkleTree.GetPieceHash(2), 
            fullMerkleTree.GetPath(1).ToArray());

        isAdded.Should().BeFalse();
    }
    
    
    [Test]
    public void Performance()
    {
        var stopWatch = Stopwatch.StartNew();   
        var tree = new MerkleTree(1000_000, Hash.Empty);
        for (var i = 0; i < 1000_000; i++)
        {
            tree.GetPath(i).ToArray();
        }
        stopWatch.Stop();
        Console.WriteLine("Excution time for {0} - {1} ms",
            TestContext.CurrentContext.Test.Name,
            stopWatch.ElapsedMilliseconds);
    }


    [Test]
    public void GetLeafStates_AfterAdd_ContainsFlagOnAddIndex()
    {
        uncompletedMerkleTree.GetLeafStates().CountTrue().Should().Be(0);

        var isAdded = uncompletedMerkleTree
            .TryAdd(1, fullMerkleTree.GetPieceHash(1), fullMerkleTree.GetPath(1).ToArray());
        
        isAdded.Should().BeTrue();

        var leafStates = uncompletedMerkleTree.GetLeafStates();
        leafStates.CountTrue().Should().Be(1);
        leafStates[1].Should().BeTrue();
    }

    [Test]
    public void GetPath()
    {
        var merkleTree = new MerkleTree(14, fullMerkleTree.RootHash);
        merkleTree.GetPath(12).ToArray().Should();
    }
}
