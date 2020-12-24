using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using System.IO;
using Endpoints.Extensions;
using Newtonsoft.Json;
using Moq;
using Microsoft.AspNetCore.TestHost;

namespace Endpoints.Test
{
    public class CrudRetrievePipelineTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public CrudRetrievePipelineTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        private TestServer CreateTestCrudServer(IDatabase<CrudId, CrudModel> database)
        {
            // Arrange
            return _fixture.CreateServer(services =>
            {
                services.AddSingleton(database);

                services.AddTransient<CreateCrudModelRetriever>();
                services.AddTransient<ReadCrudModelRetriever>();
                services.AddTransient<UpdateCrudModelRetriever>();
                services.AddTransient<DeleteCrudModelRetriever>();

                services.AddPipelines();
                services.RegisterRetrievePipeline<CrudModel, CrudId>(
                    ModelParser.GetModelFromBody,
                    ModelParser.SetResponseFromId
                );
                services.RegisterRetrievePipeline<CrudId, CrudModel>(
                    ModelParser.GetIdFromPath,
                    ModelParser.SetJsonResponse
                );
                services.RegisterRetrievePipeline<UpdateCrudModelRequest, bool>(
                    ModelParser.GetUpdateCrudModelRequest,
                    ModelParser.SuccessfulResponse
                );
                services.RegisterRetrievePipeline<CrudId, bool>(
                    ModelParser.GetIdFromPath,
                    ModelParser.SuccessfulResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                var registry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();
                endpoints.MapPost("/model", registry.GetRetrieve<CreateCrudModelRetriever, CrudModel, CrudId>());
                endpoints.MapGet("/model/{id}", registry.GetRetrieve<CrudId, CrudModel>(sp => sp.GetRequiredService<IDatabase<CrudId, CrudModel>>().Read));
                endpoints.MapPut("/model/{id}", registry.GetRetrieve<UpdateCrudModelRetriever, UpdateCrudModelRequest, bool>());
                endpoints.MapDelete("/model/{id}", registry.GetRetrieve<DeleteCrudModelRetriever, CrudId, bool>());
            }));
        }

        [Fact]
        public async Task CanCreate()
        {
            var database = new Mock<IDatabase<CrudId, CrudModel>>();
            database.Setup(d => d.Create(It.IsAny<CrudModel>()))
                .Returns(Task.FromResult(new CrudId("new-model-id")));

            using var server = CreateTestCrudServer(database.Object);
            var model = new CrudModel("", "model-body");
            var requestContent = new StringContent(JsonConvert.SerializeObject(model));

            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/model", requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("new-model-id", content);
        }

        [Fact]
        public async Task CanRead()
        {
            var database = new Mock<IDatabase<CrudId, CrudModel>>();
            database.Setup(d => d.Read(It.Is<CrudId>(id => id.Id == "model-id")))
                .Returns(Task.FromResult(new CrudModel("model-id", "model-body")));

            using var server = CreateTestCrudServer(database.Object);

            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/model/model-id");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(@"{""Id"":""model-id"",""Body"":""model-body""}", content);
        }

        [Fact]
        public async Task CanUpdate()
        {
            var database = new Mock<IDatabase<CrudId, CrudModel>>();
            database.Setup(d => d.Update(It.IsAny<CrudId>(), It.IsAny<CrudModel>()))
                .Returns(Task.CompletedTask);

            var model = new CrudModel("", "model-body");
            var requestContent = new StringContent(JsonConvert.SerializeObject(model));

            using var server = CreateTestCrudServer(database.Object);
            var client = server.CreateClient();

            // Act
            var response = await client.PutAsync("/model/model-id", requestContent);


            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("", content);
        }

        [Fact]
        public async Task CanDelete()
        {
            var database = new Mock<IDatabase<CrudId, CrudModel>>();
            database.Setup(d => d.Delete(It.IsAny<CrudId>()))
                .Returns(Task.CompletedTask);

            using var server = CreateTestCrudServer(database.Object);
            var client = server.CreateClient();

            // Act
            var response = await client.DeleteAsync("/model/model-id");


            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("", content);
        }

