using System;
using System.Collections.Generic;

namespace BitSoft.BinaryTools.Patch;

public class BinaryPatch
{
    private readonly IReadOnlyList<IBinaryPatchSegment> _segments;

    public IReadOnlyList<IBinaryPatchSegment> Segments => _segments;

    internal BinaryPatch(IReadOnlyList<IBinaryPatchSegment> segments)
    {
        _segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }
}