using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OCR.Abstractions.Models
{
    public class Thumbnail
    {
        public string FileName { get; set; }
        public string Source { get; set; }
        public string Time { get; set; }
        public Stream Content { get; set; }
        public string OcrResult { get; set; }
    }
}
