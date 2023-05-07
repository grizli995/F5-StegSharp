using StegSharp.Application.Common.Interfaces;
using StegSharp.Application.Models;
using StegSharp.Domain;
using StegSharp.Infrastructure.Util.Extensions;
using JpegLibrary;
using MethodTimer;
using System.Runtime.CompilerServices;

namespace StegSharp.Infrastructure.Services
{
    public class DCTService : IDCTService
    {
        private readonly IPaddingService _paddingService;
        public DCTService(IPaddingService paddingService)
        {
            this._paddingService = paddingService;
        }

        /// <summary>
        /// Calculates the 2-D Discrete Cosine Transformation on the entire image, for all 3 components. 
        /// </summary>
        /// <param name="input">Input YCBCR data.</param>
        /// <param name="width">Width of the original image.</param>
        /// <param name="height">Height of the original image.</param>
        /// <returns>DCTData object, containing computed result for all components.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        [Time]
        public DCTData CalculateDCT(YCBCRData input, int width, int height)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), nameof(input).ToArgumentNullExceptionMessage());

            if (width <= 0)
                throw new ArgumentNullException(nameof(width), nameof(width).ToArgumentEqualsZeroExceptionMessage());

            if (height <= 0)
                throw new ArgumentNullException(nameof(height), nameof(height).ToArgumentEqualsZeroExceptionMessage());

            //pad input to fit 8x8 blocks.
            var paddedInput = _paddingService.ApplyPadding(input, width, height);
            var paddedWidth = _paddingService.CalculatePaddedDimension(width);
            var paddedHeight = _paddingService.CalculatePaddedDimension(height);

            //split ycbcr data into blocks
            var result = CreateMCUs(paddedInput, paddedWidth, paddedHeight);

            //perform DCT calculation
            result = ApplyDCT(result, paddedWidth, paddedHeight);

            return result;
        }

        /// <summary>
        /// Quantizes input DCT data for all color components. 
        /// </summary>
        /// <param name="input">DCT data to quantisize.</param>
        /// <param name="chrominanceTable">Chrominance table used for red and blue color component. If null, will use default table.</param>
        /// <param name="luminanceTable">Luminance table used for Y color component. If null, will use default table.</param>
        /// <returns>DCTData object which contains quantized MCUs for all 3 color components.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public DCTData QuantizeDCT(DCTData input, byte[] chrominanceTable, byte[] luminanceTable)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), nameof(input).ToArgumentNullExceptionMessage());

            if (chrominanceTable == null)
                chrominanceTable = JpegStandardQuantizationTable.ChrominanceTable;

            if (luminanceTable == null)
                luminanceTable = JpegStandardQuantizationTable.LuminanceTable;

            var result = new DCTData();

            result.YDCTData = QuantizeDCTComponent(input.YDCTData, luminanceTable);
            result.CRDCTData = QuantizeDCTComponent(input.CRDCTData, chrominanceTable);
            result.CBDCTData = QuantizeDCTComponent(input.CBDCTData, chrominanceTable);

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
        private JpegBlock8x8F[] CreateMCUsForColorComponent(float[,] input, int width, int height)
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
        private JpegBlock8x8F CreateMinimumCodedUnit(float[,] input, int sWidth, int sHeight)
        {
            var result = new JpegBlock8x8F();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    result[j, i] = input[sHeight + i, sWidth + j];
                }
            }

            return result;
        }

        /// <summary>
        /// Quantize DCT data for 1 component. Divide dct element by the quantization element with the same index.
        /// </summary>
        /// <param name="input">DCT data for 1 color component.</param>
        /// <param name="quantizationTable">Quantization table.</param>
        /// <returns>Returns new DCTData object with quantized data.</returns>
        private JpegBlock8x8F[] QuantizeDCTComponent(JpegBlock8x8F[] input, byte[] quantizationTable)
        {
            var result = new JpegBlock8x8F[input.Length];

            for (int iBlock = 0; iBlock < input.Length; iBlock++)
            {
                QuantizeDCTBlock(input, quantizationTable, result, iBlock);
            }

            return result;
        }

        /// <summary>
        /// Quantize DCT block.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="quantizationTable"></param>
        /// <param name="result"></param>
        /// <param name="iBlock">Block index.</param>
        private static void QuantizeDCTBlock(JpegBlock8x8F[] input, byte[] quantizationTable, JpegBlock8x8F[] result, int iBlock)
        {
            JpegBlock8x8F tmp = input[iBlock];

            for (int iElement = 0; iElement < 64; iElement++)
            {
                tmp[iElement] = (float)Math.Round(tmp[iElement] / quantizationTable[iElement]);
            }

            result[iBlock] = tmp;
        }

        #endregion
    }
}
