using Application.Common.Interfaces;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ColorTransformationService : IColorTransformationService
    {
        public YcBcRData RGBToYCbCr(Bitmap bmp)
        {
            if (bmp == null)
                throw new ArgumentNullException(nameof(bmp));

            var result = new YcBcRData(bmp.Width, bmp.Height);

            ApplyColorTransform(bmp, result);

            return result;
        }


        #region Util

        private static void ApplyColorTransform(Bitmap bmp, YcBcRData result)
        {
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    var color = bmp.GetPixel(j, i);
                    var r = color.R;
                    var g = color.G;
                    var b = color.B;

                    result.YData[j, i] = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                    result.CBData[j, i] = (byte)(128 + (-0.16874 * r - 0.33126 * g + 0.5 * b));
                    result.CRData[j, i] = (byte)(128 + (0.5 * r - 0.41869 * g - 0.08131 * b));
                }
            }
        }

        #endregion
    }
}
