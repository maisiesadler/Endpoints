using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Endpoints.Extensions
{
    public class ParameterParseExtensions
    {
        private static readonly Regex _parameterRegex = new Regex("{([^{}/]+)}");

        public static EndpointDefinition ParseEndpointDefinition(string endpoint)
        {
            var parameters = new Dictionary<string, string>();

            var paramNames = new List<string>();
            while (_parameterRegex.IsMatch(endpoint))
            {
                var m = _parameterRegex.Match(endpoint);
                paramNames.Add(m.Groups[1].Value);
                endpoint = Regex.Replace(endpoint, "{" + m.Groups[1].Value + "}", "([^/]+)");
            }

            return new EndpointDefinition(endpoint, paramNames); 
        }

        public static Dictionary<string, string> Parse(EndpointDefinition parsedPath, string path)
        {
            var parameters = new Dictionary<string, string>();
            var match = parsedPath.Regex.Match(path);

            for (var i = 1; i < match.Groups.Count; i++)
            {
                var group = match.Groups[i];
                var param = parsedPath.ParameterNames[i - 1];
                parameters[param] = group.Value;
            }

            return parameters;
        }
    }
}
