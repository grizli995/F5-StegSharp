using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class YCBCRData
    {
        public float[,] YData { get; set; }
        public float[,] CRData { get; set; }
        public float[,] CBData { get; set; }

        public YCBCRData(int width, int height)
        {
            YData = new float[height, width];
            CRData = new float[height, width];
            CBData = new float[height, width];
        }
    }
}
