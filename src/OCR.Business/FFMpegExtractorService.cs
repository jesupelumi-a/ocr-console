using FFMpegCore;
using FFMpegCore.FFMPEG;
using OCR.Abstractions;
using OCR.Abstractions.Models;
using OCR.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OCR.Business
{
    public class FFMpegExtractorService : IImageExtractorService
    {
        public async Task<List<Thumbnail>> SplitAsync(string videoPath, int duration = 10, bool captureFirstScreen = true, bool captureLastScreen = true)
        {
            var thumbnails = new List<Thumbnail>();

            try
            {
                FFMpegStartup();

                // Step 1: Get the video content

                string fileName = Helper.GetFileName(videoPath);
                string extension = Helper.GetExtension(videoPath);

                var folderName = fileName;

                // Step 2: Create a folder with the video name
                var imageFolder = Directory.CreateDirectory(folderName);

                var tempImagePlaceholder = Path.Combine(fileName, "frame-{0}.png");

                // Step 3: Split the video into several images after every n seconds
                thumbnails = VideoSplit(videoPath, tempImagePlaceholder, duration, captureFirstScreen, captureLastScreen);

                Console.WriteLine("Image Extraction Done!!!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ImageExtractorService Exception: {ex.Message}");
            }

            return await Task.FromResult(thumbnails);
        }

        public Task<List<Thumbnail>> TestSplitAsync(string videoPath, int duration = 10, bool captureFirstScreen = true, bool captureLastScreen = true)
        {
            var thumbnails = new List<Thumbnail>();

            try
            {
                // Get all files in the folder
                DirectoryInfo d = new DirectoryInfo(videoPath);
                FileInfo[] Files = d.GetFiles("*.png");
                int i = 0;
                foreach (FileInfo file in Files)
                {
                    TimeSpan time = TimeSpan.FromSeconds(i);
                    string format = time.ToString(@"hh\:mm\:ss");

                    var source = file.FullName.Replace(@"\", @"\\");
                    var ocrPath = Path.Combine(Helper.GetBeforeLastIndexOf(source, '.') + ".txt");
                    var ocr = File.ReadAllText(ocrPath);

                    var thumbnail = new Thumbnail()
                    {
                        Source = source,
                        Time = format,
                        OcrResult = ocr
                    };
                    thumbnails.Add(thumbnail);
                    i += 10;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ImageExtractorService Exception: {ex.Message}");
            }

            return Task.FromResult(thumbnails);
        }

        private static List<Thumbnail> VideoSplit(string videoPath, string outputPath, int duration, bool captureFirstScreen, bool captureLastScreen)
        {
            try
            {
                var files = new List<Thumbnail>();
                var video = VideoInfo.FromPath(videoPath);

                string output = video.ToString();
                Console.WriteLine($"Video Duration: {video.Duration}");

                // Capture First Screen of Video
                if (captureFirstScreen)
                {
                    var imageOutput = string.Format(outputPath, 0);
                    var thumbnail = GenerateThumnail(video, outputPath, 0);
                    files.Add(thumbnail);
                }
                int i = duration;
                while (i < video.Duration.TotalSeconds)
                {
                    var newOutputPath = string.Format(outputPath, i);

                    var thumbnail = GenerateThumnail(video, outputPath, i);
                    files.Add(thumbnail);
                    i += duration;
                }
                // Capture Last Screen of Video
                if (captureLastScreen)
                {
                    int finalSecond = Convert.ToInt32(video.Duration.TotalSeconds);
                    var imageOutput = string.Format(outputPath, finalSecond);
                    var thumbnail = GenerateThumnail(video, outputPath, finalSecond);
                    files.Add(thumbnail);
                }
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VideoSplit error: {ex.Message}");
                throw ex;
            }
        }

        private static Thumbnail GenerateThumnail(VideoInfo video, string outputPath, int duration)
        {
            try
            {
                TimeSpan timeDuration = TimeSpan.FromSeconds(duration);

                TimeSpan time = TimeSpan.FromSeconds(duration);
                string format = time.ToString(@"hh\:mm\:ss");
                string format2 = time.ToString(@"hh\_mm\_ss");

                outputPath = string.Format(outputPath, format2);
                FileInfo output = new FileInfo(outputPath);

                if (File.Exists(outputPath)) File.Delete(outputPath);

                new FFMpeg()
                    .Snapshot(
                        video,
                        output,
                        null,
                        timeDuration,
                        true
                    );

                var thumbnail = new Thumbnail
                {
                    Source = outputPath,
                    Time = format
                };
                Console.WriteLine($"Thumbnail generated: {outputPath}");
                return thumbnail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateThumbnail error: {ex.Message}");
                throw ex;
            }
        }

        public void FFMpegStartup()
        {
            var CurrentDirectory = Environment.CurrentDirectory;
            var rootDirectory = Path.Combine(CurrentDirectory, "lib");
            FFMpegOptions.Configure(new FFMpegOptions { RootDirectory = rootDirectory });
        }
    }
}
