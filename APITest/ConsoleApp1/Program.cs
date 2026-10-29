
using System.Net;
using System.Net.Http.Json;

namespace ConsoleApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            const int requestCount = 5;

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7195"),
                Timeout = TimeSpan.FromSeconds(30)
            };

            var request = new
            {
                UserId = 3,
                VehicleId = 3,
                DealerId = 1,
                ServiceName = "Full Service",
                BayId = 1,
                TechnicianId = 1,
                StartTimeUtc = new DateTime(2026, 11, 2, 13, 0, 0)
            };

            var gate = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var tasks = Enumerable.Range(1, requestCount)
                .Select(async number =>
                {
                    Console.WriteLine($"Request {number}: Waiting...");

                    await gate.Task;

                    Console.WriteLine($"Request {number}: Sending...");

                    try
                    {
                        using var response = await client.PostAsJsonAsync(
                            "/api/UnifiedServiceScheduler/bookAppointment",
                            request);

                        var body =
                            await response.Content.ReadAsStringAsync();

                        Console.WriteLine(
                            $"Request {number}: {(int)response.StatusCode} " +
                            $"{response.StatusCode} | {body}");

                        return response.StatusCode;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Request {number}: ERROR - {ex}");

                        return (HttpStatusCode?)null;
                    }
                })
                .ToArray();

            Console.WriteLine("Releasing all requests...");

            gate.SetResult();

            var results = await Task.WhenAll(tasks);

            int successCount = results.Count(status =>
                status == HttpStatusCode.OK ||
                status == HttpStatusCode.Created);

            Console.WriteLine();
            Console.WriteLine($"Total requests: {requestCount}");
            Console.WriteLine($"Successful: {successCount}");

            Console.WriteLine(successCount == 1
                ? "PASS: Exactly one request succeeded."
                : "FAIL: Expected exactly one successful request.");
        }
    }
}
