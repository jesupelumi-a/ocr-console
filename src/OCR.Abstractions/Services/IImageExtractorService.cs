using OCR.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCR.Abstractions.Services
{
    public interface IImageExtractorService
    {
        Task<List<Thumbnail>> SplitAsync(string videoPath, int duration = 10, bool captureFirstScreen = true, bool captureLastScreen = true);
    }
}
