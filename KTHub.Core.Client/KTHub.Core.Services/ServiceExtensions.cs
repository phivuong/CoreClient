using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTHub.Core.Services
{
    public static class ServiceExtensions
    {
        public static TConfig ConfigurePOCO<TConfig>(this IServiceCollection services,IConfiguration configuration) where TConfig : class, new()
        {
            if (services == null)
            throw new ArgumentNullException(nameof(services));
            TConfig config = configuration != null ? configuration.GetConfig<TConfig>() : throw new ArgumentNullException(nameof(configuration));
            ServiceCollectionServiceExtensions.AddSingleton<TConfig>(services, config);
            return config;
        }
        public static TConfig GetConfig<TConfig>(this IConfiguration configuration) where TConfig : class, new()
        {
            TConfig config = new TConfig();
            ConfigurationBinder.Bind((IConfiguration)configuration.GetSection(typeof(TConfig).Name), (object)config);
            return config;
        }
    }
}
