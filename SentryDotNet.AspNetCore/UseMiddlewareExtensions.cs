﻿using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SentryDotNet.AspNetCore
{
    public static class UseMiddlewareExtensions
    {
        public static IServiceCollection AddSentryDotNet(this IServiceCollection services, ISentryClient sentryClient = null)
        {
            services.AddSingleton(sentryClient ?? new SentryClient(""));

            return services;
        }
        
        public static IApplicationBuilder UseSentryDotNet(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<SentryDotNetMiddleware>();
        }
    }
}