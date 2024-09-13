
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Final_Demo.Model
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.price)
               .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Item>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            // Define relationships
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict); // or DeleteBehavior.SetNull

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Item)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(oi => oi.ItemId)
                .OnDelete(DeleteBehavior.Restrict); // or DeleteBehavior.SetNull

             
            modelBuilder.Entity<CartItem>()
            .HasKey(ci => new { ci.CartId, ci.ItemId });

            modelBuilder.Entity<OrderItem>()
            .HasKey(oi => new { oi.OrderId, oi.ItemId });

            //modelBuilder.Entity<OrderItem>()
            //.HasOne(oi => oi.Cart)
            //.WithMany(c => c.Items)
            //.HasForeignKey(oi => oi.CartId);
            //Hotel Relationship
            

            modelBuilder.Entity<Store>()
             .HasOne(h => h.Country)
            .WithMany()
            .HasForeignKey(h => h.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
            .HasOne(h => h.State)
            .WithMany()
             .HasForeignKey(h => h.StateId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
            .HasOne(h => h.City)
            .WithMany()
            .HasForeignKey(h => h.CityId)
            .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }       
        public DbSet<Logger> Loggers { get; set; }
        public DbSet<Country> Countrys {  get; set; }
        public DbSet<State>States { get; set; }
        public DbSet <City>Citys { get; set; }
    }
}
