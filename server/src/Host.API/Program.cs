using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace Host.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(@"
      ___           ___                     ___           ___           ___                 
     /  /\         /  /\      ___          /__/\         /  /\         /  /\          ___   
    /  /::\       /  /::\    /  /\         \  \:\       /  /::\       /  /:/_        /  /\  
   /  /:/\:\     /  /:/\:\  /  /:/          \__\:\     /  /:/\:\     /  /:/ /\      /  /:/  
  /  /:/~/::\   /  /:/~/:/ /__/::\      ___ /  /::\   /  /:/  \:\   /  /:/ /::\    /  /:/   
 /__/:/ /:/\:\ /__/:/ /:/  \__\/\:\__  /__/\  /:/\:\ /__/:/ \__\:\ /__/:/ /:/\:\  /  /::\   
 \  \:\/:/__\/ \  \:\/:/      \  \:\/\ \  \:\/:/__\/ \  \:\ /  /:/ \  \:\/:/~/:/ /__/:/\:\  
  \  \::/       \  \::/        \__\::/  \  \::/       \  \:\  /:/   \  \::/ /:/  \__\/  \:\ 
   \  \:\        \  \:\        /__/:/    \  \:\        \  \:\/:/     \__\/ /:/        \  \:\
    \  \:\        \  \:\       \__\/      \  \:\        \  \::/        /__/:/          \__\/
     \__\/         \__\/                   \__\/         \__\/         \__\/                ");

            Console.WriteLine("");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder host_builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureLogging(builder =>
            {
                var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
                builder.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger());
            });

            return host_builder;
        }
    }
}
