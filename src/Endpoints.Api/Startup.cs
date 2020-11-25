using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Endpoints.Api.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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
            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Endpoints.Api", Version = "v1" });
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            //     app.UseSwagger();
            //     app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Endpoints.Api v1"));
            // }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                foreach (var (type, handler) in GetTypesWithAttribute<Handlers.HandlerAttribute>(this.GetType().Assembly))
                {
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                    {
                        System.Console.WriteLine("No empty constructor");
                        continue;
                    }
                    foreach (var (methodInfo, method) in GetMethodsWithAttribute<MethodAttribute>(type))
                    {
                        if (!methodInfo.ReturnType.IsAssignableTo(typeof(IHandlerResponse)))
                        {
                            System.Console.WriteLine("Unknown return type" + methodInfo.ReturnType);
                            continue;
                        }

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

        static IEnumerable<(Type, T)> GetTypesWithAttribute<T>(Assembly assembly)
            where T : Attribute
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<T>();
                if (attr != null)
                {
                    yield return (type, attr);
                }
            }
        }

        static IEnumerable<(MethodInfo, T)> GetMethodsWithAttribute<T>(Type type)
            where T : Attribute
        {
            foreach (MethodInfo method in type.GetMethods())
            {
                var attr = method.GetCustomAttribute<T>();
                if (attr != null)
                {
                    yield return (method, attr);
                }
            }
        }
    }
}
