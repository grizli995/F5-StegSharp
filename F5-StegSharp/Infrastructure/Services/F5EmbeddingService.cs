using Application.Common.Interfaces;
using Application.Models;
using Infrastructure.Util.Extensions;
using System.Collections;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;

namespace Infrastructure.Services
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
        public DCTData Embed(DCTData quantizedData, string password, string message)
        {
            //step 1 - Convert dct data object to array of MCUs.
            var mcuArray = _mcuConverterService.DCTDataToMCUArray(quantizedData);

            //step 2 - Permute the order of MCUs in the mcuArray
            var permutatedMcuArray = _permutationService.PermutateArray(password, mcuArray, false);

            //step 3 - Calculate n and k
            var k = _f5ParameterCalculatorService.CalculateK(permutatedMcuArray, message);
            var n = _f5ParameterCalculatorService.CalculateN(k);

            var coeffs = _mcuConverterService.MCUArrayToCoeffArray(permutatedMcuArray);

            //step 4 - save k and msgLen in first 4 bytes. (k = 1b, msgLen = 3b)
            var lastModifiedIndex = EmbedDecodingInfo(coeffs, k, message);

            //step 5 - Embed data
            var embededCoeffData = EmbedMessage(coeffs, message, k, n, lastModifiedIndex);

            var permutatedEmbededMcuData = _mcuConverterService.CoeffArrayMCUArray(embededCoeffData);

            //step 6 - Reverse permutation to get the original order of MCUs.
            var embededMCUData = _permutationService.PermutateArray(password, permutatedEmbededMcuData, true);

            var result = _mcuConverterService.MCUArrayToDCTData(embededMCUData);

            return result;
        }

        private float[] EmbedMessage(float[] coeffs, string message, int k, int n, int lastModifiedIndex)
        {
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            var messageBitLength = message.GetBitLength();
            var index = lastModifiedIndex + 1;
            var counter = 0;

            Dictionary<int, Tuple<int,int>> coeffsToEmbed = new Dictionary<int, Tuple<int, int>>();
            var byteToEmbed = 0;
            var bitsToEmbed = 0;
            var availableBitsForEmbedding = 0;
            var messageByteIndex = 0;

            var coeffCount = 0;
            var hash = 0;
            var currentCoeff = 0;
            var shrinkedCoeffId = -1;
            while (counter < messageBitLength)
            {
                //get coefficients to embed the message bits in
                while (coeffCount < n)
                {
                    currentCoeff = (int)coeffs[index];
                    if (currentCoeff != 0 && ((index % 64) != 0))
                    {
                        var coeffToEmbed = new Tuple<int, int>(index, currentCoeff);
                        if(shrinkedCoeffId != -1)
                        {
                            coeffsToEmbed.Add(shrinkedCoeffId, coeffToEmbed);
                        }
                        else
                        {
                            coeffsToEmbed.Add(coeffCount, coeffToEmbed);

                        }
                        coeffCount++;
                    }
                    index++;
                }

                for(int i = 0; i < n; i++)
                {
                    //calculate hash
                    var coeffLsb = coeffsToEmbed[i].Item2 & 1;
                    if (coeffLsb == 1)
                        hash ^= coeffLsb + 1;
                }
 
                //get k bits to embed
                for(int i = 0; i < k; i++)
                {
                    if(availableBitsForEmbedding == 0)
                    {
                        if (messageByteIndex == messageBytes.Length)
                            break;

                        byteToEmbed = messageBytes[messageByteIndex];
                        availableBitsForEmbedding = 8;
                        messageByteIndex++;
                    }

                    var nextBitToEmbed = byteToEmbed & 1;
                    byteToEmbed = byteToEmbed >> 1;
                    availableBitsForEmbedding--;
                    bitsToEmbed |= nextBitToEmbed << i;
                }

                //calculate which coeff to change
                var coeffToChange = hash ^ bitsToEmbed;
                if (coeffToChange == 0)
                {
                    //No need to change coefficients - message bits are already there
                    counter = counter + k;
                    coeffsToEmbed = new Dictionary<int, Tuple<int, int>>();
                    coeffCount = 0;
                    hash = 0;
                    shrinkedCoeffId = -1;
                }
                else
                {
                    //need to change coefficients
                    coeffToChange -= 1;
                    var coeffValueToChange = coeffsToEmbed[coeffToChange].Item2;
                    var coeffIndexToChange = coeffsToEmbed[coeffToChange].Item1;

                    if (coeffValueToChange < 0)
                        coeffs[coeffIndexToChange]++;

                    if (coeffValueToChange > 0)
                        coeffs[coeffIndexToChange]--;


                    //Check for shrinkage
                    if (coeffs[coeffIndexToChange] == 0)
                    {
                        coeffCount--;
                        shrinkedCoeffId = coeffToChange;
                        coeffsToEmbed.Remove(coeffToChange);
                        //adjust keys of other coeffs. or not, maybe first find coeffs, then calc hash. In current implementation, if shrinkage occues, the hash value is old from the previous set of coeffs.
                    }
                    else
                    {
                        counter = counter + k;
                        coeffsToEmbed = new Dictionary<int, Tuple<int, int>>();
                        coeffCount = 0;
                        hash = 0;
                        shrinkedCoeffId = -1;
                    }
                }
            }

            return coeffs;
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
            var counter = 0;

            var byteToEmbed = PrepareDecodingInfo(k, messageBitLength);

            var bitsToEmbed = new BitArray(byteToEmbed);

            while (counter < 32)
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
                counter++;
            }

            return counter;
        }

        public int PrepareDecodingInfo(int paramK, int msgLen)
        {
            var k = (byte)paramK;

            // Convert byte "a" to an int and shift it to the left by 24 bits
            int kShifted = ((int)k) << 24;

            // Use bitwise AND to get the lower 24 bits of int "b"
            int msgLenMasked = msgLen & 0x00FFFFFF;

            // Use bitwise OR to combine the shifted byte "a" and masked int "b"
            int result = kShifted | msgLenMasked;

            return result;
        }
    }
}
