using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Swagger;
using Swagger.Models;
using Microsoft.AspNetCore.TestHost;

namespace AutomatedTests.RestApiContractTesting
{
    public class Tests
    {
        private IHost server;
        private IDidacticalEnigmaRestApi api;

        [SetUp]
        public async Task Setup()
        {
            server = await Host.CreateDefaultBuilder(new string[0])
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    webBuilder.UseStartup<DidacticalEnigma.RestApi.Startup>();
                })
                .StartAsync();
            api = new DidacticalEnigmaRestApi(server.GetTestClient(), disposeHttpClient: false);
        }

        [Test]
        public async Task Test()
        {
            var input = "お前はもう死んでいる";
            var result = await api.PostTextWithHttpMessagesAsync(input);
            var dataSources = await api.ListDataSourcesWithHttpMessagesAsync();
            var words = new List<string>();
            for (int i = 0; i < input.Length; ++i)
            {
                var x = await api.RequestInformationFromDataSourcesWithHttpMessagesAsync(new DataSourceParseRequest()
                {
                    RequestedDataSources = dataSources.Body.Select(d => d.Identifier).ToList(),
                    Id = result.Body.Identifier,
                    Position = i
                });
            }
        }

        [TearDown]
        public async Task Teardown()
        {
            await server.StopAsync();
            server.Dispose();
        }
    }
}