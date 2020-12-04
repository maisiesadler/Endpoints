using Endpoints.Api.Pipelines;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            services.AddSingleton<IDbThing, DbThing>();

            services.AddTransient<MyModelPipeline>(sp => new MyModelPipeline(new TimingPipelineStage(new ExceptionHandlingPipelineStage(new GetModelFromDatabase(sp.GetRequiredService<IDbThing>())))));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/testing/{id}", async ctx => await endpoints.ServiceProvider.GetRequiredService<MyModelPipeline>().Run(ctx));
            });
        }
    }
}
