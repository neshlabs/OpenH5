using Host.Gate.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Serilog;
using System;
using System.IO;

namespace Host.Gate
{
    class Program
    {
        public static IConfiguration Configuration { get; private set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Nesh!");

            Console.WriteLine(@"
      ___           ___                       ___           ___           ___           ___                 
     /  /\         /  /\          ___        /  /\         /__/\         /  /\         /  /\          ___   
    /  /:/_       /  /::\        /  /\      /  /:/_        \  \:\       /  /::\       /  /:/_        /  /\  
   /  /:/ /\     /  /:/\:\      /  /:/     /  /:/ /\        \__\:\     /  /:/\:\     /  /:/ /\      /  /:/  
  /  /:/_/::\   /  /:/~/::\    /  /:/     /  /:/ /:/_   ___ /  /::\   /  /:/  \:\   /  /:/ /::\    /  /:/   
 /__/:/__\/\:\ /__/:/ /:/\:\  /  /::\    /__/:/ /:/ /\ /__/\  /:/\:\ /__/:/ \__\:\ /__/:/ /:/\:\  /  /::\   
 \  \:\ /~~/:/ \  \:\/:/__\/ /__/:/\:\   \  \:\/:/ /:/ \  \:\/:/__\/ \  \:\ /  /:/ \  \:\/:/~/:/ /__/:/\:\  
  \  \:\  /:/   \  \::/      \__\/  \:\   \  \::/ /:/   \  \::/       \  \:\  /:/   \  \::/ /:/  \__\/  \:\ 
   \  \:\/:/     \  \:\           \  \:\   \  \:\/:/     \  \:\        \  \:\/:/     \__\/ /:/        \  \:\
    \  \::/       \  \:\           \__\/    \  \::/       \  \:\        \  \::/        /__/:/          \__\/
     \__\/         \__\/                     \__\/         \__\/         \__\/         \__\/                
           ");

            CreateHostBuilder(args).Build().Run();
        }

        private static void LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", false, false);

            Configuration = configurationBuilder.Build();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            LoadConfiguration();

            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ClusterHostedService>();
                    services.AddSingleton<IHostedService>(_ => _.GetService<ClusterHostedService>());
                    services.AddSingleton(_ => _.GetService<ClusterHostedService>().Client);

                    services.AddHostedService<WebSocketService>();
                    services.Configure<ConsoleLifetimeOptions>(options =>
                    {
                        options.SuppressStatusMessages = true;
                    });
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", false, false);
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger());
                });
        }

    }
}
