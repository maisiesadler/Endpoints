using System.Threading.Tasks;

namespace Endpoints.Pipelines
{
    public interface IRetriever<TIn, TOut>
    {
        Task<PipelineResponse<TOut>> Retrieve(TIn input);
    }

    public interface IRetriever<TOut>
    {
        Task<PipelineResponse<TOut>> Retrieve();
    }
}
