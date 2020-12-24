using System;
using System.Threading.Tasks;
using Endpoints.Instructions;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoints.Extensions
{
    public class PipelineRegistry
    {
        private readonly IServiceProvider _serviceProvider;

        public PipelineRegistry(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public RequestDelegate Get<TRetriever, TIn, TOut>()
            where TRetriever : IRetriever<TIn, TOut>
        {
            return async ctx =>
            {
                var instructions = _serviceProvider.GetRequiredService<PipelineInstructions<TIn, TOut>>();
                var retriever = _serviceProvider.GetRequiredService<TRetriever>();

                var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(_serviceProvider);
                if (!ok)
                    throw new Exception("Could not create pipeline");

                await pipeline.Run(retriever, ctx);
            };
        }

        public RequestDelegate Get<TIn, TOut>(Func<IServiceProvider, Func<TIn, Task<TOut>>> retrieverFn)
        {
            return async ctx =>
            {
                var instructions = _serviceProvider.GetRequiredService<PipelineInstructions<TIn, TOut>>();
                var retriever = retrieverFn(_serviceProvider);

                var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(_serviceProvider);
                if (!ok)
                    throw new Exception("Could not create pipeline");

                await pipeline.Run(new FuncRetriever<TIn, TOut>(retriever), ctx);
            };
        }

        public RequestDelegate Get<TService, TIn, TOut>(Func<TService, Func<TIn, Task<TOut>>> retrieverFn)
        {
            return async ctx =>
            {
                var instructions = _serviceProvider.GetRequiredService<PipelineInstructions<TIn, TOut>>();
                var service = _serviceProvider.GetRequiredService<TService>();
                var retriever = retrieverFn(service);

                var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(_serviceProvider);
                if (!ok)
                    throw new Exception("Could not create pipeline");

                await pipeline.Run(new FuncRetriever<TIn, TOut>(retriever), ctx);
            };
        }
    }
}
