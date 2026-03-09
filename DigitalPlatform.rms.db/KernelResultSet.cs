using DuckDbResultSet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.rms
{
    public class KernelResultSet : DuckResultSet
    {
        internal string m_strQuery = ""; // 原始的查询字符串
        // public int Asc = 1; // 1 升序 -1 降序
        public bool Permanent = false; // 是否永久保存
        public object Param = null; // 其他参数

        public event GetTempFilenameEventHandler GetTempFilename = null;
        public string TempFileDir = ""; // 用于创建和存储临时文件的目录

        /*
        public KernelResultSet(Delegate_getTempFileName procGetTempFileName = null)
        {
            Open(procGetTempFileName);
        }
        */

        public KernelResultSet(ResultSetType type)
        {
            this.Type = type;
        }

        // 获得临时文件名
        string DoGetTempFilename()
        {
            if (string.IsNullOrEmpty(this.TempFileDir) == false)
            {
                Debug.Assert(string.IsNullOrEmpty(this.TempFileDir) == false, "");
                while (true)
                {
                    string strFilename = Path.Combine(this.TempFileDir, Guid.NewGuid().ToString());
                    if (File.Exists(strFilename) == false)
                    {
                        using (FileStream s = File.Create(strFilename))
                        {
                        }
                        return strFilename;
                    }
                }
            }

            if (this.GetTempFilename == null)
                return Path.GetTempFileName();

            GetTempFilenameEventArgs e = new GetTempFilenameEventArgs();
            this.GetTempFilename(this, e);
            if (String.IsNullOrEmpty(e.TempFilename) == true)
            {
                Debug.Assert(false, "虽然接管了事件，但是没有实质性动作");
                return Path.GetTempFileName();
            }

            Debug.Assert(string.IsNullOrEmpty(e.TempFilename) == false, "");
            return e.TempFilename;
        }

        public delegate string Delegate_getTempFileName();

        // 
        public void Open(Delegate_getTempFileName procGetTempFileName = null)
        {
            string filename = "";
            // 优先使用临时的函数 创建临时文件
            if (procGetTempFileName != null)
                filename = procGetTempFileName();

            if (string.IsNullOrEmpty(filename) == true)
                filename = DoGetTempFilename();

            if (File.Exists(filename) == true)
                File.Delete(filename);

            base.SetFileName(filename);
        }

        public void EnsureCreateIndex()
        {

        }

        public const char CHAR_OR = (char)0x01;
        public const char CHAR_AND = (char)0x02;
        public const char FROM_LEAD = (char)0x03;
        public const char SPLIT = (char)0x04;

    }

    public delegate void GetTempFilenameEventHandler(object sender,
        GetTempFilenameEventArgs e);

    public class GetTempFilenameEventArgs : EventArgs
    {
        public string TempFilename = "";
    }

}
