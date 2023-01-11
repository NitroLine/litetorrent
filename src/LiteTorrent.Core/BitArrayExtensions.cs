using System.Collections;

namespace LiteTorrent.Core;

public static class BitArrayExtensions
{
    public static int CountTrue(this BitArray bitArray)
    {
        var count = 0;
        for (var i = 0; i < bitArray.Count; i++)
        {
            if (bitArray[i])
                count++;
        }

        return count;
    }
}