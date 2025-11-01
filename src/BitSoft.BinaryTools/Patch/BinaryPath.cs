using System;
using System.Collections.Generic;

namespace BitSoft.BinaryTools.Patch;

public class BinaryPatch
{
    private readonly LinkedList<BinaryPathSegment> _segments;
    
    public IReadOnlyCollection<BinaryPathSegment> Segments => _segments;
    
    private BinaryPatch(LinkedList<BinaryPathSegment> segments)
    {
        _segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    public static BinaryPatch Calculate(ReadOnlySpan<byte> original, ReadOnlySpan<byte> modified)
    {
        var segments = new LinkedList<BinaryPathSegment>();

        const int NotDefined = -1;

        var startIndex = NotDefined;

        for (var i = 0; i < modified.Length; i++)
        {
            var left = original[i];
            var right = modified[i];

            if (left == right)
            {
                if (startIndex != NotDefined)
                {
                    var segment = new BinaryPathSegment(offset: startIndex, length: i -  startIndex);
                    segments.AddLast(segment);
                    startIndex = NotDefined;
                }
                continue;
            }
            
            if (startIndex == NotDefined)
                startIndex = i;
        }

        return new BinaryPatch(segments);
    }
}