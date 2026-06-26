using Diplom_CRM.Data.Entities;
using Diplom_CRM.Data.Enums;
using Diplom_CRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Activity = Diplom_CRM.Data.Entities.Activity;

namespace Diplom_CRM.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole, string>
    {
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Activity> Activities { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureContact(modelBuilder);
            ConfigureCompany(modelBuilder);
            ConfigureDeal(modelBuilder);
            ConfigureActivity(modelBuilder);
            ConfigureIdentity(modelBuilder);
        }

        private void ConfigureContact(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Contact>();

            entity.ToTable("Contacts");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.LastName)
                .IsRequired(false)
                .HasMaxLength(100);

            entity.Property(c => c.Email)
                .IsRequired(false)
                .HasMaxLength(100);

            entity.HasIndex(c => c.Email)
                .IsUnique()
                .HasFilter("\"Email\" IS NOT NULL");

            entity.Property(c => c.Phone)
                .IsRequired(false)
                .HasMaxLength(20);

            entity.Property(c => c.Company)
                .IsRequired(false)
                .HasMaxLength(100);

            entity.Property(c => c.Position)
                .IsRequired(false)
                .HasMaxLength(100);

            entity.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.Property(c => c.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.HasIndex(c => new { c.FirstName, c.LastName })
                .HasDatabaseName("IX_Contacts_FullName");
        }

        private void ConfigureCompany(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Company>();

            entity.ToTable("Companies");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(c => c.Name)
                .IsUnique();

            entity.Property(c => c.Industry)
                .IsRequired(false)
                .HasMaxLength(100);

            entity.Property(c => c.Website)
                .IsRequired(false)
                .HasMaxLength(200);

            entity.Property(c => c.Phone)
                .IsRequired(false)
                .HasMaxLength(20);

            entity.Property(c => c.Address)
                .IsRequired(false)
                .HasMaxLength(200);

            entity.Property(c => c.City)
                .IsRequired(false)
                .HasMaxLength(100);

            entity.Property(c => c.Country)
                .IsRequired(false)
                .HasMaxLength(100);

            entity.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.HasIndex(c => new { c.City, c.Country })
                .HasDatabaseName("IX_Companies_Location");
        }

        private void ConfigureDeal(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Deal>();

            entity.ToTable("Deals");
            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(d => d.Amount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            entity.Property(d => d.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(StatusEnum.New);

            entity.Property(d => d.ExpectedCloseDate)
                .IsRequired();

            entity.Property(d => d.ActualCloseDate)
                .IsRequired(false);

            entity.Property(d => d.Description)
                .IsRequired(false)
                .HasMaxLength(1000);

            entity.Property(d => d.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.Property(d => d.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.Property(d => d.ContactId)
                .IsRequired();

            entity.HasIndex(d => d.Status);
            entity.HasIndex(d => d.ExpectedCloseDate);
            entity.HasIndex(d => d.ContactId);
            entity.HasIndex(d => new { d.Status, d.ExpectedCloseDate })
                .HasDatabaseName("IX_Deals_Status_ByDate");

            entity.HasOne(d => d.Contact)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureActivity(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Activity>();

            entity.ToTable("Activities");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Type)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(a => a.Subject)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.Description)
                .IsRequired(false)
                .HasMaxLength(1000);

            entity.Property(a => a.ScheduledDate)
                .IsRequired();

            entity.Property(a => a.CompletedDate)
                .IsRequired(false);

            entity.Property(a => a.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(a => a.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.Property(a => a.ContactId)
                .IsRequired();

            entity.Property(a => a.DealId)
                .IsRequired(false);

            entity.HasIndex(a => a.ScheduledDate);
            entity.HasIndex(a => a.ContactId);
            entity.HasIndex(a => a.DealId);
            entity.HasIndex(a => a.Type);
            entity.HasIndex(a => new { a.IsCompleted, a.ScheduledDate })
                .HasDatabaseName("IX_Activities_Uncompleted_ByDate")
                .HasFilter("\"IsCompleted\" = false");

            entity.HasOne(a => a.Contact)
                .WithMany(c => c.Activities)
                .HasForeignKey(a => a.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Deal)
                .WithMany(d => d.Activities)
                .HasForeignKey(a => a.DealId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        private void ConfigureIdentity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasOne(c => c.AppUser)
                .WithMany(u => u.Companies)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Deal>()
                .HasOne(d => d.AppUser)
                .WithMany(u => u.Deals)
                .HasForeignKey(d => d.AppUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<Activity>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var activity = entry.Entity;
                    if (activity.ScheduledDate == default || activity.ScheduledDate == DateTime.MinValue)
                        activity.ScheduledDate = now;
                    else if (activity.ScheduledDate.Kind != DateTimeKind.Utc)
                        activity.ScheduledDate = DateTime.SpecifyKind(activity.ScheduledDate, DateTimeKind.Utc);

                    if (activity.CompletedDate.HasValue)
                    {
                        var cd = activity.CompletedDate.Value;
                        if (cd == default || cd == DateTime.MinValue)
                            activity.CompletedDate = null;
                        else if (cd.Kind != DateTimeKind.Utc)
                            activity.CompletedDate = DateTime.SpecifyKind(cd, DateTimeKind.Utc);
                    }
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
