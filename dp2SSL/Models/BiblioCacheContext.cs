using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using DigitalPlatform.WPF;

namespace dp2SSL
{
    public class BiblioSummaryItem
    {
        public string PII { get; set; }
        public string BiblioSummary { get; set; }
    }

    public class BiblioCacheContext : DbContext
    {
        // 书目摘要
        public DbSet<BiblioSummaryItem> BiblioSummaries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string filePath = Path.Combine(WpfClientInfo.UserDir, "biblio.db");
            optionsBuilder
                .UseSqlite($"Data Source={filePath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BiblioSummaryItem>().ToTable("summary");
            modelBuilder.Entity<BiblioSummaryItem>(entity =>
            {
                entity.HasKey(e => e.PII);
                // entity.HasIndex(e => e.PII);
            });
        }
    }
}
