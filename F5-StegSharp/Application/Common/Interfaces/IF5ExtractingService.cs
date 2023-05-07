using StegSharp.Application.Models;

namespace StegSharp.Application.Common.Interfaces
{
    public interface IF5ExtractingService
    {
        /// <summary>
        /// Extracts message from the DCTData object, based on the given password.
        /// </summary>
        /// <param name="dctData">DCTData from which the message will be extracted.</param>
        /// <param name="password">Password used for extracting.</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        /// <exception cref="MatrixEncodingException">Thrown if matrix encoding parameters are invalid.</exception>
        /// <returns>Extracted message</returns>
        string Extract(DCTData dctData, string password);
    }
}
