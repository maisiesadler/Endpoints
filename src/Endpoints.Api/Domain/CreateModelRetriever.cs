using System.Threading.Tasks;
using Endpoints.Pipelines;

namespace Endpoints.Api.Domain
{
    public class CreateModelRetriever : IRetriever<ModelRequest, CreateModelRetriever.Response>
    {
        public Task<PipelineResponse<CreateModelRetriever.Response>> Retrieve(ModelRequest input)
        {
            // var response = PipelineResponse<CreateModelPipeline.Response, string>.Fail("error-response");
            var response = PipelineResponse.Ok<CreateModelRetriever.Response>(new CreateModelRetriever.Response("new-model-id"));
            return Task.FromResult(response);
        }

        public record Response
        {
            public string Id { get; }

            public Response(string id) => (Id) = (id);
        }
    }
}
