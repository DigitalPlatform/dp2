using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;
using DigitalPlatform.Range;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Xml;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 利用 dp2Library 协议显示和管理内核各种数据库和文件资源的树形控件
    /// </summary>
    public partial class KernelResTree : System.Windows.Forms.TreeView
    {
        public ApplicationInfo AppInfo = null;

        public event GuiAppendMenuEventHandle OnSetMenu = null;

        public event GetChannelEventHandler GetChannel = null;

        public event ReturnChannelEventHandler ReturnChannel = null;

        public string Lang { get; set; }

        public int[] EnabledIndices = null;	// null表示全部发黑。如果对象存在，但是元素个数为0，表示全部发灰

        public int[] HideIndices = null;    // 要隐藏的类型

        #region	资源类型。可作Icon下标用

        public const int RESTYPE_SERVER = 2;
        public const int RESTYPE_DB = 0;
        public const int RESTYPE_FROM = 1;
        public const int RESTYPE_LOADING = 3;
        public const int RESTYPE_FOLDER = 4;
        public const int RESTYPE_FILE = 5;

        #endregion

        public KernelResTree()
        {
            InitializeComponent();

            this.ImageList = imageList_resIcon;
        }

        LibraryChannel CallGetChannel(bool bBeginLoop)
        {
            if (this.GetChannel == null)
                return null;
            GetChannelEventArgs e = new GetChannelEventArgs();
            e.BeginLoop = bBeginLoop;
            this.GetChannel(this, e);
            if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                throw new Exception(e.ErrorInfo);
            return e.Channel;
        }

        void CallReturnChannel(LibraryChannel channel, bool bEndLoop)
        {
            if (this.ReturnChannel == null)
                return;
            ReturnChannelEventArgs e = new ReturnChannelEventArgs();
            e.Channel = channel;
            e.EndLoop = bEndLoop;
            this.ReturnChannel(this, e);
        }

        static string GetNodePath(TreeNode node)
        {
            if (node == null)
                return "";

            List<string> levels = new List<string>();
            while (node != null)
            {
#if NO
                if (node.Parent == null)
                    break;
                else
#endif
                levels.Insert(0, node.Text);
                node = node.Parent;
            }

            string result = StringUtil.MakePathList(levels, "/");

            // 针对 '!/cfgs' 情况，修正为 '!cfgs'
            if (result.StartsWith(DigitalPlatform.LibraryServer.LibraryServerUtil.LOCAL_PREFIX + "/") == true)
                result = DigitalPlatform.LibraryServer.LibraryServerUtil.LOCAL_PREFIX + result.Substring(DigitalPlatform.LibraryServer.LibraryServerUtil.LOCAL_PREFIX.Length + 1);

            // 针对 '~/cfgs' 情况，修正为 '~cfgs'
            if (result.StartsWith(DigitalPlatform.rms.KernelServerUtil.LOCAL_PREFIX + "/") == true)
                result = DigitalPlatform.rms.KernelServerUtil.LOCAL_PREFIX + result.Substring(DigitalPlatform.rms.KernelServerUtil.LOCAL_PREFIX.Length + 1);

            return result;
        }

        public bool Fill(TreeNode node = null)
        {
            string strError = "";
            if (this.Fill(null, node, out strError) == -1)
            {
                MessageBox.Show(this, strError);
                return false;
            }

            return true;
        }

        // 填充全部内容
        int Fill(LibraryChannel channel_param,
            TreeNode node,
            out string strError)
        {
            strError = "";

            TreeNodeCollection children = null;
            if (node == null)
                children = this.Nodes;
            else
                children = node.Nodes;

            if (string.IsNullOrEmpty(this.Lang))
                this.Lang = "zh";

            LibraryChannel channel = null;
            TimeSpan old_timeout = new TimeSpan(0);

            if (channel_param != null)
                channel = channel_param;
            else
            {
                channel = this.CallGetChannel(true);

                old_timeout = channel.Timeout;
                channel.Timeout = new TimeSpan(0, 5, 0);
            }

            bool restoreLoading = IsLoading(node);

            try
            {
                string start_path = GetNodePath(node);

                {
                    DirItemLoader loader = new DirItemLoader(channel,
                        null,
                        start_path,
                        "",
                        this.Lang);

                    children.Clear();

                    foreach (ResInfoItem item in loader)
                    {
                        TreeNode nodeNew = new TreeNode(item.Name, item.Type, item.Type);

                        nodeNew.Tag = item;
                        if (item.HasChildren)
                            SetLoading(nodeNew);

                        if (EnabledIndices != null
                            && StringUtil.IsInList(nodeNew.ImageIndex, EnabledIndices) == false)
                            nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

                        if (HideIndices != null
                            && StringUtil.IsInList(nodeNew.ImageIndex, HideIndices) == true)
                            continue;

                        children.Add(nodeNew);
                    }
                }

                // 在根级追加 '!' 下的 dp2library 本地文件或目录
                if (string.IsNullOrEmpty(start_path))
                {
                    ResInfoItem item = new ResInfoItem();
                    item.Name = "!";
                    item.Type = 4;
                    item.HasChildren = true;

                    TreeNode nodeNew = new TreeNode(item.Name, item.Type, item.Type);

                    nodeNew.Tag = item;
                    if (item.HasChildren)
                        SetLoading(nodeNew);

                    if (EnabledIndices != null
    && StringUtil.IsInList(nodeNew.ImageIndex, EnabledIndices) == false)
                        nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

                    if (HideIndices != null
                        && StringUtil.IsInList(nodeNew.ImageIndex, HideIndices) == true)
                    {
                    }
                    else
                        children.Add(nodeNew);
                }

                restoreLoading = false;  // 防止 finally 复原
                return 0;
            }
            catch (Exception ex)
            {
                strError = "Fill() 过程出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally
            {
                if (channel_param == null)
                {
                    channel.Timeout = old_timeout;

                    this.CallReturnChannel(channel, true);
                }

                if (restoreLoading)
                {
                    SetLoading(node);
                    if (node != null)
                        node.Collapse();
                }
            }
        }

        // 在一个节点下级插入"loading..."，以便出现+号
        public static void SetLoading(TreeNode node)
        {
            if (node == null)
                return;

            // 新node
            TreeNode nodeNew = new TreeNode("loading...", RESTYPE_LOADING, RESTYPE_LOADING);

            node.Nodes.Clear();
            node.Nodes.Add(nodeNew);
        }

        // 下级是否包含loading...?
        public static bool IsLoading(TreeNode node)
        {
            if (node == null)
                return false;

            if (node.Nodes.Count == 0)
                return false;

            if (node.Nodes[0].Text == "loading...")
                return true;

            return false;
        }

        private void KernelResTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            if (node == null)
                return;

            // 需要展开
            if (IsLoading(node) == true)
            {
#if NO
                object[] pList = new object[] { node };

                this.BeginInvoke(new Delegate_Fill(this.Fill), pList);
#endif
                this.BeginInvoke(new Func<TreeNode, bool>(Fill), node);
            }

        }

        private void KernelResTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;


            menuItem = new MenuItem("编辑配置文件(&E)");
            menuItem.Click += new System.EventHandler(this.menu_editCfgFile);
            if (this.SelectedNode == null || this.SelectedNode.ImageIndex != RESTYPE_FILE)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

#if NO
            menuItem = new MenuItem("刷新(&R)");
            menuItem.Click += new System.EventHandler(this.menu_refresh);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("导出数据(&E)");
            menuItem.Click += new System.EventHandler(this.menu_export);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("导入数据(&I)");
            menuItem.Click += new System.EventHandler(this.menu_import);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("快速导入数据(&S)");
            menuItem.Click += new System.EventHandler(this.menu_quickImport);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("管理操作");
            contextMenu.MenuItems.Add(menuItem);

            MenuItem subMenuItem = new MenuItem("delete keys index");
            subMenuItem.Click += new System.EventHandler(this.menu_deleteKeysIndex);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);


            subMenuItem = new MenuItem("create keys index");
            subMenuItem.Click += new System.EventHandler(this.menu_createKeysIndex);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("disable keys index");
            subMenuItem.Click += new System.EventHandler(this.menu_disableKeysIndex);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("rebuild keys index");
            subMenuItem.Click += new System.EventHandler(this.menu_rebuildKeysIndex);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("flush pending keys");
            subMenuItem.Click += new System.EventHandler(this.menu_flushKeysCache);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 
            menuItem = new MenuItem("允许复选(&M)");
            menuItem.Click += new System.EventHandler(this.menu_toggleCheckBoxes);
            if (this.CheckBoxes == true)
                menuItem.Checked = true;
            else
                menuItem.Checked = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清除全部复选(&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearCheckBoxes);
            if (this.CheckBoxes == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("全选下级节点(&A)");
            menuItem.Click += new System.EventHandler(this.menu_checkAllSubNodes);
            menuItem.Enabled = this.CheckBoxes;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 
            menuItem = new MenuItem("在下级创建新目录或文件(&N)");
            menuItem.Click += new System.EventHandler(this.menu_newDirectoryOrFile);
            if (this.SelectedNode != null
                && (this.SelectedNode.ImageIndex == ResTree.RESTYPE_SERVER
                || this.SelectedNode.ImageIndex == ResTree.RESTYPE_DB
                || this.SelectedNode.ImageIndex == ResTree.RESTYPE_FOLDER))
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            contextMenu.MenuItems.Add(menuItem);
#endif

            if (OnSetMenu != null)
            {
                GuiAppendMenuEventArgs newargs = new GuiAppendMenuEventArgs();
                newargs.ContextMenu = contextMenu;
                OnSetMenu(this, newargs);
                if (newargs.ContextMenu != contextMenu)
                    contextMenu = newargs.ContextMenu;
            }

            if (contextMenu != null)
                contextMenu.Show(this, new Point(e.X, e.Y));
        }

        // 编辑配置文件
        void menu_editCfgFile(object sender, System.EventArgs e)
        {
            string strError = "";

            if (this.SelectedNode == null)
            {
                strError = "尚未选择要编辑的配置文件节点";
                goto ERROR1;
            }

            if (this.SelectedNode.ImageIndex != RESTYPE_FILE)
            {
                strError = "所选择的节点不是配置文件类型。请选择要编辑的配置文件节点";
                goto ERROR1;
            }

            string strPath = GetNodePath(this.SelectedNode);

            // 获得配置文件内容
            ConfigInfo info = null;

            LibraryChannel channel = null;
            TimeSpan old_timeout = new TimeSpan(0);

            channel = this.CallGetChannel(true);

            old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);

            try
            {
                int nRet = GetConfigFile(
                    channel,
                    strPath,
                    out info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 编辑配置文件
                KernelCfgFileDialog dlg = new KernelCfgFileDialog();
                dlg.Font = GuiUtil.GetDefaultFont();
                dlg.ActivePage = "content";
                dlg.Content = info.Content;
                dlg.MIME = info.MIME;
                dlg.ServerUrl = channel.Url;
                dlg.Path = strPath;
                if (this.AppInfo != null)
                    this.AppInfo.LinkFormState(dlg, "CfgFileEditDlg_state");
                else
                    dlg.StartPosition = FormStartPosition.CenterScreen;

                dlg.ShowDialog(this);

                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

                info.Content = dlg.Content;
                info.MIME = dlg.MIME;

                nRet = SaveConfigFile(channel,
                    strPath,
                    info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }
            finally
            {
                channel.Timeout = old_timeout;

                this.CallReturnChannel(channel, true);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public class ConfigInfo
        {
            public string Path { get; set; }
            public string Content { get; set; }
            public string MIME { get; set; }
            public string LocalPath { get; set; }
            public byte[] Timestamp { get; set; }
        }

        // 保存资源
        // parameters:
        //      strLocalPath    打算写入 metadata 的 localpath。如果为 null 表示不使用此参数
        // return:
        //		-1	error
        //		0	发现上载的文件其实为空，不必保存了
        //		1	已经保存
        public static int SaveObjectFile(
            LibraryChannel channel,
            Stop stop,
            string strResPath,
            string strLocalFileName,
            byte[] timestamp,
            string strMime,
            string strLocalPath,
            out string strError)
        {
            strError = "";

            // 检测文件尺寸
            FileInfo fi = new FileInfo(strLocalFileName);

            if (fi.Exists == false)
            {
                strError = "文件 '" + strLocalFileName + "' 不存在...";
                return -1;
            }

            string[] ranges = null;

            if (fi.Length == 0)
            { // 空文件
                ranges = new string[1];
                ranges[0] = "";
            }
            else
            {
                string strRange = "";
                strRange = "0-" + Convert.ToString(fi.Length - 1);

                // 按照100K作为一个chunk
                ranges = RangeList.ChunkRange(strRange,
                    channel.UploadResChunkSize // 100 * 1024
                    );
            }

            byte[] output_timestamp = null;

            string strLastModifyTime = DateTime.UtcNow.ToString("u");

        REDOWHOLESAVE:
            string strWarning = "";

            for (int j = 0; j < ranges.Length; j++)
            {
            REDOSINGLESAVE:

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                string strWaiting = "";
                if (j == ranges.Length - 1)
                    strWaiting = " 请耐心等待...";

                string strPercent = "";
                RangeList rl = new RangeList(ranges[j]);
                if (rl.Count >= 1)
                {
                    double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                    strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                }

                if (stop != null)
                    stop.SetMessage("正在上载 " + ranges[j] + "/"
                        + Convert.ToString(fi.Length)
                        + " " + strPercent + " " + strLocalFileName + strWarning + strWaiting);

                long lRet = channel.SaveResObject(
                    stop,
                    strResPath,
                    strLocalFileName,
                    strLocalPath,
                    strMime,
                    ranges[j],
j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
timestamp,
out output_timestamp,
out strError);

                timestamp = output_timestamp;

                strWarning = "";

                if (lRet == -1)
                {
                    if (channel.ErrorCode == LibraryClient.localhost.ErrorCode.TimestampMismatch)
                    {

                        timestamp = new byte[output_timestamp.Length];
                        Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                        strWarning = " (时间戳不匹配, 自动重试)";
                        if (ranges.Length == 1 || j == 0)
                            goto REDOSINGLESAVE;
                        goto REDOWHOLESAVE;
                    }

                    return -1;
                }
            }

            return 1;	// 已经保存
        }

        int SaveConfigFile(LibraryChannel channel,
            string strPath,
            ConfigInfo info,
            out string strError)
        {
            strError = "";

            string strTempFileName = Path.GetTempFileName();
            using (Stream sw = new FileStream(strTempFileName,
                FileMode.Create, FileAccess.Write))
            {
                byte[] baContent = StringUtil.GetUtf8Bytes(info.Content, true);

                sw.Write(baContent, 0, baContent.Length);
            }

            try
            {
                // 保存资源
                // return:
                //		-1	error
                //		0	发现上载的文件其实为空，不必保存了
                //		1	已经保存
                int nRet = SaveObjectFile(
                    channel,
                    null,
                    strPath,
                    strTempFileName,
                    info.Timestamp,
                    info.MIME,
                    info.LocalPath,
                    out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                if (string.IsNullOrEmpty(strTempFileName) == false)
                    File.Delete(strTempFileName);
            }
        }

        int GetConfigFile(
            LibraryChannel channel,
            string strPath,
            out ConfigInfo info,
            out string strError)
        {
            strError = "";
            info = new ConfigInfo();

            string strOutputFileName = Path.GetTempFileName();
            try
            {
                byte[] baOutputTimestamp = null;
                string strOutputPath = "";
                string strMetadata = "";

                long lRet = channel.GetRes(null,
                strPath,
                strOutputFileName,
                out strMetadata,
                out baOutputTimestamp,
                out strOutputPath,
                out strError);
                if (lRet == -1)
                    return -1;

                info.Timestamp = baOutputTimestamp;
                info.Path = strOutputPath;

                // 观察mime
                // 取metadata
                Hashtable values = StringUtil.ParseMetaDataXml(strMetadata,
                    out strError);
                if (values == null)
                {
                    strError = "ParseMedaDataXml() values == null";
                    return -1;
                }

                string strMime = (string)values["mimetype"];
                if (strMime == null || strMime == "")
                    strMime = "text";

                string strLocalPath = (string)values["localpath"];
                if (strLocalPath == null)
                    strLocalPath = "";

                info.MIME = strMime;
                info.LocalPath = strLocalPath;

                if (IsText(strMime) == true)
                {
                    using (StreamReader sr = new StreamReader(strOutputFileName, Encoding.UTF8))
                    {
                        info.Content = ConvertCrLf(sr.ReadToEnd());
                    }
                }
                else
                {
                    strError = "二进制内容 '" + strMime + "' 无法直接编辑";
                    return -1;
                }

                return 0;
            }
            finally
            {
                if (string.IsNullOrEmpty(strOutputFileName) == false)
                    File.Delete(strOutputFileName);
            }
        }

        static string ConvertCrLf(string strText)
        {
            strText = strText.Replace("\r\n", "\r");
            strText = strText.Replace("\n", "\r");
            return strText.Replace("\r", "\r\n");
        }

        // 检测 MIME 是否为 text 开头
        static bool IsText(string strMime)
        {
            string strFirstPart = StringUtil.GetFirstPartPath(ref strMime);
            if (strFirstPart.ToLower() == "text")
                return true;

            return false;
        }

    }
}
