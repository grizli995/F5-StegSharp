namespace Application.Models
{
    public class JpegComponent
    {
        public int Id { get; set; }

        public int SamplingFactor { get; set; }

        public int QuantizationTableId { get; set; }

        public int DCHuffmanTableId { get; set; }
        
        public int ACHuffmanTableId { get; set; }
    }
}
