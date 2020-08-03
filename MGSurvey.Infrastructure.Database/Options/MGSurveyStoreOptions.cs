
using System;
using Microsoft.EntityFrameworkCore;

namespace MGSurvey.Infrastructure.Database
{
    public class MGSurveyStoreOptions
    {
        public string DefaultSchema { get; set; } = null;
        public TableOptions Forms { get; set; } = new TableOptions("Forms");
        public TableOptions SurveyResponses { get; set; } = new TableOptions("SurveyResponses");
        public TableOptions FormTypes { get; set; } = new TableOptions("FormTypes");
        public TableOptions ValidationSchemas { get; set; } = new TableOptions("ValidationSchemas");
        public Action<DbContextOptionsBuilder> ConfigureDbContext { get; set; }
        public Action<IServiceProvider, DbContextOptionsBuilder> ResolveDbContextOptions { get; set; }
    }
}
