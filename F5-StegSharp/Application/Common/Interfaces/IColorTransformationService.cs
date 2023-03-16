using Application.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IColorTransformationService
    {
        public YcBcRData RGBToYCbCr(Bitmap bmp);
    }
}
