using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;

namespace Nesh.Repository.Repositories
{
    public interface IAuthRepository<T>
    {
    }

    public abstract class AuthRepository<T> : IAuthRepository<T> where T : class
    {
        protected IMongoDatabase Database { get; }
        protected string CollectionName;
        protected IMongoCollection<T> Collection;                                                                           
        private IMongoClient MongoClient { get; }
        private ILogger Logger { get; }
        private const string DATABASE = "auth";

        public AuthRepository(ILogger<IAuthRepository<T>> logger, IMongoClient mongo, string collectionName)
        {
            CollectionName = collectionName;

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            MongoClient = mongo ?? throw new ArgumentNullException(nameof(mongo));
            Database = MongoClient.GetDatabase(DATABASE);
            Collection = Database.GetCollection<T>(CollectionName);
        }
    }
}
