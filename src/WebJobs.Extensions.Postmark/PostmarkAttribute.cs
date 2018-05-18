// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs
{
    /// <summary>
    /// Attribute used to bind a parameter to a PostmarkMessage that will automatically be
    /// sent when the function completes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class PostmarkAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a string value indicating the app setting to use as the Postmark server token, 
        /// if different than the one specified in the <see cref="Extensions.Postmark.PostmarkConfiguration"/>.
        /// </summary>
        [AppSetting]
        public string ServerToken { get; set; }

        /// <summary>
        /// Gets or sets a string value indicating the app setting to use as the Postmark sender signature, 
        /// if different than the one specified in the <see cref="Extensions.Postmark.PostmarkConfiguration"/>.
        /// </summary>
        [AppSetting]
        public string SenderSignature { get; set; }

        /// <summary>
        /// Gets or sets the message "To" field. May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the message "From" field. May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the message "Subject" field. May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the message "Text body" field. May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string TextBody { get; set; }

        /// <summary>
        /// Gets or sets the message "Html body" field. May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string HtmlBody { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Track Opens" field is true or false. May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string TrackOpens { get; set; }

        /// <summary>
        /// Gets or sets the message "Tag" field. May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string Tag { get; set; }
    }
}
