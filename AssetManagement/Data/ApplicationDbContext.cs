using AssetManagement.Models.Entities;
using AssetManagement.Models.Enums;
using AssetManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace AssetManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
		
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { 
        
        }
		

		public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<Maintenance> Maintenances => Set<Maintenance>();
        public DbSet<Issue> Issues => Set<Issue>();
        public DbSet<Report> Reports => Set<Report>();

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<AssetRequest> AssetRequests => Set<AssetRequest>();


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Capture entries BEFORE saving
            var auditEntries = OnBeforeSaveChanges();

            var result = await base.SaveChangesAsync(cancellationToken);

            // Save the audit logs AFTER the main changes succeed
            if (auditEntries.Any())
            {
                AuditLogs.AddRange(auditEntries);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        private List<AuditLog> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditLog>();

            foreach (var entry in ChangeTracker.Entries())
            {
                // Skip auditing the AuditLog table itself to avoid infinite loops
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var audit = new AuditLog
                {
                    Action = entry.State.ToString().ToUpper(),
                    EntityName = entry.Entity.GetType().Name,
                    EntityID = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    OldValues = entry.State == EntityState.Added ? null :
                        JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)),
                    NewValues = entry.State == EntityState.Deleted ? null :
                        JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue))
                };
                auditEntries.Add(audit);
            }
            return auditEntries;
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>(b =>
            {
                b.ToTable("t_roles");
                b.HasKey(r => r.RoleID);
                b.Property(r => r.Name).IsRequired();
            });

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("t_users");
                b.HasKey(u => u.UserID);
                b.Property(u => u.Name).IsRequired();
                b.Property(u => u.Email).IsRequired();
                b.HasOne(u => u.Role)
                    .WithMany()
                    .HasForeignKey(u => u.RoleID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Property(u => u.Status).HasConversion(new EnumToStringConverter<UserStatus>());
            });

            modelBuilder.Entity<Category>(b =>
            {
                b.ToTable("t_categories");
                b.HasKey(c => c.CategoryID);
                b.Property(c => c.Name).IsRequired();
            });

            modelBuilder.Entity<Asset>(b =>
            {
                b.ToTable("t_assets");
                b.HasKey(a => a.AssetID);
                b.Property(a => a.Name).IsRequired();
                b.HasOne(a => a.Category)
                    .WithMany()
                    .HasForeignKey(a => a.CategoryID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Property(a => a.Status).HasConversion(new EnumToStringConverter<AssetStatus>());
                b.HasIndex(a => a.Tag).IsUnique().HasFilter("[Tag] IS NOT NULL");
            });

            modelBuilder.Entity<Assignment>(b =>
            {
                b.ToTable("t_assignments");
                b.HasKey(x => x.AssignmentID);

                b.HasOne(x => x.Asset)
                    .WithMany()
                    .HasForeignKey(x => x.AssetID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne(x => x.AssignedToUser)
                    .WithMany()
                    .HasForeignKey(x => x.AssignedToUserID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Property(x => x.Status).HasConversion(new EnumToStringConverter<AssignmentStatus>());
            });

            modelBuilder.Entity<Maintenance>(b =>
            {
                b.ToTable("t_maintenance");
                b.HasKey(m => m.MaintenanceID);
                b.HasOne(m => m.Asset)
                    .WithMany()
                    .HasForeignKey(m => m.AssetID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Property(m => m.Status).HasConversion(new EnumToStringConverter<MaintenanceStatus>());
            });

            modelBuilder.Entity<Issue>(b =>
            {
                b.ToTable("t_issues");
                b.HasKey(i => i.IssueID);
                b.HasOne(i => i.Asset)
                    .WithMany()
                    .HasForeignKey(i => i.AssetID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne(i => i.ReportedBy)
                    .WithMany()
                    .HasForeignKey(i => i.ReportedByUserID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Property(i => i.Status).HasConversion(new EnumToStringConverter<IssueStatus>());
            });

            modelBuilder.Entity<Report>(b =>
            {
                b.ToTable("t_reports");
                b.HasKey(r => r.ReportID);
            });
        }
    }
}
