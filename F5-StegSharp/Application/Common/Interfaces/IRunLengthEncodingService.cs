using JpegLibrary;

namespace Application.Common.Interfaces
{
    public interface IRunLengthEncodingService
    {
        /// <summary>
        /// Performs run length encoding on a single MCU. 
        /// </summary>
        /// <param name="block"></param>
        /// <returns>List of Tuple objects. Item1 is zeroCount, and Item2 is the value.</returns>
        public List<Tuple<int, int>> Encode(JpegBlock8x8F block);
    }
}
