﻿using OCR.Abstractions.Enums;
using OCR.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCR.Abstractions.Services
{
    public interface IResultService
    {
        void Print(List<Thumbnail> thumbnails, string filePath);
    }
}