using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Endpoints.Api.Domain;
using Endpoints.Pipelines;

namespace Endpoints.Api.Pipelines
{
    public class TimingMiddleware : IMiddleware<ModelResponse>
    {
        public async Task<PipelineResponse<ModelResponse>> Run(Func<Task<PipelineResponse<ModelResponse>>> func)
        {
            var stopwatch = Stopwatch.StartNew();
            var r = await func();
            System.Console.WriteLine($"Command took {stopwatch.ElapsedMilliseconds}ms");

            return r;
        }
    }
}
