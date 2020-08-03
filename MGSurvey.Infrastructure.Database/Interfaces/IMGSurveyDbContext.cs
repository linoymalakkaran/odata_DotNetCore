using MGSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace MGSurvey.Infrastructure.Database
{
    public interface IMGSurveyDbContext
    {
        DbContext Context { get; }
        DbSet<ValidationSchema> ValidationSchemas { get; set; } 
        DbSet<Form> Forms { get; set; }
        DbSet<SurveyResponse> SurveyResponses { get; set; }
        DbSet<SurveyResponsDetail> SurveyResponsDetails { get; set; }
        DbSet<FormType> FormTypes { get; set; }
        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

    }
}
