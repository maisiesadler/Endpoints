namespace Example.Api.Domain
{
    public record User
    {
        public string Id { get; }
        public string Name { get; }

        public User(string id, string name) => (Id, Name) = (id, name);
    }
}
