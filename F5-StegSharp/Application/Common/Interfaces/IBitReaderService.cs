namespace StegSharp.Application.Common.Interfaces
{
    public interface IBitReaderService
    {
        /// <summary>
        /// Reads bit from the binary reader and buffers the value read. 
        /// Each consecutive read appends to the buffered value and outputs the buffered value as int.
        /// </summary>
        /// <param name="br">Binary reader</param>
        /// <param name="reset">If true - resets the buffered value to 0.</param>
        /// <returns></returns>
        int Read(BinaryReader br, bool reset);
    }
}
