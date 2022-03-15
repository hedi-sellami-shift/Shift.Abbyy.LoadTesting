using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shift.Ocr.Abbyy.Load
{
    public class RequestHandler
    {
        public RequestHandler(string label = "")
        {
            _label = label;
            _myHttpClient = new();
            _content = new();
        }

        private readonly string _label;
        private readonly HttpClient _myHttpClient;
        private readonly MultipartFormDataContent _content;

        private void BuildRequestContent(string documentsDirectoryPath)
        {
            var filesPaths = Directory.GetFiles(documentsDirectoryPath);

            foreach (var filePath in filesPaths)
            {
                var fileInfo = new FileInfo(filePath);
                var fileStream = File.OpenRead(filePath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.Add("Content-Type", "multipart/form-data");
                fileContent.Headers.Add("Content-Disposition",
                    "form-data; name=\"file\"; filename=\"" + fileInfo.Name + "\"");
                _content.Add(fileContent, "file", fileInfo.Name);
            }
        }

        public async Task SendRequest(string requestUrl, string documentsDirectoryPath)
        {
            BuildRequestContent(documentsDirectoryPath);
            var timer = new Stopwatch();
            timer.Start();
            var task = _myHttpClient.PostAsync(requestUrl, _content);
            Console.WriteLine($"Request {_label}: Firing");

            var result = await task;
            Console.WriteLine($"Request {_label}: status={result.StatusCode}\ttime={timer.Elapsed}");
        }
    }
}