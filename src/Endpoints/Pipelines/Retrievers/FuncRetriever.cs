using System;
using System.Threading.Tasks;

namespace Endpoints.Pipelines.Retrievers
{
    internal class FuncRetriever<TIn, TOut> : IRetriever<TIn, TOut>
    {
        private readonly Func<TIn, Task<TOut>> _retriever;

        public FuncRetriever(Func<TIn, Task<TOut>> retriever)
        {
            _retriever = retriever;
        }

        public async Task<PipelineResponse<TOut>> Retrieve(TIn input)
        {
            var result = await _retriever(input);
            return PipelineResponse.Ok(result);
        }
    }

    internal class FuncRetriever<TOut> : IRetriever<TOut>
    {
        private readonly Func<Task<TOut>> _retriever;

        public FuncRetriever(Func<Task<TOut>> retriever)
        {
            _retriever = retriever;
        }

        public async Task<PipelineResponse<TOut>> Retrieve()
        {
            var result = await _retriever();
            return PipelineResponse.Ok(result);
        }
    }
}
