using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Codecs.Http.WebSockets.Extensions.Compression;
using DotNetty.Common;
using DotNetty.Handlers;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.IO;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Host.Gate.WebSocket
{
    public class WebSocketService : IHostedService
    {
        private readonly IClusterClient _ClusterClient;
        private readonly IConfiguration _Configuration;
        private readonly ILoggerFactory _LoggerFactory;
        private readonly ILogger _Logger;

        IEventLoopGroup _BossGroup;
        IEventLoopGroup _WorkGroup;
        ServerBootstrap _Bootstrap = new ServerBootstrap();
        IChannel _BootstrapChannel = null;

        public static bool ssl = false;

        public WebSocketService(
            IClusterClient client,
            IConfiguration config,
            ILoggerFactory loggerFactory)
        {
            ResourceLeakDetector.Level = ResourceLeakDetector.DetectionLevel.Disabled;
            _ClusterClient = client;
            _Configuration = config;
            _LoggerFactory = loggerFactory;
            _Logger = _LoggerFactory.CreateLogger<WebSocketService>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _Logger.LogInformation(
                $"\n{RuntimeInformation.OSArchitecture} {RuntimeInformation.OSDescription}" +
                $"\n{RuntimeInformation.ProcessArchitecture} {RuntimeInformation.FrameworkDescription}" +
                $"\nProcessor Count : {Environment.ProcessorCount}\n");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }
            _Logger.LogInformation($"Server garbage collection : {(GCSettings.IsServerGC ? "Enabled" : "Disabled")}");
            _Logger.LogInformation($"Current latency mode for garbage collection: {GCSettings.LatencyMode}");

            string websocketPath = _Configuration.GetSection("WebSocket")["path"];
            bool libuv = bool.Parse(_Configuration.GetSection("WebSocket")["libuv"]);
            int port = int.Parse(_Configuration.GetSection("WebSocket")["port"]);
            ssl = bool.Parse(_Configuration.GetSection("WebSocket")["ssl"]);

            if (libuv)
            {
                var dispatcher = new DispatcherEventLoopGroup();
                _BossGroup = dispatcher;
                _WorkGroup = new WorkerEventLoopGroup(dispatcher);
            }
            else
            {
                _BossGroup = new MultithreadEventLoopGroup(1);
                _WorkGroup = new MultithreadEventLoopGroup();
            }

            X509Certificate2 tlsCertificate = null;
            if (ssl)
            {
                tlsCertificate = new X509Certificate2(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dotnetty.com.pfx"), "password");
            }

            _BootstrapChannel = null;

            _Bootstrap = new ServerBootstrap();
            _Bootstrap.Group(_BossGroup, _WorkGroup);

            if (libuv)
            {
                _Bootstrap.Channel<TcpServerChannel>();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    _Bootstrap
                        .Option(ChannelOption.SoReuseport, true)
                        .ChildOption(ChannelOption.SoReuseaddr, true);
                }
            }
            else
            {
                _Bootstrap.Channel<TcpServerSocketChannel>();
            }

            _Bootstrap
                .Option(ChannelOption.SoBacklog, 8192)
                //.Handler(new LoggingHandler("LSTN"))
                .Handler(new ServerChannelRebindHandler(OnRebind))
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    if (ssl)
                    {
                        pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                    }

                    pipeline.AddLast("idleStateHandler", new IdleStateHandler(0, 0, 120));

                    //pipeline.AddLast(new LoggingHandler("CONN"));
                    //pipeline.AddLast(new HttpRequestDecoder());
                    //pipeline.AddLast(new HttpResponseEncoder());
                    pipeline.AddLast(new HttpServerCodec());
                    pipeline.AddLast(new HttpObjectAggregator(65536));
                    pipeline.AddLast(new WebSocketServerCompressionHandler());
                    pipeline.AddLast(new WebSocketServerProtocolHandler(
                        websocketPath: websocketPath,
                        subprotocols: null,
                        allowExtensions: true,
                        maxFrameSize: 65536,
                        allowMaskMismatch: true,
                        checkStartsWith: false,
                        dropPongFrames: true,
                        enableUtf8Validator: false));
                    pipeline.AddLast(new WebSocketServerHttpHandler(websocketPath, _LoggerFactory));
                    pipeline.AddLast(new WebSocketFrameAggregator(65536));
                    pipeline.AddLast(new WebSocketServerFrameHandler(_ClusterClient, _LoggerFactory));
                }));

            _BootstrapChannel = await _Bootstrap.BindAsync(IPAddress.Loopback, port);

            async void OnRebind()
            {
                await _BootstrapChannel.CloseAsync();
                _Logger.LogInformation("rebind......");
                var ch = await _Bootstrap.BindAsync(IPAddress.Loopback, port);
                _Logger.LogInformation("rebind complate");
                Interlocked.Exchange(ref _BootstrapChannel, ch);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _BootstrapChannel.CloseAsync();
                _Logger.LogInformation($"WebsocketService Shutdown");
            }
            finally
            {
                await Task.WhenAll(
                    _BossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    _WorkGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }
    }
}
