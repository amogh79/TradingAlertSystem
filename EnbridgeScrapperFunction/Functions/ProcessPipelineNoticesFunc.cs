using EnbridgeScrapperFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EnbridgeScrapperFunction.Functions
{
    public class ProcessPipelineNoticesFunc
    {
        private readonly ILogger _logger;
        private readonly IScrapperService _scrapperService;
        private readonly IConfiguration _config;

        public ProcessPipelineNoticesFunc(ILoggerFactory loggerFactory, IScrapperService scrapperService, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<ProcessPipelineNoticesFunc>();
            _scrapperService = scrapperService;
            _config = configuration;
        }

        [Function("Function1")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            Console.WriteLine($"Scraping Enbridge at {_config["EnbridgeCriticalNoticeUrl"]}");
            await _scrapperService.ScrapeNoticesAsync();
        }
    }
}
