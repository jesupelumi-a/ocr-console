using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using OCR.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OCR.Abstractions;

namespace OCR.Business
{
    public class GoogleOcrService : IOcrService
    {
        public async Task<string> ExtractText(string imagePath, int? index)
        {
            string result = "";

            try
            {
                //google auth api call
                string jsonPath = Path.Combine(Environment.CurrentDirectory, @"Google\credentials.json");
                var credential = GoogleCredential.FromFile(jsonPath)
                    .CreateScoped(ImageAnnotatorClient.DefaultScopes);
                var channel = new Grpc.Core.Channel(
                    ImageAnnotatorClient.DefaultEndpoint.ToString(),
                    credential.ToChannelCredentials());

                // Instantiates a client
                var client = ImageAnnotatorClient.Create(channel);
                // Load the image file
                var image = Image.FromFile(imagePath);
                // Perform label detection on the image file
                var response = await client.DetectTextAsync(image);

                JToken parsedJson = JToken.Parse(response.ToString());
                var beautified = parsedJson.ToString(Formatting.Indented);
                var minified = parsedJson.ToString(Formatting.None);

                Console.WriteLine($"OCR Performed - {index}");

                //string txtPath = Path.Combine(Helper.GetBeforeLastIndexOf(imagePath, '.') + ".txt");
                //if (File.Exists(txtPath)) File.Delete(txtPath);
                //File.AppendAllText(txtPath, beautified);

                result = beautified;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OcrService Exception: {ex.Message}");
            }

            return result;
        }
    }
}
