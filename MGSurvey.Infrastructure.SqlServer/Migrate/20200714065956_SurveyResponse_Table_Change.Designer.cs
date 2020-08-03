﻿// <auto-generated />
using System;
using MGSurvey.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MGSurvey.Infrastructure.SqlServer.Migrate
{
    [DbContext(typeof(MGSurveyDbContext))]
    [Migration("20200714065956_SurveyResponse_Table_Change")]
    partial class SurveyResponse_Table_Change
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("odata")
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MGSurvey.Domain.Entities.Form", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(36)")
                        .HasMaxLength(36);

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("varchar(256)")
                        .HasMaxLength(256);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("EntityData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(50000);

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(250)")
                        .HasMaxLength(250);

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("varchar(25)")
                        .HasMaxLength(25);

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("varchar(256)")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Forms");
                });

            modelBuilder.Entity("MGSurvey.Domain.Entities.SurveyResponse", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(36)")
                        .HasMaxLength(36);

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("varchar(256)")
                        .HasMaxLength(256);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("EntityData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(50000);

                    b.Property<string>("FormId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("varchar(256)")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SurveyResponses");
                });

            modelBuilder.Entity("MGSurvey.Domain.Entities.ValidationSchema", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(36)")
                        .HasMaxLength(36);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("varchar(256)")
                        .HasMaxLength(256);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("EntityData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasMaxLength(50000);

                    b.Property<string>("FormId")
                        .HasColumnType("varchar(36)")
                        .HasMaxLength(36);

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(250)")
                        .HasMaxLength(250);

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("varchar(20)")
                        .HasMaxLength(20);

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("varchar(256)")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("FormId")
                        .IsUnique()
                        .HasFilter("[FormId] IS NOT NULL");

                    b.ToTable("ValidationSchemas");
                });

            modelBuilder.Entity("MGSurvey.Domain.Entities.ValidationSchema", b =>
                {
                    b.HasOne("MGSurvey.Domain.Entities.Form", "Form")
                        .WithOne("ValidationSchema")
                        .HasForeignKey("MGSurvey.Domain.Entities.ValidationSchema", "FormId");
                });
#pragma warning restore 612, 618
        }
    }
}
