using Microsoft.Extensions.DependencyInjection;
using Endpoints.Pipelines;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Endpoints.Instructions
{
    public static class PipelineInstructionsExtensions
    {
        public static (Pipeline<TIn, TOut>, bool) TryGetPipeline<TIn, TOut>(
            this PipelineInstructions<TIn, TOut> instructions,
            IServiceProvider sp)
        {
            if (!instructions.Validate())
            {
                return (null, false);
            }

            var middleware = BuildMiddleware(instructions, sp);
            var pipeline = new Pipeline<TIn, TOut>(
                instructions.ParseModel, instructions.ParseResponse, middleware);

            return (pipeline, true);
        }


        public static Middleware<TOut> BuildMiddleware<TIn, TOut>(
            this PipelineInstructions<TIn, TOut> instructions, IServiceProvider sp)
            => BuildMiddleware(instructions.MiddlewareFunction, sp);

        public static Middleware<TOut> BuildMiddleware<TOut>(
            List<Func<IServiceProvider, IMiddleware<TOut>>> middlewareFunctions,
            IServiceProvider sp)
        {
            if (middlewareFunctions.Count == 0)
                return new DelegateMiddleware<TOut>();

            var inner = middlewareFunctions[middlewareFunctions.Count - 1](sp);
            var middleware = new MiddlewareRunner<TOut>(inner, null);
            for (var i = middlewareFunctions.Count - 2; i >= 0; i--)
            {
                var type = middlewareFunctions[i];
                inner = middlewareFunctions[i](sp);
                middleware = new MiddlewareRunner<TOut>(inner, middleware);
            }

            return middleware;
        }
    }

    public class FuncRetriever<TIn, TOut> : IRetriever<TIn, TOut>
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
}
