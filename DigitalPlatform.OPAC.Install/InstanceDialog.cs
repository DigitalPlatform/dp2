using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.Install;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace DigitalPlatform.OPAC
{
    public partial class InstanceDialog : Form
    {
        FloatingMessageForm _floatingMessage = null;

        public event CopyFilesEventHandler CopyFiles = null;

        public bool UninstallMode = false;

        public bool Changed = false;

        const int COLUMN_SITE = 0;
        const int COLUMN_VIRTUALDIR = 1;
        const int COLUMN_ERRORINFO = 2;
        const int COLUMN_DATADIR = 3;
        const int COLUMN_PHYSICALPATH = 4;

        private MessageBalloon m_firstUseBalloon = null;

        /// <summary>
        /// 调试信息。过程信息
        /// </summary>
        public string DebugInfo
        {
            get;
            set;
        }

        public InstanceDialog()
        {
            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);

#if NO
                if (this._floatingMessage != null)
                    this._floatingMessage.OnResizeOrMove();
#endif
            }
        }

        private void InstanceDialog_Load(object sender, EventArgs e)
        {
            // Debug.Assert(false, "");

            // 卸载状态
            if (UninstallMode == true)
            {
                this.button_OK.Text = "卸载";
                this.button_newInstance.Visible = false;
                this.button_deleteInstance.Visible = false;
                this.button_modifyInstance.Visible = false;
                // this.button_certificate.Visible = false;
            }
            else
            {
                this.button_Cancel.Text = "关闭";
                this.button_OK.Visible = false;
            }


            string strError = "";
            int nRet = FillInstanceList(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                // 安装状态
                if (UninstallMode == false
                    && this.listView_instance.Items.Count == 0)
                {
                    // 提示创建第一个实例
                    ShowMessageTip();
                }
            }

            listView_instance_SelectedIndexChanged(null, null);
        }

        private void listView_instance_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_instance.SelectedItems.Count == 0)
            {
                this.button_modifyInstance.Enabled = false;
                this.button_deleteInstance.Enabled = false;
            }
            else
            {
                this.button_modifyInstance.Enabled = true;
                this.button_deleteInstance.Enabled = true;
            }
        }

        void ShowMessageTip()
        {
            m_firstUseBalloon = new MessageBalloon();
            m_firstUseBalloon.Parent = this.button_newInstance;
            m_firstUseBalloon.Title = "安装 dp2OPAC 图书馆公共查询";
            m_firstUseBalloon.TitleIcon = TooltipIcon.Info;
            m_firstUseBalloon.Text = "请按此按钮创建第一个实例";

            m_firstUseBalloon.Align = BalloonAlignment.BottomRight;
            m_firstUseBalloon.CenterStem = false;
            m_firstUseBalloon.UseAbsolutePositioning = false;
            m_firstUseBalloon.Show();
        }

        void HideMessageTip()
        {
            if (m_firstUseBalloon == null)
                return;

            m_firstUseBalloon.Dispose();
            m_firstUseBalloon = null;
        }

        // 根据已有的配置，填充InstanceList
        // TODO: 另外还需要从 IIS 中寻找名为 dp2OPAC 的虚拟目录。这是为了兼容以前的安装形态
        // TODO: 创建注册表事项的时候，需要把没有写入注册表的实例信息也添加到注册表中。这样，只要看到有注册表事项，就表示只从注册表中取信息就是完整的了，不需要再从 IIS 中试探性取名为 dp2OPAC 的虚拟目录
        int FillInstanceList(out string strError)
        {
            strError = "";

            this.listView_instance.Items.Clear();

            List<OpacAppInfo> infos = null;
            int nRet = OpacAppInfo.GetOpacInfo(out infos, out strError);
            if (nRet == -1)
                return -1;

            int nErrorCount = 0;
            foreach (OpacAppInfo info in infos)
            {
                string strSite = "";
                string strVirtualDir = "";
                StringUtil.ParseTwoPart(info.IisPath, "/", out strSite, out strVirtualDir);

                strVirtualDir = "/" + strVirtualDir;

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_SITE, strSite);
                ListViewUtil.ChangeItemText(item, COLUMN_VIRTUALDIR, strVirtualDir);
                ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, info.DataDir);
                ListViewUtil.ChangeItemText(item, COLUMN_PHYSICALPATH, info.PhysicalPath);
                this.listView_instance.Items.Add(item);
                LineInfo line_info = new LineInfo();
                item.Tag = line_info;
                // line_info.PhysicalPath = info.PhysicalPath;

                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                nRet = line_info.Build(info.DataDir,
                    out strError);
                if (nRet == -1)
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, strError);
                    item.BackColor = Color.Red;
                    item.ForeColor = Color.White;

                    nErrorCount++;
                }
            }

            if (nErrorCount > 0)
                this.listView_instance.Columns[COLUMN_ERRORINFO].Width = 200;
            else
                this.listView_instance.Columns[COLUMN_ERRORINFO].Width = 0;

            return 0;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

