using System.Threading.Tasks;

namespace Endpoints.Pipelines
{
    public interface IRetriever<TIn, TOut>
    {
        Task<PipelineResponse<TOut>> Retrieve(TIn input);
    }

    public abstract class IRetriever<TOut> : IRetriever<NoType, TOut>
    {
        public Task<PipelineResponse<TOut>> Retrieve(NoType input) => Retrieve();

        public abstract Task<PipelineResponse<TOut>> Retrieve();
    }
}
