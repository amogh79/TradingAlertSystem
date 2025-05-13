using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EnbridgeScrapperFunction.Services
{
    public interface IScrapperService
    {
        Task<List<string>> ScrapeNoticesAsync();
    }
}
