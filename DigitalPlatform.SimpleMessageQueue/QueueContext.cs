using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace DigitalPlatform.SimpleMessageQueue
{
    public class QueueItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string GroupID { get; set; }

        public DateTime CreateTime { get; set; }

        public byte [] Content { get; set; }
    }

    public class QueueContext : DbContext
    {
        string _databaseFileName = null;

        public QueueContext(string databaseFileName)
        {
            _databaseFileName = databaseFileName;
        }

        // 书目摘要
        public DbSet<QueueItem> Items { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string filePath = _databaseFileName;
            optionsBuilder
                .UseSqlite($"Data Source={filePath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueueItem>().ToTable("queue");
            modelBuilder.Entity<QueueItem>(entity =>
            {
                entity.HasKey(e => new { e.ID });
            });
        }
    }

}
