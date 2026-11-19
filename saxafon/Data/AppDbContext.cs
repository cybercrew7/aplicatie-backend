using Microsoft.EntityFrameworkCore;
using saxafon.Models;

namespace saxafon.Data
{
    public class AppDbContext : DbContext
    {


        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Resources> Resources { get; set; } = null!;
        public DbSet<Rooms> Rooms { get; set; } = null!;
        public DbSet<Equipments> Equipments { get; set; } = null!;
        public DbSet<Reservations> Reservations { get; set; } = null!;



        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) {
                var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", false).AddUserSecrets<AppDbContext>().Build();
                var connectionString = builder.GetConnectionString("DefaultConnection");
                optionsBuilder.UseMySQL(connectionString);
            
            }
            base.OnConfiguring(optionsBuilder);
        }
         */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resources>().Property(e => e.status).HasConversion(v => v.ToString(), v => (Shared.Status)Enum.Parse(typeof(Shared.Status), v));
            modelBuilder.Entity<Reservations>().Property(e => e.status_rezervare).HasConversion(v => v.ToString(), v => (Shared.ReservationStatus)Enum.Parse(typeof(Shared.ReservationStatus), v));
            modelBuilder.Entity<User>().Property(e => e.role).HasConversion(v => v.ToString(), v => (Shared.Role)Enum.Parse(typeof(Shared.Role), v));

            // Reservations has two navigations to User (owner + approver), so the foreign keys must be explicit.
            modelBuilder.Entity<Reservations>()
                .HasOne(r => r.user)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservations>()
                .HasOne(r => r.approved_by_user)
                .WithMany()
                .HasForeignKey(r => r.approved_by_user_id)
                .OnDelete(DeleteBehavior.SetNull);
         
            base.OnModelCreating(modelBuilder);
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(25);
            base.ConfigureConventions(configurationBuilder);
        }
       

    }
}
