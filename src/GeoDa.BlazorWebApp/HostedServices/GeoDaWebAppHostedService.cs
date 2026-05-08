using GeoDa.BlazorWebApp.Services.Observers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace GeoDa.BlazorWebApp.HostedServices
{
    public class GeoDaWebAppHostedService : BackgroundService
    {
        private const int UpdatePeriod = 1000;

        private IObserverService _observerService;
        private readonly ILogger<GeoDaWebAppHostedService> _logger;

        public GeoDaWebAppHostedService(
            IObserverService observerService,
            ILogger<GeoDaWebAppHostedService> logger)
        {
            _observerService = observerService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tmp = new object();

            while (!stoppingToken.IsCancellationRequested)
            {
                _observerService.NotifyObservers(tmp);

                await Task.Delay(UpdatePeriod, stoppingToken);
            }
        }
    }
}
