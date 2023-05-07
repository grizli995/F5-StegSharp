using JpegLibrary;

namespace StegSharp.Application.Common.Interfaces
{
    public interface IHuffmanDecodingService
    {
        /// <summary>
        /// Reads and recodes chrominance DC from the binary reader.
        /// </summary>
        /// <param name="prevDC"></param>
        /// <param name="bw"></param>
        /// <returns></returns>
        int DecodeChrominanceDC(int prevDC, BinaryReader bw);

        /// <summary>
        /// Reads and recodes luminance DC from the binary reader.
        /// </summary>
        /// <param name="prevDC"></param>
        /// <param name="bw"></param>
        /// <returns></returns>
        int DecodeLuminanceDC(int prevDC, BinaryReader bw);

        /// <summary>
        /// Reads and recodes chrominance AC from the binary reader.
        /// </summary>
        /// <param name="bw"></param>
        /// <returns></returns>
        JpegBlock8x8F DecodeChrominanceAC(BinaryReader bw);

        /// <summary>
        /// Reads and recodes luminance AC from the binary reader.
        /// </summary>
        /// <param name="bw"></param>
        /// <returns></returns>
        JpegBlock8x8F DecodeLuminanceAC(BinaryReader bw);
    }
}
