using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Api.Pipelines;
using Endpoints.Instructions;

namespace Endpoints.Test
{
    public class PipelineBuilderTests
    {
        [Fact]
        public void CanCreatePipeline()
        {
            // Arrange
            var pi = new PipelineInstructions<MyModelPipeline, ModelRequest, ModelResponse>()
                .WithStage<TimingPipelineStage>()
                .WithStage<GetModelFromDatabase>();

            var services = new ServiceCollection();
            services.AddTransient<IDbThing, DbThing>();
            var sp = services.BuildServiceProvider();

            // Act
            var (pipeline, ok) = pi.TryGetPipeline(sp);

            // Assert
            Assert.True(ok);
            Assert.NotNull(pipeline);
        }

        [Fact]
        public void CanBuildPipelineStages()
        {
            // Arrange
            var pi = new PipelineInstructions<MyModelPipeline, ModelRequest, ModelResponse>()
                .WithStage<TimingPipelineStage>()
                .WithStage<GetModelFromDatabase>();

            var services = new ServiceCollection();
            services.AddTransient<IDbThing, DbThing>();
            var sp = services.BuildServiceProvider();

            // Act
            var (stages, ok) = pi.BuildStages(sp);

            // Assert
            Assert.True(ok);
            Assert.NotNull(stages);
        }
    }
}
