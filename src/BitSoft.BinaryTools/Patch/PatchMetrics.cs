using System.Diagnostics.Metrics;

namespace BitSoft.BinaryTools.Patch;

internal static class PatchMetrics
{
    private static readonly Meter Meter = new(name: "BitSoft.BinaryTools.Patch", version: "1.0");

    private static readonly Counter<int> DataBlocksCounter =
        Meter.CreateCounter<int>(name: "bitsoft.binary.patch.data_blocks");

    private static readonly Counter<int> CopyBlocksCounter =
        Meter.CreateCounter<int>(name: "bitsoft.binary.patch.copy_blocks");

    public static void AddDataBlock()
    {
        DataBlocksCounter.Add(delta: 1);
    }

    public static void AddCopyBlock()
    {
        CopyBlocksCounter.Add(delta: 1);
    }
}
