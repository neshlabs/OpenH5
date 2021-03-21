using MongoDB.Bson.Serialization.Attributes;
using Nesh.Core.Utils;
using System;

namespace Nesh.Repository.Models
{
    public enum Platform
    {
        Sim = 1,
        WeChat = 2,
    }
    public class Account
    {
        [BsonId]
        public Guid user_id { get; set; }

        public string user_name { get; set; }

        public string password { get; set; }

        public string hash_slat { get; set; }

        public string access_token { get; set; }

        public Platform platform { get; set; }

        public string platform_id { get; set; }

        public DateTime create_time { get; set; }

        public static Account Create()
        {
            Account account = new Account();
            account.user_id = Guid.NewGuid();
            account.hash_slat = BCrypt.Net.BCrypt.GenerateSalt();
            account.create_time = TimeUtils.Now;

            return account;
        }
    }
}
