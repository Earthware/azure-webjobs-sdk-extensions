// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
using System;
using Microsoft.Azure.WebJobs.Extensions.Postmark;

namespace Microsoft.Azure.WebJobs
{
    /// <summary>
    /// Extension methods for Postmark integration
    /// </summary>
    public static class PostmarkJobHostConfigurationExtensions
    {
        /// <summary>
        /// Enables use of the Postmark extensions
        /// </summary>
        /// <param name="config">The <see cref="JobHostConfiguration"/> to configure.</param>
        /// <param name="postmarkConfig">The <see cref="PostmarkConfiguration"/> to use.</param>
        public static void UsePostmark(this JobHostConfiguration config, PostmarkConfiguration postmarkConfig = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (postmarkConfig == null)
            {
                postmarkConfig = new PostmarkConfiguration();
            }

            config.RegisterExtensionConfigProvider(postmarkConfig);
        }
    }
}
