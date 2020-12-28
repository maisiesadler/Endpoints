using System.Collections.Generic;
using System.Threading.Tasks;

namespace Endpoints.Api.Domain
{
    public interface IDbThing
    {
        Task<List<ModelResponse>> GetAll();
        Task<ModelResponse> GetModel(ModelRequest input);
    }

    public class DbThing : IDbThing
    {
        private int i = 0;

        public async Task<List<ModelResponse>> GetAll()
        {
            await Task.Delay(500);
            return new List<ModelResponse>
            {
                new ModelResponse
                {
                    Name = "one"
                }
            };
        }

        public async Task<ModelResponse> GetModel(ModelRequest input)
        {
            await Task.Delay(500);
            return new ModelResponse { Name = (i++).ToString() + "=" + input.Id };
        }
    }
}
