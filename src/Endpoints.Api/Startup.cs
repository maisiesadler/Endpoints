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
                foreach (var (type, handler) in DiscoveryExtensions.GetHandlers(this.GetType().Assembly))
                {
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                    {
                        System.Console.WriteLine("No empty constructor");
                        continue;
                    }

                    foreach (var (methodInfo, method) in DiscoveryExtensions.GetMethods(type))
                    {
                        var endpoint = handler.Endpoint + method.Endpoint;
                        if (method is GetAttribute get)
                        {
                            endpoints.MapGet(endpoint, async context =>
                            {
                                using var scope = endpoints.ServiceProvider.CreateScope();
                                var handler = scope.ServiceProvider.GetRequiredService(type);

                                var @params = new List<object>();
                                foreach (var p in methodInfo.GetParameters())
                                {
                                    if (context.Request.RouteValues.TryGetValue(p.Name, out var paramValue))
                                    {
                                        @params.Add(paramValue);
                                    }
                                    else
                                    {
                                        @params.Add(null);
                                    }
                                }
                                var r = (IHandlerResponse)methodInfo.Invoke(handler, @params.ToArray());
                                await context.Response.WriteAsync(r.Response());
                            });
                        }
                    }
                }
            });
        }
    }
}
