using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Core;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 利用 dp2Library 协议显示和管理内核各种数据库和文件资源的树形控件
    /// </summary>
    public partial class KernelResTree : System.Windows.Forms.TreeView
    {
        public IApplicationInfo AppInfo = null;

        public event UploadFilesEventHandler UploadFiles = null;

        public event DownloadFilesEventHandler DownloadFiles = null;

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

        // 把 API 中用到的服务器文件路径转换为分级形态的字符串集合
        // 例如 !backup 要转为两级。
        static List<string> BuildPathList(string path)
        {
            List<string> aName = new List<string>(path.Split(new Char[] { '/' }));

            if (aName.Count > 0 && aName[0].StartsWith("!"))
            {
                aName.Insert(0, "!");
                aName[1] = aName[1].Substring(1);
            }

            return aName;
        }

        // 根据路径逐步展开
        // return:
        //      -1  出错
        //      0   没有出错
        public int ExpandPath(string path, out string strError)
        {
            strError = "";

            List<string> errors = new List<string>();

            List<string> aName = BuildPathList(path);

            TreeNode node = null;
            TreeNode nodeThis = null;

            /*
            string[] temp = new string[aName.Length + 1];
            Array.Copy(aName, 0, temp, 1, aName.Length);
            temp[0] = respath.Url;

            aName = temp;
            */

            foreach (string name in aName)
            {
                TreeNodeCollection nodes = null;

                if (node == null)
                    nodes = this.Nodes;
                else
                    nodes = node.Nodes;

                bool bFound = false;
                for (int j = 0; j < nodes.Count; j++)
                {
                    TreeNode currrent_node = nodes[j];

                    // 如果是其他节点，取节点正文即可
                    string strName = currrent_node.Text;
                    if (name == strName)
                    {
                        bFound = true;
                        nodeThis = nodes[j];
                        break;
                    }
                }
                if (bFound == false)
                    break;

                node = nodeThis;

                // 需要展开
                if (IsLoading(node) == true)
                {
                    int nRet = Fill(null, node, out strError);
                    if (nRet == -1)
                    {
                        errors.Add(strError);
                        break;
                    }
                }
                node.Expand();  // 即便最终层次没有找到，也要展开中间层次
            }

            if (nodeThis != null && nodeThis.Parent != null)
                nodeThis.Parent.Expand();

            this.SelectedNode = nodeThis;
            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "\r\n");
                return -1;
            }
            return 0;
        }

        public bool Fill(TreeNode node = null)
        {
            if (this.Fill(null, node, out string strError) == -1)
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

                    // 排序
                    // 这样所有对象都要一次性进入内存了
                    List<ResInfoItem> items = new List<ResInfoItem>();
                    foreach (ResInfoItem item in loader)
                    {
                        if (string.IsNullOrEmpty(item.Name))
                            continue;
                        items.Add(item);
                    }

                    items.Sort((a, b) =>
                    {
                        int nRet = a.Type - b.Type;
                        if (nRet != 0)
                            return nRet;
                        return string.Compare(a.Name, b.Name);
                    });

                    // foreach (ResInfoItem item in loader)
                    foreach (ResInfoItem item in items)
                    {
                        // 2017/9/23
                        if (string.IsNullOrEmpty(item.Name))
                            continue;

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
            catch (ChannelException ex)
            {
                strError = ex.Message;
                return -1;
#if NO
                if (ex.ErrorCode == ErrorCode.AccessDenied)
                {
                    strError = ex.Message;
                    return -1;
                }
                strError = "Fill() 过程出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
#endif
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

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新(&R)");
            menuItem.Click += new System.EventHandler(this.menu_refresh);
            //if (this.SelectedNode == null || this.SelectedNode.ImageIndex != RESTYPE_FILE)
            //    menuItem.Enabled = false;
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
            // menuItem.Enabled = this.CheckBoxes;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("上传文件 (&U)");
            menuItem.Click += new System.EventHandler(this.menu_uploadFile);
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            List<TreeNode> selected_file_nodes = GetCheckedFileNodes();

            menuItem = new MenuItem("下载文件 [" + selected_file_nodes.Count + "] (&W)");
            menuItem.Click += new System.EventHandler(this.menu_downloadDynamicFile);
            if (selected_file_nodes.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("观察 MD5 [" + selected_file_nodes.Count + "] (&M)");
            menuItem.Click += new System.EventHandler(this.menu_getmd5);
            if (selected_file_nodes.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制文件名 [" + selected_file_nodes.Count + "] (&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyFileName);
            if (selected_file_nodes.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除文件 [" + selected_file_nodes.Count + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteFile);
            if (selected_file_nodes.Count == 0)
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

        bool _hideConfirmMessageBox = false;

        private void menu_uploadFile(object sender, EventArgs e)
        {
            string strError = "";

            if (this.UploadFiles == null)
            {
                strError = "尚未绑定 UploadFiles 事件";
                goto ERROR1;
            }

            if (this.SelectedNode == null)
            {
                strError = "尚未选择代表上传目标位置的节点";
                goto ERROR1;
            }

#if NO
            List<TreeNode> nodes = this.GetCheckedFileNodes();

            if (nodes.Count == 0)
            {
                strError = "尚未选择要下载的配置文件节点";
                goto ERROR1;
            }
#endif

            if (this.SelectedNode.ImageIndex != RESTYPE_FOLDER
                && this.SelectedNode.ImageIndex != RESTYPE_FILE)
            {
                strError = "所选择的节点不是文件类型或目录类型。请选择上载操作的目标位置(前述两种类型的节点)";
                goto ERROR1;
            }

            // TODO: checked 的状态是否可以
            string strTagetFolder = GetNodePath(this.SelectedNode);
            if (this.SelectedNode.ImageIndex == RESTYPE_FILE)
                strTagetFolder = Path.GetDirectoryName(strTagetFolder);
#if NO
            List<string> paths = new List<string>();
            foreach (TreeNode node in nodes)
            {
                string strPath = GetNodePath(node);

                string strExt = Path.GetExtension(strPath);
                if (strExt == ".~state")
                {
                    strError = "不允许下载扩展名为 .~state 的状态文件 (" + strPath + ")";
                    goto ERROR1;
                }
                paths.Add(strPath);
            }
#endif

            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "请指定要上传的文件(可以多选)",
                // dlg.FileName = this.RecPathFilePath;
                // dlg.InitialDirectory = 
                Multiselect = true,
                Filter = "All files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            // 以出现一个确认源和目标的对话框
            if (this._hideConfirmMessageBox == false)
            {
                DialogResult result = MessageDialog.Show(this,
                    "确认将以下本地文件上传到位置 '" + strTagetFolder + "':\r\n---\r\n" + string.Join("\r\n", dlg.FileNames),
    MessageBoxButtons.OKCancel,
    MessageBoxDefaultButton.Button1,
    "此后不再出现本对话框",
    ref this._hideConfirmMessageBox,
    new string[] { "确定", "取消" });
                if (result == DialogResult.Cancel)
                    return;
            }

            UploadFilesEventArgs e1 = new UploadFilesEventArgs
            {
                TargetFolder = strTagetFolder,
                SourceFileNames = new List<string>(dlg.FileNames),
                FuncEnd = (bError) => { Refresh(this.SelectedNode); }   // 完成后才刷新
            };
            this.UploadFiles(this, e1);
            if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public void Refresh(TreeNode node)
        {
            if (node == null)
            {
                this.Fill(null);
                return;
            }

            string strName = node.Text;
            if (node.ImageIndex == RESTYPE_FILE)
                node = node.Parent;

            this.Fill(node);

            // 复原选择
            SelectNode(node, strName);
        }

        void menu_clearCheckBoxes(object sender, System.EventArgs e)
        {
            this.ClearChildrenCheck(null);
        }

        // 清除下级所有的选中的项(不包括自己)
        // parameters:
        //      nodeStart   起点node。如果为null, 表示从根层开始，清除全部
        public void ClearChildrenCheck(TreeNode nodeStart)
        {
            if (this.CheckBoxes == false)
                return;

            TreeNodeCollection nodes = null;
            if (nodeStart == null)
            {
                nodes = this.Nodes;
            }
            else
                nodes = nodeStart.Nodes;

            foreach (TreeNode node in nodes)
            {
                node.Checked = false;
                ClearChildrenCheck(node);	// 递归
            }
        }


        // 全选下级节点
        void menu_checkAllSubNodes(object sender, EventArgs e)
        {
            TreeNode node = this.SelectedNode;
            if (node == null)
                return;

            if (this.CheckBoxes == false)
                this.CheckBoxes = true;

            foreach (TreeNode current in node.Nodes)
            {
                current.Checked = true;
            }

            node.Expand();
        }

        void menu_toggleCheckBoxes(object sender, System.EventArgs e)
        {
            if (this.CheckBoxes == true)
                this.CheckBoxes = false;
            else
                this.CheckBoxes = true;
        }

        // return:
        //      true    留存节点
        //      false   不留存节点
        public delegate bool Delegate_filter(TreeNode node);

        public List<TreeNode> GetCheckedFileNodes(TreeNode parent = null)
        {
            return GetCheckedNodes(parent,
                (node) =>
                {
                    if (node.ImageIndex == RESTYPE_FILE)
                        return true;
                    return false;
                }
            );
        }

        // TODO: 当选择了一个目录节点，其下全部文件节点都被选中了
        public List<TreeNode> GetCheckedNodes(TreeNode parent = null,
            Delegate_filter filter_func = null)
        {
            List<TreeNode> results = new List<TreeNode>();

            if (this.CheckBoxes == false)
            {
                if (this.SelectedNode != null)
                {
                    if (filter_func == null
    || filter_func(this.SelectedNode) == true)
                        results.Add(this.SelectedNode);
                }

                return results;
            }

            TreeNodeCollection collection = null;

            if (parent == null)
                collection = this.Nodes;
            else
                collection = parent.Nodes;

            if (parent != null && parent.Checked)
            {
                if (filter_func == null
                    || filter_func(parent) == true)
                    results.Add(parent);
            }

            foreach (TreeNode child in collection)
            {
                results.AddRange(GetCheckedNodes(child, filter_func));
            }

            return results;
        }

        private static readonly Object _syncRoot_downloaders = new Object();

        List<DynamicDownloader> _downloaders = new List<DynamicDownloader>();

        // parameters:
        //      downloader  要清除的 DynamicDownloader 对象。如果为 null，表示全部清除
        void RemoveDownloader(DynamicDownloader downloader,
            bool bTriggerClose = false)
        {
            List<DynamicDownloader> list = new List<DynamicDownloader>();
            lock (_syncRoot_downloaders)
            {
                if (downloader == null)
                {
                    list.AddRange(_downloaders);
                    _downloaders.Clear();
                }
                else
                {
                    list.Add(downloader);
                    // downloader.Close();
                    _downloaders.Remove(downloader);
                }
            }

            foreach (DynamicDownloader current in list)
            {
                current.Close();
            }

        }

        void DisplayDownloaderErrorInfo(DynamicDownloader downloader)
        {
            if (string.IsNullOrEmpty(downloader.ErrorInfo) == false
                && downloader.ErrorInfo.StartsWith("~") == false)
            {
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show(this, "下载 " + downloader.ServerFilePath + "-->" + downloader.LocalFilePath + " 过程中出错: " + downloader.ErrorInfo);
                }));
                downloader.ErrorInfo = "~" + downloader.ErrorInfo;  // 只显示一次
            }
        }

        // string _usedDownloadFolder = "";

        // 下载动态文件
        // 动态文件就是一直在不断增长的文件。允许一边增长一边下载
        void menu_downloadDynamicFile(object sender, System.EventArgs e)
        {
            string strError = "";

            if (this.DownloadFiles == null)
            {
                strError = "尚未绑定 DownloadFiles 事件";
                goto ERROR1;
            }

            List<TreeNode> nodes = this.GetCheckedFileNodes();

            if (nodes.Count == 0)
            {
                strError = "尚未选择要下载的配置文件节点";
                goto ERROR1;
            }

#if NO
            if (this.SelectedNode.ImageIndex != RESTYPE_FILE)
            {
                strError = "所选择的节点不是配置文件类型。请选择要下载的配置文件节点";
                goto ERROR1;
            }
#endif
            List<string> paths = new List<string>();
            foreach (TreeNode node in nodes)
            {
                string strPath = GetNodePath(node);

                string strExt = Path.GetExtension(strPath);
                if (strExt == ".~state")
                {
                    strError = "不允许下载扩展名为 .~state 的状态文件 (" + strPath + ")";
                    goto ERROR1;
                }
                paths.Add(strPath);
            }

            DownloadFilesEventArgs e1 = new DownloadFilesEventArgs();
            e1.Action = "download";
            e1.FileNames = paths;
            this.DownloadFiles(this, e1);
            if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                strError = e1.ErrorInfo;
                goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 复制文件名到 Windows 剪贴板
        void menu_copyFileName(object sender, System.EventArgs e)
        {
            string strError = "";

            List<TreeNode> nodes = this.GetCheckedFileNodes();

            if (nodes.Count == 0)
            {
                strError = "尚未选择要复制名字的文件节点";
                goto ERROR1;
            }

            List<string> paths = new List<string>();
            foreach (TreeNode node in nodes)
            {
                string strPath = GetNodePath(node);
                paths.Add(strPath);
            }

            Clipboard.SetText(StringUtil.MakePathList(paths, "\r\n"));
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_getmd5(object sender, System.EventArgs e)
        {
            string strError = "";

            if (this.DownloadFiles == null)
            {
                strError = "尚未绑定 DownloadFiles 事件";
                goto ERROR1;
            }

            List<TreeNode> nodes = this.GetCheckedFileNodes();

            if (nodes.Count == 0)
            {
                strError = "尚未选择要查看 MD5 的配置文件节点";
                goto ERROR1;
            }

            List<string> paths = new List<string>();
            foreach (TreeNode node in nodes)
            {
                string strPath = GetNodePath(node);

                string strExt = Path.GetExtension(strPath);
                if (strExt == ".~state")
                {
                    strError = "不允许查看扩展名为 .~state 的状态文件 (" + strPath + ")";
                    goto ERROR1;
                }
                paths.Add(strPath);
            }

            DownloadFilesEventArgs e1 = new DownloadFilesEventArgs();
            e1.Action = "getmd5";
            e1.FileNames = paths;
            this.DownloadFiles(this, e1);
            if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_refresh(object sender, System.EventArgs e)
        {
            string strError = "";

            /*
            if (this.SelectedNode == null)
            {
                strError = "尚未选择要刷新的节点";
                goto ERROR1;
            }
            */

#if NO
            TreeNode node = this.SelectedNode;
            string strName = node.Text;
            if (node.ImageIndex == RESTYPE_FILE)
                node = node.Parent;

            this.Fill(node);

            // 复原选择
            SelectNode(node, strName);
#endif
            Refresh(this.SelectedNode);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 选择一个TreeNode 下的特定名字的节点
        static bool SelectNode(TreeNode parent, string strNodeName)
        {
            foreach (TreeNode child in parent.Nodes)
            {
                if (child.Text == strNodeName)
                {
                    child.TreeView.SelectedNode = child;
                    return true;
                }
            }

            return false;
        }

        // TODO: 如何限定 dp2library 2.115 以上才能使用此功能?
        // 删除文件
        void menu_deleteFile(object sender, System.EventArgs e)
        {
            string strError = "";

            List<TreeNode> selected_file_nodes = GetCheckedFileNodes();

            if (selected_file_nodes.Count == 0)
            {
                strError = "尚未选择要删除的文件节点";
                goto ERROR1;
            }

#if NO
            if (this.SelectedNode.ImageIndex != RESTYPE_FILE)
            {
                strError = "所选择的节点不是配置文件类型。请选择要删除的配置文件节点";
                goto ERROR1;
            }
#endif
            List<string> paths = new List<string>();
            foreach (TreeNode node in selected_file_nodes)
            {
                string strPath = GetNodePath(node);

                string strExt = Path.GetExtension(strPath);
                if (strExt == ".~state")
                {
                    strError = "不允许删除扩展名为 .~state 的状态文件";
                    goto ERROR1;
                }

                paths.Add(strPath);
            }

            string strNameList = StringUtil.MakePathList(paths);
            if (strNameList.Length > 1000)
                strNameList = strNameList.Substring(0, 1000) + " ... 等 " + selected_file_nodes.Count + " 个文件";

            DialogResult result = MessageBox.Show(this,
"确实要删除文件 " + strNameList + " ?",
"KernelResTree",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            LibraryChannel channel = null;
            TimeSpan old_timeout = new TimeSpan(0);

            channel = this.CallGetChannel(true);

            old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);
            try
            {
                foreach (string strPath in paths)
                {
                    FileItemInfo[] infos = null;

                    string strCurrentDirectory = Path.GetDirectoryName(strPath);
                    string strFileName = Path.GetFileName(strPath);

                    long nRet = channel.ListFile(
                        null,
                        "delete",
                        strCurrentDirectory,
                        strFileName,
                        0,
                        -1,
                        out infos,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

#if NO
                    if (nRet == 1)
                        this.SelectedNode.Remove(); // TODO: 删除任何文件后都要注意刷新去除相伴的 .~state 文件
                    else if (nRet > 1)
                    {
                        this.Fill(this.SelectedNode.Parent);
                    }
                    else
                        goto ERROR1;
#endif
                }
            }
            finally
            {
                channel.Timeout = old_timeout;

                this.CallReturnChannel(channel, true);
            }

            // 刷新显示
            List<TreeNode> parents = FindParentNodes(selected_file_nodes);
            foreach (TreeNode node in parents)
            {
                this.Fill(node);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得共同的 parent fold nodes
        List<TreeNode> FindParentNodes(List<TreeNode> nodes)
        {
            List<TreeNode> results = new List<TreeNode>();
            foreach (TreeNode node in nodes)
            {
                TreeNode parent = node.Parent;
                if (results.IndexOf(parent) == -1)
                    results.Add(parent);
            }

            // TODO: 有父子关系的，只保留父节点
            return results;
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
                dlg.Text = "编辑 " + strPath;
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
                "content,data,metadata,timestamp,outputpath,gzip",  // 2017/10/7 增加 gzip
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

        private void KernelResTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node == null)
                return;

            // 2008/11/17
            if (node.Checked == true)
            {
                node.ForeColor = SystemColors.InfoText;
                node.BackColor = SystemColors.Info;
            }
            else
            {
                node.ForeColor = SystemColors.WindowText;
                node.BackColor = SystemColors.Window;
            }

            if (node.Checked == false)
            {
                ClearOneLevelChildrenCheck(node);
            }
            else
            {
                if (node.Parent != null)
                    node.Parent.Checked = true;
            }

            // 注：事件自己会递归

        }

        // 清除下级所有的选中的项(不包括自己)
        public void ClearOneLevelChildrenCheck(TreeNode nodeStart)
        {
            if (nodeStart == null)
                return;
            foreach (TreeNode node in nodeStart.Nodes)
            {
                node.Checked = false;
                // ClearChildrenCheck(node);	// 暂时不递归
            }
        }
    }

    /// <summary>
    /// 下载文件的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void DownloadFilesEventHandler(object sender,
    DownloadFilesEventArgs e);

    /// <summary>
    /// 下载文件事件的参数
    /// </summary>
    public class DownloadFilesEventArgs : EventArgs
    {
        // 动作。有 download/getmd5 等
        public string Action { get; set; }  // [in]

        public List<string> FileNames { get; set; }  // [in]
        public string ErrorInfo { get; set; }  // [out]
    }

    /// <summary>
    /// 上传文件的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void UploadFilesEventHandler(object sender,
    UploadFilesEventArgs e);

    /// <summary>
    /// 上传文件事件的参数
    /// </summary>
    public class UploadFilesEventArgs : EventArgs
    {
        public string TargetFolder { get; set; }    // [in] 目标位置，目录
        public List<string> SourceFileNames { get; set; }  // [in] 源文件名列表
        public Delegate_end FuncEnd { get; set; }   // [in]
        public string ErrorInfo { get; set; }  // [out]
    }

    public delegate void Delegate_end(bool bError);

}
