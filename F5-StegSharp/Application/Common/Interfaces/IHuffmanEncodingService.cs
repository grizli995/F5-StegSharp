using JpegLibrary;

namespace Application.Common.Interfaces
{
    public interface IHuffmanEncodingService
    {
        void EncodeChrominanceDC(int dc, int prevDC, BinaryWriter bw);
        void EncodeLuminanceDC(int dc, int prevDC, BinaryWriter bw);
        void EncodeChrominanceAC(JpegBlock8x8F block, BinaryWriter bw);
        void EncodeLuminanceAC(JpegBlock8x8F block, BinaryWriter bw);
    }
}
