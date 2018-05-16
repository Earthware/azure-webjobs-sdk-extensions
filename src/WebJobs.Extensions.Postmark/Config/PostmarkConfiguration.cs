// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using Client;
using Microsoft.Azure.WebJobs.Extensions.Bindings;
using Microsoft.Azure.WebJobs.Extensions.Config;
using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PostmarkDotNet;

namespace Microsoft.Azure.WebJobs.Extensions.Postmark
{
    /// <summary>
    /// Defines the configuration options for the Postmark binding.
    /// </summary>
    public class PostmarkConfiguration : IExtensionConfigProvider
    {
        internal const string AzureWebJobsPostmarkServerTokenName = "AzureWebJobsPostmarkServerToken";

        private ConcurrentDictionary<string, IPostmarkClient> _postmarkClientCache = new ConcurrentDictionary<string, IPostmarkClient>();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public PostmarkConfiguration()
        {
            ClientFactory = new PostmarkClientFactory();
        }

        /// <summary>
        /// Gets or sets the Postmark Server Token. If not explicitly set, the value will be defaulted
        /// to the value specified via the 'AzureWebJobsPostmarkServerToken' app setting or the
        /// 'AzureWebJobsPostmarkServerToken' environment variable.
        /// </summary>
        public string ServerToken { get; set; }

        /// <summary>
        /// Gets or sets the default "to" address that will be used for messages.
        /// This value can be overridden by job functions.
        /// </summary>
        /// <remarks>
        /// An example of when it would be useful to provide a default value for 'to' 
        /// would be for emailing your own admin account to notify you when particular
        /// jobs are executed. In this case, job functions can specify minimal info in
        /// their bindings, for example just a Subject and Text body.
        /// </remarks>
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets the default "from" address that will be used for messages.
        /// This value can be overridden by job functions.
        /// </summary>
        public string SenderSignature { get; set; }

        internal IPostmarkClientFactory ClientFactory { get; set; }

        /// <inheritdoc />
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var metadata = new ConfigMetadata();
            context.ApplyConfig(metadata, "postmark");
            this.ToAddress = metadata.To;
            this.SenderSignature = metadata.SenderSignature;

            if (string.IsNullOrEmpty(this.ServerToken))
            {
                INameResolver nameResolver = context.Config.NameResolver;
                this.ServerToken = nameResolver.Resolve(AzureWebJobsPostmarkServerTokenName);
            }

            context
                .AddConverter<string, PostmarkMessage>(PostmarkHelpers.CreateMessage)
                .AddConverter<JObject, PostmarkMessage>(PostmarkHelpers.CreateMessage);

            var rule = context.AddBindingRule<PostmarkAttribute>();
            rule.AddValidator(ValidateBinding);
            rule.BindToCollector<PostmarkMessage>(CreateCollector);
        }

        private IAsyncCollector<PostmarkMessage> CreateCollector(PostmarkAttribute attr)
        {
            string apiKey = FirstOrDefault(attr.ServerToken, ServerToken);
            IPostmarkClient postMark = _postmarkClientCache.GetOrAdd(apiKey, a => ClientFactory.Create(a));
            return new PostmarkMessageAsyncCollector(this, attr, postMark);
        }

        private void ValidateBinding(PostmarkAttribute attribute, Type type)
        {
            ValidateBinding(attribute);
        }

        private void ValidateBinding(PostmarkAttribute attribute)
        {
            string apiKey = FirstOrDefault(attribute.ServerToken, ServerToken);

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException(
                    $"The Postmark Server Token must be set either via an '{AzureWebJobsPostmarkServerTokenName}' app setting, via an '{AzureWebJobsPostmarkServerTokenName}' environment variable, or directly in code via {nameof(PostmarkConfiguration)}.{nameof(PostmarkConfiguration.ServerToken)} or {nameof(PostmarkAttribute)}.{nameof(PostmarkAttribute.ServerToken)}.");
            }
        }

        private static string FirstOrDefault(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrEmpty(v));
        }

        // Schema for host.json 
        private class ConfigMetadata
        {
            [JsonProperty("to")]
            public string To { get; set; }

            [JsonProperty("from")]
            public string SenderSignature { get; set; }
        }
    }
}
