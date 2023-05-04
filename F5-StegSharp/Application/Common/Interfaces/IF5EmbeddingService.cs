using Application.Models;

namespace Application.Common.Interfaces
{
    public interface IF5EmbeddingService
    {
        /// <summary>
        /// Embeds a message inside the provided quantized DCTs.
        /// </summary>
        /// <param name="quantizedData">Quantized DCT data</param>
        /// <param name="password">Password used for pseudo-random number generator.</param>
        /// <param name="message">Message to embed.</param>
        /// <returns>DCTData object with modified values based on the embedded message. </returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        DCTData Embed(DCTData quantizedData, string password, string message);
    }
}
