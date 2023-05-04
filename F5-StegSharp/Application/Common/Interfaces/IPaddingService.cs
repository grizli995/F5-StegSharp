using Application.Models;

namespace Application.Common.Interfaces
{
    public interface IPaddingService
    {
        /// <summary>
        /// Applies edge extension padding to YCBCR data. 
        /// </summary>
        /// <param name="input">YCBCR input data that will be padded.</param>
        /// <param name="width">Original width</param>
        /// <param name="height">Original height</param>
        /// <returns>Returns new YCBCR data padded so that its new width and height are both divisible by 8.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        YCBCRData ApplyPadding(YCBCRData input, int width, int height);


        /// <summary>
        /// Calculate padded input (height or width)
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>New padded value, which will be equal to or greater than the input, but divisible by 8.</returns>
        int CalculatePaddedDimension(int input);
    }
}
