using SkiaSharp;

namespace StegSharp.Application.Models
{
    public class JpegInfo
    {
        public SKBitmap Bitmap { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int HorizontalPixelDensity { get; set; }
        public int VerticalPixelDensity { get; set; }
        public int Precision { get; set; }
        public YCBCRData YCBCRData { get; set; }
        public DCTData DCTData { get; set; }
        public DCTData QuantizedDCTData { get; set; }
        public DCTData EmbededData { get; set; }
        public JpegComponent[] Components { get; set; }
        public JpegQuantizationTable[] QuantizationTables { get; set; } 
        public JpegHuffmanTableData HuffmanTableData { get; set; }
        public List<JpegHuffmanTable> HuffmanTables { get; set; }

        public JpegInfo()
        {
            QuantizationTables = new JpegQuantizationTable[2];
            HuffmanTableData = new JpegHuffmanTableData();
            HuffmanTables = new List<JpegHuffmanTable>();
            Components = new JpegComponent[] 
            {
                new JpegComponent(1),
                new JpegComponent(2),
                new JpegComponent(3)
            };
        }
    }
}
