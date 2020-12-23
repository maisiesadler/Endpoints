using Microsoft.Extensions.DependencyInjection;
using Endpoints.Pipelines;
using System;

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
            this RetrievePipelineInstructions<TIn, TOut> instructions,
            IServiceProvider sp)
        {
            if (instructions.Middleware.Count == 0)
                return new DelegateMiddleware<TOut>();

            var inner = (IMiddleware<TOut>)sp.GetRequiredService(instructions.Middleware[0]);
            var middleware = new MiddlewareRunner<TOut>(inner, null);
            for (var i = 1; i < instructions.Middleware.Count; i++)
            {
                var type = instructions.Middleware[i];
                inner = (IMiddleware<TOut>)sp.GetRequiredService(instructions.Middleware[i]);
                middleware = new MiddlewareRunner<TOut>(inner, null);
            }

            return middleware;
        }
    }
}
