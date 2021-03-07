using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nesh.Core;
using Nesh.Core.Data;
using Nesh.Core.Manager;
using Nesh.Core.Utils;
using Nesh.Engine.Node;
using Nesh.Engine.Utils;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Serialization.ProtobufNet;
using Serilog;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Protobuf;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace Host.Silo
{
    class Program
    {
        public static IConfiguration Configuration { get; private set; }

        static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);
        static ISiloHost silo;

        private static void LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", false, false);

            IConfiguration config = configurationBuilder.Build();
            CacheUtils.EntityDBs = int.Parse(config.GetSection("RedisCache")["EntityDBs"]);
            PersistUtils.Connection = config.GetSection("MongoPersist")["Connection"];
            PersistUtils.EntityDB = config.GetSection("MongoPersist")["EntityDB"];
            PersistUtils.Init();

            Configuration = config;
        }

        private static void LoadGameConfigs()
        {
            string directory = Environment.CurrentDirectory;
            int index = directory.IndexOf("src");
            if (index > -1)
                directory = directory.Remove(index, directory.Length - index);

            string res_path = Path.Combine(directory, "build/res");

            DefineManager.Load(res_path);
        }

        private static void LoadModules()
        {
            var moduels = from t in Assembly.Load("Game.Modules").GetTypes()
                          where Global.IsSubClassOf(t, typeof(NModule))
                          select t;

            foreach (Type type in moduels)
            {
                NModule.LoadModule(type);
            }
        }

        static async Task StopSilo()
        {
            await silo.StopAsync();
            _siloStopped.Set();
        }

        private static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (s, a) => {
                Task.Run(StopSilo);
                /// Wait for the silo to completely shutdown before exiting. 
                _siloStopped.WaitOne();
                /// Now race to finish ... who will finish first?
                /// If I finish first, the application will hang! :(
            };

            LoadConfiguration();
            LoadGameConfigs();
            LoadModules();

            if (GCSettings.IsServerGC)
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }

            silo = await CreateSilo();

            Console.WriteLine(@"
      ___                                     ___           ___           ___           ___                 
     /  /\        ___                        /  /\         /__/\         /  /\         /  /\          ___   
    /  /:/_      /  /\                      /  /::\        \  \:\       /  /::\       /  /:/_        /  /\  
   /  /:/ /\    /  /:/      ___     ___    /  /:/\:\        \__\:\     /  /:/\:\     /  /:/ /\      /  /:/  
  /  /:/ /::\  /__/::\     /__/\   /  /\  /  /:/  \:\   ___ /  /::\   /  /:/  \:\   /  /:/ /::\    /  /:/   
 /__/:/ /:/\:\ \__\/\:\__  \  \:\ /  /:/ /__/:/ \__\:\ /__/\  /:/\:\ /__/:/ \__\:\ /__/:/ /:/\:\  /  /::\   
 \  \:\/:/~/:/    \  \:\/\  \  \:\  /:/  \  \:\ /  /:/ \  \:\/:/__\/ \  \:\ /  /:/ \  \:\/:/~/:/ /__/:/\:\  
  \  \::/ /:/      \__\::/   \  \:\/:/    \  \:\  /:/   \  \::/       \  \:\  /:/   \  \::/ /:/  \__\/  \:\ 
   \__\/ /:/       /__/:/     \  \::/      \  \:\/:/     \  \:\        \  \:\/:/     \__\/ /:/        \  \:\
     /__/:/        \__\/       \__\/        \  \::/       \  \:\        \  \::/        /__/:/          \__\/
     \__\/                                   \__\/         \__\/         \__\/         \__\/                
           ");

            Console.WriteLine("Hello Nesh!");

            _siloStopped.WaitOne();
        }

        private static async Task<ISiloHost> CreateSilo()
        {
            var conf = new RedisConfiguration()
            {
                AbortOnConnectFail = true,
                Hosts = new RedisHost[]
                {
                    new RedisHost
                    {
                        Host = Configuration.GetSection("RedisCache")["Connection"],
                        Port = int.Parse(Configuration.GetSection("RedisCache")["Port"])
                    }
                },
                AllowAdmin = true,
                ConnectTimeout = 1000,
                ServerEnumerationStrategy = new ServerEnumerationStrategy()
                {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
                }
            };

            var builder = new SiloHostBuilder()
               .Configure<ClusterOptions>(options =>
               {
                   options.ClusterId = Configuration.GetSection("Orleans")["ClusterId"];
                   options.ServiceId = Configuration.GetSection("Orleans")["ServiceId"];
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
               .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
               .UseMongoDBClient(Configuration.GetSection("Orleans")["Connection"])
               .AddSimpleMessageStreamProvider("JobsProvider")
               .AddSimpleMessageStreamProvider("TransientProvider")
               .UseMongoDBReminders(options =>
               {
                   options.DatabaseName = Configuration.GetSection("Orleans")["Database"];
                   options.CreateShardKeyForCosmos = false;
               })
                .UseMongoDBClustering(c =>
                {
                    c.DatabaseName = Configuration.GetSection("Orleans")["Database"];
                    c.CreateShardKeyForCosmos = false;
                    // c.UseJsonFormat = true;
                })
                .AddMongoDBGrainStorageAsDefault(c => c.Configure(options =>
                {
                    options.DatabaseName = Configuration.GetSection("Orleans")["Database"];
                    options.CreateShardKeyForCosmos = false;
                }))
                .AddMongoDBGrainStorage("PubSubStore", options =>
                {
                    options.DatabaseName = Configuration.GetSection("Orleans")["Database"];
                    options.CreateShardKeyForCosmos = false;
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddSerilog(new LoggerConfiguration()
                        .ReadFrom.Configuration(Configuration)
                        .CreateLogger());
                })
                .AddStartupTask<NStartupTask>()
                .ConfigureApplicationParts(parts => parts
                .AddApplicationPart(typeof(Node).Assembly).WithReferences())
                .ConfigureServices(services =>
                {
                    services.AddStackExchangeRedisExtensions<ProtobufSerializer>(conf);
                    services.AddSingleton<MongoDB.Driver.IMongoClient>(s => new MongoDB.Driver.MongoClient(PersistUtils.Connection));
                })
                //need to configure a grain storage called "PubSubStore" for using streaming with ExplicitSubscribe pubsub type
                .AddMemoryGrainStorage("PubSubStore")
                //Depends on your application requirements, you can configure your silo with other stream providers, which can provide other features, 
                //such as persistence or recoverability. For more information, please see http://dotnet.github.io/orleans/Documentation/Orleans-Streams/Stream-Providers.html
                .AddSimpleMessageStreamProvider("ClientStream");

            var host = builder.Build();
            await host.StartAsync();

            return host;
        }
    }

    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Add StackExchange.Redis with its serialization provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="redisConfiguration">The redis configration.</param>
        /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
        public static IServiceCollection AddStackExchangeRedisExtensions<T>(
            this IServiceCollection services,
            RedisConfiguration redisConfiguration)
            where T : class, ISerializer, new()
        {
            return services.AddStackExchangeRedisExtensions<T>(sp => redisConfiguration);
        }

        /// <summary>
        /// Add StackExchange.Redis with its serialization provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="redisConfigurationFactory">The redis configration factory.</param>
        /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
        public static IServiceCollection AddStackExchangeRedisExtensions<T>(
            this IServiceCollection services,
            Func<IServiceProvider, RedisConfiguration> redisConfigurationFactory)
            where T : class, ISerializer, new()
        {
            services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<ISerializer, T>();

            services.AddSingleton((provider) => provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration());

            services.AddSingleton(redisConfigurationFactory);

            return services;
        }
    }

    public class NStartupTask : IStartupTask
    {
        private readonly IGrainFactory _GrainFactory;

        public NStartupTask(IGrainFactory grain_factory)
        {
            _GrainFactory = grain_factory;
        }

        /*public Task Execute(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }*/

        public async Task Execute(CancellationToken cancellationToken)
        {
            long id2 = IdUtils.UGen.CreateId();
            NList lst = NList.New().Add(Nuid.New(id2, id2)).Add(TimeUtils.Now).Add(true);

            string json = JsonUtils.ToJson(lst);

            NList LST2 = JsonUtils.ToObject<NList>(json);
            long id1 = 12058624;

            INode node1 = _GrainFactory.GetGrain<INode>(id1);
            //int level = await node1.GetField<int>(Nuid.New(3282567168, 3282567168), "level");
            Nuid nuid1 = await node1.Create(Nuid.New(id1, id1), "player", NList.Empty);

            await node1.SetField(Nuid.New(12058624, 12058624), "ticks", NList.New().Add(Nuid.New(id2, id2)).Add(TimeUtils.Now).Add(true));

            NList res = await node1.GetField<NList>(Nuid.New(12058624, 12058624), "ticks");

            Nuid nuid2 = Nuid.New(12058624, 12058624);
            NList args = NList.New().Add(Nuid.New(id2, id2)).Add(TimeUtils.Now).Add(true);

            DateTime d1 = DateTime.Now;
            int i = 0;
            for (i = 0; i < 10000; i++)
            {
                await node1.SetField(nuid2, "ticks", args);
            }

            DateTime d2 = DateTime.Now;
            TimeSpan ts = d2 - d1;
            double ll = ts.TotalMilliseconds;

            Console.WriteLine("ll=" + ll + "i=" + i);

            NList list = await node1.GetEntities();

            string json2 = JsonUtils.ToJson(list);

            //await node1.Deactive();

            //await node1.Active();

            //long id2 = IdUtils.UGen.CreateId();
            //Nuid nuid2 = await node1.Create(Nuid.New(id2, id2), "player", NList.Empty);

            //await node1.Command(nuid1, 1, NList.New().Add(nuid2));

            //return Task.CompletedTask;
        }
    }
}
