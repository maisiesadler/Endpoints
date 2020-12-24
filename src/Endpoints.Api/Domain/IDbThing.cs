using System.Threading.Tasks;

namespace Endpoints.Api.Domain
{
    public interface IDbThing
    {
        Task<ModelResponse> GetModel(ModelRequest input);
    }

    public class DbThing : IDbThing
    {
        private int i = 0;

        public async Task<ModelResponse> GetModel(ModelRequest input)
        {
            await Task.Delay(500);
            return new ModelResponse { Name = (i++).ToString() + "=" + input.Id };
        }
    }
}
