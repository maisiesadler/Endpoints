using System;
using System.Threading;
using System.Threading.Tasks;
using Endpoints.Pipelines;

namespace Endpoints.Api.Pipelines
{
    public class ExceptionHandlingPipelineStage : PipelineStage<ModelRequest, ModelResponse>
    {
        public ExceptionHandlingPipelineStage(PipelineStage<ModelRequest, ModelResponse> next)
            : base(next ?? throw new ArgumentNullException("next"))
        {
        }

        public override Task<ModelResponse> RunAsync(ModelRequest context, CancellationToken stoppingToken)
        {
            try
            {
                return _next.RunAsync(context, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return default;
            }
        }
    }
}
