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
            try
            {
                var service = serviceProvider.GetService<IGDaxBotService>();

                service.Start();
            }
            catch(Exception ex)
            {
                logger.LogError($"{ex.Message},{ex.StackTrace}");
            }
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
            return Task.CompletedTask;
        }
    }
}
