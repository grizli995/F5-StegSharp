using MethodTimer;
using SkiaSharp;
using StegSharp.Application.Common.Interfaces;
using StegSharp.Application.Models;
using StegSharp.Domain;
using StegSharp.Infrastructure.Util.Extensions;

namespace StegSharp.Infrastructure.Services
{
    public class ColorTransformationService : IColorTransformationService
    {
        /// <summary>
        /// Converts the provided bitmap from RGB color space to YCbCr color space.
        /// </summary>
        /// <param name="bmp">Input bitmap</param>
        /// <returns>YCBCR Data containing 3 components for Y, CB and CR.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        [Time]
        public YCBCRData RGBToYCbCr(SKBitmap bmp)
        {
            if (bmp == null)
                throw new ArgumentNullException(nameof(bmp), nameof(bmp).ToArgumentNullExceptionMessage());

            var result = new YCBCRData(bmp.Width, bmp.Height);

            ApplyColorTransform(bmp, result);

            return result;
        }


        #region Util

        private void ApplyColorTransform(SKBitmap bmp, YCBCRData result)
        {
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    var color = bmp.GetPixel(j, i);
                    var r = color.Red;
                    var g = color.Green;
                    var b = color.Blue;

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
