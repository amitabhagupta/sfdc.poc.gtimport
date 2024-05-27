using azr.gitimport.poc.timer.config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace azr.gitimport.poc.timer
{
    [DisallowConcurrentExecution]
    class GitImportServiceHost : IJob
    {
        private readonly ILogger<GitImportServiceHost> _logger;
        private readonly AppSettings _appSettings;

        public GitImportServiceHost(IOptions<AppSettings> appSettings, ILogger<GitImportServiceHost> logger)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"git import poc app started at {DateTime.Now}");

            if (string.IsNullOrEmpty(_appSettings.SfClientId))
            {
                _logger.LogError($"could not fetch key vault details : {_appSettings.SfClientId}");
            }
            else
            {
                _logger.LogInformation($" key vault details fetched : {_appSettings.SfClientId}");
            }

            _logger.LogInformation($"git import poc app ended at {DateTime.Now}");
        }
    }
}
