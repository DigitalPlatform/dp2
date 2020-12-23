using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DigitalPlatform.WPF;

namespace dp2SSL
{
    public class RequestItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        // 操作者 ID。为读者证条码号，或者 ~工作人员账户名
        public string OperatorID { get; set; }  // 从 Operator 而来

        // 同步操作时间。最后一次同步操作的时间
        public DateTime SyncOperTime { get; set; }

        // 相关操作的 ID 信息。当 Action 为 "borrow" 时，这里用作对应的还书操作的 ID。也就是说凡是这个字段为空表示尚未还书
        public string LinkID { get; set; }

        public string PII { get; set; } // PII 单独从 EntityString 中抽取出来，便于进行搜索

        public string Action { get; set; }  // borrow/return/transfer

        public DateTime OperTime { get; set; }  // 操作时间。这是首次操作时间，然后重试同步的时候并不改变这个时间
        public string State { get; set; }   // 状态。sync/commerror/normalerror/空
                                            // 表示是否完成同步，还是正在出错重试同步阶段，还是从未同步过
        public string SyncErrorInfo { get; set; }   // 最近一次同步操作的报错信息
        public string SyncErrorCode { get; set; }   // 最近一次同步操作的错误码
        public int SyncCount { get; set; }

        // public Operator Operator { get; set; }  // 提起请求的读者

        // Operator 对象 JSON 化以后的字符串
        public string OperatorString { get; set; }

        //public Entity Entity { get; set; }
        // Entity 对象 JSON 化以后的字符串
        public string EntityString { get; set; }

        public string TransferDirection { get; set; } // in/out 典藏移交的方向
        public string Location { get; set; }    // 所有者馆藏地。transfer 动作会用到
        public string CurrentShelfNo { get; set; }  // 当前架号。transfer 动作会用到
        public string BatchNo { get; set; } // 批次号。transfer 动作会用到。建议可以用当前用户名加上日期构成

        // 2020/4/27
        // 借阅信息
        public string ActionString { get; set; }
    }

#if NO
    public class RequestItem
    {
        // public Operator Operator { get; set; }  // 提起请求的读者
        public string PatronName { get;set }
        public string PatronBarcode { get; set; }

        // public Entity Entity { get; set; }
        public string UID { get; set; }
        public string ReaderName { get; set; }
        public string Antenna { get; set; }
        public string PII { get; set; }
        public string ItemRecPath { get; set; }
        public string Title { get; set; }
        public string ItemLocation { get; set; }
        public string ItemCurrentLocation { get; set; }
        public string ShelfNo { get; set; }
        public string State { get; set; }

        public string Action { get; set; }  // borrow/return/transfer
        public string TransferDirection { get; set; } // in/out 典藏移交的方向
        public string Location { get; set; }    // 所有者馆藏地。transfer 动作会用到
        public string CurrentShelfNo { get; set; }  // 当前架号。transfer 动作会用到
        public string BatchNo { get; set; } // 批次号。transfer 动作会用到。建议可以用当前用户名加上日期构成
    }
#endif

    public class RequestContext : DbContext
    {
        // 滞留的请求
        public DbSet<RequestItem> Requests { get; set; }

#if NO
        // 操作日志
        public DbSet<Operation> Operations { get; set; }
#endif

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string filePath = Path.Combine(WpfClientInfo.UserDir, "actions.db");
            optionsBuilder
                .UseSqlite($"Data Source={filePath};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RequestItem>().ToTable("requests");
            modelBuilder.Entity<RequestItem>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.OperTime);
                entity.HasIndex(e => e.PII);
                // 2020/4/27
                entity.HasIndex(e => e.OperatorID);
                // entity.Property(e => e.Name).IsRequired();
            });
            /*
            modelBuilder.Entity<RequestItem>().Property(p => p.ID)
.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
*/

#if NO
            modelBuilder.Entity<Operation>().ToTable("operations");
            modelBuilder.Entity<Operation>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.OperTime);
                entity.HasIndex(e => e.PII);
                // entity.Property(e => e.Name).IsRequired();
            });
#endif
        }
    }
}