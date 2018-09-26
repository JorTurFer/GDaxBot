using GDaxBot.Coinbase.Model.Services.Coinbase;
using GDaxBot.Coinbase.Model.Services.Telegram;
using GDaxBot.Model.Services.GDaxBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace GDaxBot
{
    class Program
    {
        //IOC
        public static IServiceProvider ServiceProvider { get; private set; }

        static void Main(string[] args)
        {
            try
            {
                //Registro todo el IOC
                RegisterIOC();

                var service = ServiceProvider.GetService<IGDaxBotService>();
                var thread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    service.Start();
                });
                thread.Start();

                Console.ReadKey();

                service.Stop();

            }
            catch (Exception ex)
            {
                //Envio aviso de que hay un error
                ServiceProvider.GetService<ITelegramBot>().SendMessage($"Cierre del servicio.\nRazon->{ex.Message}");
            }
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
