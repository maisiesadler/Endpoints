using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Endpoints.Attributes;
using Endpoints.Responses;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoints.Extensions
{
    public class Endpoint
    {
        private readonly Type _type;
        private readonly HandlerAttribute _handlerAttribute;
        private readonly MethodInfo _methodInfo;
        private readonly MethodAttribute _methodAttribute;
        private readonly IServiceProvider _serviceProvider;

        private readonly EndpointDefinition _endpointDefinition;

        internal Endpoint(
            Type type,
            HandlerAttribute handlerAttribute,
            MethodInfo methodInfo,
            MethodAttribute methodAttribute,
            IServiceProvider serviceProvider)
        {
            _type = type;
            _handlerAttribute = handlerAttribute;
            _methodInfo = methodInfo;
            _methodAttribute = methodAttribute;
            _serviceProvider = serviceProvider;

            _endpointDefinition = ParameterParseExtensions.ParseEndpointDefinition(Name);
        }

        public string Name => _handlerAttribute.Endpoint + _methodAttribute.Endpoint;

        public async Task RequestDelegate(HttpContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService(_type);

            var parameters = ParameterParseExtensions.Parse(_endpointDefinition, context.Request.Path);

            var @params = new List<object>();
            foreach (var p in _methodInfo.GetParameters())
            {
                if (parameters.TryGetValue(p.Name, out var paramValue))
                {
                    @params.Add(paramValue);
                }
                else
                {
                    @params.Add(null);
                }
            }

            var r = (IHandlerResponse)_methodInfo.Invoke(handler, @params.ToArray());
            await context.Response.WriteAsync(r.Response());
        }
    }
}