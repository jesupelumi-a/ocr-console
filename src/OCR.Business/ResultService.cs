using CsvHelper;
using Newtonsoft.Json;
using OCR.Abstractions.Enums;
using OCR.Abstractions.Models;
using OCR.Abstractions.Services;
using OCR.Business.Patterns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR.Business
{
    public class ResultService : IResultService
    {
        public ResultService()
        {

        }
        
        public void Print(List<Thumbnail> thumbnails, string filePath)
        {
            var matchedPattern = GetPattern(thumbnails);

            if (matchedPattern == Pattern.A)
            {
                PatternAService.CreateCSV(thumbnails, filePath);
            }
            else
            {
                PatternDefaultService.CreateCSV(thumbnails, filePath);
            }
        }

        private Pattern GetPattern(List<Thumbnail> thumbnails)
        {
            var retVal = Pattern.Default;
            try
            {
                // Step 1: Get the first 5 ocr results
                var thumbs = thumbnails.Take(5);

                // Step 2: Run each thumb against all the models; when a model returns a match, that's our guy
                foreach (var thumb in thumbs)
                {
                    var annotations = JsonConvert.DeserializeObject<List<Annotation>>(thumb.OcrResult);
                    if (PatternAService.Match(annotations))
                    {
                        retVal = Pattern.A;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResultService - GetPattern: {ex.Message}");
            }
            return retVal;
        }
    }
}
