using System;
using System.Collections.Generic;

namespace BitSoft.BinaryTools.Patch;

public class BinaryPatch
{
    private readonly LinkedList<IBinaryPatchSegment> _segments;

    public IReadOnlyCollection<IBinaryPatchSegment> Segments => _segments;

    private BinaryPatch(LinkedList<IBinaryPatchSegment> segments)
    {
        _segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    public static BinaryPatch Calculate(ReadOnlyMemory<byte> original, ReadOnlyMemory<byte> modified)
    {
        var segments = new LinkedList<IBinaryPatchSegment>();

        const int NotDefined = -1;

        var startIndex = NotDefined;

        var originalSpan = original.Span;
        var modifiedSpan = modified.Span;

        for (var i = 0; i < modified.Length; i++)
        {
            if (originalSpan.Length == i)
            {
                if (startIndex == NotDefined)
                {
                    var length = modified.Length - original.Length;
                    var memory = modified.Slice(start: i, length: length);
                    var segment = new BinaryPatchSegment(offset: i, length: length, memory: memory);
                    segments.AddLast(segment);
                    break;
                }
                else
                {
                    var length = modified.Length - startIndex;
                    var memory = modified.Slice(start: startIndex, length: length);
                    var segment = new BinaryPatchSegment(offset: startIndex, length: length, memory: memory);
                    segments.AddLast(segment);
                    break;
                }
            }

            var left = originalSpan[i];
            var right = modifiedSpan[i];

            if (left == right)
            {
                if (startIndex != NotDefined)
                {
                    var length = i - startIndex;
                    var memory = modified.Slice(start: startIndex, length: length);
                    var segment = new BinaryPatchSegment(offset: startIndex, length: length, memory: memory);
                    segments.AddLast(segment);
                    startIndex = NotDefined;
                }

                continue;
            }

            if (startIndex == NotDefined)
                startIndex = i;
        }

        if (modified.Length < original.Length)
        {
            segments.AddLast(new EndOfFilePatchSegment(offset: modified.Length));
        }

        return new BinaryPatch(segments);
    }
}