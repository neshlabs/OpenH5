using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Serialization.ProtobufNet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Host.Gate
{
    public class ClusterHostedService : IHostedService
    {
        private readonly ILogger<ClusterHostedService> _Logger;
        private readonly IConfiguration _Config;

        public ClusterHostedService(ILogger<ClusterHostedService> logger, ILoggerProvider loggerProvider, IConfiguration config)
        {
            _Logger = logger;
            _Config = config;

            Client = new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = _Config.GetSection("Orleans")["ClusterId"];
                    options.ServiceId = _Config.GetSection("Orleans")["ServiceId"];
                })
                .Configure<SchedulingOptions>(options =>
                {
                    options.AllowCallChainReentrancy = true;
                    options.PerformDeadlockDetection = true;
                })
                .Configure<SerializationProviderOptions>(options =>
                {
                    options.SerializationProviders.Add(typeof(ProtobufNetSerializer));
                    options.FallbackSerializationProvider = typeof(ProtobufNetSerializer);
                })
                .Configure<HostOptions>(options =>
                {
                    options.ShutdownTimeout = TimeSpan.FromMinutes(1);
                })
                //.ConfigureServices()
                .ConfigureLogging(builder => builder.AddProvider(loggerProvider))
                .ConfigureApplicationParts(parts => { parts.AddFromApplicationBaseDirectory().WithReferences(); })
                .AddSimpleMessageStreamProvider("JobsProvider")
                .AddSimpleMessageStreamProvider("TransientProvider")
                .UseMongoDBClient(_Config.GetSection("Orleans")["Connection"])
                .UseMongoDBClustering(options =>
                {
                    options.DatabaseName = _Config.GetSection("Orleans")["Database"];
                    options.CreateShardKeyForCosmos = false;
                })
                .Build();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var attempt = 0;
            var maxAttempts = 100;
            var delay = TimeSpan.FromSeconds(1);
            return Client.Connect(async error =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                if (++attempt < maxAttempts)
                {
                    _Logger.LogWarning(error,
                        "Failed to connect to Orleans cluster on attempt {@Attempt} of {@MaxAttempts}.",
                        attempt, maxAttempts);

                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    _Logger.LogError(error,
                        "Failed to connect to Orleans cluster on attempt {@Attempt} of {@MaxAttempts}.",
                        attempt, maxAttempts);

                    return false;
                }
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Client.Close();
            }
            catch (OrleansException error)
            {
                _Logger.LogWarning(error, "Error while gracefully disconnecting from Orleans cluster. Will ignore and continue to shutdown.");
            }
        }

        public IClusterClient Client { get; }
    }
}
