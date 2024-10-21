using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.EntityFrameworkCore;

using DigitalPlatform.CirculationClient;

#if REMOVED
namespace dp2Circulation.OperLog
{
    public class OperLogRecord
    {
        public DateTime OperTime { get; set; }
        public string Date { get; set; }
        public long Index { get; set; }
        public string Operation { get; set; }
        public string Action { get; set; }
        public string OldRecPath { get; set; }
        public string RecPath { get; set; }
    }

    public class OperLogContext : DbContext
    {
        public DbSet<OperLogRecord> Records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string filePath = Path.Combine(Program.MainForm.UserTempDir, "~operlog_items.db");
            optionsBuilder
                .UseSqlite($"Data Source={filePath};");
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EntityItem>().ToTable("items");
            modelBuilder.Entity<EntityItem>(entity =>
            {
                entity.HasKey(x => x.OperTime);
                entity.HasKey(x => x.RecPath);
                entity.HasKey(x => x.OldRecPath);
                // entity.HasIndex(e => e.PII);
            });
        }
    }
}
#endif
