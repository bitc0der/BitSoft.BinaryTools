using System.Diagnostics.Metrics;

namespace BitSoft.BinaryTools.Patch.Metrics;

internal static class PatchMetrics
{
    private static Meter Meter { get; } = new(name: PatchMetricsConst.MeterName, version: "1.0");

    private static readonly Counter<int> DataBlocksCounter =
        Meter.CreateCounter<int>(
            name: PatchMetricsConst.Counters.DataBlocksCount,
            description: "Data blocks count"
        );

    private static readonly Counter<int> CopyBlocksCounter =
        Meter.CreateCounter<int>(
            name: PatchMetricsConst.Counters.CopyBlocksCount,
            description: "Copy blocks count"
        );

    private static readonly Counter<int> HashCalculationCounter =
        Meter.CreateCounter<int>(
            name: PatchMetricsConst.Counters.HashCalculationCount,
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
