﻿using Application.Common.Interfaces;
using JpegLibrary;

namespace Infrastructure.Services
{
    internal class PermutationService : IPermutationService
    {
        /// <summary>
        /// Pseudo-randomly permutates elements of the input MCU array, based on the provided password. 
        /// Can be used to permutate the MCU array back, to original sequence, if "reverse" is set to true.
        /// </summary>
        /// <param name="password">Password used to seed the pseudo-random number generator.</param>
        /// <param name="inputArray">Input mcu array</param>
        /// <param name="reverse">If false permutate, if true permutate in the reverse order.</param>
        /// <returns>Permutated mcu array.</returns>
        public JpegBlock8x8F[] PermutateArray(string password, JpegBlock8x8F[] inputArray, bool reverse)
        {
            var mcuCount = inputArray.Length - 1;
            JpegBlock8x8F[] permutedArray = (JpegBlock8x8F[])inputArray.Clone();

            var permutationSequence = GetPermutationSequence(password, mcuCount);

            //permutate
            if (reverse)
            {
                for (int i = mcuCount; i >= 0; i++)
                {
                    SwapElements(permutedArray, permutationSequence, i);
                }
            }
            else
            {
                for (int i = 0; i < mcuCount; i++)
                {
                    SwapElements(permutedArray, permutationSequence, i);
                }
            }

            return permutedArray;
        }

        private void SwapElements(JpegBlock8x8F[] permutedArray, Dictionary<int, int> permutationSequence, int i)
        {
            int randomIndex = permutationSequence[i];

            JpegBlock8x8F temp = permutedArray[i];
            permutedArray[i] = permutedArray[randomIndex];
            permutedArray[randomIndex] = temp;
        }

        private Dictionary<int, int> GetPermutationSequence(string password, int mcuCount)
        {
            var permutationSequence = new Dictionary<int, int>();
            var rng = new Random(password.GetHashCode());
            for (int i = 0; i < mcuCount; i++)
            {
                var randomIndex = rng.Next(0, mcuCount);
                permutationSequence.Add(i, randomIndex);
            }

            return permutationSequence;
        }
    }
}