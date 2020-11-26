using System;
using System.Collections.Generic;
using System.Reflection;

namespace Endpoints.Extensions
{
    public class RequestDelegateExtensions
    {
        public static IEnumerable<Endpoint> GetEndpoints(Assembly assembly, IServiceProvider serviceProvider)
        {
            foreach (var (type, handlerAttribute) in DiscoveryExtensions.GetHandlers(assembly))
            {
                foreach (var (methodInfo, methodAttribute) in DiscoveryExtensions.GetMethods(type))
                {
                    yield return new Endpoint(type, handlerAttribute, methodInfo, methodAttribute, serviceProvider);
                }
            }
        }
    }
}
