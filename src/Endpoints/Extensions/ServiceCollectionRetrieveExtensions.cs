using System;
using System.Threading.Tasks;
using Endpoints.Instructions;
using Endpoints.Pipelines;
using Endpoints.Pipelines.Retrievers;
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

                await TryRunPipeline<TIn, TOut>(serviceProvider, instructions, retriever, ctx);
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

                var retrieverImpl = new FuncRetriever<TIn, TOut>(retriever);
                await TryRunPipeline<TIn, TOut>(serviceProvider, instructions, retrieverImpl, ctx);
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

                var retrieverImpl = new FuncRetriever<TIn, TOut>(retriever);
                await TryRunPipeline<TIn, TOut>(serviceProvider, instructions, retrieverImpl, ctx);
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

                await TryRunPipeline<TOut>(serviceProvider, instructions, retriever, ctx);
            };
        }

        public static RequestDelegate Get<TOut>(
            this IServiceProvider serviceProvider,
            Func<IServiceProvider, Func<Task<TOut>>> retrieverFn)
        {
            return async ctx =>
            {
                var instructions = serviceProvider.GetRequiredService<PipelineInstructions<TOut>>();
                var retriever = retrieverFn(serviceProvider);

                var retrieverImpl = new FuncRetriever<TOut>(retriever);
                await TryRunPipeline<TOut>(serviceProvider, instructions, retrieverImpl, ctx);
            };
        }

        public static RequestDelegate Get<TService, TOut>(
            this IServiceProvider serviceProvider,
            Func<TService, Func<Task<TOut>>> retrieverFn)
        {
            return async ctx =>
            {
                var instructions = serviceProvider.GetRequiredService<PipelineInstructions<TOut>>();
                var service = serviceProvider.GetRequiredService<TService>();
                var retriever = retrieverFn(service);

                var retrieverImpl = new FuncRetriever<TOut>(retriever);
                await TryRunPipeline<TOut>(serviceProvider, instructions, retrieverImpl, ctx);
            };
        }

        private static async Task TryRunPipeline<TOut>(
            IServiceProvider sp,
            PipelineInstructions<TOut> instructions,
            IRetriever<TOut> retriever,
            HttpContext context)
        {
            var (pipeline, ok) = instructions.TryGetPipeline<TOut>(sp);
            if (!ok)
                throw new Exception("Could not create pipeline");

            var retrieverImpl = new RetrieverImpl<TOut>(retriever);
            await pipeline.Run(retrieverImpl, context);
        }

        private static async Task TryRunPipeline<TIn, TOut>(
            IServiceProvider sp,
            PipelineInstructions<TIn, TOut> instructions,
            IRetriever<TIn, TOut> retriever,
            HttpContext context)
        {
            var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(sp);
            if (!ok)
                throw new Exception("Could not create pipeline");

            await pipeline.Run(retriever, context);
        }
    }
}
