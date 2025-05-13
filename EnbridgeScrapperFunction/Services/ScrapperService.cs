using Azure;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using EnbridgeScrapperFunction.Helpers;
using EnbridgeScrapperFunction.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EnbridgeScrapperFunction.Services
{
    public class ScrapperService : IScrapperService
    {
        private HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly EventHubProducerClient _eventHubProducerClient;
        private readonly IScrappingHelper _scrappingHelper;

        public ScrapperService(IHttpClientFactory httpClientFactory, IConfiguration config, IAzureClientFactory<EventHubProducerClient> eventHubProducerClientFactory, IScrappingHelper scrapingHelper)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _eventHubProducerClient = eventHubProducerClientFactory.CreateClient("NoticesEventHub");
            _scrappingHelper = scrapingHelper;

        }
        public async Task<List<string>> ScrapeNoticesAsync()
        {
            try
            {
                var notices = new List<string>();

                string enbridegeNoticeUrl = _config["EnbridgeCriticalNoticeUrl"];
                string enbridgePlannedOutageUrl = _config["EnbridgePlannedOutageUrl"];

                List<NoticeModel> noticeModels = await GetCriticalNoticeModels(enbridegeNoticeUrl);

                noticeModels.AddRange(await GetPlannedOutageModels(enbridgePlannedOutageUrl));

                foreach (NoticeModel noticeModel in noticeModels)
                {
                    EventData eventData = new EventData(noticeModel.ToJsonString());
                    await _eventHubProducerClient.SendAsync(new List<EventData> { eventData });
                }

                #region Commented Code

                //string html = await _httpClient.GetStringAsync(cheniereUrl);

                //string html = await GetHtml(cheniereUrl);

                //HtmlDocument doc = new HtmlDocument();

                //doc.LoadHtml(html);

                ////panel panel-default critical - panel
                //var noticeNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'odd')]");

                //IEnumerable<HtmlNode> nodes = doc.DocumentNode.Descendants(0)
                //                                .Where(n => n.HasClass("odd") || n.HasClass("even"));

                //if (nodes.Any())
                //{

                //    foreach (HtmlNode htmlNode in nodes)
                //    {
                //        if (htmlNode.Descendants().FirstOrDefault().InnerText.Equals("Capacity Constraint", StringComparison.InvariantCultureIgnoreCase))
                //        {
                //            IEnumerable<HtmlNode> descendent = htmlNode.Descendants("a");

                //            if (descendent.Any())
                //            {
                //                var aa = descendent.FirstOrDefault().Attributes["href"].Value;
                //                string noticehtml = await GetHtml($"https://infopost.enbridge.com/infopost/{aa}");

                //                HtmlDocument noticeHtmlDoc = new HtmlDocument();
                //                noticeHtmlDoc.LoadHtml(noticehtml);

                //                HtmlNode headingNode = noticeHtmlDoc.DocumentNode.Descendants(0)
                //                                .Where(n => n.Id.Equals("heading", StringComparison.InvariantCultureIgnoreCase))
                //                                .FirstOrDefault();


                //                HtmlNode headingData = noticeHtmlDoc.DocumentNode.Descendants(0)
                //                                .Where(n => n.Id.Equals("headingData",StringComparison.InvariantCultureIgnoreCase))
                //                                .FirstOrDefault();

                //                List<string> stringHeadingNode = new List<string>();
                //                List<string> stringHeadingData = new List<string>();


                //                //foreach (HtmlNode headingNode in headingNodes) 
                //                //{
                //                //    string replacedHeadingNode = headingNode.InnerHtml.Replace("<br>", ",", StringComparison.InvariantCultureIgnoreCase);
                //                //    stringHeadingNode = replacedHeadingNode.Split(",").ToList();
                //                //}

                //                string replacedHeadingNode = headingNode.InnerHtml.Replace("<br>", "<br>", StringComparison.InvariantCultureIgnoreCase).Replace("&nbsp", string.Empty, StringComparison.InvariantCultureIgnoreCase);
                //                stringHeadingNode = replacedHeadingNode.Split("<br>").ToList();

                //                //foreach (HtmlNode headingData in headingDatas)
                //                //{
                //                //    string replacedHeadingData = headingData.InnerHtml.Replace("<br>", ",", StringComparison.InvariantCultureIgnoreCase);
                //                //    stringHeadingData = replacedHeadingData.Split(",").ToList();
                //                //}

                //                string replacedHeadingData = headingData.InnerHtml.Replace("<br>", "<br>", StringComparison.InvariantCultureIgnoreCase).Replace("&nbsp",string.Empty,StringComparison.InvariantCultureIgnoreCase);
                //                stringHeadingData = replacedHeadingData.Split("<br>").ToList();

                //                string noticeStartDate, noticeStartTime, noticeEndDate, noticeEndTime, noticeType;

                //                noticeStartDate = stringHeadingData.ElementAtOrDefault(stringHeadingNode.IndexOf("Notice Effective Date:"));
                //                noticeStartTime = stringHeadingData.ElementAtOrDefault(stringHeadingNode.IndexOf("Notice Effective Time:"));
                //                noticeEndDate = stringHeadingData.ElementAtOrDefault(stringHeadingNode.IndexOf("Notice End Date:"));
                //                noticeEndTime = stringHeadingData.ElementAtOrDefault(stringHeadingNode.IndexOf("Notice End Time:"));
                //                noticeType = stringHeadingData.ElementAtOrDefault(stringHeadingNode.IndexOf("Notice Type:"));

                //            }
                //        }                        
                //    }
                //}

                #endregion
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
            

            return new List<string>();
        }

        private async Task<List<NoticeModel>> GetPlannedOutageModels(string? enbridgePlannedOutageUrl)
        {
            List<NoticeModel> noticeModels = new List<NoticeModel>();

            if (string.IsNullOrEmpty(enbridgePlannedOutageUrl))
                return noticeModels;
            
            string html = await GetHtml(enbridgePlannedOutageUrl);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            IEnumerable<HtmlNode> nodes = document.DocumentNode.Descendants(0)
                                                .Where(n => n.HasClass("odd") || n.HasClass("even"));

            if (nodes.Any())
            {
                foreach (HtmlNode htmlNode in nodes)
                {
                    if (htmlNode.Descendants().FirstOrDefault().InnerText.Equals("Planned Service Outage", StringComparison.InvariantCultureIgnoreCase))
                    {
                        IEnumerable<HtmlNode> descendent = htmlNode.Descendants("a");

                        if (descendent.Any())
                        {
                            var noticePartUrl = descendent.FirstOrDefault().Attributes["href"].Value;
                            string noticehtml = await GetHtml($"https://infopost.enbridge.com/infopost/{noticePartUrl}");

                            string noticeText = await _scrappingHelper.ExtractNoticeText(noticehtml);

                            NoticeModel noticeModel = new NoticeModel()
                            {
                                NoticeType = NoticeType.Critical.ToString(),
                                NoticeUrl = $"https://infopost.enbridge.com/infopost/{noticePartUrl}",
                                NoticeText = noticeText

                            };
                            noticeModels.Add(noticeModel);
                        }
                    }
                }
            }

            return noticeModels;
        }

        private async Task<List<NoticeModel>> GetCriticalNoticeModels(string url)
        {
            List<NoticeModel> noticeModels = new List<NoticeModel>();

            if (string.IsNullOrEmpty(url))
                return noticeModels;

            string html = await GetHtml(url);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            IEnumerable<HtmlNode> nodes = document.DocumentNode.Descendants(0)
                                                .Where(n => n.HasClass("odd") || n.HasClass("even"));

            if (nodes.Any()) 
            {
                foreach (HtmlNode htmlNode in nodes)
                {
                    
                    if (htmlNode.Descendants().FirstOrDefault().InnerText.Equals("Capacity Constraint", StringComparison.InvariantCultureIgnoreCase))
                    {
                        IEnumerable<HtmlNode> descendent = htmlNode.Descendants("a");

                        if (descendent.Any())
                        {
                            var noticePartUrl = descendent.FirstOrDefault().Attributes["href"].Value;
                            string noticehtml = await GetHtml($"https://infopost.enbridge.com/infopost/{noticePartUrl}");

                            string noticeText = await _scrappingHelper.ExtractNoticeText(noticehtml);

                            NoticeModel noticeModel = new NoticeModel()
                            {
                                NoticeType = NoticeType.Critical.ToString(),
                                NoticeUrl = $"https://infopost.enbridge.com/infopost/{noticePartUrl}",
                                NoticeText = noticeText

                            };
                            noticeModels.Add(noticeModel);
                        }
                    }
                }
            }

            return noticeModels;
        }

        private async Task<string> GetHtml(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                using HttpResponseMessage responseMessage = await client.SendAsync(request);

                responseMessage.EnsureSuccessStatusCode();

                string responseContent = await responseMessage.Content.ReadAsStringAsync();                


                return responseContent;
            }

            
        }
    }
}
