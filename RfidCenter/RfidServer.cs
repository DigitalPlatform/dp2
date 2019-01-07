using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.RFID;

namespace RfidCenter
{
    public class RfidServer : MarshalByRefObject, IRfid, IDisposable
    {
        public void Dispose()
        {
            // Program.Rfid?.CancelRegisterString();
        }

        // 列出当前可用的 reader
        public ListReadersResult ListReaders()
        {
            // 选出已经成功打开的部分 Reader 返回
            List<string> readers = new List<string>();
            foreach(Reader reader in Program.Rfid.Readers)
            {
                if (reader.Result.Value == 0)
                    readers.Add(reader.Name);
            }
            return new ListReadersResult { Readers = readers.ToArray() };
        }

        public InventoryResult Inventory(string reader_name)
        {
            return new InventoryResult();
        }

        public GetTagInfoResult GetTagInfo(string reader_name, string uid)
        {
            return new GetTagInfoResult();
        }

        public NormalResult WriteTagInfo(
    string reader_name,
    TagInfo old_tag_info,
    TagInfo new_tag_info)
        {
            return new NormalResult();
        }

    }
}
