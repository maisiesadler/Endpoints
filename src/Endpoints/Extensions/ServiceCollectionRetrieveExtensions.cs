using System;
using System.Threading.Tasks;
using Endpoints.Instructions;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoints.Extensions
{
    public static class ServiceCollectionRetrieveExtensions
    {
        public static RequestDelegate Get<TRetriever, TIn, TOut>(
            this IServiceProvider serviceProvider)
            where TRetriever : IRetriever<TIn, TOut>
        {
            return async ctx =>
            {
                var instructions = serviceProvider.GetRequiredService<PipelineInstructions<TIn, TOut>>();
                var retriever = serviceProvider.GetRequiredService<TRetriever>();

                var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(serviceProvider);
                if (!ok)
                    throw new Exception("Could not create pipeline");

                await pipeline.Run(retriever, ctx);
            };
        }

        public static RequestDelegate Get<TRetriever, TOut>(
           this IServiceProvider serviceProvider)
           where TRetriever : IRetriever<TOut>
        {
            return async ctx =>
            {
                var instructions = serviceProvider.GetRequiredService<PipelineInstructions<TOut>>();
                var retriever = serviceProvider.GetRequiredService<TRetriever>();

                var (pipeline, ok) = instructions.TryGetPipeline<TOut>(serviceProvider);
                if (!ok)
                    throw new Exception("Could not create pipeline");

                await pipeline.Run(retriever, ctx);
            };
        }

        public static RequestDelegate Get<TIn, TOut>(
            this IServiceProvider serviceProvider,
            Func<IServiceProvider, Func<TIn, Task<TOut>>> retrieverFn)
        {
            return async ctx =>
            {
                var instructions = serviceProvider.GetRequiredService<PipelineInstructions<TIn, TOut>>();
                var retriever = retrieverFn(serviceProvider);

                var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(serviceProvider);
                if (!ok)
                    throw new Exception("Could not create pipeline");

                await pipeline.Run(new FuncRetriever<TIn, TOut>(retriever), ctx);
            };
        }

        public static RequestDelegate Get<TService, TIn, TOut>(
            this IServiceProvider serviceProvider,
            Func<TService, Func<TIn, Task<TOut>>> retrieverFn)
        {
            return async ctx =>
            {
                var instructions = serviceProvider.GetRequiredService<PipelineInstructions<TIn, TOut>>();
                var service = serviceProvider.GetRequiredService<TService>();
                var retriever = retrieverFn(service);

                var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(serviceProvider);
                if (!ok)
                    throw new Exception("Could not create pipeline");

                await pipeline.Run(new FuncRetriever<TIn, TOut>(retriever), ctx);
            };
        }
    }
}
