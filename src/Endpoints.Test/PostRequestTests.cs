using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Api.Pipelines;
using Endpoints.Pipelines;
using Endpoints.Instructions;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.IO;
using System.Text;

namespace Endpoints.Test
{
    public class PostRequestTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public PostRequestTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        public class BodyRequest
        {
            public string Id { get; set; }
            public string Body { get; set; }
        }

        public class ParseBodyPipeline : StagedPipeline<BodyRequest, ModelResponse>
        {
            public ParseBodyPipeline(PipelineStage<BodyRequest, ModelResponse> stages)
                : base(stages)
            {
            }

            protected override BodyRequest ParseModel(HttpContext context) => throw new System.NotImplementedException();

            protected override async Task<BodyRequest> ParseModelAsync(HttpContext context)
            {
                string body;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    body = await reader.ReadToEndAsync();
                }

                return new BodyRequest
                {
                    Id = context.Request.RouteValues["id"]?.ToString(),
                    Body = body,
                };
            }

            protected override async Task ParseResponse(HttpContext context, ModelResponse response)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(response.Name);
            }
        }

        public class BodyPipelineStage : PipelineStage<BodyRequest, ModelResponse>
        {
            public override Task<ModelResponse> RunAsync(BodyRequest input, CancellationToken stoppingToken)
            {
                var response = new ModelResponse
                {
                    Name = $"Id = {input.Id}, Body = {input.Body}",
                };

                return Task.FromResult(response);
            }
        }

        [Fact]
        public async Task PostWithNoParameters()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddTransient<Pipeline<BodyRequest, ModelResponse>>(sp =>
                    new PipelineInstructions<ParseBodyPipeline, BodyRequest, ModelResponse>()
                        .Register<BodyPipelineStage>()
                        .GetPipeline(sp));
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/test", async ctx => await endpoints.ServiceProvider.GetRequiredService<Pipeline<BodyRequest, ModelResponse>>().Run(ctx));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/test", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Id = , Body = ", content);
        }

        [Theory]
        [InlineData("test1")]
        [InlineData("anothername")]
        public async Task PostWithStringParameter(string @param)
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddTransient<Pipeline<BodyRequest, ModelResponse>>(sp =>
                    new PipelineInstructions<ParseBodyPipeline, BodyRequest, ModelResponse>()
                        .Register<BodyPipelineStage>()
                        .GetPipeline(sp));
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/test/{id}", async ctx => await endpoints.ServiceProvider.GetRequiredService<Pipeline<BodyRequest, ModelResponse>>().Run(ctx));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync($"/test/{@param}", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal($"Id = {@param}, Body = ", content);
        }

        [Fact]
        public async Task PostWithBody()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddTransient<Pipeline<BodyRequest, ModelResponse>>(sp =>
                    new PipelineInstructions<ParseBodyPipeline, BodyRequest, ModelResponse>()
                        .Register<BodyPipelineStage>()
                        .GetPipeline(sp));
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/test", async ctx => await endpoints.ServiceProvider.GetRequiredService<Pipeline<BodyRequest, ModelResponse>>().Run(ctx));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/test", new StringContent("Hello, world!"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Id = , Body = Hello, world!", content);
        }
    }
}
