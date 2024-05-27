using AP.Serilog.Sinks.LogAnalyitics.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azr.gitimport.poc.timer.Extensions
{
    public static class KeyVaultExtension
    {
        public static void ReadKeyVaultSecrets(this IServiceCollection services, HostBuilderContext context)
        {
            //assign app settings secrets from keyvault
            var keyVaultUtility = new KeyVaultUtility();
            var appSettingsList = context.Configuration.GetSection("AppSettings").GetChildren().ToList();

            foreach (var item in appSettingsList)
            {
                if (!string.IsNullOrEmpty(item.Value) && item.Value.Equals("{Read_KeyVault}"))
                {
                    context.Configuration[$"AppSettings:{item.Key}"] = keyVaultUtility.GetSecret(item.Key);
                }
            }
        }
    }
}
