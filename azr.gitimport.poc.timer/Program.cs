using AP.Serilog.Sinks.LogAnalyitics.Config;
using AP.Serilog.Sinks.LogAnalyitics.Extensions;
using azr.gitimport.poc.timer.config;
using azr.gitimport.poc.timer.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using System;
using System.IO;

namespace azr.gitimport.poc.timer
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "DEV"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var logger = new LoggerConfiguration()
                           .Enrich.FromLogContext()
                           .WriteTo.Console()
                        .CreateBootstrapLogger();

            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config => config.AddConfiguration(configuration))
                 .UseLogAnalytics(context => new LogAnalyticsConfiguration(new WorkspaceConfig
                 {
                     WorkspaceName = context.Configuration["LogAnalytics:WorkspaceName"],
                     ApplicationType = context.Configuration["LogAnalytics:ApplicationName"],
                     LogType = context.Configuration["LogAnalytics:LogType"],
                     QueueLimitBytes = long.Parse(context.Configuration["LogAnalytics:queueLimitBytes"]),
                     LogEventsInBatchLimit = int.Parse(context.Configuration["LogAnalytics:logEventsInBatchLimit"]),
                     Period = TimeSpan.FromSeconds(double.Parse(context.Configuration["LogAnalytics:period"])),
                 }, new KeyVaultConfig()))
                .ConfigureServices((hostContext, services) =>
                {
                    services.ReadKeyVaultSecrets(hostContext);
                    services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionJobFactory();
                        var jobKey = new JobKey("CronHostService");
                        q.AddJob<GitImportServiceHost>(opts => opts.WithIdentity(jobKey));
                        q.AddTrigger(opts => opts
                            .ForJob(jobKey)
                            .WithIdentity("CronHostService-trigger")
                            .WithCronSchedule(configuration.GetSection("AppSettings").GetSection("CronExpressionForScheduler").Value));
                        q.AddTrigger(opts => opts
                            .ForJob(jobKey)
                            .WithIdentity("CronHostService-triggerNow")
                            .StartNow());
                        // Add the Quartz.NET hosted service
                        services.AddQuartzHostedService(
                            q => q.WaitForJobsToComplete = true);

                    });
                })
                .Build().Run();
        }
    }
}