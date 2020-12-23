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
            var middleware = BuildMiddleware(instructions);
            var pipeline = new RetrievePipeline<TIn, TOut>(
                instructions.ParseModel, instructions.ParseResponse, retriever, middleware);

            return (pipeline, true);
        }

        public static Middleware<TOut> BuildMiddleware<TIn, TOut>(
            this RetrievePipelineInstructions<TIn, TOut> instructions)
        {
            if (instructions.Middleware.Count == 0)
                return new DelegateMiddleware<TOut>();

            var middleware = (Middleware<TOut>)Activator.CreateInstance(instructions.Middleware[0], (MiddlewareBase<TOut>)null);
            for (var i = 1; i < instructions.Middleware.Count; i++)
            {
                var type = instructions.Middleware[i];
                middleware = (Middleware<TOut>)Activator.CreateInstance(type, middleware);
            }

            return middleware;
        }
    }
}
