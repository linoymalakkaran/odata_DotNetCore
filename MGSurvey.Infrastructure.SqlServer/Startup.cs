/*
           This project is only to generate migrations.
           Configure your DbContext  here to generate design time migration.

           Add-Migration YourMigrationName -c YourDbContextName -s MGSurvey.Infrastructure.SqlServer  -o MigrationFolderName
           Script-Migration  -c YourDbContextName -s MGSurvey.Infrastructure.SqlServer
           Script-Migration YourMigrationName -c YourDbContextName -s MGSurvey.Infrastructure.SqlServer
*/

using System.Reflection;
using MGSurvey.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace MGSurvey.Infrastructure.SqlServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /*
           This project is only to generate migrations.
           Configure your DbContext  here to generate design time migration.

           Add-Migration YourMigrationName -c YourDbContextName -s MMGSurvey.Infrastructure.SqlServer  -o MigrationFolderName
           Script-Migration  -c YourDbContextName -s MGSurvey.Infrastructure.SqlServer
           Script-Migration YourMigrationName -c YourDbContextName -s MGSurvey.Infrastructure.SqlServer
        */
        public void ConfigureServices(IServiceCollection services)
        {
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string connectionString = Configuration.GetConnectionString("DefaultConnectionString");
            //register application database context
            services.AddMGSurveyDbContext<MGSurveyDbContext>(options =>
            {
                options.DefaultSchema = Configuration.GetValue<string>("DatabaseSchema:ApplicationDbSchema");
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString, m => m.MigrationsAssembly(migrationAssembly));
            }); 
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*
                This project is only to generate migrations, do'nt write code here. 
            */
        }
    }
}
