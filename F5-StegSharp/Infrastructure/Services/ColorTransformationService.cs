using Application.Common.Interfaces;
using Application.Models;
using Domain;
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
        public YCBCRData RGBToYCbCr(Bitmap bmp)
        {
            if (bmp == null)
                throw new ArgumentNullException(nameof(bmp));

            var result = new YCBCRData(bmp.Width, bmp.Height);

            ApplyColorTransform(bmp, result);

            return result;
        }


        #region Util

        private void ApplyColorTransform(Bitmap bmp, YCBCRData result)
        {
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    var color = bmp.GetPixel(j, i);
                    var r = color.R;
                    var g = color.G;
                    var b = color.B;

                    result.YData[i, j] = RGBToYCBCR.CalculateY(r, g, b);
                    result.CBData[i, j] = RGBToYCBCR.CalculateCB(r, g, b);
                    result.CRData[i, j] = RGBToYCBCR.CalculateCR(r, g, b);
                }
            }
        }


        #endregion
    }
}
