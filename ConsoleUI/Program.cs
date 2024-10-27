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
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            // Add services
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Add NLog logging
                    services.AddLogging(loggingBuilder => {

                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddNLog(context.Configuration.GetSection("Logging"));

                    });

                    // Register configurations
                    services.AddSingleton(context.Configuration);

                    // Register httpclient
                    services.AddHttpClient<AssignaClient>();

                    // Register auth service
                    services.AddTransient<IAuthService, AuthService>();

                    // Register task service
                    services.AddTransient<ITaskService, TaskService>();

                })
                .Build();

            // Automatically resolve and inject any dependencies required by the Startup class's constructor.
            // This helps in creating instances of classes that have dependencies registered in the DI container.

            var service = ActivatorUtilities.CreateInstance<Startup>(host.Services);
            await service.Run();
        }

        // Configure appsettings
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: false)
                   .AddEnvironmentVariables();
        }
    }
}
