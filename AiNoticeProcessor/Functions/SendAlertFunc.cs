using System;
using System.Threading.Tasks;
using AiNoticeProcessor.Services;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AiNoticeProcessor.Functions;
public class SendAlertFunc
{
    private readonly ILogger<SendAlertFunc> _logger;    
    private readonly IAlertProcessor _alertProcessor;
    public SendAlertFunc(ILogger<SendAlertFunc> logger, IAlertProcessor alertProcessor)
    {
        _logger = logger;
        _alertProcessor = alertProcessor;
    }

    [Function(nameof(SendAlertFunc))]
    public async Task Run([EventHubTrigger("%AlertsEhName%", Connection = "EhnsTasConnString", ConsumerGroup = "%AlertsEhCGName%")] EventData[] events)
    {
        try
        {
            await _alertProcessor.ProcessAlert(events);
        }
        catch (Exception ex)
        {

            throw ex;
        }        
        
    }
}