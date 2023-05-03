using Application.Models;
using System.Drawing;

namespace Application.Common.Interfaces
{
    public interface IColorTransformationService
    {
        /// <summary>
        /// Converts the provided bitmap from RGB color space to YCbCr color space.
        /// </summary>
        /// <param name="bmp">Input bitmap</param>
        /// <returns>YCBCR Data containing 3 components for Y, CB and CR.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public YCBCRData RGBToYCbCr(Bitmap bmp);
    }
}
