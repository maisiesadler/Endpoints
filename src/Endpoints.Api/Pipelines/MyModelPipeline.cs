using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Api.Pipelines
{
    public class MyModelPipeline : Pipeline<ModelRequest, ModelResponse>
    {
        public MyModelPipeline(PipelineStage<ModelRequest, ModelResponse> stages)
            : base(stages)
        {
        }

        protected override ModelRequest ParseModel(HttpContext context)
        {
            return new ModelRequest
            {
                Id = context.Request.RouteValues["id"].ToString(),
            };
        }

        protected override async Task ParseResponse(HttpContext context, ModelResponse response)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsync(response.Name);
        }
    }
}
