
using System;
using System.Linq;
using Newtonsoft.Json;
using MGSurvey.Domain.Entities;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MGSurvey.Utilities;

namespace MGSurvey.Infrastructure.Database
{
    public static class MGSurveyModelBuilderExtensions
    {
        private static EntityTypeBuilder<TEntity> ToTable<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, TableOptions configuration)
              where TEntity : class
        {
            return string.IsNullOrWhiteSpace(configuration.Schema) ? entityTypeBuilder.ToTable(configuration.Name) : entityTypeBuilder.ToTable(configuration.Name, configuration.Schema);
        }

        public static void ConfigureMGSurveyDbContext(this ModelBuilder modelBuilder, MGSurveyStoreOptions storeOptions)
        {
            if (!string.IsNullOrWhiteSpace(storeOptions.DefaultSchema)) modelBuilder.HasDefaultSchema(storeOptions.DefaultSchema);
            
            var valueComparer = new ValueComparer<IDictionary<string, object>>(
                                        (c1, c2) => c1.SequenceEqual(c2),
                                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())));

            modelBuilder.Entity<Form>(entity =>
            {
                entity.ToTable<Form>(storeOptions.Forms);
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasMaxLength(36).HasColumnType("varchar(36)").IsRequired(); 
                entity.Property(x => x.Name).HasMaxLength(250).HasColumnType("varchar(250)").IsRequired();
                entity.Property(x => x.Type).HasMaxLength(25).HasColumnType("varchar(25)").IsRequired();
                entity.Property(x => x.CreatedBy).HasMaxLength(256).HasColumnType("varchar(256)").IsRequired();
                entity.Property(x => x.CreatedDate).IsRequired();
                entity.Property(x => x.UpdatedBy).HasMaxLength(256).HasColumnType("varchar(256)");
                // apparently anything over 4K converts to nvarchar(max) on SqlServer
                entity.Property(x => x.EntityData).HasMaxLength(50000).IsRequired();  
                entity.HasOne<ValidationSchema>(x => x.ValidationSchema).WithOne(x => x.Form).HasForeignKey<ValidationSchema>(x => x.FormId);
                entity.HasMany<SurveyResponse>(e => e.SurveyResponses).WithOne(e => e.Form).HasForeignKey(e => e.FormId).IsRequired();
                entity.Property(x => x.EntityData)
             .HasConversion(
                           (v) =>JsonConvert.SerializeObject(v,new JsonSerializerSettings { 
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                TypeNameHandling = TypeNameHandling.None
                           }),
                       v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v,
                                                                                      new JsonConverter[] {
                                                                                           new MGSurveyJsonConverter()
                                                                                      }))
                .Metadata.SetValueComparer(valueComparer);
            });

            modelBuilder.Entity<ValidationSchema>(entity =>
            {
                entity.ToTable<ValidationSchema>(storeOptions.ValidationSchemas);
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasMaxLength(36).HasColumnType("varchar(36)").IsRequired();
                entity.Property(x => x.FormId).HasMaxLength(36).HasColumnType("varchar(36)");
                entity.Property(x => x.CreatedBy).HasMaxLength(256).HasColumnType("varchar(256)").IsRequired();
                entity.Property(x => x.CreatedDate).IsRequired();
                entity.Property(x => x.UpdatedBy).HasMaxLength(256).HasColumnType("varchar(256)");
                // apparently anything over 4K converts to nvarchar(max) on SqlServer
                entity.Property(x => x.EntityData).HasMaxLength(50000).IsRequired();
                entity.Property(x => x.EntityData)
               .HasConversion(
                            (v) => JsonConvert.SerializeObject(v, new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                TypeNameHandling = TypeNameHandling.None
                            }),
                        v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v,
                                                                                      new JsonConverter[] {
                                                                                           new MGSurveyJsonConverter()
                                                                                      }))
                .Metadata.SetValueComparer(valueComparer);
            });
            modelBuilder.Entity<SurveyResponse>(entity =>
            {
                entity.ToTable<SurveyResponse>(storeOptions.SurveyResponses);
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasMaxLength(36).HasColumnType("varchar(36)").IsRequired();
                entity.Property(x => x.FormId).HasMaxLength(36).HasColumnType("varchar(36)").IsRequired();
                entity.Property(x => x.CreatedBy).HasMaxLength(256).HasColumnType("varchar(256)").IsRequired();
                entity.Property(x => x.CreatedDate).IsRequired();
                entity.Property(x => x.UpdatedBy).HasMaxLength(256).HasColumnType("varchar(256)");
                // apparently anything over 4K converts to nvarchar(max) on SqlServer
                entity.Property(x => x.EntityData).HasMaxLength(50000).IsRequired();
                entity.Property(x => x.EntityData)
               .HasConversion(
                            (v) => JsonConvert.SerializeObject(v, new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                TypeNameHandling = TypeNameHandling.None
                            }),
                        v => JsonConvert.DeserializeObject<Dictionary<string, object>>(v,
                                                                                      new JsonConverter[] {
                                                                                           new MGSurveyJsonConverter()
                                                                                      }))
                .Metadata.SetValueComparer(valueComparer);
            });
            modelBuilder.Entity<FormType>(entity =>
            {
                entity.ToTable<FormType>(storeOptions.FormTypes);
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasMaxLength(36).HasColumnType("varchar(36)").IsRequired();
                entity.Property(x => x.Name).HasMaxLength(255).HasColumnType("varchar(255)").IsRequired();
                entity.Property(x => x.SecretKey).HasMaxLength(255).HasColumnType("varchar(255)").IsRequired();
          
            });
        }

    }
}
