using Application.Common.Interfaces;
using Application.Models;
using Domain;
using Infrastructure.Util.Extensions;
using System.Drawing;

namespace Infrastructure.Services
{
    public class ColorTransformationService : IColorTransformationService
    {
        /// <summary>
        /// Converts the provided bitmap from RGB color space to YCbCr color space.
        /// </summary>
        /// <param name="bmp">Input bitmap</param>
        /// <returns>YCBCR Data containing 3 components for Y, CB and CR.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public YCBCRData RGBToYCbCr(Bitmap bmp)
        {
            if (bmp == null)
                throw new ArgumentNullException(nameof(bmp), nameof(bmp).ToArgumentNullExceptionMessage());

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

                    var y = RGBToYCBCR.CalculateY(r, g, b);
                    var cb = RGBToYCBCR.CalculateCB(r, g, b);
                    var cr = RGBToYCBCR.CalculateCR(r, g, b);

                    var yColorShifted = (float)(y - 128);
                    var cbColorShifted = (float)(cb - 128);
                    var crColorShifted = (float)(cr - 128);

                    result.YData[i, j] = yColorShifted;
                    result.CBData[i, j] = cbColorShifted;
                    result.CRData[i, j] = crColorShifted;
                }
            }
        }

        #endregion
    }
}
