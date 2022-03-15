using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

namespace Shift.Ocr.Abbyy.Load
{
    class Program
    {
        const string baseAdress = "http://document-abbyocr-staging.corp.shift-technology.com/";
        const string baseLocalAdress = "http://localhost:5000/";

        static async Task Main(string[] args)
        {
            int numberRequests = 128;

            Task[] tasks = new Task[numberRequests];
            RequestHandler[] requestHandlers = new RequestHandler[numberRequests];

            for (int i = 0; i < numberRequests; i++)
            {
                requestHandlers[i] = new RequestHandler(i.ToString());
                var task = requestHandlers[i].SendRequest($"{baseAdress}analyze",
                    "C:\\Users\\hedi.sellami\\Desktop\\Shift.Ocr.Abbyy.Load\\BenchmarkDocuments");
                tasks[i] = task;
            }

            Task.WaitAll(tasks);


            // var analyzeScenario =
            //     CreateScenario(1, 30, 5, 
            //         "analyze", "analyze_docs_step", "Post",
            //         "analyze_docs_scenario");
            // NBomberRunner
            //     .RegisterScenarios(analyzeScenario)
            //     .Run();
        }


        private static Scenario CreateScenario(int rate, int fromSeconds, int warmUpFromSeconds, string path,
            string nameStep, string method, string nameScenario)
        {
            var myHttpClient = new HttpClient();
            myHttpClient.Timeout = TimeSpan.FromMinutes(4);
            var step = Step.Create(nameStep,
                clientFactory: HttpClientFactory.Create("factory", myHttpClient),
                async context =>
                {
                    var filesPaths = Directory.GetFiles("C:\\Users\\bensla\\Documents\\ShiftDocuments");
                    using var content = new MultipartFormDataContent();

                    foreach (var filePath in filesPaths)
                    {
                        var fileInfo = new FileInfo(filePath);
                        var fileStream = System.IO.File.OpenRead(filePath);
                        var fileContent = new StreamContent(fileStream);
                        fileContent.Headers.Add("Content-Type", "multipart/form-data");
                        fileContent.Headers.Add("Content-Disposition",
                            "form-data; name=\"file\"; filename=\"" + fileInfo.Name + "\"");
                        content.Add(fileContent, "file", fileInfo.Name);
                    }

                    var request = Http.CreateRequest(method, $"{baseAdress}{path}")
                        .WithHeader("Language", "French")
                        .WithBody(content)
                        .WithCheck((response) =>
                            Task.FromResult(response.IsSuccessStatusCode
                                ? Response.Ok()
                                : Response.Fail())
                        );
                    return await Http.Send(request, context);
                });


            return ScenarioBuilder
                .CreateScenario(nameScenario, step)
                .WithWarmUpDuration(TimeSpan.FromSeconds(warmUpFromSeconds))
                .WithLoadSimulations(
                    //Simulation.RampConstant(1,TimeSpan.FromSeconds(fromSeconds)));
                    Simulation.InjectPerSec(rate: rate, TimeSpan.FromSeconds(fromSeconds)));
        }
    }
}