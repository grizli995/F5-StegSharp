using Application.Common.Interfaces;
using Application.Models;
using System.Text;

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
            //step 1 - Convert dctData object to mcu array
            var mcuArray = _mcuConverterService.DCTDataToMCUArray(dctData);

            //step 2 - Permutate mcu array
            var permutatedMCUArray = _permutationService.PermutateArray(password, mcuArray, false);

            //step 3 - Convert permutated MCU array to coeff array
            var coeffs = _mcuConverterService.MCUArrayToCoeffArray(permutatedMCUArray);

            //step 4 - Read decoding info (k and msgLen) and calculate n.
            //read first 32 bits, 8 for k 24 for length
            int k, msgLen;
            var currentIndex = ReadDecodingInfo(coeffs, out k, out msgLen);
            var n = _f5ParameterCalulatorService.CalculateN(k);

            //step 5 - Read embedded message 
            var result = ReadEmbeddedMessage(coeffs, k, n, msgLen, currentIndex);

            return result;
        }

        #region Util

        private int ReadDecodingInfo(float[] coeffs, out int k, out int msgLen)
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

        public void ExtractDecodedData(int input, out int upperByte, out int lowerInt)
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

            var messageBytes = new byte[msgLen / 8];
            var messageByteIndex = 0;
            var messageBitIndex = 0;

            while (counter < msgLen)
            {
                //get n coeffs
                var coeffsToRead = GetCoefficients(coeffs, n, ref coeffCount, ref index);

                //calculate hash
                var hash = CalculateHash(n, coeffsToRead);

                ExtractMessageBits(k, messageBytes, ref messageByteIndex, ref messageBitIndex, hash);

                counter = counter + k;
                coeffCount = 0;
            }

            var result = Encoding.UTF8.GetString(messageBytes);
            return result;
        }

        private void ExtractMessageBits(int k, byte[] messageBytes, ref int messageByteIndex, ref int messageBitIndex, int hash)
        {
            for (int i = k - 1; i >= 0; i--)
            {
                var bitToExtract = (hash >> i) & 1;

                if (messageBitIndex == 8)
                {
                    messageBitIndex = 0;
                    messageByteIndex++;
                }

                //combine bitToExtract to result value
                messageBytes[messageByteIndex] = (byte)(messageBytes[messageByteIndex] << 1);

                messageBytes[messageByteIndex] = (byte)(messageBytes[messageByteIndex] | (byte)bitToExtract);
                messageBitIndex++;
            }
        }

        private int CalculateHash(int n, int[] coeffsToRead)
        {
            int hash = 0;

            for (int i = 0; i < n; i++)
            {
                var coeffToRead = coeffsToRead[i];
                var coeffLsb = coeffToRead > 0 ?
                    coeffToRead & 1 :
                    (1 - (coeffToRead & 1));

                if (coeffLsb == 1)
                    hash ^= i + 1;
            }

            return hash;
        }

        private int[] GetCoefficients(float[] coeffs, int n, ref int coeffCount, ref int index)
        {
            int[] coeffsToRead = new int[n];

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

            return coeffsToRead;
        }

        #endregion
    }
}
