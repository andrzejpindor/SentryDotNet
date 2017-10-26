﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SentryDotNet
{
    public class SentryEventBuilder
    {
        private readonly ISentryClient _client;

        public SentryEventBuilder(ISentryClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Hexadecimal string representing a uuid4 value.
        /// </summary>
        public Guid? EventId { get; set; }

        /// <summary>
        /// Indicates when the logging record was created.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// The name of the logger from which the event was created.
        /// </summary>
        public string Logger { get; set; }

        /// <summary>
        /// The platform the client was using when the event was created.
        /// This will be used by Sentry to alter various UI components.
        /// <para />
        /// Acceptable Values: as3, c, cfml, cocoa, csharp, go, java, javascript, node, objc, other, perl, php, python, and ruby
        /// </summary>
        public string Platform { get; set; } = "csharp";

        /// <summary>
        /// Information about the <see cref="SentrySdk" /> sending the event.
        /// </summary>
        public SentrySdk Sdk { get; set; } = SentrySdk.SentryDotNet;

        /// <summary>
        /// The record <see cref="SeverityLevel" />.
        /// </summary>
        public SeverityLevel? Level { get; set; }

        /// <summary>
        /// The name of the transaction (or culprit) which caused this event to be created.
        /// </summary>
        public string Culprit { get; set; }

        /// <summary>
        /// The name of the host client from which the event was created e.g. foo.example.com.
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// The version of the application.
        /// </summary>
        public string Release { get; set; }

        /// <summary>
        /// A list of tags, or additional information, for this event.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// The operating environment that the event was created e.g. production, staging.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// A list of relevant modules and their versions.
        /// </summary>
        public Dictionary<string, string> Modules { get; set; }

        /// <summary>
        /// An arbitrary mapping of additional metadata to store with the event.
        /// </summary>
        public object Extra { get; set; }

        /// <summary>
        /// An array of strings used to dictate the deduplication of this event.
        /// </summary>
        public string[] Fingerprint { get; set; }

        /// <summary>
        /// A list of <see cref="SentryException" /> related to this event.
        /// </summary>
        public List<SentryException> Exception { get; set; }

        /// <summary>
        /// A user friendly event that conveys the meaning of this event.
        /// </summary>
        public SentryMessage Message { get; set; }

        /// <summary>
        /// A trail of breadcrumbs, if any, that led up to the event creation.
        /// </summary>
        public List<SentryBreadcrumb> Breadcrumbs { get; set; } = new List<SentryBreadcrumb>();

        /// <summary>
        /// A dictionary of <see cref="ISentryContext" /> for this event.
        /// </summary>
        public Dictionary<string, ISentryContext> Contexts { get; set; } = new Dictionary<string, ISentryContext>();

        // TODO Add Http, User, and Threads "interfaces"

        public void SetMessage(string message)
        {
            if (Level == null)
            {
                Level = SeverityLevel.Info;
            }
            
            Message = new SentryMessage { Message = message };
        }

        public void SetException(Exception ex)
        {
            if (Culprit == null)
            {
                Culprit = ex.TargetSite == null
                    ? null
                    : $"{ex.TargetSite.ReflectedType.FullName} in {ex.TargetSite.Name}";
            }

            if (Message == null)
            {
                Message = new SentryMessage { Message = ex.Message };
            }

            Exception = ConvertException(ex);
        }
        
        private static List<SentryException> ConvertException(Exception ex)
        {
            var sentryException = new SentryException
            {
                Module = ex.Source,
                Stacktrace = SentryStacktrace.FromException(ex),
                Type = ex.GetType().FullName,
                Value = ex.Message
            };

            return new[] { sentryException }
                .Concat(ex.InnerException == null ? new List<SentryException>() : ConvertException(ex.InnerException))
                .ToList();
        }

        public SentryEvent Build()
        {
            return new SentryEvent
                   {
                       EventId = (EventId ?? Guid.NewGuid()).ToString("N"),
                       Timestamp = Timestamp ?? DateTime.UtcNow,
                       Logger = Logger,
                       Platform = Platform,
                       Sdk = Sdk,
                       Level = Level ?? (Exception == null ? SeverityLevel.Info : SeverityLevel.Error),
                       Culprit = Culprit,
                       ServerName = ServerName,
                       Release = Release,
                       Tags = Tags,
                       Environment = Environment,
                       Modules = Modules,
                       Extra = Extra,
                       Fingerprint = Fingerprint,
                       Exception = Exception,
                       Message = Message,
                       Breadcrumbs = Breadcrumbs,
                       Contexts = Contexts
                   };
        }

        public async Task<string> CaptureAsync()
        {
            return await _client.SendAsync(Build());
        }
    }
}
