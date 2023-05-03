using Application.Common.Interfaces;
using Application.Models;
using Infrastructure.Util.Extensions;

namespace Infrastructure.Services
{
    public class PaddingService : IPaddingService
    {
        public PaddingService() { }

        /// <summary>
        /// Applies edge extension padding to YCBCR data. 
        /// </summary>
        /// <param name="input">YCBCR input data that will be padded.</param>
        /// <param name="width">Original width</param>
        /// <param name="height">Original height</param>
        /// <returns>Returns new YCBCR data padded so that its new width and height are both divisible by 8.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public YCBCRData ApplyPadding(YCBCRData input, int width, int height)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), nameof(input).ToArgumentNullExceptionMessage());

            if (width <= 0)
                throw new ArgumentNullException(nameof(width), nameof(width).ToArgumentEqualsZeroExceptionMessage());

            if (height <= 0)
                throw new ArgumentNullException(nameof(height), nameof(height).ToArgumentEqualsZeroExceptionMessage());

            if (width % 8 == 0 && height % 8 == 0)
                return input;

            var result = new YCBCRData(CalculatePaddedDimension(width), CalculatePaddedDimension(height));

            result.YData = ApplyEdgeExtensionPadding(input.YData, width, height);
            result.CRData = ApplyEdgeExtensionPadding(input.CRData, width, height);
            result.CBData = ApplyEdgeExtensionPadding(input.CBData, width, height);

            return result;
        }

        /// <summary>
        /// Calculate padded input (height or width)
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>New padded value, which will be equal to or greater than the input, but divisible by 8.</returns>
        public int CalculatePaddedDimension(int input)
        {
            int paddedResult;

            if (input % 8 != 0)
            {
                paddedResult = input + 8 - (input % 8);
            }
            else
            {
                paddedResult = input;
            }

            return paddedResult;
        }

        #region Util

        /// <summary>
        /// Populates each item of the paddedInput.
        /// </summary>
        /// <param name="input">Input byte array, representing data of 1 color space of an image.</param>
        /// <param name="width">Original width</param>
        /// <param name="height">Original height</param>
        /// <param name="paddedWidth">Padded width</param>
        /// <param name="paddedHeight">Padded height</param>
        /// <param name="paddedInput">Padded byte array</param>
        private static void PopulatePaddedInput(float[,] input, int width, int height, int paddedWidth, int paddedHeight, float[,] paddedInput)
        {
            for (int i = 0; i < paddedHeight; i++)
            {
                for (int j = 0; j < paddedWidth; j++)
                {
                    int w = j;
                    int h = i;

                    if (i >= height)
                    {
                        h = height - 1;
                    }

                    if (j >= width)
                    {
                        w = width - 1;
                    }


                    paddedInput[i, j] = input[h, w];
                }
            }
        }

        /// <summary>
        /// Applies edge extension padding to right and bottom edge, populating new padded values with last pixel value.
        /// </summary>
        /// <param name="input">Input byte array, representing data of 1 color space of an image.</param>
        /// <param name="width">Original width</param>
        /// <param name="height">Original height</param>
        /// <returns>New padded and populated byte array.</returns>
        private float[,] ApplyEdgeExtensionPadding(float[,] input, int width, int height)
        {
            int paddedWidth = CalculatePaddedDimension(width);

            int paddedHeight = CalculatePaddedDimension(height);

            var paddedInput = new float[paddedHeight, paddedWidth];

            PopulatePaddedInput(input, width, height, paddedWidth, paddedHeight, paddedInput);

            return paddedInput;
        }

        #endregion
    }
}
