using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Nesh.Repository.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nesh.Repository.Repositories
{
    public interface IAccountRepository : IAuthRepository<Account>
    {
        Task<Account> GetByToken(string access_token);

        Task<Account> GetByPlatformId(string platform_id);

        Task<Account> GetByUserName(string user_name);

        Task CreateAccount(Account account);

        Task RefreshToken(Guid user_id, string access_token);

        Task<Account> GetByUserId(Guid user_id);
    }

    public class AccountRepository : AuthRepository<Account>, IAccountRepository
    {
        private const string COLLECTION = "account";
        public AccountRepository(ILogger<AccountRepository> logger, IMongoClient mongo) : base(logger, mongo, COLLECTION)
        {
        }

        public async Task<Account> GetByToken(string access_token)
        {
            var filter = Builders<Account>.Filter.Eq("access_token", access_token);

            return (await Collection.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<Account> GetByPlatformId(string platform_id)
        {
            var filter = Builders<Account>.Filter.Eq("platform_id", platform_id);

            return (await Collection.FindAsync(filter)).FirstOrDefault();
        }

        public async Task CreateAccount(Account account)
        {
            await this.Collection.InsertOneAsync(account);
        }

        public async Task RefreshToken(Guid user_id, string access_token)
        {
            var filter = Builders<Account>.Filter.Eq("user_id", user_id);
            var update = Builders<Account>.Update.Set("access_token", access_token);
            await Collection.UpdateOneAsync(filter, update);
        }

        public async Task<Account> GetByUserName(string user_name)
        {
            var filter = Builders<Account>.Filter.Eq("user_name", user_name);

            return (await Collection.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<Account> GetByUserId(Guid user_id)
        {
            var filter = Builders<Account>.Filter.Eq("_id", user_id);

            return (await Collection.FindAsync(filter)).FirstOrDefault();
        }
    }
}
