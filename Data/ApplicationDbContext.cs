using Microsoft.EntityFrameworkCore;
using VillaCommunityManagement.Models;

namespace VillaCommunityManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Owner> Owners { get; set; }

        public DbSet<Maintenance> Maintenances { get; set; }

        public DbSet<Income> Incomes { get; set; }

        public DbSet<Expenditure> Expenditures { get; set; }

        public DbSet<AdminLogin> AdminLogins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Owner>().ToTable("V_Owners");

            modelBuilder.Entity<Maintenance>().ToTable("Maintenance");

            modelBuilder.Entity<Income>().ToTable("Income");

            modelBuilder.Entity<Expenditure>().ToTable("Expenditure");

            modelBuilder.Entity<AdminLogin>().ToTable("AdminLogin");

            modelBuilder.Entity<Income>()
    .Property(e => e.Amount)
    .HasPrecision(18, 2);

            modelBuilder.Entity<Expenditure>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Maintenance>()
                .Property(e => e.paid)
                .HasPrecision(18, 2);
        }
    }
}