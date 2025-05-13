using System;
using System.Threading.Tasks;
using AiNoticeProcessor.Services;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AiNoticeProcessor.Functions;
public class ProcessNoticesFunc
{
    private readonly ILogger<ProcessNoticesFunc> _logger;
    private readonly INoticeProcessor _noticeProcessor;

    public ProcessNoticesFunc(ILogger<ProcessNoticesFunc> logger, INoticeProcessor noticeProcessor)
    {
        _logger = logger;
        _noticeProcessor = noticeProcessor;
    }

    [Function(nameof(ProcessNoticesFunc))]
    public async Task Run([EventHubTrigger("%NoticesEhName%", Connection = "EhnsTasConnString", ConsumerGroup = "%NoticesEHCGName%")] EventData[] events)
    {
        await _noticeProcessor.ProcessNotices(events);        
    }
}