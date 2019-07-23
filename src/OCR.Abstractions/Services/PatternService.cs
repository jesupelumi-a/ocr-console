using OCR.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCR.Abstractions.Services
{
    public abstract class PatternService
    {
        public abstract bool Match(List<Annotation> OcrResult);

        public abstract bool THRMatched(List<Annotation> OcrResult);
        public abstract bool DepthMatched(List<Annotation> OcrResult);
        public abstract bool HeadingMatched(List<Annotation> OcrResult);
        public abstract bool TRNMatched(List<Annotation> OcrResult);
    }
}
