using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IF5EmbeddingService
    {
        DCTData Embed(DCTData quantizedData, string password, string message);
    }
}
