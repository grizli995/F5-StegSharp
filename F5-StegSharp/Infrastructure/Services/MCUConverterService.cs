﻿using Application.Common.Interfaces;
using Application.Models;
using JpegLibrary;

namespace Infrastructure.Services
{
    public class MCUConverterService : IMCUConverterService
    {
        /// <summary>
        /// Combines MCUs of all components in one MCU array. Order of MCUs is Y(x), CB(x), CR(x). Where x=0 -> mcuCount.
        /// </summary>
        /// <param name="dct"></param>
        /// <returns>MCU Array</returns>
        public JpegBlock8x8F[] DCTDataToMCUArray(DCTData dct)
        {
            var numberOfComponents = 3;
            var mcuPerComponentCount = dct.YDCTData.Length;
            JpegBlock8x8F[] result = new JpegBlock8x8F[mcuPerComponentCount * numberOfComponents];
            var index = 0;

            for (int i = 0; i < mcuPerComponentCount; i++)
            {
                result[index++] = dct.YDCTData[i];
                result[index++] = dct.CBDCTData[i];
                result[index++] = dct.CRDCTData[i];
            }

            return result;
        }

        /// <summary>
        /// Converts array od MCU objects to an array of coefficients.
        /// </summary>
        /// <param name="dctArray">MCU array</param>
        /// <returns>Coefficient Array</returns>
        public float[] MCUArrayToCoeffArray(JpegBlock8x8F[] dctArray)
        {
            var coeffPerMcu = 64;
            var totalMcuCount = dctArray.Length;
            float[] result = new float[totalMcuCount * coeffPerMcu];
            var index = 0;

            for (int i = 0; i < totalMcuCount; i++)
            {
                var mcu = dctArray[i];
                for (int j = 0; j < coeffPerMcu; j++)
                {
                    result[index++] = mcu[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Converts array of consecutive MCUs into DCTData object.
        /// </summary>
        /// <param name="mcuArray">MCU Array</param>
        /// <returns>DCTData object</returns>
        public DCTData MCUArrayToDCTData(JpegBlock8x8F[] mcuArray)
        {
            var numberOfComponents = 3;
            var mcuPerComponentCount = mcuArray.Length / numberOfComponents;
            DCTData result = new DCTData(mcuPerComponentCount);
            var index = 0;

            for (int i = 0; i < mcuPerComponentCount; i++)
            {
                result.YDCTData[i] = mcuArray[index++];
                result.CBDCTData[i] = mcuArray[index++];
                result.CRDCTData[i] = mcuArray[index++];
            }

            return result;
        }

        /// <summary>
        /// Converts array od coefficients into an array of consecutive MCUs.
        /// </summary>
        /// <param name="coeffArray">Coefficient array</param>
        /// <returns>MCU Array</returns>
        public JpegBlock8x8F[] CoeffArrayMCUArray(float[] coeffArray)
        {
            var coeffPerMcu = 64;
            var totalMcuCount = coeffArray.Length / coeffPerMcu;
            JpegBlock8x8F[] result = new JpegBlock8x8F[totalMcuCount];
            var index = 0;

            for (int i = 0; i < totalMcuCount; i++)
            {
                var mcu = new JpegBlock8x8F();
                for (int j = 0; j < coeffPerMcu; j++)
                {
                    mcu[j] = coeffArray[index++];
                }
                result[i] = mcu;
            }

            return result;
        }
    }
}