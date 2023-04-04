using Application.Common.Interfaces;
using Domain;
using JpegLibrary;

namespace Infrastructure.Services
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

        public void EncodeChrominanceAC(JpegBlock8x8F block, BinaryWriter bw)
        {
            var runLengthValuePairs = _runLengthEncodingService.Encode(block);

            foreach (var item in runLengthValuePairs)
            {
                var currentValue = item.Item2;
                int currentValueBitLength = CalculateValueCategory(currentValue);

                var code = _acCrominanceCoeffTable[item.Item1 * 16 + currentValueBitLength].Item1;
                var size = _acCrominanceCoeffTable[item.Item1 * 16 + currentValueBitLength].Item2;

                BufferIt(bw, code, size);
                BufferIt(bw, currentValue, currentValueBitLength);
            }
        }

        public void EncodeLuminanceAC(JpegBlock8x8F block, BinaryWriter bw)
        {
            var runLengthValuePairs = _runLengthEncodingService.Encode(block);

            foreach (var item in runLengthValuePairs)
            {
                var currentValue = item.Item2;
                int currentValueBitLength = CalculateValueCategory(currentValue);
                //treba kad je currentValue = 0  da currentValuebitLenght  da bude = 0;
                //fali mu +1 kad indeksiram.
                var code = _acLuminanceCoeffTable[item.Item1 * 16 + currentValueBitLength].Item1;
                var size = _acLuminanceCoeffTable[item.Item1 * 16 + currentValueBitLength].Item2;

                BufferIt(bw, code, size);
                BufferIt(bw, currentValue, currentValueBitLength);
            }
        }

        public void EncodeChrominanceDC(int dc, int prevDC, BinaryWriter bw)
        {
            var diff = dc - prevDC;
            if (diff < 0)
                diff = -diff;
            var diffBitLength = CalculateValueCategory(diff);
            BufferIt(bw, _dcCrominanceDiffTable[diffBitLength].Item1, _dcCrominanceDiffTable[diffBitLength].Item2);
        }

        public void EncodeLuminanceDC(int dc, int prevDC, BinaryWriter bw)
        {
            var diff = dc - prevDC;
            if (diff < 0)
                diff = -diff;
            var diffBitLength = CalculateValueCategory(diff);
            BufferIt(bw, _dcLuminanceDiffTable[diffBitLength].Item1, _dcLuminanceDiffTable[diffBitLength].Item2);
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
            if(currentValue == 0) 
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

        private void BufferIt(BinaryWriter bw, int code, int size)
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

        #endregion
    }
}
