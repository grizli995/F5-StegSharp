using Application.Common.Interfaces;
using Infrastructure.Util;
using Infrastructure.Util.Extensions;
using JpegLibrary;
using System.Transactions;

namespace Infrastructure.Services
{
    internal class F5ParameterCalculatorService : IF5ParameterCalculatorService
    {
        private readonly IMCUConverterService _mcuConverterService;

        public F5ParameterCalculatorService(IMCUConverterService mcuConverterService)
        {
            this._mcuConverterService = mcuConverterService;
        }

        /// <summary>
        /// Calculates parameter N, based on parameter K.
        /// </summary>
        /// <param name="k">Parameter k used for matrix encoding.</param>
        /// <returns>Calculated value of N parameter.</returns>
        public int CalculateN(int k)
        {
            var n = (int)Math.Pow(2, k) - 1;
            return n;
        }

        /// <summary>
        /// Calculater parameter K of the matrix encoding. Calculates embedding rate and based on the predefined table, picks the value for k.
        /// </summary>
        /// <param name="mcus">MCU array</param>
        /// <param name="message">Message</param>
        /// <returns>Calculated value of K parameter.</returns>
        /// <exception cref="Exception"></exception>
        public int CalculateK(JpegBlock8x8F[] mcus, string message)
        {
            double messageBitLength = message.GetBitLength();
            int reservedBitsForMsgLen = 32;

            var coefficients = _mcuConverterService.MCUArrayToCoeffArray(mcus);

            var dcCoeffCount = coefficients.Length / 64;
            double availableCoefficientCount = coefficients.Where(item => item != 0 && item != 1 && item != -1).Count() - dcCoeffCount - reservedBitsForMsgLen;

            if (availableCoefficientCount <= 0)
            {
                throw new Exception("No space to store message");
            }

            var calculatedEmbeddingRate = messageBitLength / availableCoefficientCount;

            var embeddingRates = EmbeddingRateTable.Table.Select(item => item.EmbeddingRate).ToArray();
            var optimalEmbeddingRate = FindClosestValue(embeddingRates, calculatedEmbeddingRate);

            var k = EmbeddingRateTable.Table.Where(item => item.EmbeddingRate == optimalEmbeddingRate).Select(item => item.K).FirstOrDefault();
            return k;
        }

        #region Util

        private double FindClosestValue(double[] inputArray, double inputValue)
        {
            if (inputArray == null || inputArray.Length == 0)
            {
                throw new ArgumentException("Input array cannot be null or empty.");
            }

            var orderedInput = inputArray.OrderBy(item => item).ToArray();

            var i = 0;
            double result = 0;
            while (i < inputArray.Length)
            {
                if (orderedInput[i] > inputValue)
                {
                    result = orderedInput[i];
                    break;
                }
                i++;
            }

            return result;
        }

        #endregion

    }
}
