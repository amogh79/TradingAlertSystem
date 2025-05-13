using AiNoticeProcessor.Helper;
using AiNoticeProcessor.Models;
using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiNoticeProcessor.Services
{
    public class AlertProcessor : IAlertProcessor
    {
        private readonly IConfiguration _config;
        private readonly IAlertHelper _alertHelper;
        
        public AlertProcessor(IConfiguration config, IAlertHelper alertHelper)
        {
            _config = config;
            _alertHelper = alertHelper;
        }

        public async Task ProcessAlert(EventData[] input)
        {
            foreach (EventData inputItem in input)
            {
                EmailAlertModel emailAlertModel = JsonConvert.DeserializeObject<EmailAlertModel>(inputItem.EventBody.ToString().Trim());

                List<string> emailAddresses = new List<string>();

                emailAddresses.Add("someemail@gmail.com");
                
                _alertHelper.SendEmail(emailAlertModel.GetHtmlString(), emailAddresses);
            }
        }
    }
}