#if NO
            // 全部卸载
            if (this.UninstallMode == true)
            {
                nRet = this.DeleteAllInstanceAndDataDir(out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
                return;
            }
#endif

#if NO
            // 进行检查
            // return:
            //      -1  发现错误
            //      0   放弃整个保存操作
            //      1   一切顺利
            nRet = DoVerify(out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            nRet = DoModify(out strError);
            if (nRet == -1)
                goto ERROR1;
#endif

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
#if NO
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void InstanceDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            HideMessageTip();

            if (_floatingMessage != null)
                _floatingMessage.Close();
        }

        string GetAppPhysicalPath(string strSite,
            string strVirtualDir)
        {

            // 尝试用虚拟目录名
            string strAppDir = Path.Combine(
    Environment.ExpandEnvironmentVariables("%SystemDrive%\\inetpub\\wwwroot"),
    strVirtualDir.Replace("/", ""));    // 注意去掉非法路径字符
            if (Directory.Exists(strAppDir) == false)
                return strAppDir;

            // 尝试用站点名加上虚拟目录名
            for (int i = 0; ; i++)
            {
                strAppDir = Path.Combine(
    Environment.ExpandEnvironmentVariables("%SystemDrive%\\inetpub\\wwwroot"),
    strSite.Replace(" ", "") + "_" + strVirtualDir.Replace("/", ""))    // 注意去掉非法路径字符
    + (i == 0 ? "" : (i + 1).ToString());
                if (Directory.Exists(strAppDir) == false)
                    return strAppDir;
            }
        }

        // 创建一个实例。
        // 创建数据目录。创建或者修改 opac.xml文件
        int CreateInstance(ListViewItem item,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            List<OpacAppInfo> all_infos = null; // IIS 中的全部虚拟目录
            // 获得全部的虚拟目录信息，不光是 dp2OPAC 有关的
            nRet = OpacAppInfo.GetAllVirtualInfoByAppCmd(out all_infos, out strError);
            if (nRet == -1)
                return -1;

            List<OpacAppInfo> infos = null; // dp2OPAC 类型的全部虚拟目录
            // 获得全部 dp2OPAC 有关的
            nRet = OpacAppInfo.GetOpacInfoByAppCmd(out infos, out strError);
            if (nRet == -1)
                return -1;

            LineInfo line_info = (LineInfo)item.Tag;
            string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
            string strSite = ListViewUtil.GetItemText(item, COLUMN_SITE);
            string strVirtualDir = ListViewUtil.GetItemText(item, COLUMN_VIRTUALDIR);
            string strPhysicalPath = ListViewUtil.GetItemText(item, COLUMN_PHYSICALPATH);

            if (String.IsNullOrEmpty(strDataDir) == true)
            {
                strError = "数据目录尚未设置";
                return -1;
            }

            // 查找一个虚拟目录是否存在
            // return:
            //      -1  不存在
            //      其他  数组元素的下标
            nRet = OpacAppInfo.Find(infos,
                strSite,
                strVirtualDir);
            if (nRet == -1)
            {
                nRet = OpacAppInfo.Find(all_infos,
    strSite,
    strVirtualDir);
                if (nRet != -1)
                {
                    // 这表明这是一个已经存在的虚拟目录，但不是 dp2OPAC 类型的
                    strError = "虚拟目录 '" + strSite + strVirtualDir + "' 名字和一个非 dp2OPAC 类型的已经存在的虚拟目录发生了冲突，创建或者修改操作失败";
                    return -1;
                }

#if NO
                string strAppDir = Path.Combine(
                    Environment.ExpandEnvironmentVariables("%SystemDrive%\\inetpub\\wwwroot"),
                    strVirtualDir.Replace("/", ""));    // 注意去掉非法路径字符
#endif
                if (string.IsNullOrEmpty(strPhysicalPath) == true)
                {
                    strPhysicalPath = GetAppPhysicalPath(strSite,
                        strVirtualDir);
                    PathUtil.TryCreateDir(strPhysicalPath);
                }

                // 创建程序目录，并复制进基本内容
                nRet = CreateNewAppDir(strPhysicalPath,
                    strDataDir,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 注册 Web App
                // 只能用于 IIS 7 以上版本
                nRet = OpacAppInfo.RegisterWebApp(
                    strSite,
                    strVirtualDir,
                    strPhysicalPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "创建新的虚拟目录(site=" + strSite + ";virtual_dir=" + strVirtualDir + ";physicalPath=" + strPhysicalPath + ")失败: " + strError;
                    return -1;
                }

                ListViewUtil.ChangeItemText(item, COLUMN_PHYSICALPATH, strPhysicalPath);
                // line_info.PhysicalPath = strAppDir;
            }
            else
            {

                // 数据目录有可能修改，因此需要修改程序目录中的 start.xml
#if NO
                OpacAppInfo info = all_infos[nRet];
                Debug.Assert(string.IsNullOrEmpty(info.PhysicalPath) == false, "");
#endif
                Debug.Assert(string.IsNullOrEmpty(strPhysicalPath) == false, "");

                // 修改 start.xml
                nRet = CreateStartXml(Path.Combine(strPhysicalPath, "start.xml"),
                            strDataDir,
                            out strError);
                if (nRet == -1)
                    return -1;
            }

            // 探测数据目录，是否已经存在数据，是不是属于升级情形
            // return:
            //      -1  error
            //      0   数据目录不存在
            //      1   数据目录存在，但是xml文件不存在
            //      2   xml文件已经存在
            nRet = DetectDataDir(strDataDir,
        out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 2)
            {
                // 进行升级检查

                // TODO: 是否要为数据目录增配权限

            }
            else
            {
                // 需要进行最新安装，创建数据目录
                nRet = CreateNewDataDir(strDataDir, out strError);
                if (nRet == -1)
                    return -1;
            }

            // 兑现修改
            if (line_info.Changed == true)
            {
                // 保存信息到 opac.xml文件中
                // return:
                //      -1  error
                //      0   succeed
                nRet = line_info.SaveToXml(strDataDir,
                    out strError);
                if (nRet == -1)
                    return -1;

                line_info.Changed = false;
            }

            return 0;
        }

        // 进行检查
        // return:
        //      -1  发现错误
        //      0   放弃整个保存操作
        //      1   一切顺利
        int DoVerify(out string strError)
        {
            strError = "";

#if NO
            List<string> instance_names = new List<string>();
            List<string> data_dirs = new List<string>();

            // 检查实例名、数据目录是否重复
            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                string strBindings = ListViewUtil.GetItemText(item, COLUMN_BINDINGS);

                if (HasDataDirDup(strDataDir, data_dirs) == true)
                {
                    strError = "行 " + (i + 1).ToString() + " 的数据目录 '" + strDataDir + "' 和前面某行的数据目录发生了重复";
                    return -1;
                }

                if (instance_names.IndexOf(strInstanceName) != -1)
                {
                    strError = "行 " + (i + 1).ToString() + " 的实例名 '" + strInstanceName + "' 和前面某行的实例名发生了重复";
                    return -1;
                }

                data_dirs.Add(strDataDir);
                instance_names.Add(strInstanceName);

                if (String.IsNullOrEmpty(strDataDir) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的数据目录尚未设置";
                    return -1;
                }

                if (String.IsNullOrEmpty(strBindings) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的协议绑定尚未设置";
                    return -1;
                }
            }

            // TODO: 检查绑定之间的端口是否冲突
            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                string strBindings = ListViewUtil.GetItemText(item, COLUMN_BINDINGS);

                // return:
                //      -1  出错
                //      0   不重
                //      1    重复
                int nRet = IsBindingDup(strBindings,
            item,
            out strError);
                if (nRet != 0)
                {
                    strError = "实例名为 '" + strInstanceName + "' (第 " + (i + 1).ToString() + " 行)的协议绑定发生错误或者冲突: " + strError;
                    return -1;
                }

                nRet = InstallHelper.IsGlobalBindingDup(strBindings,
                    "dp2Library",
                    out strError);
                if (nRet != 0)
                {
                    strError = "实例名为 '" + strInstanceName + "' (第 " + (i + 1).ToString() + " 行)的协议绑定发生错误或者冲突: " + strError;
                    return -1;
                }
            }

            // 警告XML文件格式不正确、XML文件未找到的错误
            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);

                if (info.XmlFileNotFound == true)
                {
                    string strText = "实例 '" + item.Text + "' 的数据目录 '" + strDataDir + "' 中没有找到 library.xml 文件。\r\n\r\n要对这个数据目录进行全新安装么?\r\n\r\n(是)进行全新安装 (否)不进行任何修改和安装 (取消)放弃全部保存操作";
                    DialogResult result = MessageBox.Show(
                        this,
        strText,
        "setup_dp2library",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        info.Changed = false;
                    }
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        info.XmlFileNotFound = false;
                        info.Changed = true;
                    }
                    if (result == System.Windows.Forms.DialogResult.Cancel)
                    {
                        strError = "放弃全部保存操作";
                        return 0;
                    }
                }

                if (info.XmlFileContentError == true)
                {
                    string strText = "实例 '" + item.Text + "' 的数据目录 '" + strDataDir + "' 中已经存在的 library.xml 文件(XML)格式不正确。程序无法对它进行读取操作\r\n\r\n要对这个数据目录进行全新安装么? 这将刷新整个目录(包括database.xml文件)到最初状态\r\n\r\n(是)进行全新安装 (否)不进行任何修改和安装 (取消)放弃全部保存操作";
                    DialogResult result = MessageBox.Show(
                        this,
        strText,
        "setup_dp2library",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        info.Changed = false;
                    }
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        info.Changed = true;
                        info.XmlFileNotFound = false;
                        info.XmlFileContentError = false;
                        // TODO: 是否要进行备份?
                        File.Delete(PathUtil.MergePath(strDataDir, "library.xml"));
                    }
                    if (result == System.Windows.Forms.DialogResult.Cancel)
                    {
                        strError = "放弃全部保存操作";
                        return 0;
                    }
                }

            }

