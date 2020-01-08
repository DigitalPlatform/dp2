using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public class LibraryContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Biblio> Biblios { get; set; }

        DatabaseConfig _config = null;

        public LibraryContext(DatabaseConfig config)
        {
            _config = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseMySQL("server=localhost;database=library;user=user;password=password");
            optionsBuilder.UseMySql(_config.BuildConnectionString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.ItemRecPath);
                // entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Biblio>(entity =>
            {
                entity.HasKey(e => e.RecPath);
                /*
                entity.Property(e => e.Title).IsRequired();
                entity.HasOne(d => d.Publisher)
                  .WithMany(p => p.Books);
                  */
            });
        }
    }

    public class Item
    {
        public string ItemRecPath { get; set; }
        public string ItemBarcode { get; set; }
        public string Location { get; set; }
        public string AccessNo { get; set; }
        public string BiblioRecPath { get; set; }

        public DateTime CreateTime { get; set; }
        public string State { get; set; }

        public long Price { get; set; }
        public string Unit { get; set; }

        public string Borrower { get; set; }
        public DateTime BorrowTime { get; set; }
        public string BorrowPeriod { get; set; }
        public DateTime ReturningTime { get; set; }   // 预计还回时间
    }

    public class Biblio
    {
        public string RecPath { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }

    public class DatabaseConfig
    {
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }

        public string BuildConnectionString()
        {
            return $"server={ServerName};database={DatabaseName};user={UserName};password={Password}";
        }
    }
}
