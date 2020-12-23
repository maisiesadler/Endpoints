// using Xunit;
// using Microsoft.Extensions.DependencyInjection;
// using Endpoints.Api.Pipelines;
// using Endpoints.Instructions;
// using Endpoints.Extensions;

// namespace Endpoints.Test
// {
//     public class PipelineBuilderTests
//     {
//         [Fact]
//         public void CanCreatePipeline()
//         {
//             // Arrange
//             var pi = new PipelineInstructions<MyModelPipeline, ModelRequest, ModelResponse>();
//                 // .WithStage<TimingPipelineStage>()
//                 // .WithStage<GetModelFromDatabase>();

//             var services = new ServiceCollection();
//             services.AddTransient<IDbThing, DbThing>();
//             var sp = services.BuildServiceProvider();

//             // Act
//             var (pipeline, ok) = pi.TryGetPipeline(sp);

//             // Assert
//             Assert.True(ok);
//             Assert.NotNull(pipeline);
//         }

//         // [Fact]
//         // public void CanBuildPipelineStages()
//         // {
//         //     // Arrange
//         //     var pi = new PipelineStageInstructions<MyModelPipeline, ModelRequest, ModelResponse>()
//         //         .WithStage<TimingPipelineStage>()
//         //         .WithStage<GetModelFromDatabase>();

//         //     var services = new ServiceCollection();
//         //     services.AddTransient<IDbThing, DbThing>();
//         //     var sp = services.BuildServiceProvider();

//         //     // Act
//         //     var (stages, ok) = pi.BuildStages(sp);

//         //     // Assert
//         //     Assert.True(ok);
//         //     Assert.NotNull(stages);
//         // }

//         [Fact]
//         public void CanCreatePipelineUsingExtensions()
//         {
//             // Arrange
//             var services = new ServiceCollection();
//             services.AddPipelines();
//             services.RegisterPipeline<MyModelPipeline, ModelRequest, ModelResponse>(
//                 // builder => builder.WithStage<TimingPipelineStage>()
//                 //                   .WithStage<GetModelFromDatabase>()
//             );

//             services.AddTransient<IDbThing, DbThing>();
//             var sp = services.BuildServiceProvider();

//             // Act
//             var registry = sp.GetRequiredService<PipelineRegistry>();

//             // Assert
//             var pipeline = registry.Get<MyModelPipeline, ModelRequest, ModelResponse>();
//             Assert.NotNull(pipeline);
//         }
//     }
// }
