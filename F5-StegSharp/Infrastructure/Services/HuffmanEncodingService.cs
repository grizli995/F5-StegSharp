using StegSharp.Application.Common.Interfaces;
using StegSharp.Domain;
using JpegLibrary;

namespace StegSharp.Infrastructure.Services
{
    public class HuffmanEncodingService : IHuffmanEncodingService
    {
        private Tuple<int, int>[] _dcCrominanceDiffTable;
        private Tuple<int, int>[] _dcLuminanceDiffTable;
        private Tuple<int, int>[] _acCrominanceCoeffTable;
        private Tuple<int, int>[] _acLuminanceCoeffTable;
        private int _bufferPutBits, _bufferPutBuffer;

        private readonly IRunLengthEncodingService _runLengthEncodingService;

        public HuffmanEncodingService(IRunLengthEncodingService runLengthEncodingService)
        {
            InitHuffmanTables();
            this._runLengthEncodingService = runLengthEncodingService;
        }

        /// <summary>
        /// Encodes AC coefficient of the Chrominance component.
        /// </summary>
        /// <param name="block">MCU containing all coefficients (AC & DC)</param>
        /// <param name="bw">BinaryWriter</param>
        public void EncodeChrominanceAC(JpegBlock8x8F block, BinaryWriter bw)
        {
            EncodeAC(block, bw, _acCrominanceCoeffTable);
        }

        /// <summary>
        /// Encodes AC coefficient of the Luminance component.
        /// </summary>
        /// <param name="block">MCU containing all coefficients (AC & DC)</param>
        /// <param name="bw">BinaryWriter</param>
        public void EncodeLuminanceAC(JpegBlock8x8F block, BinaryWriter bw)
        {
            EncodeAC(block, bw, _acLuminanceCoeffTable);
        }

        /// <summary>
        /// Encodes DC coefficient of the Chrominance component.
        /// </summary>
        /// <param name="dc">DC coefficient</param>
        /// <param name="prevDC">Previous DC coefficient</param>
        /// <param name="bw">BinaryWriter</param>
        public void EncodeChrominanceDC(int dc, int prevDC, BinaryWriter bw)
        {
            EncodeDC(dc, prevDC, bw, _dcCrominanceDiffTable);
        }

        /// <summary>
        /// Encodes DC coefficient of the Luminance component.
        /// </summary>
        /// <param name="dc">DC coefficient</param>
        /// <param name="prevDC">Previous DC coefficient</param>
        /// <param name="bw">BinaryWriter</param>
        public void EncodeLuminanceDC(int dc, int prevDC, BinaryWriter bw)
        {
            EncodeDC(dc, prevDC, bw, _dcLuminanceDiffTable);
        }

        #region Util

        private void InitHuffmanTables()
        {
            this._dcLuminanceDiffTable = new Tuple<int, int>[12];
            this._dcCrominanceDiffTable = new Tuple<int, int>[12];
            this._acLuminanceCoeffTable = new Tuple<int, int>[255];
            this._acCrominanceCoeffTable = new Tuple<int, int>[255];


            ExtractTable(HuffmanEncodingTables.DCLuminanceBits, HuffmanEncodingTables.DCLuminanceValues, this._dcLuminanceDiffTable);
            ExtractTable(HuffmanEncodingTables.DCChrominanceBits, HuffmanEncodingTables.DCChrominanceValues, this._dcCrominanceDiffTable);
            ExtractTable(HuffmanEncodingTables.ACLuminanceBits, HuffmanEncodingTables.ACLuminanceValues, this._acLuminanceCoeffTable);
            ExtractTable(HuffmanEncodingTables.ACChrominanceBits, HuffmanEncodingTables.ACChrominanceValues, this._acCrominanceCoeffTable);
        }

        private static int CalculateValueCategory(int currentValue)
        {
            if (currentValue == 0)
                return 0;

            var currentValueBitLength = 1;
            var temp = currentValue;

            if (currentValue < 0)
            {
                temp = -temp;
            }
            while ((temp >>= 1) != 0)
            {
                currentValueBitLength++;
            }

            return currentValueBitLength;
        }

        private void ExtractTable(int[] bits, int[] val, Tuple<int, int>[] table)
        {
            int i, j, v;
            int p = 0, code = 0;
            for (j = 1; j < bits.Length; j++)
            {
                for (i = 0; i < bits[j]; i++)
                {
                    v = val[p];
                    table[v] = new Tuple<int, int>(code++, j);
                    p++;
                }
                code <<= 1;
            }
        }

