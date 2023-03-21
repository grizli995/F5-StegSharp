using JpegLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
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
    }
}
