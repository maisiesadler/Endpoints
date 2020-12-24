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
            var instructions = _serviceProvider.GetRequiredService<PipelineInstructions<TIn, TOut>>();
            var (pipeline, ok) = instructions.TryGetPipeline<TRetriever, TIn, TOut>(_serviceProvider);
            if (!ok)
                throw new Exception("Could not create pipeline");

            return pipeline.Run;
        }

        public RequestDelegate Get<TIn, TOut>(Func<IServiceProvider, Func<TIn, Task<TOut>>> retrieverFn)
        {
            var instructions = _serviceProvider.GetRequiredService<PipelineInstructions<TIn, TOut>>();
            var retriever = retrieverFn(_serviceProvider);
            var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(retriever, _serviceProvider);
            if (!ok)
                throw new Exception("Could not create pipeline");

            return pipeline.Run;
        }

        public RequestDelegate Get<TService, TIn, TOut>(Func<TService, Func<TIn, Task<TOut>>> retrieverFn)
        {
            var instructions = _serviceProvider.GetRequiredService<PipelineInstructions<TIn, TOut>>();
            var service = _serviceProvider.GetRequiredService<TService>();
            var retriever = retrieverFn(service);
            var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(retriever, _serviceProvider);
            if (!ok)
                throw new Exception("Could not create pipeline");

            return pipeline.Run;
        }
    }
}
