// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Config;
using Microsoft.Azure.WebJobs.Extensions.Postmark;
using Microsoft.Azure.WebJobs.Extensions.Tests.Common;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Indexers;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using PostmarkDotNet;
using Xunit;

namespace PostmarkTests
{
    [Trait("Category", "E2E")]
    public class PostmarkEndToEndTests
    {
        private const string DefaultApiKey = "Default";
        private const string ConfigApiKey = "Config";
        private const string AttributeApiKey1 = "Attribute1";
        private const string AttributeApiKey2 = "Attribute2";

        private readonly TestLoggerProvider _loggerProvider = new TestLoggerProvider();

        [Fact]
        public async Task OutputBindings_WithKeysOnConfigAndAttribute()
        {
            string functionName = nameof(PostmarkEndToEndFunctions.Outputs_AttributeAndConfig);

            Mock<IPostmarkClientFactory> factoryMock;
            Mock<Client.IPostmarkClient> clientMock;
            InitializeMocks(out factoryMock, out clientMock);

            await RunTestAsync(functionName, factoryMock.Object, configApiKey: ConfigApiKey, includeDefaultServerToken: false);

            // We expect three clients to be created. The others should be re-used because the ApiKeys match.
            factoryMock.Verify(f => f.Create(AttributeApiKey1), Times.Once());
            factoryMock.Verify(f => f.Create(AttributeApiKey2), Times.Once());
            factoryMock.Verify(f => f.Create(ConfigApiKey), Times.Once());

            // This function sends 4 messages.
            clientMock.Verify(c => c.SendMessageAsync(It.IsAny<PostmarkMessage>()), Times.Exactly(4));

            // Just make sure traces work.
            Assert.Equal(functionName, _loggerProvider.GetAllUserLogMessages().Single().FormattedMessage);

            factoryMock.VerifyAll();
            clientMock.VerifyAll();
        }

        [Fact]
        public async Task OutputBindings_WithNameResolver()
        {
            string functionName = nameof(PostmarkEndToEndFunctions.Outputs_NameResolver);

            Mock<IPostmarkClientFactory> factoryMock;
            Mock<IPostmarkClient> clientMock;
            InitializeMocks(out factoryMock, out clientMock);

            await RunTestAsync(functionName, factoryMock.Object, configApiKey: null, includeDefaultServerToken: true);

            // We expect one client to be created.
            factoryMock.Verify(f => f.Create(DefaultApiKey), Times.Once());

            // This function sends 1 message.
            clientMock.Verify(c => c.SendMessageAsync(It.IsAny<PostmarkMessage>()), Times.Once);

            // Just make sure traces work.
            Assert.Equal(functionName, _loggerProvider.GetAllUserLogMessages().Single().FormattedMessage);

            factoryMock.VerifyAll();
            clientMock.VerifyAll();
        }

        [Fact]
        public async Task OutputBindings_NoApiKey()
        {
            string functionName = nameof(PostmarkEndToEndFunctions.Outputs_NameResolver);

            Mock<IPostmarkClientFactory> factoryMock;
            Mock<IPostmarkClient> clientMock;
            InitializeMocks(out factoryMock, out clientMock);

            var ex = await Assert.ThrowsAsync<FunctionIndexingException>(
                () => RunTestAsync(functionName, factoryMock.Object, configApiKey: null, includeDefaultServerToken: false));

            Assert.Equal("The Postmark Server Token must be set either via an 'AzureWebJobsPostmarkServerToken' app setting, via an 'AzureWebJobsPostmarkServerToken' environment variable, or directly in code via PostmarkConfiguration.ServerToken or PostmarkAttribute.ServerToken.", ex.InnerException.Message);
        }

        private void InitializeMocks(out Mock<IPostmarkClientFactory> factoryMock, out Mock<IPostmarkClient> clientMock)
        {
            var mockResponse = new PostmarkResponse();
            mockResponse.Status = PostmarkStatus.Success;
            clientMock = new Mock<Client.IPostmarkClient>(MockBehavior.Strict);
            clientMock
                .Setup(c => c.SendMessageAsync(It.IsAny<PostmarkMessage>()))
                .ReturnsAsync(mockResponse);

            factoryMock = new Mock<IPostmarkClientFactory>(MockBehavior.Strict);
            factoryMock
                    .Setup(f => f.Create(It.IsAny<string>()))
                    .Returns(clientMock.Object);
        }

        private async Task RunTestAsync(string testName, IPostmarkClientFactory factory, object argument = null, string configApiKey = null, bool includeDefaultServerToken = true)
        {
            Type testType = typeof(PostmarkEndToEndFunctions);
            ExplicitTypeLocator locator = new ExplicitTypeLocator(testType);
            JobHostConfiguration config = new JobHostConfiguration
            {
                TypeLocator = locator,
            };

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(_loggerProvider);

            config.LoggerFactory = loggerFactory;

            var arguments = new Dictionary<string, object>();
            arguments.Add("triggerData", argument);

            var postmarkConfig = new PostmarkConfiguration
            {
                ServerToken = configApiKey,
                ClientFactory = factory,
                ToAddress = "ToConfig@test.com",
                SenderSignature = "FromConfig@test.com"
            };

            var resolver = new TestNameResolver();
            resolver.Values.Add("MyKey1", AttributeApiKey1);
            resolver.Values.Add("MyKey2", AttributeApiKey2);

            if (includeDefaultServerToken)
            {
                resolver.Values.Add(PostmarkConfiguration.AzureWebJobsPostmarkServerTokenName, DefaultApiKey);
            }

            config.NameResolver = resolver;

            config.UsePostmark(postmarkConfig);

            JobHost host = new JobHost(config);

            await host.StartAsync();
            await host.CallAsync(testType.GetMethod(testName), arguments);
            await host.StopAsync();
        }

        private class PostmarkEndToEndFunctions
        {
            // This function verifies Attribute and Config behavior for ApiKey
            public static void Outputs_AttributeAndConfig(
                [Postmark(ServerToken = "MyKey1")] out PostmarkMessage message,
                [Postmark] out JObject jObject,
                [Postmark(ServerToken = "MyKey1")] IAsyncCollector<PostmarkMessage> asyncCollectorMessage,
                [Postmark(ServerToken = "MyKey2")] ICollector<JObject> collectorJObject,
                TraceWriter trace)
            {
                message = new PostmarkMessage();

                jObject = JObject.Parse(@"{
                  'to': 'ToFunction@test.com',
                  'from': 'FromFunction@test.com'
                }");

                asyncCollectorMessage.AddAsync(new PostmarkMessage()).Wait();
                collectorJObject.Add(new JObject());

                trace.Warning(nameof(Outputs_AttributeAndConfig));
            }

            // This function verifies Default (NameResolver) behavior for ApiKey
            public static void Outputs_NameResolver(
                [Postmark] out PostmarkMessage message,
                TraceWriter trace)
            {
                message = new PostmarkMessage();
                trace.Warning(nameof(Outputs_NameResolver));
            }
        }
    }
}
