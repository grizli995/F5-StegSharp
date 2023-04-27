using Application.Common.Interfaces;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class F5EmbeddingService : IF5EmbeddingService
    {
        private readonly IMCUConverterService _mcuConverterService;
        public F5EmbeddingService(IMCUConverterService mcuConverterService) 
        {
            this._mcuConverterService = mcuConverterService;
        }

        public DCTData Embed(DCTData quantizedData, string password, string message)
        {
            //step 1 - Convert dct data object to array of MCUs.
            var mcuArray = _mcuConverterService.DCTDataToMCUArray(quantizedData);

            //step 2 - Permute the order of MCUs in the mcuArray

            //step 3 - Calculate n and k

            //step 4 - save k and msgLen in first 4 bytes. (k = 1b, msgLen = 3b)

            //step 5 - Embed data

            //step 6 - Reverse permutation to get the original order of MCUs.

            return null;
        }
    }
}
