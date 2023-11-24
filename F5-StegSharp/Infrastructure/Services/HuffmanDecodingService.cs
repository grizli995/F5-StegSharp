using StegSharp.Application.Common.Interfaces;
using StegSharp.Domain;
using JpegLibrary;

namespace StegSharp.Infrastructure.Services
{
    public class HuffmanDecodingService : IHuffmanDecodingService
    {
        private Dictionary<Tuple<int, int>, int> _dcCrominanceDiffDict;
        private Dictionary<Tuple<int, int>, int> _dcLuminanceDiffDict;
        private Dictionary<Tuple<int, int>, Tuple<int, int>> _acCrominanceCoeffDict;
        private Dictionary<Tuple<int, int>, Tuple<int, int>> _acLuminanceCoeffDict;

        private readonly IRunLengthEncodingService _runLengthEncodingService;
        private readonly IBitReaderService _bitReaderService;

        public HuffmanDecodingService(IRunLengthEncodingService runLengthEncodingService, IBitReaderService bitReaderService)
        {
            _runLengthEncodingService = runLengthEncodingService;
            _bitReaderService = bitReaderService;
            InitHuffmanTables();
        }

        /// <summary>
        /// Reads and recodes chrominance DC from the binary reader.
        /// </summary>
        /// <param name="prevDC"></param>
        /// <param name="bw"></param>
        /// <returns></returns>
        public int DecodeChrominanceDC(int prevDC, BinaryReader br)
        {
            return DecodeDC(prevDC, br, _dcCrominanceDiffDict);
        }

        /// <summary>
        /// Reads and recodes luminance DC from the binary reader.
        /// </summary>
        /// <param name="prevDC"></param>
        /// <param name="bw"></param>
        /// <returns></returns>
        public int DecodeLuminanceDC(int prevDC, BinaryReader br)
        {
            return DecodeDC(prevDC, br, _dcLuminanceDiffDict);
        }

        /// <summary>
        /// Reads and recodes chrominance AC from the binary reader.
        /// </summary>
        /// <param name="bw"></param>
        /// <returns></returns>
        public JpegBlock8x8F DecodeChrominanceAC(BinaryReader br)
        {
            return DecodeAC(br, _acCrominanceCoeffDict);
        }

        /// <summary>
        /// Reads and recodes luminance AC from the binary reader.
        /// </summary>
        /// <param name="bw"></param>
        /// <returns></returns>
        public JpegBlock8x8F DecodeLuminanceAC(BinaryReader br)
        {
            return DecodeAC(br, _acLuminanceCoeffDict);
        }


        public void ResetBitReader()
        {
            _bitReaderService.Reset();
        }

        #region Util

        private void InitHuffmanTables()
        {
            var dcLuminanceDiffTable = new Tuple<int, int>[12];
            var dcCrominanceDiffTable = new Tuple<int, int>[12];
            var acLuminanceCoeffTable = new Tuple<int, int>[255];
            var acCrominanceCoeffTable = new Tuple<int, int>[255];

            ExtractTable(HuffmanEncodingTables.DCLuminanceBits, HuffmanEncodingTables.DCLuminanceValues, dcLuminanceDiffTable);
            ExtractTable(HuffmanEncodingTables.DCChrominanceBits, HuffmanEncodingTables.DCChrominanceValues, dcCrominanceDiffTable);
            ExtractTable(HuffmanEncodingTables.ACLuminanceBits, HuffmanEncodingTables.ACLuminanceValues, acLuminanceCoeffTable);
            ExtractTable(HuffmanEncodingTables.ACChrominanceBits, HuffmanEncodingTables.ACChrominanceValues, acCrominanceCoeffTable);

            var acCrominanceCoeffList = acCrominanceCoeffTable.Where(item => item != null).ToList();
            var acLuminanceCoeffList = acLuminanceCoeffTable.Where(item => item != null).ToList();

            var dcCrominanceDiffList = dcCrominanceDiffTable.ToList();
            var dcLuminanceDiffList = dcLuminanceDiffTable.ToList();

            _dcCrominanceDiffDict = dcCrominanceDiffList.ToDictionary(key => key, item => dcCrominanceDiffList.IndexOf(item));
            _dcLuminanceDiffDict = dcLuminanceDiffList.ToDictionary(key => key, item => dcLuminanceDiffList.IndexOf(item));

            _acLuminanceCoeffDict = acLuminanceCoeffList.ToDictionary(key => key, item => CalculateCoefDictValue(item, acLuminanceCoeffList));
            _acCrominanceCoeffDict = acCrominanceCoeffList.ToDictionary(key => key, item => CalculateCoefDictValue(item, acCrominanceCoeffList));
        }

