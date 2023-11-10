using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Skymey_stock_polygon_tickerlist.Actions.GetTickers;

namespace Skymey_stock_polygon_tickerlist
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {

                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.AddSingleton<IHostedService, MySpecialService>();
                });
            await builder.RunConsoleAsync();
        }
    }
    public class MySpecialService : BackgroundService
    {
        GetTickers gt = new GetTickers();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string resp = "";
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    resp = gt.GetTickersFromPolygon(resp);
                    await Task.Delay(TimeSpan.FromSeconds(0));
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
