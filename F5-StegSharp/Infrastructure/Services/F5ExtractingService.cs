using Application.Common.Interfaces;
using Application.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class F5ExtractingService : IF5ExtractingService
    {
        private readonly IPermutationService _permutationService;
        private readonly IMCUConverterService _mcuConverterService;
        private readonly IF5ParameterCalculatorService _f5ParameterCalulatorService;

        public F5ExtractingService(IPermutationService permutationService,
            IMCUConverterService mcuConverterSservice,
            IF5ParameterCalculatorService f5ParameterCalulatorService)
        {
            this._permutationService = permutationService;
            this._mcuConverterService = mcuConverterSservice;
            this._f5ParameterCalulatorService = f5ParameterCalulatorService;
        }

        public string Extract(DCTData dctData, string password)
        {
            //conver to mcu array
            var mcuArray = _mcuConverterService.DCTDataToMCUArray(dctData);

            //permutate mcu array
            var permutatedMCUArray = _permutationService.PermutateArray(password, mcuArray, false);

            //convert to coeff array
            var coeffs = _mcuConverterService.MCUArrayToCoeffArray(permutatedMCUArray);

            //read first 32 bits, 8 for k 24 for length
            int k, msgLen;
            var currentIndex = GetDecodingInfo(coeffs, out k, out msgLen);
            var n = _f5ParameterCalulatorService.CalculateN(k);

            //read untill end of message, take n coeffs, calculate hash to get the k bit value

            //combine extracted values
            var result = ReadEmbeddedMessage(coeffs, k, n, msgLen, currentIndex);
            return result;
        }

        private int GetDecodingInfo(float[] coeffs, out int k, out int msgLen)
        {
            var index = 0;
            var bitsRead = 0;
            var decodeDataBitLength = 32;

            int readValue = 0;

            while (bitsRead < decodeDataBitLength)
            {
                var coeff = (int)coeffs[index];

                if (coeff != 0 && ((index % 64) != 0))
                {
                    var bit = ReadDecodingInfoBitFromCoeff(coeff);
                    readValue = readValue << 1;
                    readValue = readValue | bit;
                    bitsRead++;
                }
                index++;
            }

            ExtractDecodedData(readValue, out k, out msgLen);

            return index;
        }

        private int ReadDecodingInfoBitFromCoeff(int coeff)
        {
            if ((coeff < 0 && (coeff % 2 == 0)) || (coeff > 0 && (coeff % 2 == 1)))
                return 1;
            else return 0;
        }

        public static void ExtractDecodedData(int input, out int upperByte, out int lowerInt)
        {
            var value = (UInt32)(input);
            // Extract the upper byte by shifting the input right by 24 bits and casting to byte.
            upperByte = (byte)(value >> 24);

            // Extract the lower 24 bits by masking the input with 0xFFFFFF.
            lowerInt = (int)(value & 0xFFFFFF);
        }

        private string ReadEmbeddedMessage(float[] coeffs, int k, int n, int msgLen, int lastReadIndex)
        {
            var counter = 0;
            var coeffCount = 0;
            var index = lastReadIndex + 1;
            var hash = 0;

            int[] coeffsToRead = new int[n];
            var messageBytes = new byte[msgLen / 8];
            var messageByteIndex = 0;
            var messageBitIndex = 0;

            while (counter < msgLen)
            {
                //get n coeffs
                while (coeffCount < n)
                {
                    var coeff = (int)coeffs[index];
                    if (coeff != 0 && ((index % 64) != 0))
                    {
                        coeffsToRead[coeffCount] = coeff;
                        coeffCount++;
                    }
                    index++;
                }

                //calculate hash
                for (int i = 0; i < n; i++)
                {
                    var coeffToRead = coeffsToRead[i];
                    var coeffLsb = coeffToRead > 0 ?
                        coeffToRead & 1 :
                        (1 - (coeffToRead & 1));

                    if (coeffLsb == 1)
                        hash ^= i + 1;
                }

                for(int i = k - 1; i >= 0; i--)
                {
                    var bitToExtract = (hash >> i) & 1;


                    if(messageBitIndex == 8)
                    {
                        messageBitIndex = 0;
                        messageByteIndex++;
                    }

                    messageBytes[messageByteIndex] = (byte)(messageBytes[messageByteIndex] << 1);

                    messageBytes[messageByteIndex] = (byte)(messageBytes[messageByteIndex] | (byte)bitToExtract);
                    messageBitIndex++;
                }

                counter = counter + k;
                hash = 0;
                coeffCount = 0;
                coeffsToRead = new int[n];
                //combine hash to result value
            }

            var result = Encoding.UTF8.GetString(messageBytes);
            return result;
        }
    }
}
