using System.Diagnostics.Metrics;

namespace BitSoft.BinaryTools.Patch;

internal static class PatchMetrics
{
    private static readonly Meter Meter = new(name: "BitSoft.BinaryTools.Patch", version: "1.0");

    private static readonly Counter<int> DataBlocksCounter =
        Meter.CreateCounter<int>(
            name: "bitsoft.binary.patch.data_blocks_count",
            description: "Data blocks count"
        );

    private static readonly Counter<int> CopyBlocksCounter =
        Meter.CreateCounter<int>(
            name: "bitsoft.binary.patch.copy_blocks_count",
            description: "Copy blocks count"
        );

    private static readonly Counter<int> HashCalculationCounter =
        Meter.CreateCounter<int>(
            name: "bitsoft.binary.patch.hash_calculations_count",
            description: "Hash calculations count"
        );

    public static void AddDataBlock()
    {
        DataBlocksCounter.Add(delta: 1);
    }

    public static void AddCopyBlock()
    {
        CopyBlocksCounter.Add(delta: 1);
    }

    public static void AddHashCalculation()
    {
        HashCalculationCounter.Add(delta: 1);
    }
}
