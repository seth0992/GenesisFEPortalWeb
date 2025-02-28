using GenesisFEPortalWeb.Models.Catalog;
using GenesisFEPortalWeb.Models.Entities.Audit;
using GenesisFEPortalWeb.Models.Entities.Billing;
using GenesisFEPortalWeb.Models.Entities.Catalog;
using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Core;
using GenesisFEPortalWeb.Models.Entities.Notifications;
using GenesisFEPortalWeb.Models.Entities.Reports;
using GenesisFEPortalWeb.Models.Entities.Security;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GenesisFEPortalWeb.Database.Data
{
    public class AppDbContext : DbContext
    {

        private readonly ITenantService _tenantService;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ITenantService tenantService) : base(options)
        {
            _tenantService = tenantService;
            Database.EnsureCreated();
        }

        // Core
        public DbSet<TenantModel> Tenants { get; set; }
        public DbSet<DigitalCertificateModel> DigitalCertificates { get; set; }
        public DbSet<ApiConfigurationModel> ApiConfigurations { get; set; }

        // Security
        public DbSet<UserModel> Users { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
        public DbSet<SecretModel> Secrets { get; set; }

        // Catalog
        public DbSet<RegionModel> Regions { get; set; }
        public DbSet<ProvinceModel> Provinces { get; set; }
        public DbSet<CantonModel> Cantons { get; set; }
        public DbSet<DistrictModel> Districts { get; set; }
        public DbSet<DocumentTypeModel> DocumentTypes { get; set; }
        public DbSet<TaxTypeModel> TaxTypes { get; set; }
        public DbSet<PaymentMethodModel> PaymentMethods { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<IdentificationTypeModel> IdentificationTypes { get; set; }

        // Billing
        public DbSet<CustomerModel> Customers { get; set; }
        public DbSet<CustomerExonerationModel> CustomerExonerations { get; set; }
        public DbSet<InvoiceModel> Invoices { get; set; }
        public DbSet<InvoiceLineModel> InvoiceLines { get; set; }
        public DbSet<InvoiceStatusModel> InvoiceStatuses { get; set; }

        // Reports
        public DbSet<ReportConfigurationModel> ReportConfigurations { get; set; }
        public DbSet<GeneratedReportModel> GeneratedReports { get; set; }

        // Notifications
        public DbSet<TemplateModel> NotificationTemplates { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<NotificationDeliveryModel> NotificationDeliveries { get; set; }

        // Audit
        public DbSet<ActivityLogModel> ActivityLogs { get; set; }
        // public DbSet<SecurityLogModel> SecurityLogs { get; set; }
        public DbSet<SecurityLogModel> SecurityLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<SecurityLogModel>(entity =>
            {
                entity.Property(e => e.EventType)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.Property(e => e.SecurityStamp)
                    .HasMaxLength(450); // O el tamaño que prefieras

                entity.Property(e => e.LastPasswordChangeDate)
                    .IsRequired(false);

                entity.Property(e => e.LastSuccessfulLogin)
                    .IsRequired(false);
            });


            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, "TenantId");
                    var value = Expression.Constant(_tenantService.GetCurrentTenantId());
                    var body = Expression.Equal(property, value);
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            // Aplicar filtro global por tenant
            //modelBuilder.Entity<CustomerModel>()
            //    .HasQueryFilter(x => x.TenantId == _tenantService.GetCurrentTenantId());

        }

    }

}
