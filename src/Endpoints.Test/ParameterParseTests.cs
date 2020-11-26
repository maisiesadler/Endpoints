using Xunit;
using Endpoints.Extensions;

namespace Endpoints.Test
{
    public class ParameterParseTests
    {
        [Fact]
        public void CanParseParameters()
        {
            // Arrange
            var endpoint = "/test/{testval}";
            var endpointDefinition = ParameterParseExtensions.ParseEndpointDefinition(endpoint);

            // Act
            var parameters = ParameterParseExtensions.Parse(endpointDefinition, "/test/testing");

            // Assert
            Assert.Single(parameters);
            Assert.True(parameters.TryGetValue("testval", out var testval));
            Assert.Equal("testing", testval);
        }

        [Fact]
        public void CanParseMultipleParameters()
        {
            // Arrange
            var endpoint = "/test/{testval}/{another}";
            var endpointDefinition = ParameterParseExtensions.ParseEndpointDefinition(endpoint);

            // Act
            var parameters = ParameterParseExtensions.Parse(endpointDefinition, "/test/testing/second");

            // Assert
            Assert.Equal(2, parameters.Count);
            Assert.True(parameters.TryGetValue("testval", out var testval));
            Assert.Equal("testing", testval);
        }
    }
}
