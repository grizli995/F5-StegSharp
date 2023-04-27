namespace Application.Models
{
    public class EmbeddingRateRecord
    {
        public int K { get; set; }
        public int N { get; set; }
        public double ChangeDensity { get; set; }
        public double EmbeddingRate { get; set; }
        public double EmbeddingEfficiency { get; set; }

        public EmbeddingRateRecord(int k, int n, double changeDensity, double rate, double efficiency) 
        {
            this.K = k;
            this.N = n;
            this.ChangeDensity = changeDensity;
            this.EmbeddingRate = rate;
            this.EmbeddingEfficiency = efficiency;
        }
    }
}
