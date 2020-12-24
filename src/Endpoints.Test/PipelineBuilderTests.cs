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
using Moq;
using System.Collections.Generic;
using Endpoints.Api.Domain;

namespace Endpoints.Test
{
    public class PipelineBuilderTests
    {
        [Fact]
        public void CanCreatePipeline()
        {
            // Arrange
            var instructions = new PipelineInstructions<ModelRequest, ModelResponse>(
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
            var instructions = new PipelineInstructions<ModelRequest, ModelResponse>(
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

        [Theory]
        [InlineData(0, 1, 0, false, 0, 0)]
        [InlineData(1, 1, 1, false, 0, 0)]
        [InlineData(2, 1, 1, true, 1, 0)]
        [InlineData(3, 1, 1, true, 1, 1)]
        public async Task MiddlewareRunsInCorrectOrder(int throwIdx, int times0, int times1, bool functionRuns, int times2, int times3)
        {
            // Arrange
            var middlewareBefore1 = new Mock<Action>();
            var middlewareAfter1 = new Mock<Action>();
            var middlewareBefore2 = new Mock<Action>();
            var middlewareAfter2 = new Mock<Action>();
            var actions = new[] { middlewareBefore1, middlewareBefore2, middlewareAfter2, middlewareAfter1 };

            var throwAction = actions[throwIdx];
            throwAction.Setup(m => m()).Throws(new Exception());

            var middlewares = new List<Func<IServiceProvider, IMiddleware<ModelResponse>>>
            {
                _ => new BeforeAndAfterMiddleware(middlewareBefore1.Object, middlewareAfter1.Object),
                _ => new BeforeAndAfterMiddleware(middlewareBefore2.Object, middlewareAfter2.Object)
            };

            var services = new ServiceCollection();
            services.AddTransient<TimingMiddleware>();
            var sp = services.BuildServiceProvider();
            var middleware = PipelineInstructionsExtensions.BuildMiddleware(middlewares, sp);

            var runFunction = new Mock<Func<Task<PipelineResponse<ModelResponse>>>>();

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await middleware.Run(runFunction.Object));

            // Assert
            actions[0].Verify(m => m(), Times.Exactly(times0));
            actions[1].Verify(m => m(), Times.Exactly(times1));
            runFunction.Verify(fn => fn(), Times.Exactly(functionRuns ? 1 : 0));
            actions[2].Verify(m => m(), Times.Exactly(times2));
            actions[3].Verify(m => m(), Times.Exactly(times3));
        }

        [Fact]
        public void CanBuildPipelineWithMiddleware()
        {
            // Arrange
            var instructions = new PipelineInstructions<ModelRequest, ModelResponse>(
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
            services.AddPipeline<ModelRequest, ModelResponse>(
                ModelParser.FromBody,
                ModelParser.SetFromModelResponse,
                builder => builder.WithMiddleware<TimingMiddleware>()
            );

            services.AddTransient<TimingMiddleware>();
            services.AddTransient<DatabaseRetriever>();
            services.AddTransient<IDbThing, DbThing>();
            var sp = services.BuildServiceProvider();

            // Act
            var registry = sp.GetRequiredService<PipelineRegistry>();

            // Assert
            var pipeline = registry.Get<DatabaseRetriever, ModelRequest, ModelResponse>();
            Assert.NotNull(pipeline);
        }
    }

    internal class TimingMiddleware : IMiddleware<ModelResponse>
    {
        public async Task<PipelineResponse<ModelResponse>> Run(Func<Task<PipelineResponse<ModelResponse>>> func)
        {
            var stopwatch = Stopwatch.StartNew();
            var r = await func();
            System.Console.WriteLine($"Command took {stopwatch.ElapsedMilliseconds}ms");

            return r;
        }
    }

    internal class BeforeAndAfterMiddleware : IMiddleware<ModelResponse>
    {
        private readonly Action _before;
        private readonly Action _after;

        public BeforeAndAfterMiddleware(Action before, Action after)
        {
            _before = before;
            _after = after;
        }

        public async Task<PipelineResponse<ModelResponse>> Run(Func<Task<PipelineResponse<ModelResponse>>> func)
        {
            _before();
            var r = await func();
            _after();

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

        public async Task<PipelineResponse<ModelResponse>> Retrieve(ModelRequest input)
        {
            var result = await _dbThing.GetModel(input);
            return PipelineResponse.Ok(result);
        }
    }

    public static class ModelParser
    {
        internal static ModelRequest FromBody(HttpContext context)
        {
            throw new NotImplementedException();
        }

        internal static Task SetFromModelResponse(HttpContext arg1, PipelineResponse<ModelResponse> arg2)
        {
            throw new NotImplementedException();
        }
    }
}
