﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using PostmarkDotNet;

namespace Client
{
    internal interface IPostmarkClient
    {
        Task<PostmarkResponse> SendMessageAsync(PostmarkMessage msg);
    }
}
