using JpegLibrary;
using System.IO;

namespace Application.Common.Interfaces
{
    public interface IHuffmanEncodingService
    {
        /// <summary>
        /// Encodes DC coefficient of the Chrominance component.
        /// </summary>
        /// <param name="dc">DC coefficient</param>
        /// <param name="prevDC">Previous DC coefficient</param>
        /// <param name="bw">BinaryWriter</param>
        void EncodeChrominanceDC(int dc, int prevDC, BinaryWriter bw);

        /// <summary>
        /// Encodes DC coefficient of the Luminance component.
        /// </summary>
        /// <param name="dc">DC coefficient</param>
        /// <param name="prevDC">Previous DC coefficient</param>
        /// <param name="bw">BinaryWriter</param>
        void EncodeLuminanceDC(int dc, int prevDC, BinaryWriter bw);

        /// <summary>
        /// Encodes AC coefficient of the Chrominance component.
        /// </summary>
        /// <param name="block">MCU containing all coefficients (AC & DC)</param>
        /// <param name="bw">BinaryWriter</param>
        void EncodeChrominanceAC(JpegBlock8x8F block, BinaryWriter bw);

        /// <summary>
        /// Encodes AC coefficient of the Luminance component.
        /// </summary>
        /// <param name="block">MCU containing all coefficients (AC & DC)</param>
        /// <param name="bw">BinaryWriter</param>
        void EncodeLuminanceAC(JpegBlock8x8F block, BinaryWriter bw);

        void FlushBuffer(BinaryWriter bw);
    }
}
