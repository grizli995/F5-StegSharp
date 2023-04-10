using Application.Common.Interfaces;
using Domain;
using JpegLibrary;

namespace Infrastructure.Services
{
    public class RunLengthEncodingService : IRunLengthEncodingService
    {
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
