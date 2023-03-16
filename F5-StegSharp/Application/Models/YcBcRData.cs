using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class YCBCRData
    {
        public byte[,] YData { get; set; }
        public byte[,] CRData { get; set; }
        public byte[,] CBData { get; set; }

        public YCBCRData(int width, int height)
        {
            YData = new byte[width, height];
            CRData = new byte[width, height];
            CBData = new byte[width, height];
        }
    }
}
