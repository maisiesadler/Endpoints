using Microsoft.Extensions.DependencyInjection;
using Endpoints.Pipelines;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace Endpoints.Instructions
{
    public class PipelineInstructions<TIn, TOut>
    {
        private static readonly Type[] _pipelineStageConstructor = new[] { typeof(PipelineStage<TIn, TOut>) };

        private readonly Stack<Type> _stages = new Stack<Type>();
        private readonly Dictionary<Type, PipelineStage<TIn, TOut>> _stageToNext = new Dictionary<Type, PipelineStage<TIn, TOut>>();
        private readonly Func<PipelineStage<TIn, TOut>, Pipeline<TIn, TOut>> _pipelineCreator;
        private bool _built = false;

        public PipelineInstructions(Func<PipelineStage<TIn, TOut>, Pipeline<TIn, TOut>> pipelineCreator)
        {
            _pipelineCreator = pipelineCreator;
        }

        public PipelineInstructions<TIn, TOut> Register<TPipelineStage>(PipelineStage<TIn, TOut> next = null)
           where TPipelineStage : PipelineStage<TIn, TOut>
        {
            var type = typeof(TPipelineStage);

            _stages.Push(type);
            _stageToNext.Add(type, next);

            return this;
        }

        public Pipeline<TIn, TOut> GetPipeline(IServiceProvider serviceProvider)
        {
            var (pipeline, ok) = TryGetPipeline(serviceProvider);
            if (!ok)
                throw new InvalidOperationException("Could not create pipeline");
            return pipeline;
        }

        public (Pipeline<TIn, TOut>, bool) TryGetPipeline(IServiceProvider serviceProvider)
        {
            var (stages, ok) = BuildStages(serviceProvider);
            if (!ok)
            {
                return (null, false);
            }

            var pipeline = _pipelineCreator(stages);

            return (pipeline, ok);
        }

        public (PipelineStage<TIn, TOut>, bool) BuildStages(IServiceProvider serviceProvider)
        {
            if (_built) return (null, false);
            _built = true;

            PipelineStage<TIn, TOut> stage = null;

            while (_stages.TryPop(out var type))
            {
                if (_stageToNext.TryGetValue(type, out var nextStage))
                {
                    if (stage == null)
                    {
                        stage = CreateInnerPipelineStage(type, serviceProvider);
                    }
                    else
                    {
                        stage = (PipelineStage<TIn, TOut>)Activator.CreateInstance(type, stage);
                    }
                }
                else
                {
                    return (null, false);
                }
            }

            return (stage, stage != null);
        }

        private PipelineStage<TIn, TOut> CreateInnerPipelineStage(Type type, IServiceProvider serviceProvider)
        {
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                var ctorParams = GetConstructorParameters(type, serviceProvider);
                return (PipelineStage<TIn, TOut>)Activator.CreateInstance(type, ctorParams);
            }
            else
            {
                return (PipelineStage<TIn, TOut>)Activator.CreateInstance(type);
            }
        }

        private object[] GetConstructorParameters(Type type, IServiceProvider serviceProvider)
        {
            var publicCtor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                                .OrderBy(x => x.GetParameters().Count())
                                .FirstOrDefault();

            if (publicCtor == null)
            {
                throw new InvalidOperationException("No public constructor for " + type.Name);
            }

            var ctorParams = new List<object>();
            foreach (var p in publicCtor.GetParameters())
            {
                ctorParams.Add(serviceProvider.GetRequiredService(p.ParameterType));
            }
            return ctorParams.ToArray();
        }
    }
}
