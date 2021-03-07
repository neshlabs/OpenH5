using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static DotNetty.Codecs.Http.HttpResponseStatus;

namespace Host.Gate.WebSocket
{
    public sealed class WebSocketServerHttpHandler : SimpleChannelInboundHandler2<IFullHttpRequest>
    {
        private readonly ILogger _Logger;

        readonly string websocketPath;

        public WebSocketServerHttpHandler(string websocketPath, ILoggerFactory loggerFactory)
        {
            this.websocketPath = websocketPath;
            _Logger = loggerFactory.CreateLogger<WebSocketServerHttpHandler>();
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            // Handle a bad request.
            if (!req.Result.IsSuccess)
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(req.ProtocolVersion, BadRequest, ctx.Allocator.Buffer(0)));
                return;
            }

            // Allow only GET methods.
            if (!HttpMethod.Get.Equals(req.Method))
            {
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(req.ProtocolVersion, Forbidden, ctx.Allocator.Buffer(0)));
                return;
            }

            // Send the demo page and favicon.ico
            switch (req.Uri)
            {
                case "/benchmark":
                    {
                        IByteBuffer content = WebSocketServerBenchmarkPage.GetContent(GetWebSocketLocation(req, this.websocketPath));
                        var res = new DefaultFullHttpResponse(req.ProtocolVersion, OK, content);

                        res.Headers.Set(HttpHeaderNames.ContentType, "text/html; charset=UTF-8");
                        HttpUtil.SetContentLength(res, content.ReadableBytes);

                        SendHttpResponse(ctx, req, res);
                        return;
                    }
                case "/":
                case "/index.html":
                    {
                        IByteBuffer content = WebSocketServerIndexPage.GetContent(GetWebSocketLocation(req, this.websocketPath));
                        var res = new DefaultFullHttpResponse(req.ProtocolVersion, OK, content);

                        res.Headers.Set(HttpHeaderNames.ContentType, "text/html; charset=UTF-8");
                        HttpUtil.SetContentLength(res, content.ReadableBytes);

                        SendHttpResponse(ctx, req, res);
                        return;
                    }
                case "/favicon.ico":
                default:
                    {
                        var res = new DefaultFullHttpResponse(req.ProtocolVersion, NotFound, ctx.Allocator.Buffer(0));
                        SendHttpResponse(ctx, req, res);
                        return;
                    }
            }
        }

        static void SendHttpResponse(IChannelHandlerContext ctx, IFullHttpRequest req, IFullHttpResponse res)
        {
            // Generate an error page if response getStatus code is not OK (200).
            HttpResponseStatus responseStatus = res.Status;
            if (responseStatus.Code != 200)
            {
                ByteBufferUtil.WriteUtf8(res.Content, responseStatus.ToString());
                HttpUtil.SetContentLength(res, res.Content.ReadableBytes);
            }

            // Send the response and close the connection if necessary.
            var keepAlive = HttpUtil.IsKeepAlive(req) && responseStatus.Code == 200;
            HttpUtil.SetKeepAlive(res, keepAlive);
            var future = ctx.WriteAndFlushAsync(res);
            if (!keepAlive)
            {
                future.CloseOnComplete(ctx.Channel);
            }
        }

        static string GetWebSocketLocation(IFullHttpRequest req, string path)
        {
            bool result = req.Headers.TryGet(HttpHeaderNames.Host, out ICharSequence value);
            Debug.Assert(result, "Host header does not exist.");
            string location = value.ToString() + path;

            if (WebSocketService.ssl)
            {
                return "wss://" + location;
            }
            else
            {
                return "ws://" + location;
            }
        }
    }
}
