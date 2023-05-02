using Application.Common.Interfaces;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class F5Service : IF5Service
    {
        private readonly IColorTransformationService _colorTransformationService;
        private readonly IDCTService _dctService;
        private readonly IEncodingOrchestratorService _encodingOrchestratorService;
        private readonly IHeaderService _headerService;
        private readonly IF5EmbeddingService _embeddingService;
        private readonly IF5ExtractingService _extractingService;

        public F5Service(IColorTransformationService colorTransformationService, 
            IDCTService dCTService, 
            IEncodingOrchestratorService encodingOrchestratorService,
            IHeaderService headerService,
            IF5EmbeddingService embeddingService,
            IF5ExtractingService extractingService) 
        {
            this._colorTransformationService = colorTransformationService;
            this._dctService = dCTService;
            this._encodingOrchestratorService = encodingOrchestratorService;
            this._headerService = headerService;
            this._embeddingService = embeddingService;
            this._extractingService = extractingService;
        }

        public DCTData Embed(Image image, string password, string text, BinaryWriter bw)
        {
            //Create jpegInfo object
            JpegInfo jpeg = CreateJpegInfo(image);

            //step 0 - Create jfif headers
            _headerService.WriteHeaders(bw, jpeg);

            //Step 1 in jpeg compression - Transform image to YCBCR color space.
            jpeg.YCBCRData = _colorTransformationService.RGBToYCbCr(jpeg.Bitmap);

            //Step 2 in jpeg compression - Calculate DCT values.
            jpeg.DCTData = _dctService.CalculateDCT(jpeg.YCBCRData, jpeg.Width, jpeg.Height);

            //step 3 in jpeg compression - Quantize DCT values.
            jpeg.QuantizedDCTData = _dctService.QuantizeDCT(jpeg.DCTData, null, null);

            //step 3.5 - embedding the message
            jpeg.EmbededData = _embeddingService.Embed(jpeg.QuantizedDCTData, password, text);

            //step 4 in jpeg compression - Run length encoding and huffman encoding.
            _encodingOrchestratorService.EncodeData(jpeg.EmbededData, bw);

            _headerService.WriteEOI(bw);

            bw.Close();


            //TODO remove this.
            return jpeg.QuantizedDCTData;
        }

        public string Extract(string password, BinaryReader br)
        {
            var jpeg = new JpegInfo();
            _headerService.ParseJpegMarkers(br, jpeg);
            var quantizedDctData = _encodingOrchestratorService.DecodeData(jpeg, br);
            var message = _extractingService.Extract(quantizedDctData, password);
            return string.Empty;
        }

        public DCTData ExtractDCT(string password, BinaryReader br)
        {
            var jpeg = new JpegInfo();
            _headerService.ParseJpegMarkers(br, jpeg);
            var result = _encodingOrchestratorService.DecodeData(jpeg, br);
            var message = _extractingService.Extract(result, password);
            return result;
        }

        #region Util

        private static JpegInfo CreateJpegInfo(Image image)
        {
            var jpeg = new JpegInfo();
            jpeg.Width = image.Width;
            jpeg.Height = image.Height;
            jpeg.Bitmap = (Bitmap)image;
            return jpeg;
        }

        #endregion
    }
}
