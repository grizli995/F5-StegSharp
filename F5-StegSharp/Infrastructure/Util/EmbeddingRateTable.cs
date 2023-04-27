using Application.Models;

namespace Infrastructure.Util
{
    public static class EmbeddingRateTable
    {
        public static readonly EmbeddingRateRecord[] Table =
        {
            // n, k, changeDensity, embeddingRate, embeddingEfficiency
            new EmbeddingRateRecord(1,1,50, 100, 2),
            new EmbeddingRateRecord(2, 3, 25.00, 66.67, 2.67),
            new EmbeddingRateRecord(3, 7, 12.50, 42.86, 3.43),
            new EmbeddingRateRecord(4, 15, 6.25, 26.67, 4.27),
            new EmbeddingRateRecord(5, 31, 3.12, 16.13, 5.16),
            new EmbeddingRateRecord(6, 63, 1.56, 9.52, 6.09),
            new EmbeddingRateRecord(7, 127, 0.78, 5.51, 7.06),
            new EmbeddingRateRecord(8, 255, 0.39, 3.14, 8.03),
            new EmbeddingRateRecord(9, 511, 0.20, 1.76, 9.02)
        };
    }
}