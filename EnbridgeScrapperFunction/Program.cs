using Azure.Identity;
using EnbridgeScrapperFunction.Helpers;
using EnbridgeScrapperFunction.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddEnvironmentVariables();
                builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

                string azureAppConfigEndpoint = Environment.GetEnvironmentVariable("AzureAppConfigEndpoint");

                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(azureAppConfigEndpoint), new DefaultAzureCredential());
                    options.ConfigureKeyVault(options =>
                    {
                        options.SetCredential(new DefaultAzureCredential());
                    });
                });
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();

                services.Configure<LoggerFilterOptions>(options =>
                {
                    // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
                    // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
                    LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                        == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

                    if (toRemove is not null)
                    {
                        options.Rules.Remove(toRemove);
                    }
                });


                services.AddHttpClient("enbridge-http-client")
                     .AddResilienceHandler("http-pipeline", builder =>
                     {
                         // Refer to https://www.pollydocs.org/strategies/retry.html#defaults for retry defaults
                         builder.AddRetry(new HttpRetryStrategyOptions
                         {
                             MaxRetryAttempts = 5,
                             Delay = TimeSpan.FromSeconds(2),
                             BackoffType = DelayBackoffType.Exponential
                         });

                         // Refer to https://www.pollydocs.org/strategies/timeout.html#defaults for timeout defaults
                         builder.AddTimeout(TimeSpan.FromSeconds(5));
                     });

                services.AddAzureClients(clientBuilder =>
                {
                    clientBuilder.AddEventHubProducerClient(context.Configuration["EhnsTasConnString"], context.Configuration["NoticesEhName"]).WithName("NoticesEventHub");
                });

                services.AddScoped<IScrapperService, ScrapperService>();
                services.AddScoped<IScrappingHelper, ScrapingHelper>();
            })
            .Build();

        host.Run();
        await Task.Yield();
    }
}