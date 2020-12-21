using System.Net;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Api.Pipelines
{
    public class CreateModelPipeline : Pipeline<ModelRequest, CreateModelPipeline.Response>
    {
        protected override Task<Response> GetResponse(ModelRequest input)
        {
            return Task.FromResult(new Response(true, "new-object-id"));
        }

        protected override ModelRequest ParseModel(HttpContext context)
        {
            return new ModelRequest
            {
                Id = context.Request.RouteValues["id"]?.ToString(),
            };
        }

        protected override Task ParseResponse(HttpContext context, Response response)
        {
            context.Response.StatusCode = response.Ok
                ? (int)HttpStatusCode.OK
                : (int)HttpStatusCode.BadRequest;

            context.Response.WriteAsync(response.Id);

            return Task.CompletedTask;
        }

        public record Response
        {
            public bool Ok { get; }
            public string Id { get; }

            public Response(bool ok, string id) => (Ok, Id) = (ok, id);
        }
    }
}
