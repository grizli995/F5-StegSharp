using MethodTimer;
using StegSharp.Application.Common.Interfaces;
using StegSharp.Application.Models;
using StegSharp.Infrastructure.Util.Extensions;
using System.Collections;

namespace StegSharp.Infrastructure.Services
{
    public class F5EmbeddingService : IF5EmbeddingService
    {
        private readonly IMCUConverterService _mcuConverterService;
        private readonly IPermutationService _permutationService;
        private readonly IF5ParameterCalculatorService _f5ParameterCalculatorService;

        public F5EmbeddingService(IMCUConverterService mcuConverterService,
            IPermutationService permutationService,
            IF5ParameterCalculatorService f5ParameterCalculatorService)
        {
            this._mcuConverterService = mcuConverterService;
            this._permutationService = permutationService;
            this._f5ParameterCalculatorService = f5ParameterCalculatorService;
        }

        /// <summary>
        /// Embeds a message inside the provided quantized DCTs.
        /// </summary>
        /// <param name="quantizedData">Quantized DCT data</param>
        /// <param name="password">Password used for pseudo-random number generator.</param>
        /// <param name="message">Message to embed.</param>
        /// <returns>DCTData object with modified values based on the embedded message. </returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        [Time]
        public DCTData Embed(DCTData quantizedData, string password, string message)
        {
            if (quantizedData == null || quantizedData.YDCTData.Length <= 0)
                throw new ArgumentNullException(nameof(quantizedData), nameof(quantizedData).ToArgumentNullExceptionMessage());

            if (String.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), nameof(password).ToArgumentNullExceptionMessage());

            if (String.IsNullOrEmpty(message))
                return quantizedData;

            //step 1 - Convert dct data object to array of MCUs.
            var mcuArray = _mcuConverterService.DCTDataToMCUArray(quantizedData);

            //step 2 - Permute the order of MCUs in the mcuArray
            var permutatedMcuArray = _permutationService.PermutateArray(password, mcuArray, false);

            //step 3 - Calculate n and k
            var k = _f5ParameterCalculatorService.CalculateK(permutatedMcuArray, message);
            var n = _f5ParameterCalculatorService.CalculateN(k);

            //step 4 - Convert permutated MCU array into coefficient array.
            var coeffs = _mcuConverterService.MCUArrayToCoeffArray(permutatedMcuArray);

            //step 5 - save k and msgLen in first 4 bytes. (k = 1b, msgLen = 3b)
            var lastModifiedIndex = EmbedDecodingInfo(coeffs, k, message);

            //step 6 - Embed data
            var embededCoeffData = EmbedMessage(coeffs, message, k, n, lastModifiedIndex);

            //step 7 - convert back coefficient array to MCU array
            var permutatedEmbededMcuData = _mcuConverterService.CoeffArrayMCUArray(embededCoeffData);

            //step 8 - Reverse permutation to get the original order of MCUs.
            var embededMCUData = _permutationService.PermutateArray(password, permutatedEmbededMcuData, true);

            var result = _mcuConverterService.MCUArrayToDCTData(embededMCUData);

            return result;
        }

        #region Util

        [Time]
        private float[] EmbedMessage(float[] coeffs, string message, int k, int n, int lastModifiedIndex)
        {
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            var messageBitLength = message.GetBitLength();
            var index = lastModifiedIndex + 1;
            var counter = 0;
            Dictionary<int, Tuple<int, int>> coeffsToEmbed = new Dictionary<int, Tuple<int, int>>();
            var byteToEmbed = 0;
            var bitsToEmbed = 0;
            var availableBitsForEmbedding = 0;
            var messageByteIndex = 0;
            var coeffCount = 0;
            var shrinkageOccured = false;

            while (counter < messageBitLength)
            {
                //get coefficients to embed the message bits in
                coeffsToEmbed = GetCoefficients(coeffs, coeffsToEmbed, n, ref index, ref coeffCount);

                //calculate hash
                var hash = CalculateHash(n, coeffsToEmbed);

                //get k bits to embed
                bitsToEmbed = GetBitsToEmbed(k, shrinkageOccured, messageBytes, bitsToEmbed, ref byteToEmbed, ref availableBitsForEmbedding, ref messageByteIndex);

                //calculate which coeff to change
                var coeffToChange = hash ^ bitsToEmbed;

                if (coeffToChange == 0)
                {
                    //No need to change coefficients - message bits are already there
                    counter = ResetCounterAndCoefficients(k, counter, out coeffsToEmbed, out coeffCount, out shrinkageOccured);
                }
                else
                {
                    //need to change coefficient value
                    coeffToChange -= 1;
                    var coeffIndexToChange = coeffsToEmbed[coeffToChange].Item1;
                    ModifyCoefficient(coeffs, coeffsToEmbed, coeffToChange, coeffIndexToChange);

                    //Check for shrinkage
                    if (coeffs[coeffIndexToChange] == 0)
                    {
                        coeffCount--;
                        shrinkageOccured = true;

                        //re-adjust coeffsToEmbed dictionary. 
                        ReorderCoeffsToEmbed(n, coeffsToEmbed, coeffToChange);
                    }
                    else
                    {
                        counter = ResetCounterAndCoefficients(k, counter, out coeffsToEmbed, out coeffCount, out shrinkageOccured);
                    }
                }
            }

            return coeffs;
        }

