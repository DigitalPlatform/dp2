using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    /// <summary>
    /// RFID 功能接口
    /// </summary>
    public interface IRfid
    {
        // 列出当前可用的 reader
        ListReadersResult ListReaders();

        InventoryResult Inventory(string reader_name);

        GetTagInfoResult GetTagInfo(string reader_name, string uid);

        NormalResult WriteTagInfo(
    string reader_name,
    TagInfo old_tag_info,
    TagInfo new_tag_info);

    }

    [Serializable()]
    public class ListReadersResult : NormalResult
    {
        public string[] Readers { get; set; }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append(base.ToString() + "\r\n");
            if (Readers != null)
            {
                int i = 1;
                foreach (string reader in Readers)
                {
                    text.Append($"{i}) {reader.ToString()}\r\n");
                    i++;
                }
            }
            return text.ToString();
        }
    }

}
