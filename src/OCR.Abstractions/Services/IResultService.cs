using OCR.Abstractions.Enums;
using OCR.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCR.Abstractions.Services
{
    public interface IResultService
    {
        void CreateCSV(List<Thumbnail> thumbnails, string filePath);
        void TestCreateCSV(List<Thumbnail> thumbnails, string filePath);
    }
}
