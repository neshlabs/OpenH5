using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Nesh.Core.Data;
using Nesh.Core.Utils;
using Orleans;

namespace Host.Gate.Session
{
    class WSSession : GameSession
    {
        private IChannelHandlerContext ChannelHandlerContext { get; }
        public WSSession(IClusterClient client, ILoggerFactory loggerFactory, IChannelHandlerContext channelHandlerContext) :
            base(Protocol.WebSocket, client, loggerFactory)
        {
            ChannelHandlerContext = channelHandlerContext;
        }

        public override void OutcomingMessage(NList send_msg)
        {
            string data = JsonUtils.ToJson(send_msg);

            ChannelHandlerContext.Channel.WriteAndFlushAsync(new TextWebSocketFrame(data));
        }
    }
}
