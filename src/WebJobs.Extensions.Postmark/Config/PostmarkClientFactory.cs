// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Client;

namespace Microsoft.Azure.WebJobs.Extensions.Config
{
    internal class PostmarkClientFactory : IPostmarkClientFactory
    {
        public IPostmarkClient Create(string apiKey)
        {
            return new PostmarkClient(apiKey);
        }
    }
}
