using Azure.Messaging.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiNoticeProcessor.Services
{
    public interface INoticeProcessor
    {
        public Task ProcessNotices(EventData[] input);
    }
}
