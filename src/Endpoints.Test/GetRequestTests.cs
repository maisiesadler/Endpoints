using System.Threading.Tasks;
using System.Net;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Api.Pipelines;
using Microsoft.AspNetCore.Builder;
using Endpoints.Instructions;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Endpoints.Test
{
    public class GetRequestTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public GetRequestTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetWithNoParameters()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddTransient<Pipeline<ModelRequest, ModelResponse>>(sp =>
                    new PipelineInstructions<ModelRequest, ModelResponse>(stages => new MyModelPipeline(stages))
                        .Register<GetModelFromDatabase>()
                        .GetPipeline(sp));
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", async ctx => await endpoints.ServiceProvider.GetRequiredService<Pipeline<ModelRequest, ModelResponse>>().Run(ctx));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("0=", content);
        }

        [Theory]
        [InlineData("test1")]
        [InlineData("anothername")]
        public async Task GetWithStringParameter(string @param)
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddTransient<Pipeline<ModelRequest, ModelResponse>>(sp =>
                    new PipelineInstructions<ModelRequest, ModelResponse>(stages => new MyModelPipeline(stages))
                        .Register<GetModelFromDatabase>()
                        .GetPipeline(sp));
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test/{id}", async ctx => await endpoints.ServiceProvider.GetRequiredService<Pipeline<ModelRequest, ModelResponse>>().Run(ctx));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test/" + @param);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("0=" + @param, content);
        }

        public class TwoIdsModelRequest
        {
            public string Id { get; set; }
            public string Id2 { get; set; }
        }

        public class TwoIdsPipeline : Pipeline<TwoIdsModelRequest, ModelResponse>
        {
            public TwoIdsPipeline(PipelineStage<TwoIdsModelRequest, ModelResponse> stages)
                : base(stages)
            {
            }

            protected override TwoIdsModelRequest ParseModel(HttpContext context)
            {
                return new TwoIdsModelRequest
                {
                    Id = context.Request.RouteValues["id"]?.ToString(),
                    Id2 = context.Request.RouteValues["id2"]?.ToString(),
                };
            }

            protected override async Task ParseResponse(HttpContext context, ModelResponse response)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(response.Name);
            }
        }

        public class TestPipelineStage : PipelineStage<TwoIdsModelRequest, ModelResponse>
        {
            public override Task<ModelResponse> RunAsync(TwoIdsModelRequest input, CancellationToken stoppingToken)
            {
                var response = new ModelResponse
                {
                    Name = $"Id = {input.Id}, Id2 = {input.Id2}",
                };

                return Task.FromResult(response);
            }
        }

        [Fact]
        public async Task GetWithMultipleStringParameters()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddTransient<Pipeline<TwoIdsModelRequest, ModelResponse>>(sp =>
                    new PipelineInstructions<TwoIdsModelRequest, ModelResponse>(stages => new TwoIdsPipeline(stages))
                        .Register<TestPipelineStage>()
                        .GetPipeline(sp));
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test/{id}/{id2}", async ctx => await endpoints.ServiceProvider.GetRequiredService<Pipeline<TwoIdsModelRequest, ModelResponse>>().Run(ctx));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test/one/two");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Id = one, Id2 = two", content);
        }
    }
}
