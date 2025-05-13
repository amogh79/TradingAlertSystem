using EnbridgeScrapperFunction.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnbridgeScrapperFunction.Helpers
{
    public class ScrapingHelper : IScrappingHelper
    {
        public Task<string> ExtractNoticeText(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var sb = new StringBuilder();

            // Extract metadata fields
            var headings = doc.DocumentNode.SelectNodes("//div[@id='heading']/text()");
            var values = doc.DocumentNode.SelectNodes("//div[@id='headingData']/text()");

            if (headings != null && values != null && headings.Count == values.Count)
            {
                for (int i = 0; i < headings.Count; i++)
                {
                    var label = headings[i].InnerText.Trim().Replace(":", "");
                    var value = values[i].InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(label) && !string.IsNullOrWhiteSpace(value))
                    {
                        sb.AppendLine($"{label}: {value}");
                    }
                }
            }

            // Extract notice body text (first paragraph under "Notice Text:")
            var table = doc.DocumentNode.SelectSingleNode("//div[@id='bulletin']//table");
            if (table != null)
            {
                var paragraphs = table.SelectNodes(".//tr/td");
                foreach (var cell in paragraphs)
                {
                    var text = cell.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine(text);
                    }
                }
            }

            sb.Replace("&nbsp;", "");
            sb.Replace("&amp;", "and");

            return Task.Run(() => sb.ToString());
        }

        public NoticeDetail ExtractStructuredNotice(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var headings = doc.DocumentNode.SelectNodes("//div[@id='heading']")?.ToList();
            var values = doc.DocumentNode.SelectNodes("//div[@id='headingData']")?.ToList();

            var fieldMap = new Dictionary<string, string>();
            if (headings != null && values != null)
            {
                for (int i = 0; i < headings.Count && i < values.Count; i++)
                {
                    string key = headings[i].InnerText.Trim().TrimEnd(':');
                    string value = values[i].InnerText.Trim();
                    if (!string.IsNullOrEmpty(key))
                        fieldMap[key] = value;
                }
            }

            // Extract notice body text
            var bodyTable = doc.DocumentNode.SelectSingleNode("//div[@id='bulletin']//table");
            var bodyText = new System.Text.StringBuilder();
            if (bodyTable != null)
            {
                var rows = bodyTable.SelectNodes(".//tr");
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var cell = row.SelectSingleNode(".//td");
                        if (cell != null)
                        {
                            var text = HtmlEntity.DeEntitize(cell.InnerText.Trim());
                            if (!string.IsNullOrWhiteSpace(text))
                                bodyText.AppendLine(text);
                        }
                    }
                }
            }

            NoticeDetail noticeDetail = new NoticeDetail
            {
                NoticeType = fieldMap.GetValueOrDefault("Notice Type"),
                EffectiveDate = fieldMap.GetValueOrDefault("Notice Effective Date"),
                EndDate = fieldMap.GetValueOrDefault("Notice End Date"),
                PostingDate = fieldMap.GetValueOrDefault("Posting Date"),
                Pipeline = fieldMap.GetValueOrDefault("Pipeline"),
                Tsp = fieldMap.GetValueOrDefault("TSP"),
                NoticeId = fieldMap.GetValueOrDefault("Notice ID"),
                Critical = fieldMap.GetValueOrDefault("Critical"),
                Category = fieldMap.GetValueOrDefault("Category"),
                NoticeText = bodyText.ToString().Trim()
            };

            return noticeDetail;
        }
    }
}
