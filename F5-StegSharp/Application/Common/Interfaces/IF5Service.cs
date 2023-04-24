using Application.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IF5Service
    {
        public DCTData Embed(Image image, string password, string text, BinaryWriter bw);
        public string Extract(string password, BinaryReader br);
        public DCTData ExtractDCT(string password, BinaryReader br);
    }
}
