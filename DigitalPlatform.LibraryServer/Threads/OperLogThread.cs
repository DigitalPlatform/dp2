using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using DigitalPlatform.Xml;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 操作日志的辅助线程。负责把积累的信息写入 mongodb 日志库
    /// </summary>
    public class OperLogThread : BatchTask
    {
        List<XmlDocument> _datas = new List<XmlDocument>();

        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        public OperLogThread(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
            this.PerTime = 5 * 60 * 1000;	// 5分钟
        }

        public override string DefaultName
        {
            get
            {
                return "操作日志辅助线程";
            }
        }

        bool _errorWrited = false;  // WriteErrorLog() 是否进行过了。设立此变量是为了避免短时间内往 errorlog 写入大量内容

        /// <summary>
        /// 是否已经启用
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (this.App._mongoClient == null || this.App.ChargingOperDatabase == null)
                    return false;
                return true;
            }
        }

        // 检测一个 operation 是否需要处理
        public static bool NeedAdd(string strOperation)
        {
            if (strOperation == "borrow" || strOperation == "return")
                return true;
            return false;
        }

        public void AddOperLog(XmlDocument dom)
        {
            if (this.App._mongoClient == null || this.App.ChargingOperDatabase == null)
                return;

            this.m_lock.EnterWriteLock();
            try
            {
                if (this._datas.Count > 10000)
                {
                    if (_errorWrited == false)
                    {
                        this.App.WriteErrorLog("OperLogThread 的缓冲空间爆满，已停止追加新事项 (10000 条)");
                        _errorWrited = true;
                    }
                    this.eventActive.Set(); // 提醒线程及时处理
                    return; // 不再允许加入新事项
                }
                this._datas.Add(dom);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            this.eventActive.Set(); // 提醒线程
        }

        // 一次操作循环
        public override void Worker()
        {
            List<XmlDocument> current = new List<XmlDocument>();

            this.m_lock.EnterWriteLock();
            try
            {
                if (this._datas.Count == 0)
                    return;
                current.AddRange(this._datas);
                this._datas.Clear();
                this._errorWrited = false;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            foreach (XmlDocument dom in current)
            {
                string strOperation = DomUtil.GetElementText(dom.DocumentElement,
    "operation");
                if (strOperation == "borrow" || strOperation == "return")
                {
                    string strError = "";
                    int nRet = BuildMongoOperDatabase.AppendOperationBorrowReturn(this.App,
                        dom,
                        strOperation,
                        out strError);
                    if (nRet == -1)
                        this.App.WriteErrorLog("OperLogThread 写入 mongodb 日志库时出错: " + strError);
                }
            }
        }
    }
}
