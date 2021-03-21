using DotNetty.Buffers;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Host.Gate.Session;
using Microsoft.Extensions.Logging;
using Nesh.Core.Data;
using Nesh.Core.Utils;
using Orleans;
using System;

namespace Host.Gate.WebSocket
{
    public sealed class WebSocketServerFrameHandler : SimpleChannelInboundHandler<WebSocketFrame>
    {
        private readonly ILoggerFactory _LoggerFactory;
        private readonly IClusterClient _ClusterClient;
        private readonly ILogger _Logger;
        private WSSession _Session;

        public WebSocketServerFrameHandler(IClusterClient client, ILoggerFactory loggerFactory)
        {
            _ClusterClient = client;
            _LoggerFactory = loggerFactory;
            _Logger = _LoggerFactory.CreateLogger<WebSocketServerFrameHandler>();
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, WebSocketFrame frame)
        {
            try
            {
                if (frame is PingWebSocketFrame)
                {
                    ctx.WriteAsync(new PongWebSocketFrame((IByteBuffer)frame.Content.Retain()));
                    return;
                }

                if (frame is TextWebSocketFrame textFrame)
                {
                    var json = textFrame.Text();
                    if (json.StartsWith("throw ", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception(json.Substring(6, json.Length - 6));
                    }

                    NList recv_msg = JsonUtils.ToObject<NList>(json);
                    _Session.IncomingMessage(recv_msg);
                    return;
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
            _Session = new WSSession(_ClusterClient, _LoggerFactory, context);
            base.ChannelRegistered(context);
        }

        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            _Session.Close();
            base.ChannelUnregistered(context);
        }
    }
}
