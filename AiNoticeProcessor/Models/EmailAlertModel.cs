using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiNoticeProcessor.Models
{
    public class EmailAlertModel
    {
        public required string NoticeType { get; set; }

        public required byte[] EmailHtml { get; set; }

        public string GetHtmlString()
        {
            return Encoding.UTF8.GetString(EmailHtml);
        }
    }
}
