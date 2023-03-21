using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class JpegInfo
    {
        public Bitmap Bitmap { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public YCBCRData YCBCRData { get; set; }
        public DCTData DCTData { get; set; }
        public DCTData QuantizedDCTData { get; set; }
        public DCTData EmbededData { get; set; }
    }
}
