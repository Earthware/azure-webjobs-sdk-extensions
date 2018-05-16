// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Client;
using Microsoft.Azure.WebJobs.Extensions.Postmark;
using PostmarkDotNet;

namespace Microsoft.Azure.WebJobs.Extensions.Bindings
{
    internal class PostmarkMessageAsyncCollector : IAsyncCollector<PostmarkMessage>
    {
        private readonly PostmarkConfiguration _config;
        private readonly PostmarkAttribute _attribute;
        private readonly Collection<PostmarkMessage> _messages = new Collection<PostmarkMessage>();
        private readonly IPostmarkClient _postMark;

        public PostmarkMessageAsyncCollector(PostmarkConfiguration config, PostmarkAttribute attribute, IPostmarkClient postMark)
        {
            _config = config;
            _attribute = attribute;
            _postMark = postMark;
        }

        public Task AddAsync(PostmarkMessage item, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            PostmarkHelpers.DefaultMessageProperties(item, _config, _attribute);

            if (!PostmarkHelpers.IsToValid(item))
            {
                throw new InvalidOperationException("A 'To' address must be specified for the message.");
            }
            if (item.From == null || string.IsNullOrEmpty(item.From))
            {
                throw new InvalidOperationException("A 'From' address must be specified for the message.");
            }

            _messages.Add(item);

            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var message in _messages)
            {
                await _postMark.SendMessageAsync(message);
            }
        }        
    }
}
