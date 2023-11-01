using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace ConsoleUI.ApiClient
{
    public class AssignaClient
    {
        // props
        public HttpClient Request { get; }
        public AssignaClient(IConfiguration config, HttpClient client)
        {
            Request = client;
            Request.BaseAddress = new Uri(config.GetSection("ApiClient:BaseAddress").Value);
            Request.Timeout = new TimeSpan(0, 0, 30);
            Request.DefaultRequestHeaders.Clear();

        }
    }
}
