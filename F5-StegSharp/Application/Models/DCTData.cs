using JpegLibrary;

namespace StegSharp.Application.Models
{
    public class DCTData
    {
        public JpegBlock8x8F[] YDCTData { get; set; }
        public JpegBlock8x8F[] CRDCTData { get; set; }
        public JpegBlock8x8F[] CBDCTData { get; set; }

        public DCTData()
        {
            this.YDCTData = new JpegBlock8x8F[] { };
            this.CRDCTData = new JpegBlock8x8F[] { };
            this.CBDCTData = new JpegBlock8x8F[] { };
        }

        public DCTData(int dctCount)
        {
            this.YDCTData = new JpegBlock8x8F[dctCount];
            this.CRDCTData = new JpegBlock8x8F[dctCount];
            this.CBDCTData = new JpegBlock8x8F[dctCount];
        }
    }
}
