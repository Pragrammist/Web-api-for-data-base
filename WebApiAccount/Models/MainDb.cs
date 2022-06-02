using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using UserAccountsDataBaseWebApi;


namespace WebApiAccount.Models
{
    public class MainDb : DbContext
    {
        public object GetTableByType<T>() where T: class
        {
            
            if (Users.EntityType.ClrType == typeof(T))
            {
                return Users;
            }
            else if (Avatars.EntityType.ClrType == typeof(T))
            {
                return Avatars;
            }
            else if(Bills.EntityType.ClrType == typeof(T))
            {
                return Bills;
            }
            else if (Guilds.EntityType.ClrType == typeof(T))
            {
                return Guilds;
            }
            else if (Orders.EntityType.ClrType == typeof(T))
            {
                return Orders;
            }
            else if (Products.EntityType.ClrType == typeof(T))
            {
                return Products;
            }
            else if (Reports.EntityType.ClrType == typeof(T))
            {
                return Reports;
            }
            else
            {
                return null;
            }

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Avatar> Avatars { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<OrderAndProduct> OrderAndProducts { get; set; }
        private void OverallConstructor()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        public MainDb()
        {
            OverallConstructor();
        }
        public MainDb(DbContextOptions<MainDb> options) : base(options)
        {
            OverallConstructor();   
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=WebApiUsersDb2;Trusted_Connection=True;").UseLazyLoadingProxies();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            // использование Fluent API
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasOne(u => u.Avatar).WithOne(a => a.User).HasForeignKey<User>(a => a.AvatarId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(u => u.Reports).WithOne(r => r.User).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<User>().HasOne(u => u.Bill).WithOne(b => b.User).HasForeignKey<User>(b => b.BillId).OnDelete(DeleteBehavior.SetNull); ;
            modelBuilder.Entity<User>().HasOne(u => u.Guild).WithMany(g => g.Users).HasForeignKey(u => u.GuildId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Guild>().HasOne(g => g.Bill).WithOne(b => b.Guild).HasForeignKey<Bill>(b => b.GuildId).OnDelete(DeleteBehavior.SetNull); ;
            

            



            modelBuilder.Entity<Bill>().HasMany(b => b.Orders).WithOne(o => o.Bill).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>().HasMany(o => o.Products).WithMany(p => p.Orders)
                .UsingEntity<OrderAndProduct>(j => 
                j.HasOne(p => p.Product).WithMany(o => o.OrderAndProducts).HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.SetNull), 
                j => 
                j.HasOne(o=> o.Order).WithMany(o => o.OrderAndProducts).HasForeignKey(o=> o.OrderId).OnDelete(DeleteBehavior.SetNull));
        }
    }
}
