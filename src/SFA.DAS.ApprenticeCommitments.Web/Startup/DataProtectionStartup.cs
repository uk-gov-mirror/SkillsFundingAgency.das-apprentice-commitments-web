﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace SFA.DAS.ApprenticeCommitments.Web.Startup
{
    public static class DataProtectionStartup
    {
        public static IServiceCollection AddDataProtection(
            this IServiceCollection services,
            DataProtectionConnectionStrings configuration,
            IWebHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
            {
                if (configuration != null)
                {
                    var redisConnectionString = configuration.RedisConnectionString;
                    var dataProtectionKeysDatabase = configuration.DataProtectionKeysDatabase;

                    var redis = ConnectionMultiplexer
                        .Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

                    services.AddDataProtection()
                        .SetApplicationName("apprentice-commitments")
                        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
                }
            }
            return services;
        }
    }

    public class DataProtectionConnectionStrings
    {
        public string RedisConnectionString { get; set; }
        public string DataProtectionKeysDatabase { get; set; }
    }
}