using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Api.Pipelines;
using Endpoints.Instructions;
using Endpoints.Extensions;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using System.Diagnostics;

namespace Endpoints.Test
{
    public class RetrievePipelineBuilderTests
    {
        [Fact]
        public void CanCreatePipeline()
        {
            // Arrange
            var instructions = new RetrievePipelineInstructions<ModelRequest, ModelResponse>(
                ModelParser.FromBody,
                ModelParser.SetFromModelResponse);

            var services = new ServiceCollection();
            services.AddTransient<DatabaseRetriever>();
            services.AddTransient<IDbThing, DbThing>();
            var sp = services.BuildServiceProvider();

            // Act
            var (pipeline, ok) = instructions.TryGetPipeline<DatabaseRetriever, ModelRequest, ModelResponse>(sp);

            // Assert
            Assert.True(ok);
            Assert.NotNull(pipeline);
        }

        [Fact]
        public void CanBuildMiddleware()
        {
            // Arrange
            var instructions = new RetrievePipelineInstructions<ModelRequest, ModelResponse>(
                ModelParser.FromBody,
                ModelParser.SetFromModelResponse)
                .WithMiddleware<TimingMiddleware>();

            var services = new ServiceCollection();
            services.AddTransient<TimingMiddleware>();
            var sp = services.BuildServiceProvider();

            // Act
            var middleware = instructions.BuildMiddleware(sp);

            // Assert
            Assert.NotNull(middleware);
        }

        [Fact]
        public void CanBuildPipelineWithMiddleware()
        {
            // Arrange
            var instructions = new RetrievePipelineInstructions<ModelRequest, ModelResponse>(
                ModelParser.FromBody,
                ModelParser.SetFromModelResponse)
                .WithMiddleware<TimingMiddleware>();

            var services = new ServiceCollection();
            services.AddTransient<TimingMiddleware>();
            services.AddTransient<DatabaseRetriever>();
            services.AddTransient<IDbThing, DbThing>();
            var sp = services.BuildServiceProvider();

            // Act
            var (pipeline, ok) = instructions.TryGetPipeline<DatabaseRetriever, ModelRequest, ModelResponse>(sp);

            // Assert
            Assert.True(ok);
            Assert.NotNull(pipeline);
        }

        [Fact]
        public void CanCreatePipelineUsingExtensions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddPipelines();
            services.RegisterRetrievePipeline<ModelRequest, ModelResponse>(
                ModelParser.FromBody,
                ModelParser.SetFromModelResponse
            // builder => builder.WithStage<TimingPipelineStage>()
            //                   .WithStage<GetModelFromDatabase>()
            );

            services.AddTransient<DatabaseRetriever>();
            services.AddTransient<IDbThing, DbThing>();
            var sp = services.BuildServiceProvider();

            // Act
            var registry = sp.GetRequiredService<PipelineRegistry>();

            // Assert
            var pipeline = registry.GetRetrieve<DatabaseRetriever, ModelRequest, ModelResponse>();
            Assert.NotNull(pipeline);
        }
    }

    internal class TimingMiddleware : IMiddleware<ModelResponse>
    {
        public async Task<ModelResponse> Run(Func<Task<ModelResponse>> func)
        {
            var stopwatch = Stopwatch.StartNew();
            var r = await func();
            System.Console.WriteLine($"Command took {stopwatch.ElapsedMilliseconds}ms");

            return r;
        }
    }

    internal class DatabaseRetriever : IRetriever<ModelRequest, ModelResponse>
    {
        private readonly IDbThing _dbThing;

        public DatabaseRetriever(IDbThing dbThing)
        {
            _dbThing = dbThing;
        }

        public async Task<ModelResponse> Retrieve(ModelRequest input)
        {
            return await _dbThing.GetModel(input);
        }
    }

    public static class ModelParser
    {
        internal static ModelRequest FromBody(HttpContext context)
        {
            throw new NotImplementedException();
        }

        internal static Task SetFromModelResponse(HttpContext arg1, ModelResponse arg2)
        {
            throw new NotImplementedException();
        }
    }
}
