using StegSharp.Application.Models;

namespace StegSharp.Application.Common.Interfaces
{
    public interface IEncodingOrchestratorService
    {
        /// <summary>
        /// Orchestrates both encodings required for JPEG compression. (Run-Length & Huffman encoding)
        /// </summary>
        /// <param name="quantizedDCTData">MCU data to encode</param>
        /// <param name="bw">BinaryWriter</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public void EncodeData(DCTData quantizedDCTData, BinaryWriter bw);

        /// <summary>
        /// Orchestrates both decodings required for JPEG compression. (Run-Length & Huffman decoding)
        /// </summary>
        /// <param name="jpeg">Jpeg information</param>
        /// <param name="bw">BinaryWriter</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public DCTData DecodeData(JpegInfo jpeg, BinaryReader bw);
    }
}
