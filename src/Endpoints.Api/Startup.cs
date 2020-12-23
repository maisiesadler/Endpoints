using Endpoints.Api.Pipelines;
using Endpoints.Extensions;
using Endpoints.Pipelines;
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

            services.AddPipelines();
            services.RegisterPipeline<MyModelPipeline, ModelRequest, ModelResponse>();
                // builder => builder.WithStage<TimingPipelineStage>()
                //     .WithStage<ExceptionHandlingPipelineStage>()
                //     .WithStage<GetModelFromDatabase>());

            services.RegisterPipeline<CreateModelPipeline, ModelRequest, PipelineResponse<CreateModelPipeline.Response>>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                var registry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();

                endpoints.MapPost("/testing", registry.Get<CreateModelPipeline, ModelRequest, PipelineResponse<CreateModelPipeline.Response>>());
                endpoints.MapGet("/testing/{id}", registry.Get<MyModelPipeline, ModelRequest, ModelResponse>());
            });
        }
    }
}
