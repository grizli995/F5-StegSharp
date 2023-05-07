using StegSharp.Application.Models;
using JpegLibrary;

namespace StegSharp.Application.Common.Interfaces
{
    public interface IMCUConverterService
    {
        /// <summary>
        /// Combines MCUs of all components in one MCU array. Order of MCUs is Y(x), CB(x), CR(x). Where x=0 -> mcuCount.
        /// </summary>
        /// <param name="dct"></param>
        /// <returns>MCU Array</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        JpegBlock8x8F[] DCTDataToMCUArray(DCTData dct);

        /// <summary>
        /// Converts array od MCU objects to an array of coefficients.
        /// </summary>
        /// <param name="dctArray">MCU array</param>
        /// <returns>Coefficient Array</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        float[] MCUArrayToCoeffArray(JpegBlock8x8F[] dctArray);

        /// <summary>
        /// Converts array od coefficients into an array of consecutive MCUs.
        /// </summary>
        /// <param name="coeffArray">Coefficient array</param>
        /// <returns>MCU Array</returns>
        JpegBlock8x8F[] CoeffArrayMCUArray(float[] coeffArray);

        /// <summary>
        /// Converts array of consecutive MCUs into DCTData object.
        /// </summary>
        /// <param name="mcuArray">MCU Array</param>
        /// <returns>DCTData object</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        DCTData MCUArrayToDCTData(JpegBlock8x8F[] mcuArray);
    }
}
