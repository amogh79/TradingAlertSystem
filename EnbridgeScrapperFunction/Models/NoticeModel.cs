using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnbridgeScrapperFunction.Models
{
    public class NoticeModel
    {
        public required string NoticeType { get; set; } = "Critical";

        public required string NoticeUrl { get; set; }

        public string? NoticeText { get; set; }

        //public NoticeDetail? NoticeDetail { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
