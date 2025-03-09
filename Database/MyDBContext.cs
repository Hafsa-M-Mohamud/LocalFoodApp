using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Assignment3BAD.Models;

namespace Assignment3BAD.Database
{
    public class MyDBContext : IdentityDbContext<ApplicationUser>
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options) { }

        // DbSet for dine eksisterende modeller
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripStop> TripStops { get; set; }
        public DbSet<Cook> Cooks { get; set; }
        public DbSet<Cyclist> Cyclists { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CyclistStats> CyclistStats { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<RatingSystem> Ratings { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DishOrder> DishOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity Configuration
            base.OnModelCreating(modelBuilder);

            // Cook og ApplicationUser relation
            modelBuilder.Entity<Cook>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cook)
                .HasForeignKey<Cook>(c => c.UserId);

            // Cyclist og ApplicationUser relation
            modelBuilder.Entity<Cyclist>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cyclist)
                .HasForeignKey<Cyclist>(c => c.UserId);

            // RatingSystem configuration
            modelBuilder.Entity<RatingSystem>()
                .HasKey(rs => rs.RatingID);

            modelBuilder.Entity<CyclistStats>()
                .HasKey(cs => cs.CyclistStatsID);

            // Dish and Cook relationship
            modelBuilder.Entity<Dish>()
                .HasOne(d => d.Cook)
                .WithMany(c => c.Dishes)
                .HasForeignKey(d => d.CookID);

            // Configure the Price property in Dish
            modelBuilder.Entity<Dish>()
                .Property(d => d.Price)
                .HasColumnType("decimal(18,2)");

            // Customer and Order relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerID);

            // Order and DishOrder relationship
            modelBuilder.Entity<Order>()
                .HasMany(o => o.DishOrders)
                .WithOne(dishOrder => dishOrder.Order)
                .HasForeignKey(dishOrder => dishOrder.OrderID)
                .OnDelete(DeleteBehavior.Restrict);

            // Dish and DishOrder relationship
            modelBuilder.Entity<DishOrder>()
                .HasOne(dishOrder => dishOrder.Dish)
                .WithMany(d => d.DishOrders)
                .HasForeignKey(dishOrder => dishOrder.DishID)
                .OnDelete(DeleteBehavior.Restrict);

            // Cyclist and Trip relationship
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Cyclist)
                .WithMany(c => c.Trips)
                .HasForeignKey(t => t.CyclistID);

            // Corrected Trip and DeliveryType relationship
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Stops)
                .WithOne(dt => dt.Trip)
                .HasForeignKey(dt => dt.TripID);

            // Cyclist and CyclistStats relationship
            modelBuilder.Entity<CyclistStats>()
                .HasOne(cs => cs.Cyclist)
                .WithMany(c => c.CyclistStats)
                .HasForeignKey(cs => cs.CyclistID);

            // RatingSystem relationships with Cook, Customer, and Cyclist
            modelBuilder.Entity<RatingSystem>()
                .HasOne(rs => rs.Cook)
                .WithMany(c => c.RatingSystems)
                .HasForeignKey(rs => rs.CookID);

            modelBuilder.Entity<RatingSystem>()
                .HasOne(rs => rs.Customer)
                .WithMany(c => c.RatingSystems)
                .HasForeignKey(rs => rs.CustomerID);

            modelBuilder.Entity<RatingSystem>()
                .HasOne(rs => rs.Cyclist)
                .WithMany(c => c.RatingSystems)
                .HasForeignKey(rs => rs.CyclistID);
        }
    }
}
