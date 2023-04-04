namespace Domain
{
    /// <summary>
    /// JPEG markers
    /// </summary>
    public enum JpegMarker : byte
    {
        /// <summary>
        /// Padding
        /// </summary>
        Padding = 0xFF,

        /// <summary>
        /// Start of image
        /// </summary>
        StartOfImage = 0xD8,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App0 = 0xE0,

        /// <summary>
        /// Define quantization table(s)
        /// </summary>
        DefineQuantizationTable = 0xDB,

        /// <summary>
        /// Start of Frame marker, non-differential, Huffman coding, Baseline DCT
        /// </summary>
        StartOfFrame0 = 0xC0,

        /// <summary>
        ///  Define Huffman table(s)
        /// </summary>
        DefineHuffmanTable = 0xC4,

        /// <summary>
        /// Start of scan
        /// </summary>
        StartOfScan = 0xDA,

        /// <summary>
        /// End of image
        /// </summary>
        EndOfImage = 0xD9,
    }

}
