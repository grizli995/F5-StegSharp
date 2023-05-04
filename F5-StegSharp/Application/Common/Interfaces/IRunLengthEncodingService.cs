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

        /// <summary>
        /// Performs run length decoding on a single MCU. 
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns>MCU object decoded from the input run length pairs.</returns>
        public JpegBlock8x8F Decode(List<Tuple<int, int>> pairs);
    }
}
