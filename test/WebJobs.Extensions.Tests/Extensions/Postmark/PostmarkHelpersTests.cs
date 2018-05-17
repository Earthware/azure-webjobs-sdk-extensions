// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Bindings;
using Microsoft.Azure.WebJobs.Extensions.Postmark;
using Newtonsoft.Json.Linq;
using PostmarkDotNet;
using Xunit;

namespace PostmarkTests
{
    public class PostmarkHelpersTests
    {
        [Fact]
        public void DefaultMessageProperties_CreatesExpectedMessage()
        {
            PostmarkAttribute attribute = new PostmarkAttribute();
            PostmarkConfiguration config = new PostmarkConfiguration
            {
                ServerToken = "12345",
                SenderSignature = "Test2 <test2@test.com>",
                ToAddress = "Test <test@test.com>",
                Tag = "test tag",
                TrackOpens = true
            };

            PostmarkMessage message = new PostmarkMessage();
            message.Subject = "TestSubject";
            message.TextBody = "TestText";
            message.HtmlBody = "<b>TestText</b>";

            PostmarkHelpers.DefaultMessageProperties(message, config, attribute);

            Assert.Same(config.SenderSignature, config.SenderSignature);
            Assert.Equal("Test <test@test.com>", message.To);
            Assert.Equal("TestSubject", message.Subject);
            Assert.Equal("<b>TestText</b>", message.HtmlBody);
            Assert.Equal("test tag", message.Tag);
            Assert.False(message.TrackOpens);
        }

        [Fact]
        public void CreateMessage_CreatesExpectedMessage()
        {
            // single recipient
            string mail = @"{
              'to': 'Test Account 2 <test2@acme.com>',
              'from': 'test3@contoso.com',
              'subject': 'Test Subject',
              'TextBody': 'Test Text',
              'HtmlBody': '<b>TestText</b>'
            }";

            var result = PostmarkHelpers.CreateMessage(mail);

            Assert.Equal("Test Account 2 <test2@acme.com>", result.To);
            Assert.Equal("test3@contoso.com", result.From);
            Assert.Equal("Test Subject", result.Subject);
            Assert.Equal("Test Text", result.TextBody);
        }

        [Fact]
        public void CreateConfiguration_CreatesExpectedConfiguration()
        {
            JObject config = new JObject();
            var result = PostmarkHelpers.CreateConfiguration(config);

            Assert.Null(result.SenderSignature);
            Assert.Null(result.ToAddress);

            config = new JObject
            {
                { "postMark", new JObject
                    {
                        { "to", "Testing1 <test1@test.com>" },
                        { "from", "Testing2 <test2@test.com>" }
                    }
                }
            };
            result = PostmarkHelpers.CreateConfiguration(config);

            Assert.Equal("Testing1 <test1@test.com>", result.ToAddress);
            Assert.Equal("Testing2 <test2@test.com>", result.SenderSignature);
        }

        [Fact]
        public void IsToValid()
        {
            // Null Personalization
            PostmarkMessage mail = new PostmarkMessage();
            Assert.False(PostmarkHelpers.IsToValid(mail));

            // Empty Personalization
            Assert.False(PostmarkHelpers.IsToValid(mail));

            // valid
            mail.To = "test@test.com";
            Assert.True(PostmarkHelpers.IsToValid(mail));
        }
    }
}
