using System.Threading.Tasks;
using Example.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Example.Controllers.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IDatabase _database;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IDatabase database,
            ILogger<UserController> logger)
        {
            _database = database;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetUserResponse>> Get(string id)
        {
            var result = await _database.Get(id);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(new GetUserResponse { Id = result.Id, Name = result.Name });
        }


        [HttpPost]
        public async Task<ActionResult<CreateUserResponse>> Post([FromBody] CreateUserRequest createUserRequest)
        {
            var result = await _database.Create(createUserRequest.Name);
            return Ok(new CreateUserResponse { Id = result });
        }
    }
    public class CreateUserRequest
    {
        public string Name { get; set; }
    }

    public class CreateUserResponse
    {
        public string Id { get; set; }
    }

    public class GetUserResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
