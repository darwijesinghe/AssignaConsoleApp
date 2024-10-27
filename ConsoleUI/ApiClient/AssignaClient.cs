using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace ConsoleUI.ApiClient
{
    public class AssignaClient
    {
        /// <summary>
        /// Gets the configured <see cref="HttpClient"/> instance used to send API requests
        /// </summary>
        public HttpClient Request { get; }

        /// <summary>
        /// Constructs an instance of the <see cref="AssignaClient"/> class, configuring the provided HttpClient with base address, timeout, and cleared headers.
        /// </summary>
        /// <param name="config">The configuration source for retrieving the API base address.</param>
        /// <param name="client">The HttpClient instance to configure for API requests.</param>
        public AssignaClient(IConfiguration config, HttpClient client)
        {
            Request             = client;
            Request.BaseAddress = new Uri(config.GetSection("ApiClient:BaseAddress").Value);
            Request.Timeout     = new TimeSpan(0, 0, 30);
            Request.DefaultRequestHeaders.Clear();
        }
    }
}
