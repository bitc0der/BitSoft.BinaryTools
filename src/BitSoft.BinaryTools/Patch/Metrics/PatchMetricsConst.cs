namespace BitSoft.BinaryTools.Patch.Metrics;

public static class PatchMetricsConst
{
    public const string MeterName = "BitSoft.BinaryTools.Patch";

    public static class Counters
    {
        public const string DataBlocksCount = "bitsoft.binary.patch.data_blocks_count";
        public const string CopyBlocksCount = "bitsoft.binary.patch.copy_blocks_count";
        public const string HashCalculationCount = "bitsoft.binary.patch.hash_calculations_count";
    }
}
