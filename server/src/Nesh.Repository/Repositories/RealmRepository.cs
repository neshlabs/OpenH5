using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Nesh.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nesh.Repository.Repositories
{
    public interface IRealmRepository : IAuthRepository<Realm>
    {
        Task<Realm> GetRealm(int realm);
    }

    public class RealmRepository : AuthRepository<Realm>, IRealmRepository
    {
        private const string COLLECTION = "realm";
        public RealmRepository(ILogger<RealmRepository> logger, IMongoClient mongo) : base(logger, mongo, COLLECTION)
        {
        }

        public async Task<Realm> GetRealm(int realm)
        {
            var filter = Builders<Realm>.Filter.Eq("realm", realm);

            return (await Collection.FindAsync(filter)).FirstOrDefault();
        }
    }
}
