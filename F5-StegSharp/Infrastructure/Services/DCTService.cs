using Application.Common.Interfaces;
using Application.Models;
using JpegLibrary;
using System.Runtime.CompilerServices;

namespace Infrastructure.Services
{
    public class DCTService : IDCTService
    {
        private readonly IPaddingService _paddingService;
        public DCTService(IPaddingService paddingService)
        {
            this._paddingService = paddingService;
        }

        public DCTData CalculateDCT(YCBCRData input, int width, int height)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            DCTData result = new DCTData();

            //pad input to fit 8x8 blocks.
            var paddedInput = _paddingService.ApplyPadding(input, width, height);
            var paddedWidth = _paddingService.CalculatePaddedDimension(width);
            var paddedHeight = _paddingService.CalculatePaddedDimension(height);

            //split ycbcr data into blocks
            result = CreateMCUs(paddedInput, paddedWidth, paddedHeight);

            //perform DCT calculation
            result = ApplyDCT(result, paddedWidth, paddedHeight);

            return result;
        }

        #region Util

        /// <summary>
        /// Calculates DCT values for all 3 color components.
        /// </summary>
        /// <param name="dctData">Input</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Three arrays of MCUs which contain values from DCT calculation.</returns>
        private DCTData ApplyDCT(DCTData dctData, int width, int height)
        {
            dctData.YDCTData = ApplyDCTToColorComponent(dctData.YDCTData, width, height);
            dctData.CRDCTData = ApplyDCTToColorComponent(dctData.CRDCTData, width, height);
            dctData.CBDCTData = ApplyDCTToColorComponent(dctData.CBDCTData, width, height);

            return dctData;
        }

        /// <summary>
        /// Calculates DCT values for a single color component.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>An array of MCUs which contain values from DCT calculation.</returns>
        private JpegBlock8x8F[] ApplyDCTToColorComponent(JpegBlock8x8F[] input, int width, int height)
        {
            JpegBlock8x8F[] result = new JpegBlock8x8F[width * height / 64];

            Unsafe.SkipInit(out JpegBlock8x8F inputFBuffer);
            Unsafe.SkipInit(out JpegBlock8x8F outputFBuffer);
            Unsafe.SkipInit(out JpegBlock8x8F tempFBuffer);

            for (int i = 0; i < input.Length; i++)
            {
                inputFBuffer = input[i];
                FastFloatingPointDCT.TransformFDCT(ref inputFBuffer, ref outputFBuffer, ref tempFBuffer);
                result[i] = outputFBuffer;
            }

            return result;
        }

        /// <summary>
        /// Creates MCUs for an entire image, for all 3 color components.
        /// </summary>
        /// <param name="padddedInput">Padded input</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Arrays of MCUs for a all color components.</returns>
        private DCTData CreateMCUs(YCBCRData padddedInput, int width, int height)
        {
            var result = new DCTData();

            result.YDCTData = CreateMCUsForColorComponent(padddedInput.YData, width, height);
            result.CRDCTData = CreateMCUsForColorComponent(padddedInput.CRData, width, height);
            result.CBDCTData = CreateMCUsForColorComponent(padddedInput.CBData, width, height);

            return result;
        }

        /// <summary>
        /// Creates an array of MCUs for a single color component.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Array of MCUs for a single color component.</returns>
        private JpegBlock8x8F[] CreateMCUsForColorComponent(byte[,] input, int width, int height)
        {
            JpegBlock8x8F[] res = new JpegBlock8x8F[width * height / 64];

            var counter = 0;

            for (int i = 0; i < height; i = i + 8)
            {
                for (int j = 0; j < width; j = j + 8)
                {
                    var mcu = CreateMinimumCodedUnit(input, j, i);
                    res[counter++] = mcu;
                }
            }

            return res;
        }

        /// <summary>
        /// Creates MCU from coordinates of the upper-left item in the MCU that will be created.
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="sWidth">starting width</param>
        /// <param name="sHeight">starting height</param>
        /// <returns>MCU object which is a 8x8 block.</returns>
        private JpegBlock8x8F CreateMinimumCodedUnit(byte[,] input, int sWidth, int sHeight)
        {
            var result = new JpegBlock8x8F();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    result[i] = input[sWidth + j, sHeight + i];
                }
            }

            return result;
        }

        #endregion
    }
}
