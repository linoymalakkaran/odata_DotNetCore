using System.Linq;
using MGSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace MGSurvey.Infrastructure.Database
{
    public class MGSurveyDbContext : DbContext, IMGSurveyDbContext
    {
        private readonly MGSurveyStoreOptions _mgSurveyStoreOptions;
        public MGSurveyDbContext(DbContextOptions options, MGSurveyStoreOptions mgSurveyStoreOptions) : base(options)
        {
            _mgSurveyStoreOptions = mgSurveyStoreOptions;
            if (_mgSurveyStoreOptions == null)
            {
                _mgSurveyStoreOptions = new MGSurveyStoreOptions();
            }
        }

        public DbSet<ValidationSchema> ValidationSchemas { get; set; }
        public DbSet<SurveyResponsDetail> SurveyResponsDetails { get; set; }
        public DbSet<Form> Forms { get; set; } 
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<FormType> FormTypes { get; set; }
        public DbContext Context
        {
            get
            {
                return this;
            }
        } 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             
            modelBuilder.HasDbFunction(
                typeof(DbJsonValueExtensions).GetMethod(nameof(DbJsonValueExtensions.JSON_VALUE),
                                                    new System.Type[] { typeof(string), typeof(string) }))
            .HasTranslation(args =>
            {
                var arguments = args.ToList();
                SqlUnaryExpression sqlUnaryExp = null;
                SqlConstantExpression sqlConstExp = null;
                string sql = "";
                if (arguments[0] is SqlUnaryExpression)
                    sqlUnaryExp = arguments[0] as SqlUnaryExpression;
                if (arguments[1] is SqlConstantExpression)
                    sqlConstExp = arguments[1] as SqlConstantExpression;

                if (sqlUnaryExp != null)
                {
                    var sqlColumnExp = sqlUnaryExp.Operand as ColumnExpression;
                    
                    return new SqlFunctionExpression(
                                                    null,
                                                    null,
                                                    "JSON_VALUE",
                                                    false,
                                                    new[] { sqlColumnExp as SqlExpression, sqlConstExp },
                                                    true,
                                                    typeof(string),
                                                    null
                                                   ); 
                } 
                else 
                {
                    return new SqlFunctionExpression(
                                                    null,
                                                    null,
                                                    "JSON_VALUE",
                                                    false,
                                                    args,
                                                    true,
                                                    typeof(string),
                                                    null
                                                   ); 
                } 
            });

            modelBuilder.ConfigureMGSurveyDbContext(_mgSurveyStoreOptions);
            base.OnModelCreating(modelBuilder);
        }
    }
}
