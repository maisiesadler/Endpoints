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
            services.RegisterRetrievePipeline<ModelRequest, ModelResponse>(
                MyModelRetriever.ParseModel,
                MyModelRetriever.ParseResponse
            );
                // builder => builder.WithStage<TimingPipelineStage>()
                //     .WithStage<ExceptionHandlingPipelineStage>()
                //     .WithStage<GetModelFromDatabase>());

            services.RegisterRetrievePipeline<ModelRequest, PipelineResponse<CreateModelRetriever.Response>>(
                CreateModelRetriever.ParseModel,
                CreateModelRetriever.ParseResponse
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                var registry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();

                endpoints.MapPost("/testing", registry.GetRetrieve<CreateModelRetriever, ModelRequest, PipelineResponse<CreateModelRetriever.Response>>());
                endpoints.MapGet("/testing/{id}", registry.GetRetrieve<MyModelRetriever, ModelRequest, ModelResponse>());
            });
        }
    }
}
