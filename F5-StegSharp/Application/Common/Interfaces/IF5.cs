﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IF5Service
    {
        public void Embed(Image image, string password, string text);
        public void Extract();
    }
}
