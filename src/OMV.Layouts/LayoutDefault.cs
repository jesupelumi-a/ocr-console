using CsvHelper;
using Newtonsoft.Json;
using OCR.Abstractions;
using OCR.Abstractions.Enums;
using OCR.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OMV.Layouts
{
    public class LayoutDefault
    {
        public bool MatchAndCreateCSV(List<Thumbnail> thumbnails, string csvPath)
        {
            bool retVal = false;

            try
            {
                CreateCSV(thumbnails, csvPath);
                return true;
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
                csv.WriteField("Easting");
                csv.WriteField("Northing");
                csv.WriteField("Heading");
                csv.WriteField("KP");
                csv.WriteField("Pitch");
                csv.WriteField("Roll");
                csv.WriteField("THR");
                csv.WriteField("TRN");

                csv.WriteField("Description");
                csv.NextRecord();

                foreach (var item in annotations)
                {
                    var data = item.Description;

                    csv.WriteField(item.Time);

                    csv.WriteField(GetExtractedData(data, DataType.Altitude));
                    csv.WriteField(GetExtractedData(data, DataType.BTY));
                    csv.WriteField(GetExtractedData(data, DataType.Depth));
                    csv.WriteField(GetExtractedData(data, DataType.Easting));
                    csv.WriteField(GetExtractedData(data, DataType.Northing));
                    csv.WriteField(GetExtractedData(data, DataType.Heading));
                    csv.WriteField(GetExtractedData(data, DataType.KP));
                    csv.WriteField(GetExtractedData(data, DataType.Pitch));
                    csv.WriteField(GetExtractedData(data, DataType.Roll));
                    csv.WriteField(GetExtractedData(data, DataType.THR));
                    csv.WriteField(GetExtractedData(data, DataType.TRN));

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
                result = PerformSecondDataExtraction(firstExtract);
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
                        delimiters.Add("ALTITUDE:"); delimiters.Add("ALTITUDE");
                        delimiters.Add("Altitude:"); delimiters.Add("Altitude");
                        delimiters.Add("ALT:"); delimiters.Add("ALT");
                        delimiters.Add("Alt:"); delimiters.Add("Alt");
                        delimiters.Add(" A:"); delimiters.Add(" A ");
                        break;
                    case DataType.BTY:
                        delimiters.Add("BTY:"); delimiters.Add("BTY");
                        break;
                    case DataType.Depth:
                        delimiters.Add("DEPTH:"); delimiters.Add("DEPTH");
                        delimiters.Add("Depth:"); delimiters.Add("Depth");
                        delimiters.Add("DPT:"); delimiters.Add("DPT");
                        delimiters.Add("DEP:"); delimiters.Add("DEP");
                        delimiters.Add("Dep:"); delimiters.Add("Dep");
                        delimiters.Add(" D:"); delimiters.Add(" D ");
                        break;
                    case DataType.Easting:
                        delimiters.Add("EASTING"); delimiters.Add("EASTING:");
                        delimiters.Add("Easting:"); delimiters.Add("Easting");
                        delimiters.Add(" E:"); delimiters.Add(" E ");
                        break;
                    case DataType.Northing:
                        delimiters.Add("NORTHING:"); delimiters.Add("NORTHING");
                        delimiters.Add("Northing:"); delimiters.Add("Northing");
                        delimiters.Add(" N:"); delimiters.Add(" N ");
                        break;
                    case DataType.Heading:
                        delimiters.Add("Heading:"); delimiters.Add("Heading");
                        delimiters.Add("HDG:"); delimiters.Add("HDG");
                        delimiters.Add("Hdg:"); delimiters.Add("Hdg");
                        delimiters.Add(" H:"); delimiters.Add(" H ");
                        break;
                    case DataType.KP:
                        delimiters.Add("KP:"); delimiters.Add("KP");
                        delimiters.Add("Kp:"); delimiters.Add("Kp");
                        break;
                    case DataType.Pitch:
                        delimiters.Add("PITCH:"); delimiters.Add("PITCH");
                        delimiters.Add("Pitch:"); delimiters.Add("Pitch");
                        delimiters.Add(" P:"); delimiters.Add(" P ");
                        break;
                    case DataType.Roll:
                        delimiters.Add("ROLL:"); delimiters.Add("ROLL");
                        delimiters.Add("Roll:"); delimiters.Add("Roll");
                        delimiters.Add(" R:"); delimiters.Add(" Re ");
                        break;
                    case DataType.TRN:
                        delimiters.Add("TRN:"); delimiters.Add("TRN");
                        break;
                    case DataType.THR:
                        delimiters.Add("THR:"); delimiters.Add("THR");
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
                System.Console.WriteLine($"PerformFirstDataExtraction Exception: {ex.Message}");
            }

            return extract;
        }

        public static string PerformSecondDataExtraction(string data)
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
                extract = new string(charList.ToArray());
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"PerformSecondDataExtraction Exception: {ex.Message}");
            }

            return extract.Trim();
        }
    }
}
