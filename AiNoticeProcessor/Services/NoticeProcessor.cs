using AiNoticeProcessor.Helper;
using AiNoticeProcessor.Models;
using Azure;
//using Azure.AI.Inference;
using Azure.AI.OpenAI;
//using Azure.AI.OpenAI.Chat;
//using OpenAI.Chat;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenAI.Chat;

//using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
//using OpenAI;

namespace AiNoticeProcessor.Services
{
    public class NoticeProcessor : INoticeProcessor
    {
        private readonly IConfiguration _config;
        private readonly AzureOpenAIClient _azureOpenAIClient;
        private readonly ChatClient _openAiChatClient;
        private readonly EventHubProducerClient _eventHubProducerClient;
        private readonly IAlertHelper _alertHelper;
        
        public NoticeProcessor(IConfiguration config, AzureOpenAIClient azureOpenAIClient, IAzureClientFactory<EventHubProducerClient> eventHubProducerClientFactory, IAlertHelper alertHelper)
        {
            _config = config;
            _alertHelper = alertHelper;
            _eventHubProducerClient = eventHubProducerClientFactory.CreateClient("AlertsEventHub");
            //_openAiChatClient = azureOpenAIClient.GetChatClient(_config["AzureOpenAiDeploymentName"]);

            //_azureOpenAIClient = new AzureOpenAIClient(new Uri(_config[""]), new System.ClientModel.ApiKeyCredential(""));
        }

        public async Task ProcessNotices(EventData[] input)
        {
            ChatCompletionOptions options = new ChatCompletionOptions
            {                
                MaxOutputTokenCount = 32768,
                Temperature = 0f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f
            };


            foreach (EventData inputItem in input)
            {              
                
                NoticeModel noticeModel = JsonConvert.DeserializeObject<NoticeModel>(inputItem.EventBody.ToString().Trim());
                

                string prompt = @$"From this notice text analyze and identify if its a trading signal answer in yes or no ""{noticeModel.NoticeText}""";

                string chatResponse = await CallOpenAiClientWithPrompt(prompt, options);
                

                if (!string.IsNullOrEmpty(chatResponse) && chatResponse.Contains("yes", StringComparison.InvariantCultureIgnoreCase))
                {
                    prompt = $"Analyze the trading signal and create a trading alert email output in html, include in a table";

                    chatResponse = await CallOpenAiClientWithPrompt(prompt, options);

                    EmailAlertModel emailAlertModel = new EmailAlertModel()
                    {
                        NoticeType = noticeModel.NoticeType,
                        EmailHtml = Encoding.UTF8.GetBytes(chatResponse)
                    };

                    EventData eventData = new EventData(noticeModel.ToJsonString());
                    await _eventHubProducerClient.SendAsync(new List<EventData> { eventData });

                }
            }
        }

        private async Task<string> CallOpenAiClientWithPrompt(string prompt, ChatCompletionOptions options)
        {
            AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(new Uri(_config["AzureOpenAiEndPoint"]), new System.ClientModel.ApiKeyCredential(_config["AzureOpenAiKey"]));
            var chatClient = azureOpenAIClient.GetChatClient(_config["AzureOpenAiDeploymentName"]);

            ChatCompletion completion = await chatClient.CompleteChatAsync([new UserChatMessage(prompt)], options);
            return completion.Content[0].Text;
        }
    }
}
