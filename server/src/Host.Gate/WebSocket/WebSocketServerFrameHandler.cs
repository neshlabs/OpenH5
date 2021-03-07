using DotNetty.Buffers;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Nesh.Core;
using Nesh.Core.Data;
using Nesh.Core.Utils;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Host.Gate.WebSocket
{
    public sealed class WebSocketServerFrameHandler : SimpleChannelInboundHandler<WebSocketFrame>
    {
        private readonly ILogger _Logger;
        private readonly IClusterClient _Client;

        public WebSocketServerFrameHandler(IClusterClient client, ILoggerFactory loggerFactory)
        {
            _Client = client;
            _Logger = loggerFactory.CreateLogger<WebSocketServerFrameHandler>();
        }

        protected override async void ChannelRead0(IChannelHandlerContext ctx, WebSocketFrame frame)
        {
            try
            {
                if (frame is PingWebSocketFrame)
                {
                    await ctx.WriteAsync(new PongWebSocketFrame((IByteBuffer)frame.Content.Retain()));
                    return;
                }

                if (frame is TextWebSocketFrame textFrame)
                {
                    var msg = textFrame.Text();
                    if (msg.StartsWith("throw ", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception(msg.Substring(6, msg.Length - 6));
                    }

                    /*NList list = JsonUtils.ToObject<NList>(msg);
                    string data = JsonUtils.ToJson(list);
                    // Echo the frame
                    await ctx.Channel.WriteAndFlushAsync(new TextWebSocketFrame(data));*/


                    long id1 = 12058624;
                    INode node1 = _Client.GetGrain<INode>(id1);
                    NList list = await node1.GetEntities();
                    //NList list2 = NList.New().Add(true).Add(1000).Add("adasdasdasdas");
                    string data = JsonUtils.ToJson(list);

                    await ctx.Channel.WriteAndFlushAsync(new TextWebSocketFrame(data));

                    return;
                }

                if (frame is BinaryWebSocketFrame binaryFrame)
                {
                    // Echo the frame
                    await ctx.Channel.WriteAndFlushAsync(new BinaryWebSocketFrame(binaryFrame.Content.RetainedDuplicate()));
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "ChannelRead0 error");
            }
        }

        //public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            _Logger.LogError(e, $"{nameof(WebSocketServerFrameHandler)} caught exception:");
            ctx.CloseAsync();
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            switch (evt)
            {
                case IdleStateEvent stateEvent:
                    _Logger.LogWarning($"{nameof(WebSocketServerFrameHandler)} caught idle state: {stateEvent.State}");
                    break;

                case WebSocketServerProtocolHandler.HandshakeComplete handshakeComplete:
                    if (context.Pipeline.Get<WebSocketServerHttpHandler>() != null) { context.Pipeline.Remove<WebSocketServerHttpHandler>(); }
                    _Logger.LogInformation($"RequestUri: {handshakeComplete.RequestUri}, \r\nHeaders:{handshakeComplete.RequestHeaders}, \r\nSubprotocol: {handshakeComplete.SelectedSubprotocol}");
                    break;

                default:
                    break;
            }
        }

        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            base.ChannelRegistered(context);
        }

        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            base.ChannelUnregistered(context);
        }
    }
}