        private void WriteBits(BinaryWriter bw, int code, int size)
        {
            int putBuffer = code;
            int putBits = this._bufferPutBits;
            putBuffer &= (1 << size) - 1;
            putBits += size;
            putBuffer <<= 24 - putBits;
            putBuffer |= this._bufferPutBuffer;

            while (putBits >= 8)
            {
                int c = putBuffer >> 16 & 0xFF;
                WriteByte(bw, c);
                if (c == 0xFF)
                    WriteByte(bw, 0);
                putBuffer <<= 8;
                putBits -= 8;
            }
            this._bufferPutBuffer = putBuffer;
            this._bufferPutBits = putBits;
        }

        private void WriteByte(BinaryWriter bw, int b)
        {
            bw.Write((byte)b);
        }

        public void FlushBuffer(BinaryWriter bw)
        {
            int PutBuffer = this._bufferPutBuffer;
            int PutBits = this._bufferPutBits;
            while (PutBits >= 8)
            {
                int c = PutBuffer >> 16 & 0xFF;
                WriteByte(bw, c);
                if (c == 0xFF)
                    WriteByte(bw, 0);
                PutBuffer <<= 8;
                PutBits -= 8;
            }
            if (PutBits > 0)
            {
                int c = PutBuffer >> 16 & 0xFF;
                WriteByte(bw, c);
            }
        }

        private void EncodeACCoeff(BinaryWriter bw, Tuple<int, int> item, Tuple<int, int>[] coeffTable)
        {
            //Prepare currentValue to appropriate bit representation.
            var currentAbsoluteValue = item.Item2;
            var currentValue = currentAbsoluteValue;
            if (currentAbsoluteValue < 00)
            {
                currentAbsoluteValue = -currentAbsoluteValue;
                currentValue--;
            }

            int currentValueBitLength = CalculateValueCategory(currentAbsoluteValue);

            WriteACCoeffCategory(bw, item, coeffTable, currentValueBitLength);

            WriteACCoeffValue(bw, item, currentValue, currentValueBitLength);
        }

        private void WriteACCoeffValue(BinaryWriter bw, Tuple<int, int> item, int currentValue, int currentValueBitLength)
        {
            if ((item.Item1 != 0 && item.Item2 != 0) || (item.Item1 != 15 && item.Item2 != 0))
            {
                WriteBits(bw, currentValue, currentValueBitLength);
            }
        }

        private void WriteACCoeffCategory(BinaryWriter bw, Tuple<int, int> item, Tuple<int, int>[] coeffTable, int currentValueBitLength)
        {
            var acCoefCategoryCode = coeffTable[item.Item1 * 16 + currentValueBitLength].Item1;
            var acCoefCategorySize = coeffTable[item.Item1 * 16 + currentValueBitLength].Item2;

            WriteBits(bw, acCoefCategoryCode, acCoefCategorySize);
        }

        private void EncodeDC(int dc, int prevDC, BinaryWriter bw, Tuple<int, int>[] diffTable)
        {
            //Prepare diffValue to appropriate bit representation.
            var diffAbsoluteValue = dc - prevDC;
            var diffValue = diffAbsoluteValue;
            if (diffAbsoluteValue < 0)
            {
                diffAbsoluteValue = -diffAbsoluteValue;
                diffValue--;
            }

            var diffBitLength = CalculateValueCategory(diffAbsoluteValue);

            WriteDCDiffCoeffCategory(bw, diffBitLength, diffTable);

            WriteDCDiffCoeffValue(bw, diffValue, diffBitLength);
        }

        private void WriteDCDiffCoeffValue(BinaryWriter bw, int diffValue, int diffBitLength)
        {
            if (diffBitLength != 0)
            {
                WriteBits(bw, diffValue, diffBitLength);
            }
        }

        private void WriteDCDiffCoeffCategory(BinaryWriter bw, int diffBitLength, Tuple<int, int>[] diffTable)
        {
            WriteBits(bw, diffTable[diffBitLength].Item1, diffTable[diffBitLength].Item2);
        }

        private void EncodeAC(JpegBlock8x8F block, BinaryWriter bw, Tuple<int, int>[] coeffTable)
        {
            var runLengthValuePairs = _runLengthEncodingService.Encode(block);
            var coeffCount = 1;

            foreach (var item in runLengthValuePairs)
            {
                if (coeffCount == 64)
                    return;

                EncodeACCoeff(bw, item, coeffTable);

                //Increment coefficient count. If it reaches 64, no need for EndOfBlock marker.
                coeffCount = coeffCount + 1 + item.Item1;
            }
        }

        #endregion
    }
}