        private int ResetCounterAndCoefficients(int k, int counter, out Dictionary<int, Tuple<int, int>> coeffsToEmbed, out int coeffCount, out bool shrinkageOccured)
        {
            counter = counter + k;
            coeffsToEmbed = new Dictionary<int, Tuple<int, int>>();
            coeffCount = 0;
            shrinkageOccured = false;
            return counter;
        }

        private void ReorderCoeffsToEmbed(int n, Dictionary<int, Tuple<int, int>> coeffsToEmbed, int coeffToChange)
        {
            var c = coeffToChange;
            while (c < n - 1)
            {
                coeffsToEmbed.Remove(c);
                coeffsToEmbed.Add(c, coeffsToEmbed[c + 1]);
                c++;
            }
            coeffsToEmbed.Remove(n - 1);
        }

        private void ModifyCoefficient(float[] coeffs, Dictionary<int, Tuple<int, int>> coeffsToEmbed, int coeffToChange, int coeffIndexToChange)
        {
            var coeffValueToChange = coeffsToEmbed[coeffToChange].Item2;
            if (coeffValueToChange < 0)
                coeffs[coeffIndexToChange]++;

            if (coeffValueToChange > 0)
                coeffs[coeffIndexToChange]--;
        }

        private int GetBitsToEmbed(int k, bool shrinkageOccured, byte[] messageBytes, int bitsToEmbed, ref int byteToEmbed, ref int availableBitsForEmbedding, ref int messageByteIndex)
        {
            if (!shrinkageOccured)
            {
                //if no shrinkage, get new bits to embed, else bits to embed stay the same.
                bitsToEmbed = 0;
                for (int i = 0; i < k; i++)
                {
                    if (availableBitsForEmbedding == 0)
                    {
                        if (messageByteIndex == messageBytes.Length)
                            break;

                        byteToEmbed = messageBytes[messageByteIndex];
                        availableBitsForEmbedding = 8;
                        messageByteIndex++;
                    }

                    var nextBitToEmbed = (byteToEmbed >> (availableBitsForEmbedding - 1)) & 1;
                    availableBitsForEmbedding--;
                    bitsToEmbed = bitsToEmbed << 1;
                    bitsToEmbed |= nextBitToEmbed;
                }
            }

            return bitsToEmbed;
        }

        private int CalculateHash(int n, Dictionary<int, Tuple<int, int>> coeffsToEmbed)
        {
            int hash = 0;

            for (int i = 0; i < n; i++)
            {
                var coeffToEmbed = coeffsToEmbed[i].Item2;
                var coeffLsb = coeffToEmbed > 0 ?
                    coeffToEmbed & 1 :
                    (1 - (coeffToEmbed & 1));

                if (coeffLsb == 1)
                    hash ^= i + 1;
            }

            return hash;
        }

        private Dictionary<int, Tuple<int, int>> GetCoefficients(float[] coeffs, Dictionary<int, Tuple<int, int>> coeffsToEmbed, int n, ref int index, ref int coeffCount)
        {
            int currentCoeff;

            while (coeffCount < n)
            {
                currentCoeff = (int)coeffs[index];
                if (currentCoeff != 0 && ((index % 64) != 0))
                {
                    AddCoefficientToArray(index, coeffsToEmbed, coeffCount, currentCoeff);
                    coeffCount++;
                }
                index++;
            }

            return coeffsToEmbed;
        }

        private void AddCoefficientToArray(int index, Dictionary<int, Tuple<int, int>> coeffsToEmbed, int coeffCount, int currentCoeff)
        {
            var coeffToEmbed = new Tuple<int, int>(index, currentCoeff);

            coeffsToEmbed.Add(coeffCount, coeffToEmbed);
        }

        /// <summary>
        /// Encodes K parameter and msgLen into coefficient array. 
        /// Accounts for shrinkage, and returns the coeff index which was last modified.
        /// </summary>
        /// <param name="coeffs">List of all coefficients.</param>
        /// <param name="k">Parameter k</param>
        /// <param name="message">Message</param>
        /// <returns>Coeff index which was last modified</returns>
        private int EmbedDecodingInfo(float[] coeffs, int k, string message)
        {
            var messageBitLength = message.GetBitLength();
            var index = 0;
            var counter = 31;

            var decodingInfo = PrepareDecodingInfo(k, messageBitLength);

            var bytesToEmbed = BitConverter.GetBytes(decodingInfo);
            var bitsToEmbed = new BitArray(bytesToEmbed);

            while (counter >= 0)
            {
                var coeff = (int)coeffs[index];
                var bitToEmbed = Convert.ToInt32(bitsToEmbed[counter]);

                if (index % 64 != 0 && coeff != 0)
                {
                    counter = EmbedDecodingInfoBit(coeffs, index, counter, coeff, bitToEmbed);
                }

                index++;
            }

            return index;
        }

        private int EmbedDecodingInfoBit(float[] coeffs, int index, int counter, int coeff, int bitToEmbed)
        {
            if (coeff > 0 && (coeff & 1) != bitToEmbed)
            {
                coeffs[index]--;
            }
            else if (coeff < 0 && (coeff & 1) == bitToEmbed)
            {
                coeffs[index]++;
            }

            if (coeffs[index] != 0)
            {
                counter--;
            }

            return counter;
        }

        public int PrepareDecodingInfo(int paramK, int msgLen)
        {
            var k = (byte)paramK;

            int kShifted = ((int)k) << 24;

            int msgLenMasked = msgLen & 0x00FFFFFF;

            int result = kShifted | msgLenMasked;

            return result;
        }

        #endregion

    }
}
