using System;
using System.Threading.Tasks;
using Endpoints.Pipelines;

namespace Example.Api.Adapter
{
    public class ErrorHandlingMiddleware<TOut> : IMiddleware<TOut>
    {
        public async Task<PipelineResponse<TOut>> Run(Func<Task<PipelineResponse<TOut>>> func)
        {
            try
            {
                return await func();
            }
            catch (System.Exception ex)
            {
                return PipelineResponse.Fail<TOut>(ex, "Unhandled exception");    
            }
        }
    }
}
