using Application.Models;

namespace Application.Common.Interfaces
{
    public interface IHeaderService
    {
        /// <summary>
        /// Writes required JPEG headers to the binary writer. Supports baseline jpegs, with no subsampling.
        /// </summary>
        /// <param name="bw">Binary writer</param>
        /// <param name="jpeg">Jpeg Information</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        void WriteHeaders(BinaryWriter bw, JpegInfo jpeg);

        /// <summary>
        /// Writes "End Of Image" markers and data.
        /// </summary>
        /// <param name="bw"></param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        void WriteEOI(BinaryWriter bw);

        /// <summary>
        /// Reads jpeg byte by byte, untill the start of the entropy coded segment.
        /// </summary>
        /// <param name="br">Binary Reader</param>
        /// <param name="jpeg">Jpeg object where we will save parsed data.</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        void ParseJpegMarkers(BinaryReader br, JpegInfo jpeg);
    }
}
