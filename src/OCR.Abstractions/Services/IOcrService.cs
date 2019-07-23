using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCR.Abstractions.Services
{
    public interface IOcrService
    {
        Task<string> ExtractText(string imagePath, int? index);
    }
}
