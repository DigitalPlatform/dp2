using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    // 通过册条码号获得册记录路径的枚举器
    public class ItemBarcodeLoader : IEnumerable
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

        List<string> m_barcodes = new List<string>();

        public List<string> Barcodes
        {
            get
            {
                return this.m_barcodes;
            }
            set
            {
                this.m_barcodes = value;
            }
        }

        public LibraryChannel Channel
        {
            get;
            set;
        }

        public Stop Stop
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            List<string> batch = new List<string>();
            for (int index = 0; index < m_barcodes.Count; index++)
            {
                string s = m_barcodes[index];

                if (string.IsNullOrEmpty(s) == true)
                    throw new Exception("册条码号字符串不允许为空 (index=" + index.ToString() + ")");

                batch.Add(s);

                // 每100个一批，或者最后一次
                if (batch.Count >= 100 ||
                    (index == m_barcodes.Count - 1 && batch.Count > 0))
                {
                    REDO_GETITEMINFO:
                    string strBiblio = "";
                    string strResult = "";
                    string strError = "";
                    long lRet = this.Channel.GetItemInfo(
                        this.Stop,
                        "@barcode-list:" + StringUtil.MakePathList(batch),
                        "get-path-list",
                        out strResult,
                        "", // strBiblioType,
                        out strBiblio,
                        out strError);
                    if (lRet == -1)
                    {
                        if (this.Prompt != null)
                        {
                            MessagePromptEventArgs e = new MessagePromptEventArgs();
                            e.MessageText = "通过册条码号获得记录路径时发生错误： " + strError;
                            e.Actions = "yes,no,cancel";
                            this.Prompt(this, e);
                            if (e.ResultAction == "cancel")
                                throw new Exception(strError);
                            else if (e.ResultAction == "yes")
                            {
                                if (this.Stop != null)
                                    this.Stop.Continue();
                                goto REDO_GETITEMINFO;
                            }
                            else
                            {
                            }
                        }
                        else
                            throw new ChannelException(Channel.ErrorCode, strError);
                    }

                    List<string> recpaths = StringUtil.SplitList(strResult);

                    if (batch.Count != recpaths.Count)
                    {
                        strError = "batch.Count != recpaths.Count";
                        throw new Exception(strError);
                    }
                    Debug.Assert(batch.Count == recpaths.Count, "");

                    int i = 0;
                    foreach (string recpath in recpaths)
                    {
                        EntityItem item = new EntityItem();
                        item.Barcode = batch[i];

                        if (string.IsNullOrEmpty(recpath) == false
                            && recpath[0] == '!')
                            item.ErrorInfo = recpath.Substring(1);
                        else
                            item.RecPath = recpath;
                        i++;

                        yield return item;
                    }

                    batch.Clear();
                }
            }
        }
    }

    /// <summary>
    /// 书目信息事项
    /// </summary>
    public class EntityItem
    {
        /// <summary>
        /// 记录路径
        /// </summary>
        public string RecPath = "";

        /// <summary>
        /// 册条码号
        /// </summary>
        public string Barcode = "";
#if NO
        /// <summary>
        /// 记录内容
        /// </summary>
        public string Content = "";
        /// <summary>
        /// 时间戳
        /// </summary>
        public byte[] Timestamp = null;
        /// <summary>
        /// 记录元数据
        /// </summary>
        public string Metadata = "";
#endif

        /// <summary>
        /// 错误码
        /// </summary>
        public ErrorCode ErrorCode = ErrorCode.NoError;

        /// <summary>
        /// 错误信息字符串
        /// </summary>
        public string ErrorInfo = "";
    }
}
