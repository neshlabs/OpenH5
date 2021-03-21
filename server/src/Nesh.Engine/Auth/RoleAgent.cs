using Game.Resources.Msg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nesh.Core;
using Nesh.Core.Auth;
using Nesh.Core.Data;
using Nesh.Repository.Repositories;
using Orleans;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nesh.Engine.Auth
{
    public class RoleAgent : Grain, IRoleAgent
    {
        private long Role { get; set; }
        private ILogger _Logger { get; set; }
        private IAsyncStream<NList> _ClientStream { get; set; }
        private IAccountRepository AccountRepository { get; set; }
        private Dictionary<int, Func<NList, Task>> _DispatchMsgs { get; set; }
        
        public override async Task OnActivateAsync()
        {
            Role = this.GetPrimaryKeyLong();
            _Logger = ServiceProvider.GetService<ILoggerFactory>().CreateLogger("RoleAgent[" + Role + "]");
            
            _DispatchMsgs = new Dictionary<int, Func<NList, Task>>();

            AccountRepository = ServiceProvider.GetService<IAccountRepository>();

            INode node = GrainFactory.GetGrain<INode>(Role);

            await node.BindAgent(this);

            await base.OnActivateAsync();
        }

        public Task BindSession(Guid user_id, string stream)
        {
            _ClientStream = GetStreamProvider(StreamProviders.AgentProvider).GetStream<NList>(user_id, stream);

            return Task.CompletedTask;
        }

        public async Task SendMessage(NList message)
        {
            await _ClientStream.OnNextAsync(message);
        }

        public async Task OnResponse(int message_id, NList message)
        {
            try
            {
                Func<NList, Task> found = null;
                if (_DispatchMsgs.TryGetValue(message_id, out found))
                {
                    return;
                }

                await found(message);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "OnResponse message_id={0} message={1}", message_id, message);
            }
        }
    }
}
