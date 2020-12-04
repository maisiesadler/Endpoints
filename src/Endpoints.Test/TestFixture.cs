using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Endpoints.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Endpoints.Test
{
    public class TestFixture
    {
        public TestServer CreateServer() => new TestServer(GetWebHostBuilder());
        public TestServer CreateServer(Action<IServiceCollection> configureServices, Action<IApplicationBuilder> configureApp)
            => new TestServer(GetWebHostBuilder(configureServices, configureApp));

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

        private IWebHostBuilder GetWebHostBuilder(
            Action<IServiceCollection> configureServices,
            Action<IApplicationBuilder> configureApp)
        {
            return new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    // config.AddJsonFile("testsettings.json", optional: false, reloadOnChange: false);
                })
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    configureServices(services);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    configureApp(app);
                });
        }
    }
}
