// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using PostmarkDotNet;

namespace Client
{
    internal class PostmarkClient : IPostmarkClient
    {
        private PostmarkDotNet.PostmarkClient _client;

        public PostmarkClient(string serverToken)
        {
            _client = new PostmarkDotNet.PostmarkClient(serverToken);
        }

        public async Task<PostmarkResponse> SendMessageAsync(PostmarkMessage msg)
        {
            try
            {
                var response = await _client.SendMessageAsync(msg);

                if (response.Status != PostmarkStatus.Success)
                {
                    string body = response.Message;
                    throw new InvalidOperationException(body);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
