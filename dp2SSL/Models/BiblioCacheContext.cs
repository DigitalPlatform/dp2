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
        // UII，或者 PII
        public string PII { get; set; }

        public string BiblioSummary { get; set; }
    }

    // 册记录
    public class EntityItem
    {
        // UII，或者 PII
        public string PII { get; set; }

        public string RecPath { get; set; }
        public string Xml { get; set; }
        public byte[] Timestamp { get; set; }
    }

    public class PatronItem
    {
        public string PII { get; set; }
        // 绑定的 UID。形态如 ",xxxx,xxxx,xxxx,"，注意逗号的用法。检索时候用 like "%,xxxx,%"
        public string Bindings { get; set; }

        public string RecPath { get; set; }
        public string Xml { get; set; }
        public byte[] Timestamp { get; set; }

        // 2020/6/18
        public DateTime LastWriteTime { get; set; } // 最后更新时间
    }

    public class BiblioCacheContext : DbContext
    {
        // 书目摘要
        public DbSet<BiblioSummaryItem> BiblioSummaries { get; set; }
        // 册记录
        public DbSet<EntityItem> Entities { get; set; }
        // 读者记录
        public DbSet<PatronItem> Patrons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string filePath = Path.Combine(WpfClientInfo.UserDir, "biblio.db");
            optionsBuilder
                .UseSqlite($"Data Source={filePath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ***
            modelBuilder.Entity<BiblioSummaryItem>().ToTable("summary");
            modelBuilder.Entity<BiblioSummaryItem>(entity =>
            {
                entity.HasKey(e => e.PII);
                // entity.HasIndex(e => e.PII);
            });

            // ***
            modelBuilder.Entity<EntityItem>().ToTable("entity");
            modelBuilder.Entity<EntityItem>(entity =>
            {
                entity.HasKey(e => e.PII);
                // entity.HasIndex(e => e.PII);
            });

            // ***
            modelBuilder.Entity<PatronItem>().ToTable("patron");
            modelBuilder.Entity<PatronItem>(entity =>
            {
                entity.HasKey(e => e.PII);
                // entity.HasIndex(e => e.Bindings);
            });
        }
    }
}
