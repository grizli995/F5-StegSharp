using SkiaSharp;

namespace StegSharp.Application.Common.Interfaces
{
    public interface IF5Service
    {
        /// <summary>
        /// Embeds the input message with the provided password using F5 algorithm, and writes jpeg image as output inside the provided BinaryWriter.
        /// </summary>
        /// <param name="image">Image to embed the message in.</param>
        /// <param name="password">Password used for embedding the message.</param>
        /// <param name="message">Message to embed.</param>
        /// <param name="bw">BinaryWriter where the jpeg will be written.</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public void Embed(SKBitmap image, string password, string message, BinaryWriter bw);

        /// <summary>
        /// Embeds the input message with the provided password using F5 algorithm, and writes jpeg image as output inside the provided BinaryWriter.
        /// </summary>
        /// <param name="imagePath">Path to the Image to embed the message in.</param>
        /// <param name="password">Password used for embedding the message.</param>
        /// <param name="message">Message to embed.</param>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public void Embed(string imagePath, string outPath, string password, string message);


        /// <summary>
        /// Extracts the hidden message, based on the provided password using F5 algorithm, from the jpeg image that is inside the binaryReader.
        /// </summary>
        /// <param name="password">Password used for extracting the message.</param>
        /// <param name="br">BinaryWriter where the jpeg will be written.</param>
        /// <returns>Extracted message.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public string Extract(string password, BinaryReader br);

        /// <summary>
        /// Extracts the hidden message, based on the provided password using F5 algorithm, from the jpeg image that is inside the binaryReader.
        /// </summary>
        /// <param name="password">Password used for extracting the message.</param>
        /// <param name="imagePath">Path to the Image to embed the message in.</param>
        /// <returns>Extracted message.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation is unsuccessful.</exception>
        public string Extract(string imagePath, string password);
    }
}
