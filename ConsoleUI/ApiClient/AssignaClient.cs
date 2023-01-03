using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace ConsoleUI.ApiClient
{
    public class AssignaClient
    {
        // props
        public HttpClient request { get; }
        public AssignaClient(IConfiguration config, HttpClient client)
        {
            this.request = client;
            this.request.BaseAddress = new Uri(config.GetSection("ApiClient:BaseAddress").Value);
            this.request.Timeout = new TimeSpan(0, 0, 30);
            this.request.DefaultRequestHeaders.Clear();

        }
    }
}
