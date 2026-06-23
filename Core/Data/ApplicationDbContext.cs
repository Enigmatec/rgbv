using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace Core.Data
{
    public class ApplicationDbContext : IdentityDbContext<
        ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Organisation> Organisations { get; set; }

        public DbSet<CaseCategory> CaseCategories { get; set; }

        public DbSet<Case> Cases { get; set; }

        public DbSet<FollowUp> FollowUps { get; set; }

        public DbSet<State> States { get; set; }

        public DbSet<LocalGovernmentArea> LocalGovernmentAreas { get; set; }

        public DbSet<Entry> Entries { get; set; }

        public DbSet<Ward> Wards { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<FileUpload> FilesUploads { get; set; }

        public DbSet<ServiceProvided> ServicesProvided { get; set; }

        public DbSet<ComplaintForm> ComplaintForms { get; set; }
        public DbSet<Donor> Donors { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            CreateEnumAsStrings(builder);

            AddNonClusteredIndex(builder);

            //builder.Entity<ApplicationUser>().OwnsOne(r => r.LocalGovernments, build =>
            //{
            //    build.Property("LocalGovernmentAreas").HasColumnName("LocalGovernmentAreas");
            //});

            builder.Entity<ApplicationUser>().HasMany(c => c.CreatedCases)
                .WithOne(c => c.CreatedBy);
            builder.Entity<ApplicationUser>().HasMany(c => c.ApprovedCases)
                .WithOne(c => c.ApprovedBy);
            builder.Entity<ApplicationUser>().HasMany(c => c.ValidatedCases)
                .WithOne(c => c.ValidatedBy);

            builder.Entity<ApplicationUser>().HasQueryFilter(c => c.IsSoftDeleted == false);

            builder.Entity<Organisation>().HasQueryFilter(c => c.IsSoftDeleted == false);

            //builder.Entity<Organisation>().Property(c => c.OrganisationType).HasConversion(c => c.ToString(), c => Enum.Parse<OrganisationType>(c));

            //builder.Entity<Case>().Property(c => c.IsSurviorContinuousThreat).HasConversion(c => c.ToString(), c => Enum.Parse<YesOrNo>(c));
            //builder.Entity<Case>().Property(c => c.HasCaseBeenClosed).HasConversion(c => c.ToString(), c => Enum.Parse<YesOrNo>(c));

            //builder.Entity<Case>().Property(c => c.WasViolenceFatal).HasConversion(c => c.ToString(), c => Enum.Parse<YesOrNo>(c));
            //builder.Entity<Case>().Property(c => c.HasSurviorReceivedService).HasConversion(c => c.ToString(), c => Enum.Parse<YesOrNo>(c));
            //builder.Entity<Case>().Property(c => c.DoestheSurviorWantJustice).HasConversion(c => c.ToString(), c => Enum.Parse<YesOrNo>(c));
            //builder.Entity<Case>().Property(c => c.TimeOfDay).HasConversion(c => c.ToString(), c => Enum.Parse<TimeOfDay>(c));
            //builder.Entity<Case>().Property(c => c.DoesSurviorLiveAlone).HasConversion(c => c.ToString(), c => Enum.Parse<YesOrNo>(c));

            //builder.Entity<FollowUp>().Property(c => c.HasClientReceivedjustice).HasConversion(c => c.ToString(), c => Enum.Parse<YesOrNo>(c));
            //builder.Entity<FollowUp>().Property(c => c.HasCaseBeenClosed).HasConversion(c => c.ToString(), c => Enum.Parse<YesOrNo>(c));

            //builder.Entity<Entry>().Property(c => c.Type).HasConversion(c => c.ToString(), c => Enum.Parse<EntryType>(c));
            //builder.Entity<Entry>().Property(c => c.Field).HasConversion(c => c.ToString(), c => Enum.Parse<Field>(c));

            // builder.Entity<ApplicationRole>().HasData(
            //new ApplicationRole()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "Administrator",
            //    NormalizedName = "ADMINISTRATOR",
            //    Description = "Population council super administrator with highest control rights"
            //},
            //new ApplicationRole()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "Organisation Supervisor",
            //    NormalizedName = "ORGANISATION SUPERVISOR",
            //    Description = "Supervisor with rights to data of an organisation"
            //},

            //new ApplicationRole()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "State Supervisor",
            //    NormalizedName = "STATE SUPERVISOR",
            //    Description = "Supervisor with rights to data of just one state"
            //},
            //new ApplicationRole()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "CSO",
            //    NormalizedName = "CSO",
            //    Description = "Civil Society Organisation"
            //},
            //new ApplicationRole()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "Service Provider",
            //    NormalizedName = "SERVICE PROVIDER",
            //    Description = "Service Provider"
            //},

            //new ApplicationRole()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "LGA Supervisor",
            //    NormalizedName = "LGA SUPERVISOR",
            //    Description = "Supervisor with rights to data of just one lga"
            //},
            // new ApplicationRole()
            // {
            //     Id = Guid.NewGuid().ToString(),
            //     Name = "Implementing Partner",
            //     NormalizedName = "IMPLEMENTING PARTNER",
            //     Description = "View only access to organisation's data"
            // }
            //  );
        }

        /// <summary>
        /// Converts enums to string
        /// </summary>
        /// <param name="builder"></param>
        private static void CreateEnumAsStrings(ModelBuilder builder)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var nullable = Nullable.GetUnderlyingType(property.ClrType);

                    if (property.ClrType.BaseType == typeof(Enum))
                    {
                        var type = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
                        var converter = Activator.CreateInstance(type, new ConverterMappingHints()) as ValueConverter;

                        property.SetValueConverter(converter);
                    }
                    else if (nullable != null && nullable.IsEnum)

                    {
                        var type = typeof(EnumToStringConverter<>).MakeGenericType(nullable);
                        var converter = Activator.CreateInstance(type, new ConverterMappingHints()) as ValueConverter;

                        property.SetValueConverter(converter);
                    }
                }
            }
        }

        private void AddNonClusteredIndex(ModelBuilder builder)
        {
            //cases
            builder.Entity<Case>().HasIndex(e => e.TimeOfDay).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.IncidentCode).IsUnique().IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.HasCaseBeenClosed).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.SexOfSurvior).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.VulnerablePopulation).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.DateReported).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.DateOfIncident).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.SerialNumber).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.ValidatedAt).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.LgaValidatedAt).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.ApprovedAt).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.CreatedAt).IsUnique(false).IsClustered(false);
            builder.Entity<Case>().HasIndex(e => e.ModifiedAt).IsUnique(false).IsClustered(false);

            //org
            builder.Entity<Organisation>().HasIndex(e => e.OrganisationType).IsUnique(false).IsClustered(false);
            builder.Entity<Organisation>().HasIndex(e => e.Name).IsUnique().IsClustered(false);
            builder.Entity<Organisation>().HasIndex(e => e.Code).IsUnique().IsClustered(false);
            builder.Entity<Organisation>().HasIndex(e => e.CreatedAt).IsUnique(false).IsClustered(false);
            builder.Entity<Organisation>().HasIndex(e => e.ModifiedAt).IsUnique(false).IsClustered(false);

            //users
            builder.Entity<ApplicationUser>().HasIndex(c => c.Code).IsUnique().IsClustered(false);
            builder.Entity<ApplicationUser>().HasIndex(e => e.Type).IsUnique(false).IsClustered(false);
            builder.Entity<ApplicationUser>().HasIndex(e => e.Email).IsUnique().IsClustered(false);
            builder.Entity<ApplicationUser>().HasIndex(e => e.Designation).IsUnique(false).IsClustered(false);
            builder.Entity<ApplicationUser>().HasIndex(e => e.CreatedAt).IsUnique(false).IsClustered(false);
            builder.Entity<ApplicationUser>().HasIndex(e => e.ModifiedAt).IsUnique(false).IsClustered(false);
            builder.Entity<ApplicationUser>().Property(o => o.Email).HasMaxLength(100);

            //services
            builder.Entity<ServiceProvided>().HasIndex(e => e.ServiceProvisionCode).IsUnique().IsClustered(false);
            builder.Entity<ServiceProvided>().HasIndex(e => e.IncidentCode).IsUnique(false).IsClustered(false);
            builder.Entity<ServiceProvided>().HasIndex(e => e.SexOfSurvivorOrVictim).IsUnique(false).IsClustered(false);
            builder.Entity<ServiceProvided>().HasIndex(e => e.CreatedAt).IsUnique(false).IsClustered(false);
            builder.Entity<ServiceProvided>().HasIndex(e => e.ModifiedAt).IsUnique(false).IsClustered(false);
        }
    }
}