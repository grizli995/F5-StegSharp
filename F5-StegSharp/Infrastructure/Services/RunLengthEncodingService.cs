﻿using StegSharp.Application.Common.Interfaces;
using StegSharp.Domain;
using JpegLibrary;

namespace StegSharp.Infrastructure.Services
{
    public class RunLengthEncodingService : IRunLengthEncodingService
    {
        /// <summary>
        /// Performs run length encoding on a single MCU. 
        /// </summary>
        /// <param name="block"></param>
        /// <returns>List of Tuple objects. Item1 is zeroCount, and Item2 is the value.</returns>
        public List<Tuple<int, int>> Encode(JpegBlock8x8F block)
        {
            var result = new List<Tuple<int, int>>();
            var zeroCount = 0;

            for (int i = 1; i < JpegSorting.JpegNaturalOrder.Length; i++)
            {
                var currentValue = (int)block[JpegSorting.JpegNaturalOrder[i]];

                if (currentValue == 0)
                {
                    zeroCount++;
                }
                else
                {
                    result.Add(new Tuple<int, int>(zeroCount, currentValue));
                    zeroCount = 0;
                }

                while (zeroCount > 15)
                {
                    result.Add(new Tuple<int, int>(15, 0));
                    zeroCount -= 16;
                }
            }

            result.Add(new Tuple<int, int>(0, 0));

            return result;
        }

        /// <summary>
        /// Performs run length decoding on a single MCU. 
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns>MCU object decoded from the input run length pairs.</returns>
        public JpegBlock8x8F Decode(List<Tuple<int, int>> pairs)
        {
            var result = new JpegBlock8x8F();
            var i = 1;

            foreach (var pair in pairs)
            {
                if (pair.Item1 == 0 && pair.Item2 == 0)
                {
                    for (i = i; i < 64; i++)
                    {
                        result[JpegSorting.JpegNaturalOrder[i]] = 0;
                    }
                }
                else
                {
                    for (int c = 0; c < pair.Item1; c++)
                    {
                        result[JpegSorting.JpegNaturalOrder[i]] = 0;
                        i++;
                    }

                    result[JpegSorting.JpegNaturalOrder[i]] = pair.Item2;
                    i++;
                }
            }

            return result;
        }

        #region Util

        private static void AddEndOfBlockMarker(List<Tuple<int, int>> result)
        {
            var i = result.Count - 1;

            while (i >= 0 && result[i].Item2 == 0)
            {
                result.RemoveAt(i);
                i--;
            }

            result.Add(new Tuple<int, int>(0, 0));
        }

        #endregion

    }
}
