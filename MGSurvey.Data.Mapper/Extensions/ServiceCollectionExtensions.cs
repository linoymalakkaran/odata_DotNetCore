using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace MGSurvey.Data.Mapper.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMGSurveyAutoMapper(this IServiceCollection services)
        {
            var configuration = new MapperConfiguration(config =>
            { ;
                config.AddProfile<FormMapperProfile>(); 
            });

            services.AddSingleton<IMapper>(configuration.CreateMapper());
        }
    }
}
