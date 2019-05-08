using System;
using Hangfire;
using JetBrains.Annotations;
using Lykke.Job.NeoGasDistributor.Settings;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.Sdk;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Job.NeoGasDistributor
{
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "NeoGasDistributor API",
            ApiVersion = "v1"
        };
        
        [UsedImplicitly]
        public IServiceProvider ConfigureServices(
            IServiceCollection services)
        {
            return services
                .AddHangfire(cfg => { })
                .BuildServiceProvider<AppSettings>(options =>
                {
                    options.SwaggerOptions = _swaggerOptions;
                    
                    options.Logs = logs =>
                    {
                        logs.AzureTableName = "NeoGasDistributionPlannerLog";
                        logs.AzureTableConnectionStringResolver = settings => settings.NeoGasDistributor.Db.LogsConnString;
                        
                        logs.Extended = opt =>
                        {
                            opt.AddAdditionalSlackChannel(
                                "CommonBlockChainIntegration",
                                slackOptions =>
                                {
                                    slackOptions.MinLogLevel = LogLevel.Information;
                                    slackOptions.IncludeHealthNotifications();
                                    slackOptions.SpamGuard.DisableGuarding();
                                });

                            opt.AddAdditionalSlackChannel(
                                "CommonBlockChainIntegrationImportantMessages",
                                slackOptions =>
                                {
                                    slackOptions.MinLogLevel = LogLevel.Warning;
                                    slackOptions.IncludeHealthNotifications();
                                    slackOptions.SpamGuard.DisableGuarding();
                                });
                        };
                    };
                });
        }
        
        [UsedImplicitly]
        public void Configure(
            IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
            });

            app.UseHangfireDashboard();
        }
    }
}
