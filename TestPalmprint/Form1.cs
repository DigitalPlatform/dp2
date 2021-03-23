using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using PalmDrivers.First;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace TestPalmprint
{
    public partial class Form1 : Form
    {
        public static PalmDriver PalmDriver = new PalmDriver();

        public Form1()
        {
            InitializeComponent();
        }

        private void MenuItem_crossCompare_Click(object sender, EventArgs e)
        {
            string strError = "";
            var result = PalmDriver.Init(0);
            if (result.Value == -1)
            {
                strError = $"PalmDriver.Init() error: {result.ErrorInfo}";
                goto ERROR1;
            }
            try
            {
                List<PalmprintItem> items = new List<PalmprintItem>();
                using (LibraryChannel channel = new LibraryChannel())
                {
                    LoginDlg dlg = new LoginDlg();

                    dlg.ServerUrl = "url";
                    dlg.UserName = "username";
                    dlg.Password = "password";
                    dlg.SavePassword = true;
                    dlg.ShowDialog(this);

                    if (dlg.DialogResult != DialogResult.OK)
                    {
                        strError = "放弃登录";
                        goto ERROR1;
                    }

                    channel.Url = dlg.ServerUrl;

                    long lRet = channel.Login(dlg.UserName,
                        dlg.Password,
                        "client=test|0.01",
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    int nRet = GetCurrentOwnerReaderNameList(
    channel,
    out List<string> readerdbnames,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    foreach (string strReaderDbName in readerdbnames)
                    {

                        lRet = channel.SearchReader(null,  // stop,
strReaderDbName,
"",
-1,
"掌纹时间戳",
"left",
"zh",
null,   // strResultSetName
"", // strOutputStyle
out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (lRet == 0)
                            continue;

                        ResultSetLoader loader = new ResultSetLoader(channel,
    null,
    null,
    $"id,cols,format:cfgs/browse_palmprint",// : $"id,cols,format:cfgs/browse_palmprinttimestamp",
    "zh");

                        foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                        {
                            if (record.Cols == null || record.Cols.Length < 3)
                                continue;

                            if (string.IsNullOrEmpty(record.Cols[0]) == true)
                                continue;   // 读者记录中没有指纹信息

                            // timestamp | barcode | fingerprint
                            var timestamp = record.Cols[0];
                            var barcode = record.Cols[1];
                            var palmprint = record.Cols[2];

                            PalmprintItem item = new PalmprintItem
                            {
                                Barcode = barcode,
                                FeatureString = palmprint
                            };
                            items.Add(item);
                        }
                    }
                }


                // 比对
                foreach (var item in items)
                {
                    string start = item.Barcode;
                    int i = 0;
                    foreach (var another in items)
                    {
                        Thread.Sleep(500);

                        var compare_result = PalmDriver.CompareFeature(
                            item.FeatureString,
                            another.FeatureString);
                        if (compare_result.Value == -1)
                            goto ERROR1;

                        i++;
                    }
                }
            }
            finally
            {
                PalmDriver.Free();
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得当前帐户所管辖的读者库名字
        static int GetCurrentOwnerReaderNameList(
            LibraryChannel channel,
            out List<string> readerdbnames,
            out string strError)
        {
            strError = "";
            readerdbnames = new List<string>();
            // int nRet = 0;

            long lRet = channel.GetSystemParameter(null,
    "system",
    "readerDbGroup",
    out string strValue,
    out strError);
            if (lRet == -1)
                return -1;

            // 新方法
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            try
            {
                dom.DocumentElement.InnerXml = strValue;
            }
            catch (Exception ex)
            {
                strError = "category=system,name=readerDbGroup所返回的XML片段在装入InnerXml时出错: " + ex.Message;
                return -1;
            }

            string strLibraryCodeList = channel.LibraryCodeList;

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            foreach (XmlElement node in nodes)
            {
                string strLibraryCode = node.GetAttribute("libraryCode");

                if (IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                        continue;
                }

                string strDbName = node.GetAttribute("name");
                readerdbnames.Add(strDbName);
            }

            return 0;
        }

        public static bool IsGlobalUser(string strLibraryCodeList)
        {
            if (strLibraryCodeList == "*" || string.IsNullOrEmpty(strLibraryCodeList) == true)
                return true;
            return false;
        }

    }

    public class PalmprintItem
    {
        public string Barcode { get; set; }
        public string FeatureString { get; set; }
    }
}
