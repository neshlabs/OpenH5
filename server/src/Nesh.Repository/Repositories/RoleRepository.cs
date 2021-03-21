using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Nesh.Core.Utils;
using Nesh.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nesh.Repository.Repositories
{
    public interface IRoleRepository : IAuthRepository<Role>
    {
        Task<Role> GetRealmRole(Guid user_id, int realm);

        Task<Role> CreateRealmRole(Guid user_id, int realm);
    }

    public class RoleRepository : AuthRepository<Role>, IRoleRepository
    {
        private const string COLLECTION = "role";
        public RoleRepository(ILogger<RoleRepository> logger, IMongoClient mongo) : base(logger, mongo, COLLECTION)
        {
        }

        public async Task<Role> CreateRealmRole(Guid user_id, int realm)
        {
            Role role = new Role();
            role.id = 1;
            role.user_id = user_id;
            role.realm_id = realm;
            role.create_time = TimeUtils.Now;

            await this.Collection.InsertOneAsync(role);

            return role;
        }

        public async Task<Role> GetRealmRole(Guid user_id, int realm)
        {
            FilterDefinitionBuilder<Role> builder = Builders<Role>.Filter;
            FilterDefinition<Role> filter = builder.And(builder.Eq("user_id", user_id), builder.Eq("realm", realm));

            return (await Collection.FindAsync(filter)).FirstOrDefault();
        }
    }
}
