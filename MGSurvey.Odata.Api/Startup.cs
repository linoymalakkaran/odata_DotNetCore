
using System.Linq;
using MGSurvey.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using MGSurvey.Data.Mapper.Extensions;
using Microsoft.AspNetCore.Builder;
using MGSurvey.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.OData.Formatter.Serialization;
using MGSurvey.Utilities;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MGSurvey.Odata.Api.Validators;

namespace MGSurvey.Odata.Api
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("DefaultConnectionString");
            services.AddSingleton<IConfiguration>(x => Configuration);
            //register identity database context options
            services.AddSingleton(x => new DbContextOptionsBuilder<MGSurveyDbContext>().UseSqlServer(connectionString).AddInterceptors(new DbInterceptor()));
            //add schema validation service
            services.AddScoped<ISchemaValidatorService, SchemaValidatorService>();
            //register application database context
            services.AddMGSurveyDbContext<MGSurveyDbContext>(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString).AddInterceptors(new DbInterceptor());
                options.DefaultSchema = Configuration.GetValue<string>("DatabaseSchema:ApplicationDbSchema");
            });


            var maqtaAppSecretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("m@qta_@pp_!@#$%^&*()"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = "MG.App.Security",
                            ValidAudience = "MG.App.Apppintment.Api",
                            IssuerSigningKey = maqtaAppSecretKey
                        };
                    });


            services.AddAuthorization(options =>
            {
                options.AddPolicy("Member",
                    policy => policy.RequireClaim("MembershipId"));
            });
            //services.AddCors(options =>
            //{
            //    options.AddDefaultPolicy(builder =>
            //    {
            //        builder.AllowAnyHeader();
            //        builder.AllowAnyMethod();
            //        builder.AllowAnyOrigin();
            //        builder.AllowCredentials();
            //    });
            //});

            services.AddCors(c =>
            {
                c.AddPolicy("CorsPolicy", options => options.SetIsOriginAllowed(origin=>true).AllowCredentials().AllowAnyHeader().AllowAnyMethod());
            });


       

            services.AddControllers(options=> {
                options.ModelBinderProviders.Insert(0, new MGSurveyModelBinderProvider());
            })
           .AddNewtonsoftJson(options =>
            {
                // Use the default property (Pascal) casing
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                // Configure a custom converter
                options.SerializerSettings.Converters.Add(new MGSurveyJsonConverter());
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            })
           .AddXmlSerializerFormatters();

            services.AddMGSurveyAutoMapper();
            services.AddOData();
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseCors();
            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseJWTInHeader();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseODataBatching();

            IEdmModel model = MGSurveyODataEdmModelBuilder.BuildEdmModel();
            app.UseEndpoints(endpoints =>
            {
                //un comment if we want open/default route also to be odata based
                //endpoints.MapODataRoute("nullPrefix", null, b =>
                //{
                //    b.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model);
                //    b.AddService<Microsoft.AspNet.OData.Query.Expressions.FilterBinder>(Microsoft.OData.ServiceLifetime.Singleton, sp => new ODataFilter(sp));
                //    b.AddService<IEnumerable<IODataRoutingConvention>>(Microsoft.OData.ServiceLifetime.Singleton,
                //        sp => ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", endpoints.ServiceProvider));
                //});

                endpoints.EnableDependencyInjection();
                endpoints.Select().Expand().Filter().OrderBy().MaxTop(100).Count().SkipToken();
                endpoints.MapODataRoute("odata", "odata", b =>
                {
                    b.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model);
                    b.AddService<ODataSerializerProvider>(Microsoft.OData.ServiceLifetime.Singleton, s => new CustomODataSerializerProvider(s));
                    b.AddService<Microsoft.AspNet.OData.Query.Expressions.FilterBinder>(Microsoft.OData.ServiceLifetime.Singleton, sp => new CustomODataFilter(sp));
                    b.AddService<IEnumerable<IODataRoutingConvention>>(Microsoft.OData.ServiceLifetime.Singleton,
                        sp => ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", endpoints.ServiceProvider));
                });
            });
        }
    }
}
