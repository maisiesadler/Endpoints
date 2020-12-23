using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Api.Pipelines;
using Endpoints.Instructions;
using Endpoints.Extensions;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Endpoints.Pipelines;

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

        // [Fact]
        // public void CanBuildPipelineStages()
        // {
        //     // Arrange
        //     var pi = new PipelineStageInstructions<MyModelPipeline, ModelRequest, ModelResponse>()
        //         .WithStage<TimingPipelineStage>()
        //         .WithStage<GetModelFromDatabase>();

        //     var services = new ServiceCollection();
        //     services.AddTransient<IDbThing, DbThing>();
        //     var sp = services.BuildServiceProvider();

        //     // Act
        //     var (stages, ok) = pi.BuildStages(sp);

        //     // Assert
        //     Assert.True(ok);
        //     Assert.NotNull(stages);
        // }

        [Fact]
        public void CanCreatePipelineUsingExtensions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddPipelines();
            services.RegisterPipeline<MyModelPipeline, ModelRequest, ModelResponse>(
                // builder => builder.WithStage<TimingPipelineStage>()
                //                   .WithStage<GetModelFromDatabase>()
            );

            services.AddTransient<IDbThing, DbThing>();
            var sp = services.BuildServiceProvider();

            // Act
            var registry = sp.GetRequiredService<PipelineRegistry>();

            // Assert
            var pipeline = registry.Get<MyModelPipeline, ModelRequest, ModelResponse>();
            Assert.NotNull(pipeline);
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
