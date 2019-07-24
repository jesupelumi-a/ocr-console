using System;
using System.Collections.Generic;
using System.Text;

namespace OCR.Abstractions.Models
{
    public class OcrResult
    {
        public List<Annotation> Annotations { get; set; }
        public Annotation MainAnnotation { get { return Annotations?.Count > 0 ? Annotations[0] : new Annotation { Description = "", }; } }
    }
}
