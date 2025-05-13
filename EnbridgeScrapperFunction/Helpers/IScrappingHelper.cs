using EnbridgeScrapperFunction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnbridgeScrapperFunction.Helpers
{
    public interface IScrappingHelper
    {
        public Task<string> ExtractNoticeText(string html);
        public NoticeDetail ExtractStructuredNotice(string html);
    }
}