#endif

            return 1;
        }

#if NO
        // 兑现修改。
        // 创建数据目录。创建或者修改 opac.xml文件
        int DoModify(out string strError)
        {
            strError = "";
            int nRet = 0;

            List<OpacAppInfo> infos = null;
            nRet = OpacAppInfo.GetOpacInfo(out infos, out strError);
            if (nRet == -1)
                return -1;

            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo line_info = (LineInfo)item.Tag;
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                string strSite = ListViewUtil.GetItemText(item, COLUMN_SITE);
                string strVirtualDir = ListViewUtil.GetItemText(item, COLUMN_VIRTUALDIR);

                if (String.IsNullOrEmpty(strDataDir) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的数据目录尚未设置";
                    return -1;
                }

                // 探测虚拟目录是否已经创建
                // infos 中

                // 查找一个虚拟目录是否存在
                // return:
                //      -1  不存在
                //      其他  数组元素的下标
                nRet = OpacAppInfo.Find(infos,
                    strSite,
                    strVirtualDir);
                if (nRet == -1)
                {
                    string strAppDir = Path.Combine(
                        Environment.ExpandEnvironmentVariables("%SystemDrive%\\inetpub\\wwwroot"),
                        strVirtualDir.Replace("/", ""));    // 注意去掉非法路径字符
                    PathUtil.CreateDirIfNeed(strAppDir);

                    // 创建程序目录，并复制进基本内容
                    nRet = CreateNewAppDir(strAppDir,
                        strDataDir,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 注册 Web App
                    // 只能用于 IIS 7 以上版本
                    nRet = OpacAppInfo.RegisterWebApp(
                        strSite,
                        strVirtualDir,
                        strAppDir,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "创建新的虚拟目录(site="+strSite+";virtual_dir="+strVirtualDir+";physicalPath="+strAppDir+")失败: " + strError;
                        return -1;
                    }

                    line_info.PhysicalPath = strAppDir;
                }

                // 探测数据目录，是否已经存在数据，是不是属于升级情形
                // return:
                //      -1  error
                //      0   数据目录不存在
                //      1   数据目录存在，但是xml文件不存在
                //      2   xml文件已经存在
                nRet = DetectDataDir(strDataDir,
            out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 2)
                {
                    // 进行升级检查


                }
                else
                {
                    // 需要进行最新安装，创建数据目录
                    nRet = CreateNewDataDir(strDataDir,
    out strError);
                    if (nRet == -1)
                        return -1;
                }


                // 兑现修改
                if (line_info.Changed == true)
                {
                    // 保存信息到 opac.xml文件中
                    // return:
                    //      -1  error
                    //      0   succeed
                    nRet = line_info.SaveToXml(strDataDir,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    line_info.Changed = false;
                }

            }

#if NO
            // 删除注册表中多余的instance信息
            for (int i = this.listView_instance.Items.Count; ; i++)
            {
                // 删除虚拟目录。数据目录不用删除，因为界面操作当时已经删除过了


            }
#endif

            return 0;
        }
#endif
        // 探测数据目录，是否已经存在数据，是不是属于升级情形
        // return:
        //      -1  error
        //      0   数据目录不存在
        //      1   数据目录存在，但是xml文件不存在
        //      2   xml文件已经存在
        public static int DetectDataDir(string strDataDir,
            out string strError)
        {
            strError = "";

            try
            {
                DirectoryInfo di = new DirectoryInfo(strDataDir);
                if (di.Exists == false)
                    return 0;

                string strExistingFileName = Path.Combine(strDataDir,
                    "opac.xml");
                if (File.Exists(strExistingFileName) == true)
                    return 2;

                return 1;
            }
            catch (Exception ex)
            {
                strError = "检测数据目录名 '" + strDataDir + "' 时出现异常: " + ex.Message;
                return -1;
            }
        }

        // 创建数据目录，并复制进基本内容
        int CreateNewDataDir(string strDataDir,
            out string strError)
        {
            strError = "";

            try
            {
                PathUtil.TryCreateDir(strDataDir);
            }
            catch (Exception ex)
            {
                // 2018/1/27
                strError = "CreateNewDataDir() 出现异常: " + ex.Message;
                return -1;
            }

            Debug.Assert(this.CopyFiles != null, "");

            CopyFilesEventArgs e = new CopyFilesEventArgs();
            e.Action = "data";
            e.DataDir = strDataDir;
            this.CopyFiles(this, e);
            if (string.IsNullOrEmpty(e.ErrorInfo) == false)
            {
                strError = "拷贝文件到数据目录 '" + strDataDir + "' 时发生错误：" + e.ErrorInfo;
                return -1;
            }

            return 0;
        }

        // 创建程序目录，并复制进基本内容
        // parameters:
        //      strDataDir  要写入 start.xml 中的数据目录路径
        int CreateNewAppDir(string strAppDir,
            string strDataDir,
            out string strError)
        {
            strError = "";

            PathUtil.TryCreateDir(strAppDir);

            Debug.Assert(this.CopyFiles != null, "");

            CopyFilesEventArgs e = new CopyFilesEventArgs();
            e.Action = "app";
            e.DataDir = strAppDir;
            this.CopyFiles(this, e);
            if (string.IsNullOrEmpty(e.ErrorInfo) == false)
            {
                strError = "拷贝文件到程序目录 '" + strAppDir + "' 时发生错误：" + e.ErrorInfo;
                return -1;
            }

            // 修改 start.xml
            int nRet = CreateStartXml(Path.Combine(strAppDir, "start.xml"),
                        strDataDir,
                        out strError);
            if (nRet == -1)
                return -1;

            // 创建 EventLog 2016/11/26
            {
                // 创建事件日志目录
                if (!EventLog.SourceExists("dp2opac"))
                    EventLog.CreateEventSource("dp2opac", "DigitalPlatform");

                EventLog Log = new EventLog();
                Log.Source = "dp2opac";

                Log.WriteEntry("dp2OPAC 安装成功。", EventLogEntryType.Information);
            }

            return 0;
        }

        // 创建start.xml文件
        // parameters:
        //      strFileName start.xml文件名
        private int CreateStartXml(string strFileName,
            string strDataDir,
            out string strError)
        {
            strError = "";

            // TODO: 是否可以修改为装载已有文件，然后修改必要的元素和属性 ?
            try
            {
                string strXml = "<root datadir=''/>";

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);

                DomUtil.SetAttr(dom.DocumentElement, "datadir", strDataDir);

                dom.Save(strFileName);

                return 0;
            }
            catch (Exception ex)
            {
                strError = "创建 start.xml 文件时出错：" + ex.Message;
                return -1;
            }
        }

        private void button_newInstance_Click(object sender, EventArgs e)
        {
            HideMessageTip();

            OneInstanceDialog new_instance_dlg = new OneInstanceDialog();
            GuiUtil.AutoSetDefaultFont(new_instance_dlg);
            new_instance_dlg.Text = "创建一个新实例";
            new_instance_dlg.CreateMode = true;
#if NO
            if (this.listView_instance.Items.Count == 0)
            {
                new_instance_dlg.InstanceName = "/dp2OPAC";
            }
            else
            {
                new_instance_dlg.InstanceName = GetNewInstanceName(this.listView_instance.Items.Count + 1);
            }
#endif

            new_instance_dlg.VerifyInstanceName += new VerifyEventHandler(new_instance_dlg_VerifyInstanceName);
            new_instance_dlg.VerifyDataDir += new VerifyEventHandler(new_instance_dlg_VerifyDataDir);
            new_instance_dlg.LoadXmlFileInfo += new LoadXmlFileInfoEventHandler(new_instance_dlg_LoadXmlFileInfo);

            new_instance_dlg.StartPosition = FormStartPosition.CenterScreen;
            if (new_instance_dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, COLUMN_SITE, new_instance_dlg.SiteName);
            ListViewUtil.ChangeItemText(item, COLUMN_VIRTUALDIR, new_instance_dlg.InstanceName);
            ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, new_instance_dlg.DataDir);
            this.listView_instance.Items.Add(item);

            new_instance_dlg.LineInfo.Changed = true;
            item.Tag = new_instance_dlg.LineInfo;

            ListViewUtil.SelectLine(item, true);

            this.Changed = true;

            string strError = "";

            // TODO: 最好出现一个浮动窗口显示正在创建实例
            this._floatingMessage.Text = "正在创建实例，请稍候 ...";
            this.Enabled = false;
            try
            {
                int nRet = CreateInstance(item,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.Enabled = true;
                this._floatingMessage.Text = "";
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        ListViewItem m_currentEditItem = null;

        private void button_modifyInstance_Click(object sender, EventArgs e)
        {
            string strError = "";

            HideMessageTip();

            if (this.listView_instance.SelectedItems.Count == 0)
            {
                strError = "尚未选择要修改的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_instance.SelectedItems[0];
            this.m_currentEditItem = item;

            OneInstanceDialog modify_instance_dlg = new OneInstanceDialog();
            GuiUtil.AutoSetDefaultFont(modify_instance_dlg);
            modify_instance_dlg.Text = "修改一个实例";
            modify_instance_dlg.CreateMode = false;

            modify_instance_dlg.SiteName = ListViewUtil.GetItemText(item, COLUMN_SITE);
            modify_instance_dlg.InstanceName = ListViewUtil.GetItemText(item, COLUMN_VIRTUALDIR);

            modify_instance_dlg.DataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
            modify_instance_dlg.LineInfo = (LineInfo)item.Tag;

            modify_instance_dlg.VerifyInstanceName += new VerifyEventHandler(modify_instance_dlg_VerifyInstanceName);
            modify_instance_dlg.VerifyDataDir += new VerifyEventHandler(modify_instance_dlg_VerifyDataDir);
            modify_instance_dlg.LoadXmlFileInfo += new LoadXmlFileInfoEventHandler(modify_instance_dlg_LoadXmlFileInfo);


            modify_instance_dlg.StartPosition = FormStartPosition.CenterScreen;
            if (modify_instance_dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewUtil.ChangeItemText(item, COLUMN_SITE, modify_instance_dlg.SiteName);
            ListViewUtil.ChangeItemText(item, COLUMN_VIRTUALDIR, modify_instance_dlg.InstanceName);
            ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, modify_instance_dlg.DataDir);

            modify_instance_dlg.LineInfo.Changed = true;
            item.Tag = modify_instance_dlg.LineInfo;

            ListViewUtil.SelectLine(item, true);

            this.Changed = true;

            // TODO: 最好出现一个浮动窗口显示正在创建实例
            this.Enabled = false;
            try
            {
                int nRet = CreateInstance(item,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.Enabled = true;
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void modify_instance_dlg_LoadXmlFileInfo(object sender, LoadXmlFileInfoEventArgs e)
        {
            Debug.Assert(String.IsNullOrEmpty(e.DataDir) == false, "");

            string strError = "";
            LineInfo info = new LineInfo();
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            int nRet = info.Build(e.DataDir,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            Debug.Assert(nRet == 1, "");

            e.LineInfo = info;
        }

        void modify_instance_dlg_VerifyDataDir(object sender, VerifyEventArgs e)
        {
            bool bRet = IsDataDirDup(e.Value,
                this.m_currentEditItem);
            if (bRet == true)
                e.ErrorInfo = "数据目录 '" + e.Value + "' 和已存在的其他实例发生了重复";
        }

        void modify_instance_dlg_VerifyInstanceName(object sender, VerifyEventArgs e)
        {
            bool bRet = IsInstanceNameDup(e.Value,
                this.m_currentEditItem);
            if (bRet == true)
                e.ErrorInfo = "实例名 '" + e.Value + "' 和已存在的其他实例发生了重复";
        }

        private void button_deleteInstance_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            HideMessageTip();

            if (this.listView_instance.SelectedItems.Count == 0)
            {
                strError = "尚未选择要删除的事项";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
    "确实要删除所选择的 " + this.listView_instance.SelectedItems.Count.ToString() + " 个实例?",
    "InstanceDialog",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            // 删除应用程序目录
            foreach (ListViewItem item in this.listView_instance.SelectedItems)
            {
                string strSite = ListViewUtil.GetItemText(item, COLUMN_SITE);
                string strVirtualDir = ListViewUtil.GetItemText(item, COLUMN_VIRTUALDIR);
                string strPhysicalPath = ListViewUtil.GetItemText(item, COLUMN_PHYSICALPATH);

                // 注销 Web App。调用后，程序物理目录没有删除。apppool 没有删除。
                // 只能用于 IIS 7 以上版本
                nRet = OpacAppInfo.UnregisterWebApp(
            strSite,
            strVirtualDir,
            out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                // 删除应用程序物理目录
                //LineInfo info = (LineInfo)item.Tag;
                //if (info != null)
                {
                    if (string.IsNullOrEmpty(strPhysicalPath) == false)
                    {
                        // return:
                        //      -1  出错。包括出错后重试然后放弃
                        //      0   成功
                        nRet = InstallHelper.DeleteDataDir(strPhysicalPath,
            out strError);
                        if (nRet == -1)
                            MessageBox.Show(this, strError);
                    }
                }
            }



            List<string> datadirs = new List<string>();
            foreach (ListViewItem item in this.listView_instance.SelectedItems)
            {
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                if (String.IsNullOrEmpty(strDataDir) == true)
                    continue;

                if (Directory.Exists(strDataDir) == false)
                    continue;

                datadirs.Add(strDataDir);
            }

            ListViewUtil.DeleteSelectedItems(this.listView_instance);

            this.Changed = true;

            // 如果数据目录已经存在，提示是否连带删除数据目录
            if (datadirs.Count > 0)
            {
                strError = "";
                result = MessageBox.Show(this,
    "所选定的实例信息已经删除。\r\n\r\n要删除它们所对应的下列数据目录么?\r\n" + StringUtil.MakePathList(datadirs, "\r\n"),
    "InstanceDialog",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    foreach (string strDataDir in datadirs)
                    {
                        string strTempError = "";
                        // return:
                        //      -1  出错。包括出错后重试然后放弃
                        //      0   成功
                        nRet = InstallHelper.DeleteDataDir(strDataDir,
            out strTempError);
                        if (nRet == -1)
                            strError += strTempError + "\r\n";
                    }
                    if (String.IsNullOrEmpty(strError) == false)
                        goto ERROR1;
                }
            }

            return;
            ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        private void listView_instance_DoubleClick(object sender, EventArgs e)
        {
            button_modifyInstance_Click(sender, e);
        }

        void new_instance_dlg_LoadXmlFileInfo(object sender, LoadXmlFileInfoEventArgs e)
        {
            Debug.Assert(String.IsNullOrEmpty(e.DataDir) == false, "");

            string strError = "";
            LineInfo info = new LineInfo();
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            int nRet = info.Build(e.DataDir,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            Debug.Assert(nRet == 1, "");

            e.LineInfo = info;
        }


        void new_instance_dlg_VerifyDataDir(object sender, VerifyEventArgs e)
        {
            bool bRet = IsDataDirDup(e.Value,
                (ListViewItem)null);
            if (bRet == true)
                e.ErrorInfo = "数据目录 '" + e.Value + "' 和已存在的其他实例发生了重复";
        }

        void new_instance_dlg_VerifyInstanceName(object sender, VerifyEventArgs e)
        {
            bool bRet = IsInstanceNameDup(e.Value,
                (ListViewItem)null);
            if (bRet == true)
                e.ErrorInfo = "实例名 '" + e.Value + "' 和已存在的其他实例发生了重复";
        }

        // parameters:
        //      strInstanceName 站点名和虚拟目录名的组合。例如 Default Web Site/dp2OPAC
        // return:
        //      false   不重
        //      true    重复
        bool IsInstanceNameDup(string strInstanceName,
            ListViewItem exclude_item)
        {
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                if (item == exclude_item)
                    continue;
                string strCurrent = ListViewUtil.GetItemText(item, COLUMN_SITE) + ListViewUtil.GetItemText(item, COLUMN_VIRTUALDIR);
                if (String.Compare(strInstanceName, strCurrent, true) == 0)
                    return true;
            }

            return false;
        }

        // return:
        //      false   不重
        //      true    重复
        bool IsDataDirDup(string strDataDir,
            ListViewItem exclude_item)
        {
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                if (item == exclude_item)
                    continue;
                string strCurrent = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                if (String.IsNullOrEmpty(strCurrent) == true)
                    continue;

                if (PathUtil.IsEqual(strDataDir, strCurrent) == true)
                    return true;
            }

            return false;
        }

        private void InstanceDialog_Move(object sender, EventArgs e)
        {
#if NO
            if (this._floatingMessage != null)
                this._floatingMessage.OnResizeOrMove();
#endif
        }

#if NO
        // 获得一个目前尚未被使用过的instancename值
        string GetNewInstanceName(int nStart)
        {
        REDO:
            string strResult = "/dp2OPAC" + nStart.ToString();
            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_VIRTUALDIR);

                if (string.Compare(strResult, strInstanceName, true) == 0)
                {
                    nStart++;
                    goto REDO;
                }
            }

            return strResult;
        }
#endif
    }

    // ListView中每一行的隐藏信息
    public class LineInfo
    {
        public string CertificateSN = "";

        public string SerialNumber = "";

        // *** dp2library 服务器信息
        // dp2library URL
        public string LibraryUrl = "";
        // username
        public string LibraryUserName = "";
        // password
        public string LibraryPassword = "";
        // dp2library datadir
        public string LibraryReportDir = "";

#if NO
        // *** supervisor 账户信息
        public string SupervisorUserName = null;
        public string SupervisorPassword = null;  // null表示不修改以前的密码
        public string SupervisorRights = null;

        //
        public string LibraryName = "";
#endif

        // 应用程序的物理路径
        // public string PhysicalPath = "";

        // 内容是否发生过修改
        public bool Changed = false;

        // XML文件没有找到
        public bool XmlFileNotFound = false;
        // XML文件内容格式错误
        public bool XmlFileContentError = false;

        public void Clear()
        {
            this.LibraryUrl = "";
            this.LibraryUserName = "";
            this.LibraryPassword = "";

            this.XmlFileNotFound = false;
            this.XmlFileContentError = false;
            this.Changed = false;
        }

        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        public int Build(string strDataDir,
            out string strError)
        {
            strError = "";

            this.Clear();

            string strFilename = Path.Combine(strDataDir, "opac.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                this.XmlFileNotFound = true;
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                this.XmlFileContentError = true;
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ex.Message;
                return -1;
            }

            XmlNode library_server = dom.DocumentElement.SelectSingleNode("libraryServer");
            if (library_server == null)
            {
                strError = "文件 " + strFilename + " 内容不合法，根下的<libraryServer>元素不存在。";
                return -1;
            }

            // DomUtil.SetAttr(nodeDatasource, "mode", null);

            this.LibraryUrl = DomUtil.GetAttr(library_server, "url");
            this.LibraryUserName = DomUtil.GetAttr(library_server, "username");

            this.LibraryPassword = DomUtil.GetAttr(library_server, "password");
            if (string.IsNullOrEmpty(this.LibraryPassword) == false)
            {
                try
                {
                    this.LibraryPassword = Cryptography.Decrypt(this.LibraryPassword, "dp2circulationpassword");
                }
                catch
                {
                    strError = "<libraryServer password='???' /> 中的密码不正确";
                    return -1;
                }
            }

            this.LibraryReportDir = DomUtil.GetAttr(library_server, "reportDir");

            return 1;
        }

        // return:
        //      -1  error
        //      0   succeed
        public int SaveToXml(string strDataDir,
            out string strError)
        {
            strError = "";

            string strFilename = Path.Combine(strDataDir, "opac.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + " 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ex.Message;
                return -1;
            }

            XmlNode library_server = dom.DocumentElement.SelectSingleNode("libraryServer");
            if (library_server == null)
            {
                library_server = dom.CreateElement("libraryServer");
                dom.DocumentElement.AppendChild(library_server);
            }

            DomUtil.SetAttr(library_server,
                "url",
                this.LibraryUrl);
            DomUtil.SetAttr(library_server,
                 "username",
                 this.LibraryUserName);

            string strPassword = Cryptography.Encrypt(this.LibraryPassword, "dp2circulationpassword");
            DomUtil.SetAttr(library_server,
                "password",
                strPassword);

            DomUtil.SetAttr(library_server,
                "reportDir",
                this.LibraryReportDir);

            dom.Save(strFilename);
            return 0;
        }


    }

}

