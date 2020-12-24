using Microsoft.Extensions.DependencyInjection;
using Endpoints.Pipelines;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Endpoints.Instructions
{
    public static class RetrievePipelineInstructionsExtensions
    {
        public static (RetrievePipeline<TIn, TOut>, bool) TryGetPipeline<TRetriever, TIn, TOut>(
            this RetrievePipelineInstructions<TIn, TOut> instructions,
            IServiceProvider sp)
            where TRetriever : IRetriever<TIn, TOut>
        {
            var retriever = sp.GetRequiredService<TRetriever>();
            return instructions.TryGetPipeline(retriever, sp);
        }

        public static (RetrievePipeline<TIn, TOut>, bool) TryGetPipeline<TIn, TOut>(
            this RetrievePipelineInstructions<TIn, TOut> instructions,
            Func<TIn, Task<TOut>> retriever,
            IServiceProvider sp)
        {
            return instructions.TryGetPipeline(new FuncRetriever<TIn, TOut>(retriever), sp);
        }

        private static (RetrievePipeline<TIn, TOut>, bool) TryGetPipeline<TIn, TOut>(
            this RetrievePipelineInstructions<TIn, TOut> instructions,
            IRetriever<TIn, TOut> retriever,
            IServiceProvider sp)
        {
            if (!instructions.Validate())
            {
                return (null, false);
            }

            var middleware = BuildMiddleware(instructions, sp);
            var pipeline = new RetrievePipeline<TIn, TOut>(
                instructions.ParseModel, instructions.ParseResponse, retriever, middleware);

            return (pipeline, true);
        }


        public static Middleware<TOut> BuildMiddleware<TIn, TOut>(
            this RetrievePipelineInstructions<TIn, TOut> instructions, IServiceProvider sp)
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

        public async Task<TOut> Retrieve(TIn input)
        {
            return await _retriever(input);
        }
    }
}
