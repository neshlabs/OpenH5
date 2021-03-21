using Game.Resources.Msg;
using Microsoft.Extensions.Logging;
using Nesh.Core.Auth;
using Nesh.Core.Data;
using Orleans;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Host.Gate.Session
{
    public enum Protocol
    {
        WebSocket = 1,
        TcpSocket = 2,
        UdpSocket = 3,
        KcpSocket = 4,
    }

    abstract class GameSession
    {
        protected IClusterClient _ClusterClient;
        protected ILogger _Logger;
        protected IAgentObserver _AgentObserver;
        protected IUser _User;
        protected StreamSubscriptionHandle<NList> _SubscriptionHandle;
        protected Protocol Protocol;
        protected IRoleAgent RoleAgent;

        protected bool Inited { get; set; }

        public GameSession(Protocol protocol, IClusterClient client, ILoggerFactory loggerFactory)
        {
            Protocol = protocol;
            _ClusterClient = client;
            _Logger = loggerFactory.CreateLogger<GameSession>();
            Inited = false;
        }

        public abstract void OutcomingMessage(NList message);

        internal void IncomingMessage(NList message)
        {
            int message_id = message.Get<int>(0);
            NList dispatch_msg = message.GetRange(1, message.Count - 1);

            if (message_id == SystemMsg.CLIENT.ACCESS_TOKEN && !Inited)
            {
                string access_token = dispatch_msg.Get<string>(0);
                int realm = dispatch_msg.Get<int>(1);
                IAccessToken token = _ClusterClient.GetGrain<IAccessToken>(access_token);
                Guid user_id = token.GetUserId().Result;
                _User = _ClusterClient.GetGrain<IUser>(user_id);
                long role = _User.GetRole(realm).Result;

                RoleAgent = _ClusterClient.GetGrain<IRoleAgent>(role);
                RoleAgent.BindSession(user_id, Protocol.ToString());
                _AgentObserver = new AgentObserver(this);
                var stream = _ClusterClient.GetStreamProvider(StreamProviders.AgentProvider).GetStream<NList>(user_id, Protocol.ToString());
                _SubscriptionHandle = stream.SubscribeAsync(_AgentObserver).Result;

                Inited = true;

                OutcomingMessage(NList.New().Add(SystemMsg.SERVER.ACCESS_TOKEN).Add(true));
            }
            else if (Inited)
            {
                if (RoleAgent != null)
                {
                    RoleAgent.OnResponse(message_id, dispatch_msg);
                }
            }
        }

        internal void Close()
        {
            if (_SubscriptionHandle != null) _SubscriptionHandle.UnsubscribeAsync();
        }
    }

    class AgentObserver : IAgentObserver
    {
        private GameSession _Session;

        public AgentObserver(GameSession session)
        {
            _Session = session;
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task OnNextAsync(NList msg, StreamSequenceToken token = null)
        {
            if (_Session != null)
            {
                _Session.OutcomingMessage(msg);
            }
            return Task.CompletedTask;
        }
    }
}
