using Application.Models;

namespace Application.Common.Interfaces
{
    public interface IEncodingOrchestratorService
    {
        /// <summary>
        /// Orchestrates the encoding process for all MCUs. Covers both run length encoding and huffman encoding.
        /// </summary>
        /// <param name="quantizedDCTData"></param>
        /// <param name="bw"></param>
        public void EncodeData(DCTData quantizedDCTData, BinaryWriter bw);

        /// <summary>
        /// Orchestrates the decoding process for all MCUs. Covers both run length decoding and huffman decoding.
        /// </summary>
        /// <param name="quantizedDCTData"></param>
        /// <param name="bw"></param>
        public DCTData DecodeData(JpegInfo jpeg, BinaryReader bw);
    }
}
