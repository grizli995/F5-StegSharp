using Application.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IDCTService
    {
        /// <summary>
        /// Performs DCT calculations for all color components.
        /// </summary>
        /// <param name="input">YCBCRData input</param>
        /// <param name="width">Original image width</param>
        /// <param name="height">Original image height</param>
        /// <returns>DCTData object which contains MCUs for all 3 color components.</returns>
        public DCTData CalculateDCT(YCBCRData input, int width, int height);

        /// <summary>
        /// Quantizes input DCT data for all color components. 
        /// </summary>
        /// <param name="input">DCT data to quantisize.</param>
        /// <param name="chrominanceTable">Chrominance table used for red and blue color component. If null, will use default table.</param>
        /// <param name="luminanceTable">Luminance table used for Y color component. If null, will use default table.</param>
        /// <returns></returns>
        public DCTData QuantizeDCT(DCTData input, byte[] chrominanceTable, byte[] luminanceTable);
    }
}
