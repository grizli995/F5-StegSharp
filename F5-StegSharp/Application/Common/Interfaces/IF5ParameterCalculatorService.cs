using JpegLibrary;
using StegSharp.Application.Common.Exceptions;

namespace StegSharp.Application.Common.Interfaces
{
    public interface IF5ParameterCalculatorService
    {
        /// <summary>
        /// Calculater parameter K of the matrix encoding. Calculates embedding rate and based on the predefined table, picks the value for k.
        /// </summary>
        /// <param name="mcus">MCU array</param>
        /// <param name="message">Message</param>
        /// <returns>Calculated value of K parameter.</returns>
        /// <exception cref="CapacityException">Thrown when there is not enough capacity for the message.</exception>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        int CalculateK(JpegBlock8x8F[] mcus, string message);

        /// <summary>
        /// Calculates parameter N, based on parameter K.
        /// </summary>
        /// <param name="k">Parameter k used for matrix encoding.</param>
        /// <returns>Calculated value of N parameter.</returns>
        int CalculateN(int k);
    }
}
