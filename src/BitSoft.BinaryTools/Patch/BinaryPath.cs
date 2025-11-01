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

    public static BinaryPatch Calculate(ReadOnlyMemory<byte> original, ReadOnlyMemory<byte> modified)
    {
        var segments = new LinkedList<BinaryPathSegment>();

        const int NotDefined = -1;

        var startIndex = NotDefined;

        var originalSpan = original.Span;
        var modifiedSpan = modified.Span;

        for (var i = 0; i < modified.Length; i++)
        {
            var left = originalSpan[i];
            var right = modifiedSpan[i];

            if (left == right)
            {
                if (startIndex != NotDefined)
                {
                    var length = i - startIndex;
                    var memory = modified.Slice(start: startIndex, length: length);
                    var segment = new BinaryPathSegment(offset: startIndex, length: length, memory: memory);
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