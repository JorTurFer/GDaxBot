using GDaxBot.Coinbase.Model.Services.Coinbase;
using GDaxBot.Coinbase.Model.Services.Telegram;
using GDaxBot.Data;
using GDaxBot.Model.Services.GDaxBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GDaxBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = new HostBuilder()
                 .ConfigureHostConfiguration(configHost =>
                 {
                     configHost.SetBasePath(Directory.GetCurrentDirectory());
                 })
                 .ConfigureAppConfiguration((hostContext, configApp) =>
                 {
                     configApp.SetBasePath(Directory.GetCurrentDirectory());
                     configApp.AddJsonFile($"appsettings.json", true);
                     configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true);
                     var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");

                     var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                         devEnvironmentVariable.ToLower() == "development";
                     if (isDevelopment)
                     {
                         configApp.AddUserSecrets("79a3edd0-2092-40a2-a04d-dcb46d5ca9ed");
                     }
                 })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<GDaxBotDbContext>(options =>
                        options.UseMySql(hostContext.Configuration.GetConnectionString("GDaxBot")), ServiceLifetime.Transient);

                    services.AddSingleton<ITelegramBot, TelegramBot>();
                    services.AddSingleton<ICoinbaseService, CoinbaseService>();
                    services.AddSingleton<IGDaxBotService, GDaxBotService>();

                    services.AddLogging();
                    services.AddHostedService<ApplicationLifetimeHostedService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
