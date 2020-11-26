using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Endpoints.Extensions
{
    public class EndpointDefinition
    {
        public Regex Regex { get; }
        public IReadOnlyList<string> ParameterNames { get; }

        internal EndpointDefinition(string endpoint, IReadOnlyList<string> parameterNames)
        {
            Regex = new Regex(endpoint);  
            ParameterNames = parameterNames;
        }
    }
}
