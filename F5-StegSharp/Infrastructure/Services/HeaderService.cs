using Application.Common.Interfaces;
using Application.Models;
using Domain;
using Infrastructure.Util.Extensions;

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
        /// Writes "End Of Image" markers and data.
        /// </summary>
        /// <param name="bw"></param>
        public void WriteEOI(BinaryWriter bw)
        {
            bw.Write((byte)JpegMarker.Padding);
            bw.Write((byte)JpegMarker.EndOfImage);
        }

        /// <summary>
        /// Reads jpeg byte by byte, untill the start of the entropy coded segment.
        /// </summary>
        /// <param name="br">Binary Reader</param>
        /// <param name="jpeg">Jpeg object where we will save parsed data.</param>
        public void ParseJpegMarkers(BinaryReader br, JpegInfo jpeg)
        {
            var currentByte = br.ReadByte();
            byte previousByte;
            var loop = true;
            while (loop)
            {
                previousByte = currentByte;
                currentByte = br.ReadByte();

                if (previousByte == (byte)JpegMarker.Padding && currentByte != (byte)JpegMarker.Padding)
                {
                    var marker = ParseJpegMarker(currentByte);
                    switch (marker)
                    {
                        case JpegMarker.StartOfImage:
                            break;
                        case JpegMarker.App0:
                            currentByte = ParseApp0Segment(br, jpeg);
                            break;
                        case JpegMarker.DefineQuantizationTable:
                            currentByte = ParseQuantizationTable(br, jpeg);
                            break;
                        case JpegMarker.StartOfFrame0:
                            currentByte = ParseSOF0(br, jpeg);
                            break;
                        case JpegMarker.DefineHuffmanTable:
                            currentByte = ParseHuffmanTables(br, jpeg);
                            break;
                        case JpegMarker.StartOfScan:
                            currentByte = ParseStartOfScan(br, jpeg);
                            loop = false;
                            break;
                        default:
                            currentByte = ReadUnsupportedSegment(br);
                            break;
                    }
                }
            }
        }


        #region Util

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
            for (var i = 0; i < table.Length; i++)
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

        //Reading header markers

        /// <summary>
        /// Reads through entire unsupported segment, without parsing any data.
        /// </summary>
        /// <param name="br">Binary Reader</param>
        /// <returns>Last read byte from Binary reader.</returns>
        private byte ReadUnsupportedSegment(BinaryReader br)
        {
            var length = br.Read2Bytes();
            byte currentByte = br.ReadByte();

            for (int i = 0; i < length - 3; i++)
            {
                currentByte = br.ReadByte();
            }

            return currentByte;
        }

        /// <summary>
        /// Parse byte as JpegMarker. If value is unsupported, return null.
        /// </summary>
        /// <param name="marker"></param>
        /// <returns></returns>
        private JpegMarker? ParseJpegMarker(byte marker)
        {
            if (Enum.IsDefined(typeof(JpegMarker), marker))
                return (JpegMarker)marker;
            else
                return null;
        }

        /// <summary>
        /// Parse App0 segment.
        /// </summary>
        /// <param name="br">Binary Reader</param>
        /// <param name="jpeg">Jpeg object where we will save parsed data.</param>
        /// <returns>Last read byte from Binary reader.</returns>
        /// <exception cref="Exception"></exception>
        private byte ParseApp0Segment(BinaryReader br, JpegInfo jpeg)
        {
            var length = br.Read2Bytes();
            byte[] jfifVersion = new byte[7] {
                0x4a, 0x46, 0x49, 0x46, 0x00,           //"JFIF\0"
                0x01, 0x01                              //version
            };

            for (int i = 0; i < 7; i++)
            {
                if (jfifVersion[i] != br.ReadByte())
                {
                    throw new Exception("Error reading Jfif version.");
                }
            }

            var units = br.ReadByte();

            var horizontalPixelDensity = br.Read2Bytes();
            var verticalPixelDensity = br.Read2Bytes();

            jpeg.HorizontalPixelDensity = horizontalPixelDensity;
            jpeg.VerticalPixelDensity = verticalPixelDensity;

            var thumbnailData = br.Read2Bytes();

            var currentByte = (byte)(thumbnailData & 0xFF);
            return currentByte;
        }

        /// <summary>
        /// Parse DQT segment.
        /// </summary>
        /// <param name="br">Binary Reader</param>
        /// <param name="jpeg">Jpeg object where we will save parsed data.</param>
        /// <returns>Last read byte from Binary reader.</returns>
        private byte ParseQuantizationTable(BinaryReader br, JpegInfo jpeg)
        {
            var length = br.Read2Bytes();

            if (length > 67)
            {
                //Read consecutive DQTs
                var dest = br.ReadByte();
                var current = ReadDQTForComponent(br, jpeg, dest);
                
                dest = br.ReadByte();
                current = ReadDQTForComponent(br, jpeg, dest);
                
                return current;
            }

            var destination = br.ReadByte();
            var currentByte = ReadDQTForComponent(br, jpeg, destination);

            return currentByte;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="br">Binary Reader</param>
        /// <param name="jpeg">Jpeg object where we will save parsed data.</param>
        /// <param name="destination"></param>
        /// <returns>Last read byte from Binary reader.</returns>
        /// <exception cref="Exception"></exception>
        private byte ReadDQTForComponent(BinaryReader br, JpegInfo jpeg, byte destination)
        {
            if (destination != 0 && destination != 1)
                throw new Exception("Unsupported DQT destination.");

            jpeg.QuantizationTables[destination] = new JpegQuantizationTable(destination);
            jpeg.QuantizationTables[destination].Values = ReadDQTData(br);

            var currentByte = jpeg.QuantizationTables[destination].Values.Last();

            return currentByte;
        }

        /// <summary>
        /// Reads quantization table data.
        /// </summary>
        /// <param name="br">Binary reader</param>
        /// <returns></returns>
        private byte[] ReadDQTData(BinaryReader br)
        {
            var result = new byte[64];

            for (int i = 0; i < 64; i++)
            {
                result[JpegSorting.JpegNaturalOrder[i]] = br.ReadByte();
            }

            return result;
        }

        /// <summary>
        /// Parse SOF0 - Start of file for baseline jpegs.
        /// </summary>
        /// <param name="br">Binary Reader</param>
        /// <param name="jpeg">Jpeg object where we will save parsed data.</param>
        /// <returns>Last read byte from Binary reader.</returns>
        private byte ParseSOF0(BinaryReader br, JpegInfo jpeg)
        {
            var length = br.Read2Bytes();
            var precision = br.ReadByte();
            var height = br.Read2Bytes();
            var width = br.Read2Bytes();
            var numberOfComponents = br.ReadByte();
            var currentByte = numberOfComponents;

            jpeg.Height = height;
            jpeg.Width = width;
            jpeg.Precision = precision;
            jpeg.Components = new JpegComponent[numberOfComponents];

            for (int i = 0; i < numberOfComponents; i++)
            {
                var componentId = br.ReadByte();
                var sampFactor = br.ReadByte();
                var tableId = br.ReadByte();
                currentByte = tableId;

                jpeg.Components[i] = new JpegComponent
                {
                    SamplingFactor = sampFactor,
                    Id = componentId,
                    QuantizationTableId = tableId
                };
            }

            return currentByte;
        }

        /// <summary>
        /// Parses Define huffman table segment.
        /// </summary>
        /// <param name="br">Binary Reader</param>
        /// <param name="jpeg">Jpeg object where we will save parsed data.</param>
        /// <returns>Last read byte from Binary reader.</returns>
        private byte ParseHuffmanTables(BinaryReader br, JpegInfo jpeg)
        {
            var length = br.Read2Bytes();
            byte currentByte;

            if (length > 0xFF)
            {
                currentByte = ParseConsecutiveHuffmanTables(br, jpeg);
                return currentByte;
            }

            currentByte = br.ReadByte();
            var classDestination = currentByte;
            var dhtClass = (byte)(currentByte >> 4);
            var dhtDestination = (byte)(currentByte & 0x0F);

            byte[] bitsArray = ReadBitsArray(br);

            var hufValCount = length - 3 - bitsArray.Length;
            byte[] hufValArray = ReadHufValArray(br, hufValCount);

            //TODO check if necessary
            SaveHuffmanTableData(jpeg, dhtClass, dhtDestination, bitsArray, hufValArray);

            SaveHuffmanTable(jpeg, classDestination, bitsArray, hufValArray);

            currentByte = hufValArray.Last();
            return currentByte;
        }

        private void SaveHuffmanTable(JpegInfo jpeg, byte classDestination, byte[] bitsArray, byte[] hufValArray)
        {
            jpeg.HuffmanTables.Add(new JpegHuffmanTable
            {
                Id = classDestination,
                Bits = bitsArray.Select(item => (int)item).ToArray(),
                HufVal = hufValArray.Select(item => (int)item).ToArray()
            });
        }

        private byte[] ReadHufValArray(BinaryReader br, int hufValCount)
        {
            var hufValArray = new byte[hufValCount];
            for (int i = 0; i < hufValCount; i++)
            {
                hufValArray[i] = br.ReadByte();
            }

            return hufValArray;
        }

        private byte[] ReadBitsArray(BinaryReader br)
        {
            var bitsArray = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                bitsArray[i] = br.ReadByte();
            }

            return bitsArray;
        }

        private void SaveHuffmanTableData(JpegInfo jpeg, byte dhtClass, byte dhtDestination, byte[] bitsArray, byte[] hufValArray)
        {
            if (dhtClass == 0 && dhtDestination == 0)
            {
                jpeg.HuffmanTableData.DCLuminanceBits = bitsArray.Select(item => (int)item).ToArray();
                jpeg.HuffmanTableData.DCLuminanceValues = hufValArray.Select(item => (int)item).ToArray();

            }
            if (dhtClass == 0 && dhtDestination == 1)
            {
                jpeg.HuffmanTableData.DCChrominanceBits = bitsArray.Select(item => (int)item).ToArray();
                jpeg.HuffmanTableData.DCChrominanceValues = hufValArray.Select(item => (int)item).ToArray();

            }
            if (dhtClass == 1 && dhtDestination == 0)
            {
                jpeg.HuffmanTableData.ACLuminanceBits = bitsArray.Select(item => (int)item).ToArray();
                jpeg.HuffmanTableData.ACLuminanceValues = hufValArray.Select(item => (int)item).ToArray();

            }
            if (dhtClass == 1 && dhtDestination == 1)
            {
                jpeg.HuffmanTableData.ACChrominanceBits = bitsArray.Select(item => (int)item).ToArray();
                jpeg.HuffmanTableData.ACChrominanceValues = hufValArray.Select(item => (int)item).ToArray();
            }

        }

        private byte ParseConsecutiveHuffmanTables(BinaryReader br, JpegInfo jpeg)
        {
            for(int h = 0; h < 4; h++)
            {
                int hufValCount;
                byte[] hufValArray;
                var currentByte = br.ReadByte();

                var classDestination = currentByte;
                var dhtClass = (byte)(classDestination >> 4);
                var dhtDestination = (byte)(classDestination & 0x0F);

                var bitsArray = ReadBitsArray(br);

                hufValCount = CalculateHufvalCount(dhtClass);

                hufValArray = ReadHufValArray(br, hufValCount);
                jpeg.HuffmanTables.Add(new JpegHuffmanTable
                {
                    Id = classDestination,
                    Bits = bitsArray.Select(item => (int)item).ToArray(),
                    HufVal = hufValArray.Select(item => (int)item).ToArray()
                });
            }


            return (byte)jpeg.HuffmanTables.Last().HufVal.Last();






            ////Luminance tables
            //for (int i = 0; i < HuffmanEncodingTables.DCLuminanceBits.Length; i++)
            //{
            //    jpeg.HuffmanTableData.DCLuminanceBits[i] = br.ReadByte();
            //}
            //for (int i = 0; i < HuffmanEncodingTables.DCLuminanceValues.Length; i++)
            //{
            //    jpeg.HuffmanTableData.DCLuminanceValues[i] = br.ReadByte();
            //}
            //for (int i = 0; i < HuffmanEncodingTables.ACLuminanceBits.Length; i++)
            //{
            //    jpeg.HuffmanTableData.ACLuminanceBits[i] = br.ReadByte();
            //}
            //for (int i = 0; i < HuffmanEncodingTables.ACLuminanceValues.Length; i++)
            //{
            //    jpeg.HuffmanTableData.ACLuminanceValues[i] = br.ReadByte();
            //}

            ////Chrominance tables
            //for (int i = 0; i < HuffmanEncodingTables.DCChrominanceBits.Length; i++)
            //{
            //    jpeg.HuffmanTableData.DCChrominanceBits[i] = br.ReadByte();
            //}
            //for (int i = 0; i < HuffmanEncodingTables.DCChrominanceValues.Length; i++)
            //{
            //    jpeg.HuffmanTableData.DCChrominanceValues[i] = br.ReadByte();
            //}
            //for (int i = 0; i < HuffmanEncodingTables.ACChrominanceBits.Length; i++)
            //{
            //    jpeg.HuffmanTableData.ACChrominanceBits[i] = br.ReadByte();
            //}
            //for (int i = 0; i < HuffmanEncodingTables.ACChrominanceValues.Length; i++)
            //{
            //    jpeg.HuffmanTableData.ACChrominanceValues[i] = br.ReadByte();
            //}

            return (byte)jpeg.HuffmanTableData.ACChrominanceValues.Last();
        }

        private static int CalculateHufvalCount(byte dhtClass)
        {
            int hufValCount;
            if (dhtClass == 0x00)
                hufValCount = 12;                                                       //DC
            else
                hufValCount = HuffmanEncodingTables.ACLuminanceValues.Length;           //AC
            return hufValCount;
        }

        /// <summary>
        /// Parse start of scan segment.
        /// </summary>
        /// <param name="br">Binary Reader</param>
        /// <param name="jpeg">Jpeg object where we will save parsed data.</param>
        /// <returns>Last read byte from Binary reader.</returns>
        private byte ParseStartOfScan(BinaryReader br, JpegInfo jpeg)
        {
            var length = br.Read2Bytes();

            var componentCount = br.ReadByte();
            var currentByte = componentCount;
            for (int i = 0; i < componentCount; i++)
            {
                var componentId = br.ReadByte();
                var tableIds = br.ReadByte();

                var dcTableId = (byte)(tableIds >> 4);
                var acTableId = (byte)(tableIds & 0x0F);

                var component = jpeg.Components.Where(item => item.Id == componentId).FirstOrDefault();
                component.DCHuffmanTableId = dcTableId;
                component.ACHuffmanTableId = acTableId;
            }

            for (int i = 0; i < length - 3 - (2 * componentCount); i++)
            {
                currentByte = br.ReadByte();
            }

            return currentByte;
        }

        #endregion
    }
}