        public static class ModelParser
        {
            public static CrudId GetIdFromPath(HttpContext context)
            {
                var id = context.Request.RouteValues["id"]?.ToString();
                return new CrudId(id);
            }

            public static async Task<CrudModel> GetModelFromBody(HttpContext context)
            {
                var body = await context.ParseBody();
                return JsonConvert.DeserializeObject<CrudModel>(body);
            }

            public static async Task<UpdateCrudModelRequest> GetUpdateCrudModelRequest(HttpContext context)
            {
                var id = context.Request.RouteValues["id"]?.ToString();

                var body = await context.ParseBody();
                var model = JsonConvert.DeserializeObject<CrudModel>(body);

                return new UpdateCrudModelRequest(id, model);
            }

            public static async Task SetResponseFromId(HttpContext context, CrudId response)
            {
                await context.Response.WriteAsync(response.Id);
            }

            public static async Task SetJsonResponse(HttpContext context, object o)
            {
                await context.Response.WriteAsync(JsonConvert.SerializeObject(o));
            }

            public static Task SuccessfulResponse(HttpContext context, bool response)
            {
                context.Response.StatusCode = 200;

                return Task.CompletedTask;
            }
        }

        public record UpdateCrudModelRequest
        {
            public CrudId Id { get; set; }
            public CrudModel Model { get; set; }

            public UpdateCrudModelRequest(string id, CrudModel model) => (Id, Model) = (new CrudId(id), model);
        }

        public record CrudModel
        {
            public string Id { get; set; }
            public string Body { get; set; }

            public CrudModel(string id, string body) => (Id, Body) = (id, body);
        }

        public record CrudId
        {
            public string Id { get; set; }

            public CrudId(string id) => (Id) = (id);
        }

        public interface IDatabase<TId, TModel>
        {
            Task<TId> Create(TModel model);
            Task<TModel> Read(TId id);
            Task Update(TId id, TModel model);
            Task Delete(TId id);
        }

        public class CreateCrudModelRetriever : IRetriever<CrudModel, CrudId>
        {
            private readonly IDatabase<CrudId, CrudModel> _database;

            public CreateCrudModelRetriever(IDatabase<CrudId, CrudModel> database)
            {
                _database = database;
            }

            public async Task<CrudId> Retrieve(CrudModel input)
            {
                return await _database.Create(input);
            }
        }

        public class ReadCrudModelRetriever : IRetriever<CrudId, CrudModel>
        {
            private readonly IDatabase<CrudId, CrudModel> _database;

            public ReadCrudModelRetriever(IDatabase<CrudId, CrudModel> database)
            {
                _database = database;
            }

            public async Task<CrudModel> Retrieve(CrudId input)
            {
                return await _database.Read(input);
            }
        }

        public class UpdateCrudModelRetriever : IRetriever<UpdateCrudModelRequest, bool>
        {
            private readonly IDatabase<CrudId, CrudModel> _database;

            public UpdateCrudModelRetriever(IDatabase<CrudId, CrudModel> database)
            {
                _database = database;
            }

            public async Task<bool> Retrieve(UpdateCrudModelRequest input)
            {
                await _database.Update(input.Id, input.Model);
                return true;
            }
        }

        public class DeleteCrudModelRetriever : IRetriever<CrudId, bool>
        {
            private readonly IDatabase<CrudId, CrudModel> _database;

            public DeleteCrudModelRetriever(IDatabase<CrudId, CrudModel> database)
            {
                _database = database;
            }

            public async Task<bool> Retrieve(CrudId input)
            {
                await _database.Delete(input);
                return true;
            }

            protected CrudId Retrieve(HttpContext context)
            {
                var id = context.Request.RouteValues["id"]?.ToString();
                return new CrudId(id);
            }
        }
    }
}
