using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiNoticeProcessor.Helper
{
    public interface IAlertHelper
    {
        public Task<bool> SendEmail(string emailBody, List<string> toEmailAddresses);
        
    }
}
