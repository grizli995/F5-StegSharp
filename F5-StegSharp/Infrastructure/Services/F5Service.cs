using StegSharp.Application.Common.Interfaces;
using StegSharp.Application.Models;
using StegSharp.Infrastructure.Util.Extensions;
using System.Drawing;

namespace StegSharp.Infrastructure.Services
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

        /// <summary>
        /// Embeds the input message with the provided password using F5 algorithm, and writes jpeg image as output inside the provided BinaryWriter.
        /// </summary>
        /// <param name="image">Image to embed the message in.</param>
        /// <param name="password">Password used for embedding the message.</param>
        /// <param name="message">Message to embed.</param>
        /// <param name="bw">BinaryWriter where the jpeg will be written.</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public void Embed(Image image, string password, string message, BinaryWriter bw)
        {
            //Validate inputs and create jpegInfo object
            if (image == null)
                throw new ArgumentNullException(nameof(image), nameof(image).ToArgumentNullExceptionMessage());

            if (bw == null)
                throw new ArgumentNullException(nameof(bw), nameof(bw).ToArgumentNullExceptionMessage());

            if (String.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), nameof(password).ToArgumentNullExceptionMessage());

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
            jpeg.EmbededData = _embeddingService.Embed(jpeg.QuantizedDCTData, password, message);

            //step 4 in jpeg compression - Run length encoding and huffman encoding.
            _encodingOrchestratorService.EncodeData(jpeg.EmbededData, bw);

            //step 5 - write end of image header and close binaryWritter.
            _headerService.WriteEOI(bw);
            bw.Close();

            return;
        }

        /// <summary>
        /// Extracts the hidden message, based on the provided password using F5 algorithm, from the jpeg image that is inside the binaryReader.
        /// </summary>
        /// <param name="password">Password used for extracting the message.</param>
        /// <param name="br">BinaryWriter where the jpeg will be written.</param>
        /// <returns>Extracted message.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public string Extract(string password, BinaryReader br)
        {
            //Validate inputs and create jpegInfo object
            if (br == null)
                throw new ArgumentNullException(nameof(br), nameof(br).ToArgumentNullExceptionMessage());

            if (String.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), nameof(password).ToArgumentNullExceptionMessage());

            var jpeg = new JpegInfo();

            //Parse jpeg markers
            _headerService.ParseJpegMarkers(br, jpeg);

            //Read entropy coded data and decode it.
            var quantizedDctData = _encodingOrchestratorService.DecodeData(jpeg, br);

            //Extract the message from the decoded data.
            var message = _extractingService.Extract(quantizedDctData, password);

            return message;
        }

        #region Util

        private JpegInfo CreateJpegInfo(Image image)
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
