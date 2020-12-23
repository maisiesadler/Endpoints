using System.Net;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Api.Pipelines
{
    public class MyModelRetriever : IRetriever<ModelRequest, ModelResponse>
    {
        private readonly IDbThing _dbThing;

        public MyModelRetriever(IDbThing dbThing)
        {
            _dbThing = dbThing;
        }

        public async Task<ModelResponse> Retrieve(ModelRequest input)
        {
            return await _dbThing.GetModel(input);
        }

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
}
