// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Mail;
using Microsoft.Azure.WebJobs.Extensions.Postmark;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PostmarkDotNet;

namespace Microsoft.Azure.WebJobs.Extensions.Bindings
{
    internal class PostmarkHelpers
    {
        internal static void DefaultMessageProperties(PostmarkMessage mail, PostmarkConfiguration config, PostmarkAttribute attribute)
        {
            // Apply message defaulting
            if (mail.From == null)
            {
                if (!string.IsNullOrEmpty(attribute.From))
                {
                    mail.From = attribute.From;
                }
                else if (config.SenderSignature != null)
                {
                    mail.From = config.SenderSignature;
                }
            }

            if (!IsToValid(mail))
            {
                if (!string.IsNullOrEmpty(attribute.To))
                {
                    mail.To = attribute.To;
                }
                else if (config.ToAddress != null)
                {
                    mail.To = config.ToAddress;
                }
            }

            if (string.IsNullOrEmpty(mail.Subject) &&
                !string.IsNullOrEmpty(attribute.Subject))
            {
                mail.Subject = attribute.Subject;
            }

            if ((mail.TextBody == null) &&
                !string.IsNullOrEmpty(attribute.TextBody))
            {
                mail.TextBody = attribute.TextBody;
            }

            if ((mail.HtmlBody == null) &&
                !string.IsNullOrEmpty(attribute.TextBody))
            {
                mail.TextBody = attribute.TextBody;
            }

            if ((mail.Tag == null) &&
                !string.IsNullOrEmpty(attribute.Tag))
            {
                mail.Tag = attribute.Tag;
            }
            else if (config.Tag != null)
            {
                mail.Tag = config.Tag;
            }

            if (mail.TrackOpens == null)
            {
                var tempBool = false;
                bool.TryParse(attribute.TrackOpens, out tempBool);
                mail.TrackOpens = tempBool;
            }
            else
            {
                mail.TrackOpens = config.TrackOpens;
            }
        }

        internal static PostmarkMessage CreateMessage(string input)
        {
            JObject json = JObject.Parse(input);
            return CreateMessage(json);
        }

        internal static PostmarkMessage CreateMessage(JObject input)
        {
            return input.ToObject<PostmarkMessage>();
        }

        internal static string CreateString(PostmarkMessage input)
        {
            return CreateString(JObject.FromObject(input));
        }

        internal static string CreateString(JObject input)
        {
            return input.ToString(Formatting.None);
        }

        internal static bool IsToValid(PostmarkMessage item)
        {
            return item.To != null ? item.To.Any() : false;
        }

        internal static PostmarkConfiguration CreateConfiguration(JObject metadata)
        {
            PostmarkConfiguration postmarkConfig = new PostmarkConfiguration();

            JObject configSection = (JObject)metadata.GetValue("postMark", StringComparison.OrdinalIgnoreCase);
            JToken value = null;
            if (configSection != null)
            {
                if (configSection.TryGetValue("from", StringComparison.OrdinalIgnoreCase, out value))
                {
                    postmarkConfig.SenderSignature = (string)value;
                }

                if (configSection.TryGetValue("to", StringComparison.OrdinalIgnoreCase, out value))
                {
                    postmarkConfig.ToAddress = (string)value;
                }
            }

            return postmarkConfig;
        }
    }
}
