using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nesh.Core.Auth;
using Nesh.Repository.Models;
using Nesh.Repository.Repositories;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Nesh.Engine.Auth
{
    public class User : Grain, IUser
    {
        private Guid _UserId { get; set; }
        private ILogger _Logger { get; set; }
        private IRoleRepository RoleRepository { get; set; }
        private IAccountRepository AccountRepository { get; set; }

        public override Task OnActivateAsync()
        {
            _UserId = this.GetPrimaryKey();
            _Logger = ServiceProvider.GetService<ILoggerFactory>().CreateLogger("User[" + _UserId + "]");

            RoleRepository = ServiceProvider.GetService<IRoleRepository>();

            AccountRepository = ServiceProvider.GetService<IAccountRepository>();

            return base.OnActivateAsync();
        }

        public async Task<long> GetRole(int realm)
        {
            Role role = await RoleRepository.GetRealmRole(_UserId, realm);
            if (role == null)
            {
                role = await RoleRepository.CreateRealmRole(_UserId, realm);
            }

            return role.id;
        }
    }
}
