using System.Threading.Tasks;
using Endpoints.Pipelines;

namespace Example.Api.Domain
{
    public class CreateUserInteractor : IRetriever<CreateUserRequest, CreateUserResponse>
    {
        private readonly IDatabase _database;

        public CreateUserInteractor(IDatabase database)
        {
            _database = database;
        }
        public async Task<PipelineResponse<CreateUserResponse>> Retrieve(CreateUserRequest request)
        {
            var id = await _database.Create(request.Name);
            return PipelineResponse.Ok(new CreateUserResponse(id));
        }
    }

    public record CreateUserRequest
    {
        public string Name { get; set; }

        public CreateUserRequest(string name) => (Name) = (name);
    }

    public record CreateUserResponse
    {
        public string Id { get; set; }

        public CreateUserResponse(string id) => (Id) = (id);
    }
}
