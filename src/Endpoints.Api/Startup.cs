using System;
using System.Collections.Generic;
using Endpoints.Api.Handlers;
using Endpoints.Attributes;
using Endpoints.Extensions;
using Endpoints.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoints.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            services.AddScoped<TestHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                foreach (var endpoint in RequestDelegateExtensions.GetEndpoints(this.GetType().Assembly, endpoints.ServiceProvider))
                {
                    endpoints.MapGet(endpoint.Name, endpoint.RequestDelegate);
                }
            });
        }
    }
}
