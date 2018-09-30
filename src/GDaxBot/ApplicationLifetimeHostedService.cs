﻿using GDaxBot.Coinbase.Model.Services.Coinbase;
using GDaxBot.Coinbase.Model.Services.Telegram;
using GDaxBot.Data;
using GDaxBot.Model.Services.GDaxBot;
using Microsoft.EntityFrameworkCore;
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
        IServiceProvider serviceProvider;

        IApplicationLifetime appLifetime;
        ILogger<ApplicationLifetimeHostedService> logger;
        IHostingEnvironment environment;
        IConfiguration configuration;
        public ApplicationLifetimeHostedService(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<ApplicationLifetimeHostedService> logger,
            IApplicationLifetime appLifetime,
            IServiceProvider serviceProvider)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.environment = environment;
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {          
            var service = serviceProvider.GetService<IGDaxBotService>();

            service.Start();
            return Task.CompletedTask;
        }

        private void OnStarted()
        {

        }

        private void OnStopping()
        {
            serviceProvider.GetService<ITelegramBot>().SendMessage($"Ceerrando el servicio");
            var service = serviceProvider.GetService<IGDaxBotService>();
            service.Stop();
        }

        private void OnStopped()
        {

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            var service = serviceProvider.GetService<IGDaxBotService>();
            service.Stop();
            serviceProvider.GetService<ITelegramBot>().SendMessage($"Cierre del servicio");
            return Task.CompletedTask;
        }
    }
}
