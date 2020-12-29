using System.Threading.Tasks;
using System.Net;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using Endpoints.Extensions;
using Endpoints.Api.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Endpoints.Test
{
    public class ServiceCollectionRetrieveExtensionsTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public ServiceCollectionRetrieveExtensionsTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        public class DatabaseAllRetriever : IRetriever<List<ModelResponse>>
        {
            private readonly IDbThing _dbThing;

            public DatabaseAllRetriever(IDbThing dbThing)
            {
                _dbThing = dbThing;
            }

            public async Task<PipelineResponse<List<ModelResponse>>> Retrieve()
            {
                var result = await _dbThing.GetAll();
                return PipelineResponse.Ok(result);
            }
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

            public static async Task ParseResponse(HttpContext context, List<ModelResponse> response)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(string.Join(", ", response.Select(s => s.Name)));
            }
        }

        [Fact]
        public async Task PipelineWithInputSetupWithRetriever()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddTransient<DatabaseRetriever>();
                services.AddPipeline<ModelRequest, ModelResponse>(
                    ModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", endpoints.ServiceProvider.Get<DatabaseRetriever, ModelRequest, ModelResponse>());
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("0=", content);
        }

        [Fact]
        public async Task PipelineWithInputSetupWithServiceFunction()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddPipeline<ModelRequest, ModelResponse>(
                    ModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", endpoints.ServiceProvider.Get<IDbThing, ModelRequest, ModelResponse>(db => db.GetModel));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("0=", content);
        }

        [Fact]
        public async Task PipelineWithInputSetupWithFunction()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddPipeline<ModelRequest, ModelResponse>(
                    ModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", endpoints.ServiceProvider.Get<ModelRequest, ModelResponse>(sp => sp.GetRequiredService<IDbThing>().GetModel));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("0=", content);
        }

        [Fact]
        public async Task PipelineWithNoInputSetupWithRetriever()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddTransient<DatabaseAllRetriever>();
                services.AddPipeline<List<ModelResponse>>(
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", endpoints.ServiceProvider.Get<DatabaseAllRetriever, List<ModelResponse>>());
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("one", content);
        }

        [Fact]
        public async Task PipelineWithNoInputSetupWithServiceFunction()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddPipeline<List<ModelResponse>>(
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", endpoints.ServiceProvider.Get<IDbThing, List<ModelResponse>>(db => db.GetAll));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("one", content);
        }

        [Fact]
        public async Task PipelineWithNoInputSetupWithFunction()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing, DbThing>();
                services.AddPipeline<List<ModelResponse>>(
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", endpoints.ServiceProvider.Get<List<ModelResponse>>(sp => sp.GetRequiredService<IDbThing>().GetAll));
            }));
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("one", content);
        }
    }
}
