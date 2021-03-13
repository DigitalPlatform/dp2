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
    public class AccountItem
    {
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public string Rights { get; set; }
        public string LibraryCodeList { get; set; }
        public string Access { get; set; }
    }

    // 工作人员账户本地缓存数据库
    public class AccountCacheContext : DbContext
    {
        // 账户
        public DbSet<AccountItem> Accounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string filePath = Path.Combine(WpfClientInfo.UserDir, "account.db");
            optionsBuilder
                .UseSqlite($"Data Source={filePath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ***
            modelBuilder.Entity<AccountItem>().ToTable("account");
            modelBuilder.Entity<AccountItem>(entity =>
            {
                entity.HasKey(e => e.UserName);
            });
        }
    }
}
