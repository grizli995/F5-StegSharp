using Application.Models;
using JpegLibrary;

namespace Application.Common.Interfaces
{
    public interface IMCUConverterService
    {
        JpegBlock8x8F[] DCTDataToMCUArray(DCTData dct);

        float[] MCUArrayToCoeffArray(JpegBlock8x8F[] dctArray);

        JpegBlock8x8F[] CoeffArrayMCUArray(float[] coeffArray);

        DCTData MCUArrayToDCTData(JpegBlock8x8F[] mcuArray);
    }
}