        private static Tuple<int, int> CalculateCoefDictValue(Tuple<int, int> item, List<Tuple<int, int>> acLuminanceCoeffList)
        {
            var index = acLuminanceCoeffList.IndexOf(item);
            if (index == 0)
            {
                return new Tuple<int, int>(0, 0);
            }
            index = index - 1;

            var item1 = 0;
            var item2 = 0;
            item1 = index / 10;

            if (item1 == 15)
            {
                if (item2 == 10)
                    return new Tuple<int, int>(item1, 0);
                else
                {
                    item2 = (index % 10);
                }
            }
            else
            {
                item2 = 1 + (index % 10);
            }

            if (item1 == 16)
                return new Tuple<int, int>(15, 10);

            return new Tuple<int, int>(item1, item2);
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

        private int CalculateDC(int prevDC, int diffValue)
        {
            int dc = diffValue + prevDC;
            return dc;
        }

        private int ReadDiffValue(BinaryReader br, int diffValueCodeLength, ref bool isNegativeNumber)
        {
            int diffValue = _bitReaderService.Read(br, true);
            if (diffValue == 0)
            {
                isNegativeNumber = true;
            }
            for (int i = 0; i < diffValueCodeLength - 1; i++)
            {
                diffValue = _bitReaderService.Read(br, false);
            }

            return diffValue;
        }

        private int DecodeDC(int prevDC, BinaryReader br, Dictionary<Tuple<int, int>, int> diffDict)
        {
            var dc = 0;
            int bitLength = 0;

            bitLength++;
            var diffCategoryCode = _bitReaderService.Read(br, true);
            var key = new Tuple<int, int>(diffCategoryCode, bitLength);

            while (true)
            {
                if (diffDict.TryGetValue(key, out int diffValueCodeLength))
                {
                    var diffValue = 0;
                    bool isNegativeNumber = false;

                    if (diffValueCodeLength != 0)
                    {
                        diffValue = ReadDiffValue(br, diffValueCodeLength, ref isNegativeNumber);
                    }

                    diffValue = ToTwosComplement(diffValue, diffValueCodeLength, isNegativeNumber ? 0 : 1);
                    dc = CalculateDC(prevDC, diffValue);
                    break;
                }
                else
                {
                    diffCategoryCode = _bitReaderService.Read(br, false);
                    bitLength++;
                    key = new Tuple<int, int>(diffCategoryCode, bitLength);
                }
            }

            return dc;
        }

        private int ToTwosComplement(int value, int numberOfBits, int firstBit)
        {
            if (firstBit == 1)
            {
                return value;
            }
            value = value | (1 << numberOfBits);

            int invertedValue = (~value) & ((1 << numberOfBits) - 1);
            return -(invertedValue);

        }

        private JpegBlock8x8F DecodeAC(BinaryReader br, Dictionary<Tuple<int, int>, Tuple<int, int>> coeffDict)
        {
            var pairs = ReadRunLengthPairs(br, coeffDict);

            var result = _runLengthEncodingService.Decode(pairs);

            return result;
        }

        private List<Tuple<int, int>> ReadRunLengthPairs(BinaryReader br, Dictionary<Tuple<int, int>, Tuple<int, int>> coefDict)
        {
            var runLengthValuePairs = new List<Tuple<int, int>>();
            int coeffCount = 1;

            var bitLength = 1;
            var coefCategoryCode = _bitReaderService.Read(br, true);
            var key = new Tuple<int, int>(coefCategoryCode, bitLength);

            while (coeffCount < 64)
            {
                if (coefDict.TryGetValue(key, out Tuple<int, int> runLengthCategoryPair))
                {
                    if ((runLengthCategoryPair.Item1 != 0 && runLengthCategoryPair.Item2 != 0) ||
                        (runLengthCategoryPair.Item1 != 15 && runLengthCategoryPair.Item2 != 0))
                    {
                        //Normal case handling - read coeff value after reading zeroLengthCategory pair.
                        var coeffValue = ReadCoeffValueFromCategory(br, runLengthCategoryPair);
                        runLengthValuePairs.Add(new Tuple<int, int>(runLengthCategoryPair.Item1, coeffValue));
                    }
                    else
                    {
                        //handle special cases 0/0 and 15/0
                        //no coeff value to read.
                        runLengthValuePairs.Add(runLengthCategoryPair);
                        if (runLengthCategoryPair.Item1 == 0)
                        {
                            break;
                        }
                    }

                    coeffCount = coeffCount + 1 + runLengthCategoryPair.Item1;

                    if (coeffCount > 63)
                        break;

                    coefCategoryCode = _bitReaderService.Read(br, true);
                    bitLength = 1;
                    key = new Tuple<int, int>(coefCategoryCode, bitLength);
                }
                else
                {
                    coefCategoryCode = _bitReaderService.Read(br, false);
                    bitLength++;
                    key = new Tuple<int, int>(coefCategoryCode, bitLength);
                }
            }

            return runLengthValuePairs;
        }

        private int ReadCoeffValueFromCategory(BinaryReader br, Tuple<int, int> runLengthCategoryPair)
        {
            var value = _bitReaderService.Read(br, true);
            var firstBit = value;

            for (int i = 0; i < runLengthCategoryPair.Item2 - 1; i++)
            {
                value = _bitReaderService.Read(br, false);
            }

            //check if the value is a negative number. If so, handle it.
            value = ToTwosComplement(value, runLengthCategoryPair.Item2, firstBit);

            return value;
        }


        #endregion
    }
}
