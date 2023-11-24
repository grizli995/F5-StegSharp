using StegSharp.Application.Common.Interfaces;
using StegSharp.Application.Models;
using StegSharp.Infrastructure.Util.Extensions;
using JpegLibrary;
using MethodTimer;

namespace StegSharp.Infrastructure.Services
{
    public class EncodingOrchestratorService : IEncodingOrchestratorService
    {
        private readonly IHuffmanEncodingService _huffmanEncodingService;
        private readonly IHuffmanDecodingService _huffmanDecodingService;
        private readonly IPaddingService _paddingService;

        public EncodingOrchestratorService(IHuffmanEncodingService hhuffmanEncodingService,
            IHuffmanDecodingService huffmanDecodingService,
            IPaddingService paddingService)
        {
            this._huffmanEncodingService = hhuffmanEncodingService;
            this._paddingService = paddingService;
            this._huffmanDecodingService = huffmanDecodingService;
        }

        /// <summary>
        /// Orchestrates both encodings required for JPEG compression. (Run-Length & Huffman encoding)
        /// </summary>
        /// <param name="quantizedDCTData">MCU data to encode</param>
        /// <param name="bw">BinaryWriter</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        [Time]
        public void EncodeData(DCTData quantizedDCTData, BinaryWriter bw)
        {
            if (quantizedDCTData == null)
                throw new ArgumentNullException(nameof(quantizedDCTData), nameof(quantizedDCTData).ToArgumentNullExceptionMessage());

            if (bw == null)
                throw new ArgumentNullException(nameof(bw), nameof(bw).ToArgumentNullExceptionMessage());

            var mcuCount = quantizedDCTData.YDCTData.Length;
            var prevDc_Y = 0;
            var prevDc_Cr = 0;
            var prevDc_Cb = 0;

            for (int i = 0; i < mcuCount; i++)
            {
                var yMCU = quantizedDCTData.YDCTData[i];
                var crMCU = quantizedDCTData.CRDCTData[i];
                var cbMCU = quantizedDCTData.CBDCTData[i];

                EncodeMCUComponent(prevDc_Y, yMCU, bw, true);
                EncodeMCUComponent(prevDc_Cb, cbMCU, bw, false);
                EncodeMCUComponent(prevDc_Cr, crMCU, bw, false);

                prevDc_Y = (int)yMCU[0];
                prevDc_Cr = (int)crMCU[0];
                prevDc_Cb = (int)cbMCU[0];
            }

            _huffmanEncodingService.FlushBuffer(bw);
        }

        /// <summary>
        /// Orchestrates both decodings required for JPEG compression. (Run-Length & Huffman decoding)
        /// </summary>
        /// <param name="jpeg">Jpeg information</param>
        /// <param name="bw">BinaryWriter</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        [Time]
        public DCTData DecodeData(JpegInfo jpeg, BinaryReader br)
        {
            if (jpeg == null)
                throw new ArgumentNullException(nameof(jpeg), nameof(jpeg).ToArgumentNullExceptionMessage());

            if (br == null)
                throw new ArgumentNullException(nameof(br), nameof(br).ToArgumentNullExceptionMessage());

            int mcuCount = CalculateMCUCount(jpeg);
            var result = new DCTData(mcuCount);

            var prevDc_Y = 0;
            var prevDc_Cr = 0;
            var prevDc_Cb = 0;

            for (int i = 0; i < mcuCount; i++)
            {
                result.YDCTData[i] = DecodeMCUComponent(br, prevDc_Y, true);
                result.CBDCTData[i] = DecodeMCUComponent(br, prevDc_Cb, false);
                result.CRDCTData[i] = DecodeMCUComponent(br, prevDc_Cr, false);

                prevDc_Y = (int)result.YDCTData[i][0];
                prevDc_Cb = (int)result.CBDCTData[i][0];
                prevDc_Cr = (int)result.CRDCTData[i][0];
            }

            _huffmanDecodingService.ResetBitReader();

            return result;
        }

        #region

        private void EncodeMCUComponent(int prevDC, JpegBlock8x8F mcu, BinaryWriter bw, bool isLuminance)
        {
            if (isLuminance)
            {
                _huffmanEncodingService.EncodeLuminanceDC((int)mcu[0], prevDC, bw);
                _huffmanEncodingService.EncodeLuminanceAC(mcu, bw);
            }
            else
            {
                _huffmanEncodingService.EncodeChrominanceDC((int)mcu[0], prevDC, bw);
                _huffmanEncodingService.EncodeChrominanceAC(mcu, bw);
            }
        }

        private int CalculateMCUCount(JpegInfo jpeg)
        {
            int paddedHeight = _paddingService.CalculatePaddedDimension(jpeg.Height);
            int paddedWidth = _paddingService.CalculatePaddedDimension(jpeg.Width);
            var mcuCount = paddedHeight * paddedWidth / 64;
            return mcuCount;
        }

        private JpegBlock8x8F DecodeMCUComponent(BinaryReader br, int prevDc, bool isLuminance)
        {
            int dc;
            JpegBlock8x8F result;
            if (isLuminance)
            {
                dc = _huffmanDecodingService.DecodeLuminanceDC(prevDc, br);
                result = _huffmanDecodingService.DecodeLuminanceAC(br);
            }
            else
            {
                dc = _huffmanDecodingService.DecodeChrominanceDC(prevDc, br);
                result = _huffmanDecodingService.DecodeChrominanceAC(br);
            }

            result[0] = dc;
            return result;
        }

        #endregion
    }
}
