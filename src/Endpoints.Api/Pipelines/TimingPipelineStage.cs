using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Endpoints.Pipelines;

namespace Endpoints.Api.Pipelines
{
    public class TimingPipelineStage : PipelineStage<ModelRequest, ModelResponse>
    {
        public TimingPipelineStage(PipelineStage<ModelRequest, ModelResponse> next)
            : base(next ?? throw new ArgumentNullException("next"))
        {
        }

        public override async Task<ModelResponse> RunAsync(ModelRequest context, CancellationToken stoppingToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var r = await _next.RunAsync(context, stoppingToken);
            System.Console.WriteLine($"Command took {stopwatch.ElapsedMilliseconds}ms");

            return r;
        }
    }
}
