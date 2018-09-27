using GDaxBot.Coinbase.Model.Services.Coinbase;
using GDaxBot.Coinbase.Model.Services.Telegram;
using GDaxBot.Model.Services.GDaxBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GDaxBot
{
    public class ApplicationLifetimeHostedService : IHostedService
    {
        //IOC
        public static IServiceProvider ServiceProvider { get; private set; }

        IApplicationLifetime appLifetime;
        ILogger<ApplicationLifetimeHostedService> logger;
        IHostingEnvironment environment;
        IConfiguration configuration;
        public ApplicationLifetimeHostedService(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<ApplicationLifetimeHostedService> logger,
            IApplicationLifetime appLifetime)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.environment = environment;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            RegisterIOC();

            var service = ServiceProvider.GetService<IGDaxBotService>();

            service.Start();
            return Task.CompletedTask;
        }

        private void OnStarted()
        {

        }

        private void OnStopping()
        {

        }

        private void OnStopped()
        {

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            var service = ServiceProvider.GetService<IGDaxBotService>();
            service.Stop();
            ServiceProvider.GetService<ITelegramBot>().SendMessage($"Cierre del servicio");
            return Task.CompletedTask;
        }

        static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ITelegramBot, TelegramBot>();
            services.AddSingleton<ICoinbaseService, CoinbaseService>();
            services.AddSingleton<IGDaxBotService, GDaxBotService>();
        }

        static void RegisterIOC()
        {
            IConfigurationRoot Configuration;
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");

            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";
            //Determines the working environment as IHostingEnvironment is unavailable in a console app
            var builder = new ConfigurationBuilder();
            // tell the builder to look for the appsettings.json file
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            //only add secrets in development
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }

            Configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();

            //Map the implementations of your classes here ready for DI
            services.Configure<Settings>(Configuration.GetSection(nameof(Settings))).AddOptions();

            RegisterServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
