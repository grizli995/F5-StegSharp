using JpegLibrary;

namespace Application.Common.Interfaces
{
    public interface IPermutationService
    {
        /// <summary>
        /// Pseudo-randomly permutates elements of the input MCU array, based on the provided password. 
        /// Can be used to permutate the MCU array back, to original sequence, if "reverse" is set to true.
        /// </summary>
        /// <param name="password">Password used to seed the pseudo-random number generator.</param>
        /// <param name="inputArray">Input mcu array</param>
        /// <param name="reverse">If false permutate, if true permutate in the reverse order.</param>
        /// <returns>Permutated mcu array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        JpegBlock8x8F[] PermutateArray(string password, JpegBlock8x8F[] inputArray, bool reverse);
    }
}
