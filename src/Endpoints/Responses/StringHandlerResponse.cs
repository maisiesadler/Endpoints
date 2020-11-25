namespace Endpoints.Responses
{
    public class StringHandlerResponse : IHandlerResponse
    {
        public string S { get; }

        public StringHandlerResponse(string s)
        {
            S = s;
        }

        public static implicit operator StringHandlerResponse(string s) => new StringHandlerResponse(s);

        public string Response() => S;
    }
}
