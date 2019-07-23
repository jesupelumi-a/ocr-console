using CsvHelper;
using Newtonsoft.Json;
using OCR.Abstractions;
using OCR.Abstractions.Enums;
using OCR.Abstractions.Models;
using OCR.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OCR.Business.Patterns
{
    public static class PatternAService
    {
        public static void CreateCSV(List<Thumbnail> thumbnails, string csvPath)
        {
            var annotations = new List<Annotation>();
            foreach (var thumbnail in thumbnails)
            {
                var ocrResult = new OcrResult
                {
                    Annotations = JsonConvert.DeserializeObject<List<Annotation>>(thumbnail.OcrResult)
                };
                var annotation = ocrResult.MainAnnotation;
                annotation.Time = thumbnail.Time;
                annotations.Add(annotation);
            }

            var csvString = new StringWriter();
            using (var csv = new CsvWriter(csvString))
            {
                csv.Configuration.Delimiter = ",";
                csv.WriteField("Time");
                csv.WriteField("Altitude");
                csv.WriteField("BTY");
                csv.WriteField("Depth");
                csv.WriteField("Heading");
                csv.WriteField("Pitch");
                csv.WriteField("Roll");

                csv.WriteField("Description");
                csv.NextRecord();

                foreach (var item in annotations)
                {
                    var data = item.Description;

                    csv.WriteField(item.Time);

                    csv.WriteField(GetExtractedData(data, DataType.Altitude));
                    csv.WriteField(GetExtractedData(data, DataType.BTY));
                    csv.WriteField(GetExtractedData(data, DataType.Depth));
                    csv.WriteField(GetExtractedData(data, DataType.Heading));
                    csv.WriteField(GetExtractedData(data, DataType.Pitch));
                    csv.WriteField(GetExtractedData(data, DataType.Roll));

                    csv.WriteField(item.Description.Replace('\n', ' '));
                    csv.NextRecord();
                }
            }
            File.AppendAllText(csvPath, csvString.ToString());
        }

        public static string GetExtractedData(string data, DataType type)
        {
            string result = "";

            try
            {
                // Step 1: Get all text after the datatype
                var firstExtract = PerformFirstDataExtraction(data, type);

                // Step 2: Get all text until first alphabet
                result = PerformSecondDataExtraction(firstExtract, type);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"GetExtractedData Exception: {ex.Message}");
            }

            return result;
        }

        public static string PerformFirstDataExtraction(string data, DataType type)
        {
            var extract = "";
            var delimiters = new List<string>();
            try
            {
                switch (type)
                {
                    case DataType.Altitude:
                        delimiters.Add("ALT:"); delimiters.Add("ALT");
                        break;
                    case DataType.BTY:
                        delimiters.Add("BTY:"); delimiters.Add("BTY");
                        break;
                    case DataType.Depth:
                        delimiters.Add("DPT:"); delimiters.Add("DPT");
                        break;
                    case DataType.Heading:
                        delimiters.Add("HDG:"); delimiters.Add("HDG");
                        break;
                    case DataType.Pitch:
                        delimiters.Add("\nP:"); delimiters.Add("\nP "); delimiters.Add("\nP");
                        //delimiters.Add("P: "); delimiters.Add("P ");
                        break;
                    case DataType.Roll:
                        delimiters.Add("\nR:"); delimiters.Add("\nR "); delimiters.Add("\nR");
                        //delimiters.Add("R:"); delimiters.Add("R ");
                        break;
                    default:
                        break;
                }

                foreach (var delimiter in delimiters)
                {
                    extract = Helper.GetAfter(data, delimiter);
                    if (!string.IsNullOrEmpty(extract)) break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PerformFirstDataExtraction Exception: {ex.Message}");
            }

            return extract;
        }

        public static string PerformSecondDataExtraction(string data, DataType? type = null)
        {
            string extract = "";

            try
            {
                List<char> charList = new List<char>();
                List<char> dataList = new List<char>();
                dataList.AddRange(data);

                foreach (var _char in dataList)
                {
                    if (char.IsLetter(_char)) break;
                    charList.Add(_char);
                }
                var _extract = new string(charList.ToArray());
                _extract = _extract.Trim();

                if (type == DataType.Pitch)
                    _extract = Helper.GetBeforeString(_extract, "\n");

                if (!string.IsNullOrEmpty(_extract))
                {
                    var format = new NumberFormatInfo();
                    format.NegativeSign = "-";
                    format.NumberDecimalSeparator = ".";

                    var _data = double.Parse(_extract, format);
                    extract = _extract != "-0" ? _data.ToString() : "-0";
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"PerformSecondDataExtraction Exception: {ex.Message} - {type} = {data}");
            }

            return extract;
        }

        #region Pattern Match

        public static bool Match(List<Annotation> ocrResult)
        {
            bool retVal = THRMatched(ocrResult) &&
                          DepthMatched(ocrResult) &&
                          HeadingMatched(ocrResult) &&
                          TRNMatched(ocrResult);
            return retVal;
        }

        public static bool THRMatched(List<Annotation> ocrResult)
        {
            bool retVal = false;

            int thrMinX0 = 25; int thrMaxX0 = 35; int thrMinY0 = 70; int thrMaxY0 = 80;
            int thrMinX1 = 60; int thrMaxX1 = 70; int thrMinY1 = 70; int thrMaxY1 = 80;
            int thrMinX2 = 60; int thrMaxX2 = 70; int thrMinY2 = 60; int thrMaxY2 = 70;
            int thrMinX3 = 25; int thrMaxX3 = 35; int thrMinY3 = 60; int thrMaxY3 = 70;
            retVal = ocrResult.Exists(d => ((d.BoundingPoly.Vertices[0].X >= thrMinX0 && d.BoundingPoly.Vertices[0].X <= thrMaxX0
                                              && d.BoundingPoly.Vertices[0].Y >= thrMinY0 && d.BoundingPoly.Vertices[0].Y <= thrMaxY0) ||

                                                (d.BoundingPoly.Vertices[1].X >= thrMinX1 && d.BoundingPoly.Vertices[1].X <= thrMaxX1
                                              && d.BoundingPoly.Vertices[1].Y >= thrMinY1 && d.BoundingPoly.Vertices[1].Y <= thrMaxY1) ||

                                                (d.BoundingPoly.Vertices[2].X >= thrMinX2 && d.BoundingPoly.Vertices[2].X <= thrMaxX2
                                              && d.BoundingPoly.Vertices[2].Y >= thrMinY2 && d.BoundingPoly.Vertices[2].Y <= thrMaxY2) ||

                                                (d.BoundingPoly.Vertices[3].X >= thrMinX3 && d.BoundingPoly.Vertices[3].X <= thrMaxX3
                                              && d.BoundingPoly.Vertices[3].Y >= thrMinY3 && d.BoundingPoly.Vertices[3].Y <= thrMaxY3))

                                              && d.Description.Contains("THR")
                                                );

            return retVal;
        }

        public static bool DepthMatched(List<Annotation> ocrResult)
        {
            bool retVal = false;

            int dftMinX0 = 19; int dftMaxX0 = 29; int dftMinY0 = 73; int dftMaxY0 = 79;
            int dftMinX1 = 61; int dftMaxX1 = 71; int dftMinY1 = 73; int dftMaxY1 = 79;
            int dftMinX2 = 61; int dftMaxX2 = 71; int dftMinY2 = 86; int dftMaxY2 = 96;
            int dftMinX3 = 19; int dftMaxX3 = 29; int dftMinY3 = 86; int dftMaxY3 = 96;
            retVal = ocrResult.Exists(d => ((d.BoundingPoly.Vertices[0].X >= dftMinX0 && d.BoundingPoly.Vertices[0].X <= dftMaxX0
                                              && d.BoundingPoly.Vertices[0].Y >= dftMinY0 && d.BoundingPoly.Vertices[0].Y <= dftMaxY0) ||

                                                (d.BoundingPoly.Vertices[1].X >= dftMinX1 && d.BoundingPoly.Vertices[1].X <= dftMaxX1
                                              && d.BoundingPoly.Vertices[1].Y >= dftMinY1 && d.BoundingPoly.Vertices[1].Y <= dftMaxY1) ||

                                                (d.BoundingPoly.Vertices[2].X >= dftMinX2 && d.BoundingPoly.Vertices[2].X <= dftMaxX2
                                              && d.BoundingPoly.Vertices[2].Y >= dftMinY2 && d.BoundingPoly.Vertices[2].Y <= dftMaxY2) ||

                                                (d.BoundingPoly.Vertices[3].X >= dftMinX3 && d.BoundingPoly.Vertices[3].X <= dftMaxX3
                                              && d.BoundingPoly.Vertices[3].Y >= dftMinY3 && d.BoundingPoly.Vertices[3].Y <= dftMaxY3))

                                              && d.Description.Contains("DPT")
                                                );

            return retVal;
        }

        public static bool HeadingMatched(List<Annotation> ocrResult)
        {
            bool retVal = false;

            int hdgMinX0 = 20; int hdgMaxX0 = 30; int hdgMinY0 = 90; int hdgMaxY0 = 100;
            int hdgMinX1 = 60; int hdgMaxX1 = 70; int hdgMinY1 = 90; int hdgMaxY1 = 100;
            int hdgMinX2 = 60; int hdgMaxX2 = 70; int hdgMinY2 = 105; int hdgMaxY2 = 115;
            int hdgMinX3 = 20; int hdgMaxX3 = 30; int hdgMinY3 = 105; int hdgMaxY3 = 115;
            retVal = ocrResult.Exists(d => ((d.BoundingPoly.Vertices[0].X >= hdgMinX0 && d.BoundingPoly.Vertices[0].X <= hdgMaxX0
                                              && d.BoundingPoly.Vertices[0].Y >= hdgMinY0 && d.BoundingPoly.Vertices[0].Y <= hdgMaxY0) ||

                                                (d.BoundingPoly.Vertices[1].X >= hdgMinX1 && d.BoundingPoly.Vertices[1].X <= hdgMaxX1
                                              && d.BoundingPoly.Vertices[1].Y >= hdgMinY1 && d.BoundingPoly.Vertices[1].Y <= hdgMaxY1) ||

                                                (d.BoundingPoly.Vertices[2].X >= hdgMinX2 && d.BoundingPoly.Vertices[2].X <= hdgMaxX2
                                              && d.BoundingPoly.Vertices[2].Y >= hdgMinY2 && d.BoundingPoly.Vertices[2].Y <= hdgMaxY2) ||

                                                (d.BoundingPoly.Vertices[3].X >= hdgMinX3 && d.BoundingPoly.Vertices[3].X <= hdgMaxX3
                                              && d.BoundingPoly.Vertices[3].Y >= hdgMinY3 && d.BoundingPoly.Vertices[3].Y <= hdgMaxY3))

                                              && d.Description.Contains("HDG")
                                                );

            return retVal;
        }

        public static bool TRNMatched(List<Annotation> ocrResult)
        {
            bool retVal = false;

            int trnMinX0 = 20; int trnMaxX0 = 30; int trnMinY0 = 112; int trnMaxY0 = 122;
            int trnMinX1 = 60; int trnMaxX1 = 70; int trnMinY1 = 112; int trnMaxY1 = 122;
            int trnMinX2 = 60; int trnMaxX2 = 70; int trnMinY2 = 125; int trnMaxY2 = 135;
            int trnMinX3 = 20; int trnMaxX3 = 30; int trnMinY3 = 125; int trnMaxY3 = 135;
            retVal = ocrResult.Exists(d => ((d.BoundingPoly.Vertices[0].X >= trnMinX0 && d.BoundingPoly.Vertices[0].X <= trnMaxX0
                                              && d.BoundingPoly.Vertices[0].Y >= trnMinY0 && d.BoundingPoly.Vertices[0].Y <= trnMaxY0) ||

                                                (d.BoundingPoly.Vertices[1].X >= trnMinX1 && d.BoundingPoly.Vertices[1].X <= trnMaxX1
                                              && d.BoundingPoly.Vertices[1].Y >= trnMinY1 && d.BoundingPoly.Vertices[1].Y <= trnMaxY1) ||

                                                (d.BoundingPoly.Vertices[2].X >= trnMinX2 && d.BoundingPoly.Vertices[2].X <= trnMaxX2
                                              && d.BoundingPoly.Vertices[2].Y >= trnMinY2 && d.BoundingPoly.Vertices[2].Y <= trnMaxY2) ||

                                                (d.BoundingPoly.Vertices[3].X >= trnMinX3 && d.BoundingPoly.Vertices[3].X <= trnMaxX3
                                              && d.BoundingPoly.Vertices[3].Y >= trnMinY3 && d.BoundingPoly.Vertices[3].Y <= trnMaxY3))

                                              && d.Description.Contains("TRN")
                                                );

            return retVal;
        }

        #endregion
    }
}
