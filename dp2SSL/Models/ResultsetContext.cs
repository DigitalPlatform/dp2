using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DigitalPlatform.WPF;

namespace dp2SSL
{
    // 结果集事项
    public class ResultsetItem
    {
        // 结果集名
        public string ResultsetName { get; set; }
        // 记录 ID
        public int ID { get; set; }
    }

    public class ResultsetContext : DbContext
    {
        // 结果集的集合
        public DbSet<ResultsetItem> Items { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string filePath = Path.Combine(WpfClientInfo.UserDir, "resultsets.db");
            optionsBuilder
                .UseSqlite($"Data Source={filePath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResultsetItem>().ToTable("resultsets");
            modelBuilder.Entity<ResultsetItem>(entity =>
            {
                entity.HasKey(e => new { e.ResultsetName, e.ID });
                // entity.HasIndex(e => e.ID);
            });
        }
    }

}
