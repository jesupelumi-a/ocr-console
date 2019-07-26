using CsvHelper;
using Newtonsoft.Json;
using OCR.Abstractions;
using OCR.Abstractions.Enums;
using OCR.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OMV.Layouts
{
    public class LayoutB
    {
        public static bool IsKpType { get; set; }

        public bool MatchAndCreateCSV(List<Thumbnail> thumbnails, string csvPath)
        {
            bool retVal = false;

            try
            {
                // Step 1: Get the first 5 ocr results
                var thumbs = thumbnails.Take(5);

                foreach (var thumb in thumbs)
                {
                    var annotations = JsonConvert.DeserializeObject<List<Annotation>>(thumb.OcrResult);
                    if (IsKpMatched(annotations))
                    {
                        IsKpType = true;
                        break;
                    }
                }

                foreach (var thumb in thumbs)
                {
                    var annotations = JsonConvert.DeserializeObject<List<Annotation>>(thumb.OcrResult);
                    if (Match(annotations))
                    {
                        CreateCSV(thumbnails, csvPath);
                        retVal = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OMV.Layouts - LayoutA: {ex.Message}");
            }

            return retVal;
        }

        public static void CreateCSV(List<Thumbnail> thumbnails, string csvPath)
        {
            var annotations = new List<Annotation>();
            int i = 0;
            foreach (var thumbnail in thumbnails)
            {
                var ocrResult = new OcrResult
                {
                    Annotations = JsonConvert.DeserializeObject<List<Annotation>>(thumbnail.OcrResult)
                };
                var annotation = ocrResult.MainAnnotation;
                annotation.Time = thumbnail.Time;
                annotations.Add(annotation);
                i++;
            }

            var csvString = new StringWriter();
            using (var csv = new CsvWriter(csvString))
            {
                csv.Configuration.Delimiter = ",";
                csv.WriteField("Time");
                csv.WriteField("Easting");
                csv.WriteField("Northing");
                csv.WriteField("Depth");
                csv.WriteField("Heading");
                if (IsKpType) csv.WriteField("Kp");
                csv.WriteField("Description");
                csv.NextRecord();

                i = 0;
                foreach (var item in annotations)
                {
                    var data = !string.IsNullOrEmpty(item.Description) ? item.Description : "";

                    csv.WriteField(item.Time);
                    csv.WriteField(GetExtractedData(data, DataType.Easting));
                    csv.WriteField(GetExtractedData(data, DataType.Northing));
                    csv.WriteField(GetExtractedData(data, DataType.Depth));
                    csv.WriteField(GetExtractedData(data, DataType.Heading));
                    if (IsKpType) csv.WriteField(GetExtractedData(data, DataType.Kp));
                    csv.WriteField(data.Replace('\n', ' '));
                    csv.NextRecord();
                    i++;
                }
            }
            File.AppendAllText(csvPath, csvString.ToString());
        }

        public static string GetExtractedData(string data, DataType type)
        {
            string result = "";

            try
            {
                if (string.IsNullOrEmpty(data))
                    return result;
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
                    case DataType.Easting:
                        delimiters.Add("\nE:"); delimiters.Add("\nE");
                        break;
                    case DataType.Northing:
                        delimiters.Add("N:"); delimiters.Add("N");
                        break;
                    case DataType.Depth:
                        delimiters.Add("\nD:"); delimiters.Add("\nD");
                        delimiters.Add("\nB:"); delimiters.Add("\nB");
                        break;
                    case DataType.Heading:
                        delimiters.Add("\nH:"); delimiters.Add("\nH");
                        break;
                    case DataType.Kp:
                        delimiters.Add("\nKP:"); delimiters.Add("\nKP");
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

                _extract = Helper.GetBeforeString(_extract, "\n");
                _extract = _extract.Replace(' ', '.').Replace(',', '.');
                _extract = Helper.RemoveAllExceptLastIndexOf(_extract, ".");

                if (!string.IsNullOrEmpty(_extract))
                {
                    var format = new NumberFormatInfo();
                    format.NegativeSign = "-";
                    format.NumberDecimalSeparator = ".";

                    if (double.TryParse(_extract, NumberStyles.Any, format, out double _data))
                        extract = _data.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PerformSecondDataExtraction Exception: {ex.Message} - {type} = {data}");
            }

            return extract;
        }

        #region Pattern Match

        public static bool Match(List<Annotation> ocrResult)
        {
            bool retVal = IsEastingMatched(ocrResult) &&
                          IsNorthingMatched(ocrResult) &&
                          IsDepthMatched(ocrResult);

            return retVal;
        }

        public static bool IsEastingMatched(List<Annotation> ocrResult)
        {
            bool retVal = false;

            int eastingMinX0 = 421; int eastingMaxX0 = 431; int eastingMinY0 = 15; int eastingMaxY0 = 25;
            int eastingMinX1 = 440; int eastingMaxX1 = 670; int eastingMinY1 = 15; int eastingMaxY1 = 25;
            int eastingMinX2 = 440; int eastingMaxX2 = 670; int eastingMinY2 = 35; int eastingMaxY2 = 45;
            int eastingMinX3 = 421; int eastingMaxX3 = 431; int eastingMinY3 = 35; int eastingMaxY3 = 45;
            retVal = ocrResult.Exists(d => ((d.BoundingPoly.Vertices[0].X >= eastingMinX0 && d.BoundingPoly.Vertices[0].X <= eastingMaxX0
                                              && d.BoundingPoly.Vertices[0].Y >= eastingMinY0 && d.BoundingPoly.Vertices[0].Y <= eastingMaxY0) ||

                                                (d.BoundingPoly.Vertices[1].X >= eastingMinX1 && d.BoundingPoly.Vertices[1].X <= eastingMaxX1
                                              && d.BoundingPoly.Vertices[1].Y >= eastingMinY1 && d.BoundingPoly.Vertices[1].Y <= eastingMaxY1) ||

                                                (d.BoundingPoly.Vertices[2].X >= eastingMinX2 && d.BoundingPoly.Vertices[2].X <= eastingMaxX2
                                              && d.BoundingPoly.Vertices[2].Y >= eastingMinY2 && d.BoundingPoly.Vertices[2].Y <= eastingMaxY2) ||

                                                (d.BoundingPoly.Vertices[3].X >= eastingMinX3 && d.BoundingPoly.Vertices[3].X <= eastingMaxX3
                                              && d.BoundingPoly.Vertices[3].Y >= eastingMinY3 && d.BoundingPoly.Vertices[3].Y <= eastingMaxY3))

                                              && d.Description.Contains("E")
                                                );

            return retVal;
        }

        public static bool IsNorthingMatched(List<Annotation> ocrResult)
        {
            bool retVal = false;

            int dftMinX0 = 545; int dftMaxX0 = 555; int dftMinY0 = 15; int dftMaxY0 = 25;
            int dftMinX1 = 560; int dftMaxX1 = 675; int dftMinY1 = 15; int dftMaxY1 = 25;
            int dftMinX2 = 560; int dftMaxX2 = 675; int dftMinY2 = 25; int dftMaxY2 = 35;
            int dftMinX3 = 545; int dftMaxX3 = 555; int dftMinY3 = 25; int dftMaxY3 = 35;
            retVal = ocrResult.Exists(d => ((d.BoundingPoly.Vertices[0].X >= dftMinX0 && d.BoundingPoly.Vertices[0].X <= dftMaxX0
                                              && d.BoundingPoly.Vertices[0].Y >= dftMinY0 && d.BoundingPoly.Vertices[0].Y <= dftMaxY0) ||

                                                (d.BoundingPoly.Vertices[1].X >= dftMinX1 && d.BoundingPoly.Vertices[1].X <= dftMaxX1
                                              && d.BoundingPoly.Vertices[1].Y >= dftMinY1 && d.BoundingPoly.Vertices[1].Y <= dftMaxY1) ||

                                                (d.BoundingPoly.Vertices[2].X >= dftMinX2 && d.BoundingPoly.Vertices[2].X <= dftMaxX2
                                              && d.BoundingPoly.Vertices[2].Y >= dftMinY2 && d.BoundingPoly.Vertices[2].Y <= dftMaxY2) ||

                                                (d.BoundingPoly.Vertices[3].X >= dftMinX3 && d.BoundingPoly.Vertices[3].X <= dftMaxX3
                                              && d.BoundingPoly.Vertices[3].Y >= dftMinY3 && d.BoundingPoly.Vertices[3].Y <= dftMaxY3))

                                              && d.Description.Contains("N")
                                                );

            return retVal;
        }

        public static bool IsDepthMatched(List<Annotation> ocrResult)
        {
            bool retVal = false;

            int hdgMinX0 = 33; int hdgMaxX0 = 43; int hdgMinY0 = 45; int hdgMaxY0 = 55;
            int hdgMinX1 = 50; int hdgMaxX1 = 60; int hdgMinY1 = 45; int hdgMaxY1 = 55;
            int hdgMinX2 = 50; int hdgMaxX2 = 60; int hdgMinY2 = 60; int hdgMaxY2 = 70;
            int hdgMinX3 = 33; int hdgMaxX3 = 43; int hdgMinY3 = 60; int hdgMaxY3 = 70;
            retVal = ocrResult.Exists(d => ((d.BoundingPoly.Vertices[0].X >= hdgMinX0 && d.BoundingPoly.Vertices[0].X <= hdgMaxX0
                                              && d.BoundingPoly.Vertices[0].Y >= hdgMinY0 && d.BoundingPoly.Vertices[0].Y <= hdgMaxY0) ||

                                                (d.BoundingPoly.Vertices[1].X >= hdgMinX1 && d.BoundingPoly.Vertices[1].X <= hdgMaxX1
                                              && d.BoundingPoly.Vertices[1].Y >= hdgMinY1 && d.BoundingPoly.Vertices[1].Y <= hdgMaxY1) ||

                                                (d.BoundingPoly.Vertices[2].X >= hdgMinX2 && d.BoundingPoly.Vertices[2].X <= hdgMaxX2
                                              && d.BoundingPoly.Vertices[2].Y >= hdgMinY2 && d.BoundingPoly.Vertices[2].Y <= hdgMaxY2) ||

                                                (d.BoundingPoly.Vertices[3].X >= hdgMinX3 && d.BoundingPoly.Vertices[3].X <= hdgMaxX3
                                              && d.BoundingPoly.Vertices[3].Y >= hdgMinY3 && d.BoundingPoly.Vertices[3].Y <= hdgMaxY3))

                                              && d.Description.Contains("D")
                                                );

            return retVal;
        }

        public static bool IsKpMatched(List<Annotation> ocrResult)
        {
            bool retVal = false;

            int minX0 = 422; int maxX0 = 432; int minY0 = 50; int maxY0 = 60;
            int minX1 = 450; int maxX1 = 525; int minY1 = 50; int maxY1 = 60;
            int minX2 = 450; int maxX2 = 525; int minY2 = 70; int maxY2 = 70;
            int minX3 = 422; int maxX3 = 432; int minY3 = 70; int maxY3 = 70;
            retVal = ocrResult.Exists(d => ((d.BoundingPoly.Vertices[0].X >= minX0 && d.BoundingPoly.Vertices[0].X <= maxX0
                                              && d.BoundingPoly.Vertices[0].Y >= minY0 && d.BoundingPoly.Vertices[0].Y <= maxY0) ||

                                                (d.BoundingPoly.Vertices[1].X >= minX1 && d.BoundingPoly.Vertices[1].X <= maxX1
                                              && d.BoundingPoly.Vertices[1].Y >= minY1 && d.BoundingPoly.Vertices[1].Y <= maxY1) ||

                                                (d.BoundingPoly.Vertices[2].X >= minX2 && d.BoundingPoly.Vertices[2].X <= maxX2
                                              && d.BoundingPoly.Vertices[2].Y >= minY2 && d.BoundingPoly.Vertices[2].Y <= maxY2) ||

                                                (d.BoundingPoly.Vertices[3].X >= minX3 && d.BoundingPoly.Vertices[3].X <= maxX3
                                              && d.BoundingPoly.Vertices[3].Y >= minY3 && d.BoundingPoly.Vertices[3].Y <= maxY3))

                                              && d.Description.Contains("KP")
                                                );

            return retVal;
        }

        #endregion
    }
}
