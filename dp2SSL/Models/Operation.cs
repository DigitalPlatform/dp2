using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2SSL
{
    public class Operation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string Action { get; set; }  // inventory/checkout/checkin/patron/opendoor/closedoor
        public string UID { get; set; } // RFID 标签的 UID
        public string PII { get; set; } // RFID 标签的 PII
        public string Title { get; set; }   // 书名，或者读者姓名
        public string Parameter { get; set; }   // 操作的附加参数

        public DateTime OperTime { get; set; }  // 操作时间
        public string Operator { get; set; }    // 操作者
    }
}
