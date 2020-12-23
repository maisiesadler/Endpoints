using Microsoft.Extensions.DependencyInjection;
using Endpoints.Pipelines;
using System;
using System.Collections.Generic;

namespace Endpoints.Instructions
{
    public static class RetrievePipelineInstructionsExtensions
    {
        public static (RetrievePipeline<TIn, TOut>, bool) TryGetPipeline<TRetriever, TIn, TOut>(
            this RetrievePipelineInstructions<TIn, TOut> instructions,
            IServiceProvider sp)
            where TRetriever : IRetriever<TIn, TOut>
        {
            if (!instructions.Validate())
            {
                return (null, false);
            }

            var retriever = sp.GetRequiredService<TRetriever>();
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
}
