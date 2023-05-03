using Application.Common.Interfaces;

namespace Infrastructure.Services
{
    public class BitReaderService : IBitReaderService
    {
        private byte BufferedByte;
        private byte PreviousBufferedByte;
        private int CurrentBitIndex;
        private int BufferedResult;

        public BitReaderService()
        {
            Initialize();
        }

        /// <summary>
        /// Reads bit from the binary reader and buffers the value read. 
        /// Each consecutive read appends to the buffered value and outputs the buffered value as int.
        /// </summary>
        /// <param name="br">Binary reader</param>
        /// <param name="reset">If true - resets the buffered value to 0.</param>
        /// <returns></returns>
        public int Read(BinaryReader br, bool reset)
        {
            if(CurrentBitIndex == 0)
            {
                PreviousBufferedByte = BufferedByte;
                BufferedByte = br.ReadByte();
            }
            if(CurrentBitIndex > 7)
            {
                PreviousBufferedByte = BufferedByte;
                BufferedByte = br.ReadByte();

                //Handle byte stuffing.
                //Since FF is a special marker, if "FF" is encountered in the entropy coded segment, it will have "00" bytes after it.
                if(PreviousBufferedByte == 0xFF && BufferedByte == 0x00)
                {
                    PreviousBufferedByte = BufferedByte;
                    BufferedByte = br.ReadByte();
                }
                CurrentBitIndex = 0;
            }

            if(reset)
            {
                BufferedResult = 0;
            }

            bool bit = (BufferedByte & (1 << (7 - CurrentBitIndex))) != 0;
            BufferedResult = (BufferedResult << 1) | (bit ? 1 : 0);

            CurrentBitIndex++;

            return BufferedResult;
        }

        private void Initialize()
        {
            BufferedByte = 0;
            BufferedResult = 0;
            CurrentBitIndex = 0;
            PreviousBufferedByte = 0;
        }
    }
}
