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
    }
}
