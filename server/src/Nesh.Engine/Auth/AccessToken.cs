using Game.Resources.Msg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nesh.Core;
using Nesh.Core.Auth;
using Nesh.Core.Data;
using Nesh.Repository.Models;
using Nesh.Repository.Repositories;
using Orleans;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nesh.Engine.Auth
{
    public class AccessToken : Grain, IAccessToken
    {
        private string _Token { get; set; }
        private ILogger _Logger { get; set; }
        private IAccountRepository AccountRepository { get; set; }

        public override Task OnActivateAsync()
        {
            _Token = this.GetPrimaryKeyString();
            _Logger = ServiceProvider.GetService<ILoggerFactory>().CreateLogger("AccessToken[" + _Token + "]");

            AccountRepository = ServiceProvider.GetService<IAccountRepository>();

            return base.OnActivateAsync();
        }

        public async Task<Guid> GetUserId()
        {
            Account account = await AccountRepository.GetByToken(_Token);

            return account != null ? account.user_id : Guid.Empty;
        }
    }
}
