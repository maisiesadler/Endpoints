using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Api.Pipelines;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using Endpoints.Extensions;
using Endpoints.Api.Domain;

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

        public class ModelParser
        {
            public static async Task<BodyRequest> ParseModel(HttpContext context)
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

            public static async Task ParseResponse(HttpContext context, ModelResponse response)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(response.Name);
            }
        }

        public class ModelRetriever : IRetriever<BodyRequest, ModelResponse>
        {
            public Task<PipelineResponse<ModelResponse>> Retrieve(BodyRequest input)
            {
                var response = new ModelResponse
                {
                    Name = $"Id = {input.Id}, Body = {input.Body}",
                };

                return Task.FromResult(PipelineResponse.Ok(response));
            }
        }

        [Fact]
        public async Task PostWithNoParameters()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddTransient<ModelRetriever>();
                services.AddPipelines();
                services.AddPipeline<BodyRequest, ModelResponse>(
                    ModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                var registry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();
                endpoints.MapPost("/test", registry.Get<ModelRetriever, BodyRequest, ModelResponse>());
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
                services.AddTransient<ModelRetriever>();
                services.AddPipelines();
                services.AddPipeline<BodyRequest, ModelResponse>(
                    ModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                var registry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();
                endpoints.MapPost("/test/{id}", registry.Get<ModelRetriever, BodyRequest, ModelResponse>());
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
                services.AddTransient<ModelRetriever>();
                services.AddPipelines();
                services.AddPipeline<BodyRequest, ModelResponse>(
                    ModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                var registry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();
                endpoints.MapPost("/test", registry.Get<ModelRetriever, BodyRequest, ModelResponse>());
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
