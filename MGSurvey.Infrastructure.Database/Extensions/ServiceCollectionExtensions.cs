using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace MGSurvey.Infrastructure.Database
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// MGSurvey db context 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="storeOptionsAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddMGSurveyDbContext(this IServiceCollection services,
           Action<MGSurveyStoreOptions> storeOptionsAction = null)
        {
            return services.AddMGSurveyDbContext<MGSurveyDbContext>(storeOptionsAction);
        }
        /// <summary>
        /// MGSurvey db context 
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="storeOptionsAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddMGSurveyDbContext<TContext>(this IServiceCollection services,
                                                                         Action<MGSurveyStoreOptions> storeOptionsAction = null)
                                                                          where TContext : DbContext, IMGSurveyDbContext
        {

            var options = new MGSurveyStoreOptions();
            services.AddSingleton(options);
            storeOptionsAction?.Invoke(options);
            if (options.ResolveDbContextOptions != null)
            {
                services.AddDbContext<TContext>(options.ResolveDbContextOptions);
            }
            else
            {
                services.AddDbContext<TContext>(dbCtxBuilder =>
                {
                    options.ConfigureDbContext?.Invoke(dbCtxBuilder);
                });
            }
            services.AddScoped<IMGSurveyDbContext, TContext>();
            return services;
        }
    }
  
}
