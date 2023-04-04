﻿using Application.Models;

namespace Application.Common.Interfaces
{
    public interface IHeaderService
    {
        void WriteHeaders(BinaryWriter bw, JpegInfo jpeg);

        void WriteEOI(BinaryWriter bw);
    }
}