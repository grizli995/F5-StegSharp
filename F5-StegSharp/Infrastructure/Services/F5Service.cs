using StegSharp.Application.Common.Exceptions;
using StegSharp.Application.Common.Interfaces;
using StegSharp.Application.Models;
using StegSharp.Infrastructure.Util.Extensions;
using SkiaSharp;
using MethodTimer;

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
        [Time]
        public void Embed(SKBitmap image, string password, string message, BinaryWriter bw)
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
        /// Embeds the input message with the provided password using F5 algorithm, and writes jpeg image as output inside the provided BinaryWriter.
        /// </summary>
        /// <param name="imagePath">Path to the Image to embed the message in.</param>
        /// <param name="password">Password used for embedding the message.</param>
        /// <param name="message">Message to embed.</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        [Time]
        public void Embed(string imagePath, string outPath, string password, string message)
        {
            if (String.IsNullOrEmpty(imagePath))
                throw new ArgumentNullException(nameof(imagePath), nameof(imagePath).ToArgumentNullExceptionMessage());

            if (String.IsNullOrEmpty(outPath))
                throw new ArgumentNullException(nameof(outPath), nameof(outPath).ToArgumentNullExceptionMessage());

            SKBitmap image = SKBitmap.Decode(imagePath);

            using (FileStream fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    this.Embed(image, password, message, binaryWriter);
                }
            }
        }

        /// <summary>
        /// Extracts the hidden message, based on the provided password using F5 algorithm, from the jpeg image that is inside the binaryReader.
        /// </summary>
        /// <param name="password">Password used for extracting the message.</param>
        /// <param name="br">BinaryWriter where the jpeg will be written.</param>
        /// <returns>Extracted message.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        [Time]
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

        /// <summary>
        /// Extracts the hidden message, based on the provided password using F5 algorithm, from the jpeg image that is inside the binaryReader.
        /// </summary>
        /// <param name="password">Password used for extracting the message.</param>
        /// <param name="imagePath">Path to the Image to embed the message in.</param>
        /// <returns>Extracted message.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        [Time]
        public string Extract(string imagePath, string password)
        {
            if (String.IsNullOrEmpty(imagePath))
                throw new ArgumentNullException(nameof(imagePath), nameof(imagePath).ToArgumentNullExceptionMessage());

            string message;
            using (FileStream fileStream = new FileStream(imagePath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    message = this.Extract(password, binaryReader);
                }
            }

            return message;
        }

        #region Util

        private JpegInfo CreateJpegInfo(SKBitmap image)
        {
            var jpeg = new JpegInfo();
            jpeg.Width = image.Width;
            jpeg.Height = image.Height;
            jpeg.Bitmap = (SKBitmap)image;
            return jpeg;
        }

        #endregion
    }
}
