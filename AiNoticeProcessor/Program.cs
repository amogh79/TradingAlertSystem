using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using AiNoticeProcessor.Services;
using AiNoticeProcessor.Helper;

namespace AiNoticeProcessor
{
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

                services.AddSingleton(new AzureOpenAIClient(new Uri(context.Configuration["AzureOpenAiEndPoint"]), new System.ClientModel.ApiKeyCredential(context.Configuration["AzureOpenAiKey"])));
                services.AddAzureClients(clientBuilder =>
                {
                    clientBuilder.AddEventHubProducerClient(context.Configuration["EhnsTasConnString"], context.Configuration["AlertsEHName"]).WithName("AlertsEventHub");
                });

                services.AddScoped<INoticeProcessor, NoticeProcessor>();
                services.AddScoped<IAlertHelper, AlertHelper>();
                services.AddScoped<IAlertProcessor, AlertProcessor>();
                
            })
            .Build();

            host.Run();
            await Task.Yield();
        }
    }
}
