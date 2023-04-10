using Application.Common.Interfaces;
using Application.Models;
using JpegLibrary;

namespace Infrastructure.Services
{
    public class EncodingOrchestratorService : IEncodingOrchestratorService
    {
        private readonly IHuffmanEncodingService _huffmanEncodingService;

        public EncodingOrchestratorService(IHuffmanEncodingService hhuffmanEncodingService)
        {
            this._huffmanEncodingService = hhuffmanEncodingService;
        }

        public void EncodeData(DCTData quantizedDCTData, BinaryWriter bw)
        {
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

        private void EncodeMCUComponent(int prevDC, JpegBlock8x8F mcu, BinaryWriter bw, bool isLuminance)
        {
            if(isLuminance)
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
    }
}
