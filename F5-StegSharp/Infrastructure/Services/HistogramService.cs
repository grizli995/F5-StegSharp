using StegSharp.Application.Common.Interfaces;
using StegSharp.Application.Models;
using StegSharp.Infrastructure.Util.Extensions;
using System.Xml.Linq;

namespace StegSharp.Infrastructure.Services
{
    public class HistogramService : IHistogramService
    {
        private readonly IHeaderService _headerService;
        private readonly IEncodingOrchestratorService _encodingOrchestratorService;
        private readonly IMCUConverterService _mcuConverterService;

        public HistogramService(IHeaderService headerService, IEncodingOrchestratorService encodingOrchestratorService,
            IMCUConverterService mcuConverterService)
        {
            _headerService = headerService;
            _encodingOrchestratorService = encodingOrchestratorService;
            _mcuConverterService = mcuConverterService;
        }

        public Dictionary<float, int> GetHistogramFromImagePath(string imagePath)
        {
            //Validate inputs and create jpegInfo object
            if (imagePath == null)
                throw new ArgumentNullException(nameof(imagePath), nameof(imagePath).ToArgumentNullExceptionMessage());


            var result = new Dictionary<float, int>();

            using (FileStream fileStream = new FileStream(imagePath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {

                    var jpeg = new JpegInfo();

                    //Parse jpeg markers
                    _headerService.ParseJpegMarkers(binaryReader, jpeg);

                    //Read entropy coded data and decode it.
                    var quantizedDctData = _encodingOrchestratorService.DecodeData(jpeg, binaryReader);

                    //step 1 - Convert dctData object to mcu array
                    var mcuArray = _mcuConverterService.DCTDataToMCUArray(quantizedDctData);


                    //step 3 - Convert permutated MCU array to coeff array
                    var coeffs = _mcuConverterService.MCUArrayToCoeffArray(mcuArray);

                    result = coeffs.GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count()); ;
                }
            }
            return result;
        }
    }
}
