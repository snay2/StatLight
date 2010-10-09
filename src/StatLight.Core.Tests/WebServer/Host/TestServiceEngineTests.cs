using System;
using System.IO;
using System.Net;
using System.Threading;
using Moq;
using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.Properties;
using StatLight.Core.Serialization;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;
using StatLight.Core.WebServer.Host;
using System.Collections.Generic;

namespace StatLight.Core.Tests.WebServer.Host
{
    [TestFixture]
    public class TestServiceEngineTests : FixtureBase
    {
        private TestServiceEngine _testServiceEngine;

        private string _baseUrl;
        private WebClient _webClient;
        private Func<byte[]> _xapToTestFactory;
        private byte[] _hostXap;
        private string _serializedConfiguration;
        private Mock<PostHandler> _mockPostHandler;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            const string machineName = "localhost";
            const int port = 38881;
            var consoleLogger = new ConsoleLogger(LogChatterLevels.Full);
            _xapToTestFactory = () => new byte[] { 0, 1, 2, 3, 4 };
            _hostXap = new byte[] { 5, 4, 2, 1, 4 };
            _serializedConfiguration = new ClientTestRunConfiguration(UnitTestProviderType.MSTest, new List<string>(), "", 1, "test").Serialize();
            var responseFactory = new ResponseFactory(_xapToTestFactory, _hostXap, _serializedConfiguration);

            _mockPostHandler = new Mock<PostHandler>();
            _testServiceEngine = new TestServiceEngine(consoleLogger, machineName, port, responseFactory, _mockPostHandler.Object);
            _webClient = new WebClient();

            _baseUrl = "http://{0}:{1}/".FormatWith(machineName, port);

            _testServiceEngine.Start();
        }

        protected override void After_all_tests()
        {
            base.After_all_tests();

            _testServiceEngine.Stop();
        }

        [Test]
        public void Should_server_the_ClientAccessPolicy_file()
        {
            GetString(StatLightServiceRestApi.ClientAccessPolicy)
                .ShouldEqual(Resources.ClientAccessPolicy);
        }

        [Test]
        public void Should_server_the_CrossDomain_file()
        {
            GetString(StatLightServiceRestApi.CrossDomain)
                .ShouldEqual(Resources.CrossDomain);
        }

        [Test]
        public void Should_server_the_GetHtmlTestPage_file()
        {
            var expectedFile = Resources.TestPage.Replace("BB86D193-AD39-494A-AEB7-58F948BA5D93", 1.ToString());
            GetString(StatLightServiceRestApi.GetHtmlTestPage)
                .ShouldEqual(expectedFile);
        }

        [Test]
        public void Should_serve_the_GetXapToTest_file()
        {
            _webClient.DownloadData(GetUrl(StatLightServiceRestApi.GetXapToTest))
                .ShouldEqual(_xapToTestFactory());
        }

        [Test]
        public void Should_serve_the_GetTestPageHostXap_file()
        {
            _webClient.DownloadData(GetUrl(StatLightServiceRestApi.GetTestPageHostXap))
                .ShouldEqual(_hostXap);
        }

        [Test]
        public void Should_serve_the_GetTestRunConfiguration_file()
        {
            _webClient.DownloadString(GetUrl(StatLightServiceRestApi.GetTestRunConfiguration))
                .ShouldEqual(_serializedConfiguration);
        }

        [Test]
        public void Should_accept_postedMessages()
        {
            string messageWritten = "Hello World!";
            byte[] data = messageWritten.ToByteArray();
            Stream openWrite = _webClient.OpenWrite(GetUrl(StatLightServiceRestApi.PostMessage));
            openWrite.Write(data, 0, data.Length);
            openWrite.Close();
            Thread.Sleep(1000);
            _mockPostHandler.Verify(v=>v.Handle(messageWritten));
        }

        private string GetString(string path)
        {
            var url = GetUrl(path);
            return _webClient.DownloadString(url);
        }

        private string GetUrl(string path)
        {
            return _baseUrl + path;
        }
    }
}