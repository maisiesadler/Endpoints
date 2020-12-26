using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<EndpointApiVsControllerApi>();

            Console.WriteLine("Done!");
        }

    }

    public class EndpointApiVsControllerApi
    {
        private IHttpClientFactory _httpClientFactory;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            var sp = services.BuildServiceProvider();
            _httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
        }

        [Benchmark]
        public async Task Api() => await PostAsync("http://localhost:5002/user");

        [Benchmark]
        public async Task CApi() => await PostAsync("http://localhost:5003/user");

        private async Task PostAsync(string requestUri)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                await client.PostAsync(requestUri, new StringContent(@"{""User"": ""Maisie""}"));
            }
        }
    }
}
