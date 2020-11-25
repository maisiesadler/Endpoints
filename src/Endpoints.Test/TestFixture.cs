using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Endpoints.Api;

namespace Endpoints.Test
{
    public class TestFixture : IDisposable
    {
        public TestServer CreateServer() => new TestServer(GetWebHostBuilder());
        
        public void Dispose()
        {
        }

        private IWebHostBuilder GetWebHostBuilder()
        {
            return new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    // config.AddJsonFile("testsettings.json", optional: false, reloadOnChange: false);
                })
                .UseStartup<Startup>();
        }
    }
}
