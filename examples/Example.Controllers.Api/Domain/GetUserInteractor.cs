using System.Threading.Tasks;
using Endpoints.Pipelines;

namespace Example.Api.Domain
{
    public class GetUserInteractor : IRetriever<GetUserRequest, GetUserResponse>
    {
        public const string ErrorsNoUser = "No user";

        private readonly IDatabase _database;

        public GetUserInteractor(IDatabase database)
        {
            _database = database;
        }
        public async Task<PipelineResponse<GetUserResponse>> Retrieve(GetUserRequest request)
        {
            throw new System.InvalidOperationException();
            var user = await _database.Get(request.Id);
            if (user == null)
            {
                return PipelineResponse.Fail<GetUserResponse>(default, ErrorsNoUser);
            }

            return PipelineResponse.Ok(new GetUserResponse(user.Id, user.Name));
        }
    }

    public record GetUserRequest
    {
        public string Id { get; set; }

        public GetUserRequest(string id) => (Id) = (id);
    }

    public record GetUserResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public GetUserResponse(string id, string name) => (Id, Name) = (id, name);
    }
}
