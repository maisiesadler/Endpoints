using System;
using System.Threading.Tasks;
using Endpoints.Pipelines;

namespace Endpoints.Api.Pipelines
{
    public class ExceptionHandlingMiddleware : IMiddleware<ModelResponse>
    {
        public async Task<PipelineResponse<ModelResponse>> Run(Func<Task<PipelineResponse<ModelResponse>>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return default;
            }
        }
    }
}
