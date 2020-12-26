using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Example.Api.Domain
{
    public interface IDatabase
    {
        Task<User> Get(string id);
        Task<string> Create(string name);
    }

    public class MemoryDatabase : IDatabase
    {
        private readonly Dictionary<string, User> _users = new Dictionary<string, User>();

        public Task<User> Get(string id)
        {
            var user = _users.GetValueOrDefault(id);
            return Task.FromResult(user);
        }

        public Task<string> Create(string name)
        {
            var id = Guid.NewGuid().ToString();
            var user = new User(id, name);
            _users.Add(id, user);

            return Task.FromResult(id);
        }
    }
}
