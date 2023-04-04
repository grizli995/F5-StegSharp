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

        public F5Service(IColorTransformationService colorTransformationService, 
            IDCTService dCTService, 
            IEncodingOrchestratorService encodingOrchestratorService,
            IHeaderService headerService) 
        {
            this._colorTransformationService = colorTransformationService;
            this._dctService = dCTService;
            this._encodingOrchestratorService = encodingOrchestratorService;
            this._headerService = headerService;
        }

        public void Embed(Image image, string password, string text, BinaryWriter bw)
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

            //step 4 in jpeg compression - Run length encoding and huffman encoding.
            _encodingOrchestratorService.EncodeData(jpeg.QuantizedDCTData, bw);

            _headerService.WriteEOI(bw);
        }


        public void Extract()
        {
            throw new NotImplementedException();
        }

        private static JpegInfo CreateJpegInfo(Image image)
        {
            var jpeg = new JpegInfo();
            jpeg.Width = image.Width;
            jpeg.Height = image.Height;
            jpeg.Bitmap = (Bitmap)image;
            return jpeg;
        }
        //Color[,] originalRGB = new Color[bmp.Width, bmp.Height];
        //for (int i = 0; i < bmp.Height; i++)
        //{
        //    for (int j = 0; j < bmp.Width; j++)
        //    {
        //        originalRGB[j, i] = bmp.GetPixel(j, i);
        //    }
        //}
    }
}
