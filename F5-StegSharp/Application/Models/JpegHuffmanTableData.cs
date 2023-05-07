using StegSharp.Domain;

namespace StegSharp.Application.Models
{
    public class JpegHuffmanTableData
    {
        public int[] DCLuminanceBits;

        public int[] DCLuminanceValues;

        public int[] DCChrominanceBits;

        public int[] DCChrominanceValues;

        public int[] ACLuminanceBits;

        public int[] ACChrominanceBits;

        public int[] ACLuminanceValues;

        public int[] ACChrominanceValues;

        public JpegHuffmanTableData() 
        {
            DCLuminanceBits = new int[HuffmanEncodingTables.DCLuminanceBits.Length];
            
            DCLuminanceValues = new int[HuffmanEncodingTables.DCLuminanceValues.Length];

            DCChrominanceBits = new int[HuffmanEncodingTables.DCChrominanceBits.Length];

            DCChrominanceValues = new int[HuffmanEncodingTables.DCChrominanceValues.Length];

            ACLuminanceBits = new int[HuffmanEncodingTables.ACLuminanceBits.Length];

            ACChrominanceBits = new int[HuffmanEncodingTables.ACChrominanceBits.Length];

            ACLuminanceValues = new int[HuffmanEncodingTables.ACLuminanceValues.Length];

            ACChrominanceValues = new int[HuffmanEncodingTables.ACChrominanceValues.Length];
        }

    }
}
