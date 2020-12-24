using Endpoints.Api.Pipelines;
using Endpoints.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            services.AddSingleton<IDbThing, DbThing>();
            services.AddSingleton<CreateModelRetriever>();
            services.AddSingleton<MyModelRetriever>();
            services.AddSingleton<TimingMiddleware>();
            services.AddSingleton<ExceptionHandlingMiddleware>();

            services.AddPipelines();
            services.AddPipeline<ModelRequest, ModelResponse>(
                ModelParser.ParseModel,
                ModelParser.ParseResponse,
                builder => builder
                    .WithMiddleware<ExceptionHandlingMiddleware>()
                    .WithMiddleware<TimingMiddleware>()
            );

            services.AddPipeline<ModelRequest, CreateModelRetriever.Response>(
                ModelParser.ParseModel,
                ModelParser.ParseCreateModelResponse
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                var registry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();

                endpoints.MapPost("/testing", registry.Get<CreateModelRetriever, ModelRequest, CreateModelRetriever.Response>());
                endpoints.MapGet("/testing/{id}", registry.Get<MyModelRetriever, ModelRequest, ModelResponse>());
            });
        }
    }
}
