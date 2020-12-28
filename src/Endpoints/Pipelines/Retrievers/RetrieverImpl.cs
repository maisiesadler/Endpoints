using System.Threading.Tasks;

namespace Endpoints.Pipelines.Retrievers
{
    internal class RetrieverImpl<TOut> : IRetriever<NoType, TOut>
    {
        private readonly IRetriever<TOut> _retriever;

        public Task<PipelineResponse<TOut>> Retrieve(NoType input) => Retrieve();

        public RetrieverImpl(IRetriever<TOut> retriever)
        {
            _retriever = retriever;
        }

        public async Task<PipelineResponse<TOut>> Retrieve() => await _retriever.Retrieve();
    }
}
