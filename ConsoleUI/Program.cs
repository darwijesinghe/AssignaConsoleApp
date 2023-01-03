using ConsoleUI.ApiClient;
using ConsoleUI.Interfaces;
using ConsoleUI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleUI
{
    class Program
    {
        // main method
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            // add services
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // add NLog logging
                    services.AddLogging(loggingBuilder => {

                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddNLog(context.Configuration.GetSection("Logging"));

                    });

                    // add configurations
                    services.AddSingleton(context.Configuration);

                    // add httpclient
                    services.AddHttpClient<AssignaClient>();

                    // auth service
                    services.AddTransient<IAuthService, AuthService>();

                    // task service
                    services.AddTransient<ITaskService, TaskService>();

                })
                .Build();

            var service = ActivatorUtilities.CreateInstance<Startup>(host.Services);
            await service.Run();
        }

        // configure appsettings
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: false)
                .AddEnvironmentVariables();
        }
    }
}
