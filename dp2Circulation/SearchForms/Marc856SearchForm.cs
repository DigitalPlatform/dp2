using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.GUI;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 856 字段检索窗
    /// </summary>
    public partial class Marc856SearchForm : MyForm
    {
        Hashtable m_biblioTable = new Hashtable(); // 书目记录路径 --> 书目信息

        public Marc856SearchForm()
        {
            InitializeComponent();
        }

        private void Marc856SearchForm_Load(object sender, EventArgs e)
        {
            CreateColumns();
        }

        private void Marc856SearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Marc856SearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 新加入一行
        public ListViewItem AddLine(
            string strBiblioRecPath,
            BiblioInfo biblio_info,
            MarcField field)
        {
            // 将 BiblioInfo 储存起来
            BiblioInfo existing = (BiblioInfo)this.m_biblioTable[strBiblioRecPath];
            if (existing == null)
                this.m_biblioTable[strBiblioRecPath] = biblio_info;

            // 设置各列内容
            string u = field.select("subfield[@name='u']").FirstContent;
            string x = field.select("subfield[@name='x']").FirstContent;

            Hashtable table = StringUtil.ParseParameters(x, ';', ':');
            string strType = (string)table["type"];
            string strRights = (string)table["rights"];
            string strSize = (string)table["size"];
            string strSource = (string)table["source"];

            ListViewItem item = new ListViewItem();

            ListViewUtil.ChangeItemText(item, COLUMN_RECPATH, strBiblioRecPath);

            ListViewUtil.ChangeItemText(item, COLUMN_FIELDINDEX, this.listView_records.Items.Count.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_URL, u);
            ListViewUtil.ChangeItemText(item, COLUMN_TYPE, strType);
            ListViewUtil.ChangeItemText(item, COLUMN_SIZE, strSize);
            ListViewUtil.ChangeItemText(item, COLUMN_SOURCE, strSource);

            ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, strRights);
            this.listView_records.Items.Add(item);

            LineInfo line_info = new LineInfo();
            line_info.Field = field;
            item.Tag = line_info;
            return item;
        }

        const int COLUMN_RECPATH = 0;
        const int COLUMN_SUMMARY = 1;
        const int COLUMN_FIELDINDEX = 2;
        const int COLUMN_URL = 3;
        const int COLUMN_TYPE = 4;
        const int COLUMN_SIZE = 5;
        const int COLUMN_SOURCE = 6;
        const int COLUMN_RIGHTS = 7;

        void CreateColumns()
        {
            string[] titles = new string[] {
                "书目摘要",
                "字段序号",
                "$uURL",
                "$x type 类型",
                "$x size 尺寸",
                "$x source 来源",
                "$x rights 权限",
            };
            int i = 1;
            foreach (string title in titles)
            {
                ColumnHeader header = new ColumnHeader();

                if (i < this.listView_records.Columns.Count)
                    header = this.listView_records.Columns[i];
                else
                {
                    header = new ColumnHeader();
                    header.Width = 100;
                    this.listView_records.Columns.Add(header);
                }

                header.Text = title;
                i++;
            }
        }

        // parameters:
        //      lStartIndex 调用前已经做过的事项数。为了准确显示 Progress
        // return:
        //      -2  获得书目摘要的权限不够
        //      -1  出错
        //      0   用户中断
        //      1   完成
        public int FillBiblioSummaryColumn(List<ListViewItem> items,
            long lStartIndex,
            bool bDisplayMessage,
            bool bPrepareLoop,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            if (bPrepareLoop)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在填充书目摘要 ...");
                stop.BeginLoop();
            }

            try
            {
                stop.SetProgressRange(0, items.Count);

                List<string> biblio_recpaths = new List<string>();  // 尺寸可能比 items 数组小，没有包含里面不具有 parent id 列的事项
                // List<int> colindex_list = new List<int>();  // 存储每个 item 对应的 parent id colindex。数组大小等于 items 数组大小
                foreach (ListViewItem item in items)
                {
                    string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
                    biblio_recpaths.Add(strBiblioRecPath);
                }

                CacheableBiblioLoader loader = new CacheableBiblioLoader();
                loader.Channel = this.Channel;
                loader.Stop = this.stop;
                loader.Format = "summary";
                loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
                loader.RecPaths = biblio_recpaths;

                var enumerator = loader.GetEnumerator();

                int i = 0;
                foreach (ListViewItem item in items)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        return 0;
                    }

                    string strRecPath = ListViewUtil.GetItemText(item, 0);
                    if (stop != null && bDisplayMessage == true)
                    {
                        stop.SetMessage("正在刷新浏览行 " + strRecPath + " 的书目摘要 ...");
                        stop.SetProgressValue(lStartIndex + i);
                    }

                    try
                    {
                        bool bRet = enumerator.MoveNext();
                        if (bRet == false)
                        {
                            Debug.Assert(false, "还没有到结尾, MoveNext() 不应该返回 false");
                            // TODO: 这时候也可以采用返回一个带没有找到的错误码的元素
                            strError = "error 1";
                            return -1;
                        }
                    }
                    catch (ChannelException ex)
                    {
                        strError = ex.Message;
                        if (ex.ErrorCode == ErrorCode.AccessDenied)
                            return -2;
                        return -1;
                    }

                    BiblioItem biblio = (BiblioItem)enumerator.Current;
                    // Debug.Assert(biblio.RecPath == strRecPath, "m_loader 和 items 的元素之间 记录路径存在严格的锁定对应关系");

                    ListViewUtil.ChangeItemText(item,
                        1,
                        biblio.Content);
                    i++;
                    stop.SetProgressValue(i);
                }

                return 1;
            }
            catch (Exception ex)
            {
                strError = "填充书目摘要的过程出现异常: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                if (bPrepareLoop)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    stop.HideProgress();
                }
            }
        }
    }

    class LineInfo
    {
        /// <summary>
        /// 856 字段对象
        /// </summary>
        public MarcField Field = null;
    }
}
