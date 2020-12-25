using Endpoints.Extensions;
using Example.Api.Adapter;
using Example.Api.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Example.Api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDatabase, MemoryDatabase>();
            services.AddTransient<CreateUserInteractor>();
            services.AddTransient<GetUserInteractor>();
            services.AddTransient<ErrorHandlingMiddleware<GetUserResponse>>();

            services.AddPipeline<CreateUserRequest, CreateUserResponse>(
                ModelParser.CreateUserRequestFromBody,
                ModelParser.SetJsonResponse<CreateUserResponse>
            );
            services.AddPipeline<GetUserRequest, GetUserResponse>(
                ModelParser.GetUserRequestFromPath,
                ModelParser.SetGetUserResponse,
                builder => builder.WithMiddleware<ErrorHandlingMiddleware<GetUserResponse>>()
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/user/{id}", endpoints.ServiceProvider.Get<GetUserInteractor, GetUserRequest, GetUserResponse>());
                endpoints.MapPost("/user", endpoints.ServiceProvider.Get<CreateUserInteractor, CreateUserRequest, CreateUserResponse>());
            });
        }
    }
}
