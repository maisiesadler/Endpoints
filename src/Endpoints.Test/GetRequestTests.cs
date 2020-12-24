using System.Threading.Tasks;
using System.Net;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Api.Pipelines;
using Microsoft.AspNetCore.Builder;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using Endpoints.Extensions;

namespace Endpoints.Test
{
    public class GetRequestTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public GetRequestTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        public class DatabaseRetriever : IRetriever<ModelRequest, ModelResponse>
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
            public static ModelRequest ParseModel(HttpContext context)
            {
                return new ModelRequest
                {
                    Id = context.Request.RouteValues["id"]?.ToString(),
                };
            }

            public static async Task ParseResponse(HttpContext context, ModelResponse response)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(response.Name);
            }
        }

        [Fact]
        public async Task GetWithNoParameters()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddTransient<DatabaseRetriever>();
                services.AddPipelines();
                services.RegisterRetrievePipeline<ModelRequest, ModelResponse>(
                    ModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                var registry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();
                endpoints.MapGet("/test", registry.GetRetrieve<DatabaseRetriever, ModelRequest, ModelResponse>());
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
                services.AddTransient<DatabaseRetriever>();
                services.AddPipelines();
                services.RegisterRetrievePipeline<ModelRequest, ModelResponse>(
                    ModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                var registry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();
                endpoints.MapGet("/test/{id}", registry.GetRetrieve<DatabaseRetriever, ModelRequest, ModelResponse>());
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

        public class TwoIdsModelParser
        {
            public static TwoIdsModelRequest ParseModel(HttpContext context)
            {
                return new TwoIdsModelRequest
                {
                    Id = context.Request.RouteValues["id"]?.ToString(),
                    Id2 = context.Request.RouteValues["id2"]?.ToString(),
                };
            }
        }

        public class ModelFromTwoIdsRetriever : IRetriever<TwoIdsModelRequest, ModelResponse>
        {
            public Task<PipelineResponse<ModelResponse>> Retrieve(TwoIdsModelRequest input)
            {
                var response = new ModelResponse
                {
                    Name = $"Id = {input.Id}, Id2 = {input.Id2}",
                };

                return Task.FromResult(PipelineResponse.Ok(response));
            }
        }

        [Fact]
        public async Task GetWithMultipleStringParameters()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddTransient<ModelFromTwoIdsRetriever>();
                services.AddPipelines();
                services.RegisterRetrievePipeline<TwoIdsModelRequest, ModelResponse>(
                    TwoIdsModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                var pipelineRegistry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();
                endpoints.MapGet("/test/{id}/{id2}", pipelineRegistry.GetRetrieve<ModelFromTwoIdsRetriever, TwoIdsModelRequest, ModelResponse>());
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
