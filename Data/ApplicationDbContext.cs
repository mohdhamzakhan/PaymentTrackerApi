using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PaymentTrackerApi.Models;

namespace PaymentTrackerApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CampaignLog> CampaignLogs => Set<CampaignLog>();
        public DbSet<PaymentDetail> PaymentDetails => Set<PaymentDetail>();
        public DbSet<TrainBillDetail> TrainBillDetails => Set<TrainBillDetail>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PaymentDetail>(entity =>
            {
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");

                // Indexes so the "search by phone / UTR" lookups are fast.
                entity.HasIndex(p => p.PhoneNumber);
                entity.HasIndex(p => p.UtrNumber);

                entity.HasOne(p => p.CampaignLog)
                      .WithMany(c => c.PaymentDetails)
                      .HasForeignKey(p => p.CampaignLogId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<CampaignLog>(entity =>
            {
                entity.HasIndex(c => c.Destination);
            });

            builder.Entity<TrainBillDetail>(entity =>
            {
                entity.Property(t => t.TotalInvoiceAmount).HasColumnType("decimal(18,2)");
                entity.HasIndex(t => t.BillNumber);
                entity.HasIndex(t => t.TrainNumber);
                entity.HasIndex(t => t.ManagerMobileNo);
            });
        }
    }
}
