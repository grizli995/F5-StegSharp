using Application.Common.Interfaces;
using Application.Models;
using Domain;

namespace Infrastructure.Services
{
    public class HeaderService : IHeaderService
    {
        public HeaderService() { }

        public void WriteHeaders(BinaryWriter bw, JpegInfo jpeg)
        {
            WriteSOI(bw);
            WriteApp0(bw);
            WriteDQT(bw);
            WriteSOF0(bw, jpeg);
            WriteDHT(bw);
            WriteSOS(bw);
        }

        /// <summary>
        /// Writes "Start Of Image" markers and data.
        /// </summary>
        /// <param name="bw"></param>
        private void WriteSOI(BinaryWriter bw)
        {
            bw.Write((byte)JpegMarker.Padding);
            bw.Write((byte)JpegMarker.StartOfImage);
        }

        /// <summary>
        /// Writes "Application 0" markers and data.
        /// </summary>
        /// <param name="bw"></param>
        private void WriteApp0(BinaryWriter bw)
        {
            bw.Write((byte)JpegMarker.Padding);
            bw.Write((byte)JpegMarker.App0);
            var x = new byte[16] {
            0x00, 0x10,                             //length
            0x4a, 0x46, 0x49, 0x46, 0x00,           //"JFIF\0"
            0x01, 0x01,                             //version
            0x01,                                   //units
            0x00, 0x60, 0x00, 0x60,                 //density
            0x00, 0x00                              //thumbnail
            };
            bw.Write(x);
        }

        /// <summary>
        /// Writes "Define Quantization Tables" markers and data.
        /// </summary>
        /// <param name="bw"></param>
        private void WriteDQT(BinaryWriter bw)
        {
            //luminance
            WriteDQTMarkers(bw, 0x00);
            WriteDQTData(bw, JpegStandardQuantizationTable.LuminanceTable);

            //chrominance
            //WriteDQTMarkers(bw, 0x01);
            bw.Write((byte)0x01);
            WriteDQTData(bw, JpegStandardQuantizationTable.ChrominanceTable);
        }

        private static void WriteDQTMarkers(BinaryWriter bw, byte destination)
        {
            bw.Write((byte)JpegMarker.Padding);
            bw.Write((byte)JpegMarker.DefineQuantizationTable);
            bw.Write(new byte[2] { 0x00, 0x84 });                   //length
            bw.Write(destination);
        }

        private void WriteDQTData(BinaryWriter bw, byte[] table)
        {
            for(var i = 0; i< table.Length; i++)
            {
                bw.Write(table[JpegSorting.JpegNaturalOrder[i]]);
            }
        }

        /// <summary>
        /// Writes "Start Of Frame 0" markers and data.
        /// </summary>
        /// <param name="bw"></param>
        private void WriteSOF0(BinaryWriter bw, JpegInfo jpeg)
        {
            bw.Write((byte)JpegMarker.Padding);
            bw.Write((byte)JpegMarker.StartOfFrame0);

            List<byte> SOF = new List<byte>(19)
            {
                0x00, 0x11,
                0x08,
                (byte)(jpeg.Height >> 8 & 0xFF),
                (byte)(jpeg.Height & 0xFF),
                (byte)(jpeg.Width >> 8 & 0xFF),
                (byte)(jpeg.Width & 0xFF),
                0x03//JpegInfo.NumComponents
            };
            for (var i = 0; i < 3/*JpegInfo.NumberOfComponents*/; i++)
            {
                SOF.Add((byte)(i + 1));
                SOF.Add(0x11);      //samp factor 1x1

                if (i == 0)
                {
                    SOF.Add(0x00);
                }
                else
                {
                    SOF.Add(0x01);
                }
            }

            bw.Write(SOF.ToArray());

        }

        /// <summary>
        /// Writes "Define Huffman Tables" markers and data.
        /// </summary>
        /// <param name="bw"></param>
        private void WriteDHT(BinaryWriter bw)
        {
            bw.Write((byte)JpegMarker.Padding);
            bw.Write((byte)JpegMarker.DefineHuffmanTable);
            bw.Write(new byte[2] { 0x01, 0xA2 });                   //length

            bw.Write(ConvertToByteArray(HuffmanEncodingTables.DCLuminanceBits));
            bw.Write(ConvertToByteArray(HuffmanEncodingTables.DCLuminanceValues));
            bw.Write(ConvertToByteArray(HuffmanEncodingTables.ACLuminanceBits));
            bw.Write(ConvertToByteArray(HuffmanEncodingTables.ACLuminanceValues));

            bw.Write(ConvertToByteArray(HuffmanEncodingTables.DCChrominanceBits));
            bw.Write(ConvertToByteArray(HuffmanEncodingTables.DCChrominanceValues));
            bw.Write(ConvertToByteArray(HuffmanEncodingTables.ACChrominanceBits));
            bw.Write(ConvertToByteArray(HuffmanEncodingTables.ACChrominanceValues));
        }

        private static byte[] ConvertToByteArray(int[] input)
        {
            return input.Select(item => (byte)item).ToArray();
        }

        /// <summary>
        /// Writes "Start Of Scan" markers and data.
        /// </summary>
        /// <param name="bw"></param>
        private void WriteSOS(BinaryWriter bw)
        {
            bw.Write((byte)JpegMarker.Padding);
            bw.Write((byte)JpegMarker.StartOfScan);

            bw.Write(new byte[2] { 0x00, 0x0C });

            bw.Write((byte)0x03);                 //Component count

            bw.Write((byte)0x01);                 //component id (Y)
            bw.Write((byte)0x00);                 //component table ids - dc table 0 ac table 0.

            bw.Write((byte)0x02);                 //component id (CR)
            bw.Write((byte)0x11);                 //component table ids - dc table 1 ac table 1.
            
            bw.Write((byte)0x03);                 //component id (CB)
            bw.Write((byte)0x11);                 //component table ids - dc table 1 ac table 1.

            bw.Write((byte)0x00);
            bw.Write((byte)0x3f);
            bw.Write((byte)0x00);

        }

        /// <summary>
        /// Writes "End Of Image" markers and data.
        /// </summary>
        /// <param name="bw"></param>
        public void WriteEOI(BinaryWriter bw)
        {
            bw.Write((byte)JpegMarker.Padding);
            bw.Write((byte)JpegMarker.EndOfImage);
        }
    }
}
