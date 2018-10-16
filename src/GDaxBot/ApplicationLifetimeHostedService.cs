using GDaxBot.Model.Services.GDaxBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Loader;
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
        IGDaxBotService _service;
        public ApplicationLifetimeHostedService(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<ApplicationLifetimeHostedService> logger,
            IApplicationLifetime appLifetime,
            IServiceProvider serviceProvider,
            IGDaxBotService service)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.environment = environment;
            this.serviceProvider = serviceProvider;
            this._service = service;
            AssemblyLoadContext.Default.Unloading += ApplicationClosing;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            AssemblyLoadContext.Default.Unloading += ApplicationClosing;

            try
            {
                _service.Start();
            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message},{ex.StackTrace}");
            }
            return Task.CompletedTask;
        }

        private void ApplicationClosing(AssemblyLoadContext obj)
        {           
            _service.Stop();
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
