using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.CSharp;
using Microsoft.VisualBasic;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;

namespace DigitalPlatform.Script
{
    /// <summary>
    /// 脚本管理
    /// </summary>
    public class ScriptManager
    {
        public event CreateDefaultContentEventHandler CreateDefaultContent = null;

        public ApplicationInfo applicationInfo = null;

        public static int m_nLockTimeout = 5000;	// 5000=5秒

        public string CfgFilePath = "";	// 配置文件名

        XmlDocument dom = new XmlDocument();

        bool m_bChanged = false;	// DOM是否有修改

        public string DefaultCodeFileDir = "";

        public string DataDir = ""; // 数据目录

        public ScriptManager()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        // 保存
        public void Save()
        {
            if (m_bChanged == true &&
                CfgFilePath != "")
            {
                dom.Save(CfgFilePath);
                m_bChanged = false;
            }

        }

        // 内存内容是否发生过改变
        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return m_bChanged;
            }
            set
            {
                m_bChanged = value;
            }
        }

        // 从xml文件中装载内容到内存
        public void Load(bool bAutoCreate = true)
        {
            if (string.IsNullOrEmpty(CfgFilePath) == true)
                throw (new ArgumentException("ScriptManager.CfgFilePath 成员值不应为空"));

            try
            {
                dom.Load(this.CfgFilePath);
            }
            catch (FileNotFoundException)
            {
                // 2011/11/13 自动创建
                if (bAutoCreate == false)
                    throw;

                /*
                // 触发事件
                if (this.CreateProjectXmlFile != null)
                {
                    AutoCreateProjectXmlFileEventArgs e1 = new AutoCreateProjectXmlFileEventArgs();
                    e1.Filename = scriptManager.CfgFilePath;
                    this.CreateProjectXmlFile(this, e1);
                }
                 * */

                ScriptManager.CreateDefaultProjectsXmlFile(this.CfgFilePath,
                    "clientcfgs");

                dom.Load(this.CfgFilePath);
            }
            catch (Exception ex0)
            {
                throw new Exception("装载文件 '" + this.CfgFilePath + "' 到 XmlDocument 时出错: " + ex0.Message, ex0);
            }

            // 缺省代码目录
            DefaultCodeFileDir = DomUtil.GetAttr(
                dom.DocumentElement,
                ".",
                "defaultCodeFileDir");
            if (string.IsNullOrEmpty(DefaultCodeFileDir) == true)
            {
#if NO
                // 设置为执行程序目录之下clientcfgs目录
                DefaultCodeFileDir =
                    Path.Combine(Environment.CurrentDirectory, "clientcfgs");
#endif
                if (String.IsNullOrEmpty(this.DataDir) == true)
                    throw new Exception("当 defaultCodeFileDir 属性为空时，需要定义 this.DataDir 值");

                DefaultCodeFileDir = Path.Combine(this.DataDir, "clientcfgs");

            }
            else
            {
                string strPath = "";
                if (Path.IsPathRooted(DefaultCodeFileDir) == true)
                    strPath = DefaultCodeFileDir;
                else
                {
                    if (String.IsNullOrEmpty(this.DataDir) == true)
                        throw new Exception("当DefaultCodeFileDir=" + DefaultCodeFileDir + "为相对路径时，需要定义this.DataDir帮助确定全路径。");
                    strPath = Path.Combine(this.DataDir, DefaultCodeFileDir);
                }

                DirectoryInfo di = new DirectoryInfo(strPath);
                DefaultCodeFileDir = di.FullName;
            }
        }

        string MacroPath(string strPath)
        {
            if (String.IsNullOrEmpty(this.DefaultCodeFileDir) == true)
                return strPath;

            // 测试strPath1是否为strPath2的下级目录或文件
            if (PathUtil.IsChildOrEqual(strPath, this.DefaultCodeFileDir) == true)
            {
                string strPart = strPath.Substring(this.DefaultCodeFileDir.Length);
                return "%default_code_file_dir%" + strPart;
            }

            return strPath;
        }

        string UnMacroPath(string strPath)
        {
            if (String.IsNullOrEmpty(this.DefaultCodeFileDir) == true)
                return strPath;

            return strPath.Replace("%default_code_file_dir%", this.DefaultCodeFileDir);
        }

        // 将配置文件的内容填入TreeView
        public void FillTree(TreeView treeView)
        {
            /*
            if (CfgFilePath == "") 
            {
                throw(new Exception("CfgFilePath成员值为空"));
            }

            dom.Load(CfgFilePath);
            */
            if (this.dom.DocumentElement == null)
                Load();

            treeView.Nodes.Clear();// 清除以前残余的

            FillOneLevel(treeView, null, dom.DocumentElement);

            /*
            // 缺省代码目录
            DefaultCodeFileDir = DomUtil.GetAttr(
                dom.DocumentElement, 
                ".",
                "defaultCodeFileDir");
            if (DefaultCodeFileDir == "")
            {
                // 设置为执行程序目录之下clientcfgs目录
                DefaultCodeFileDir = 
                    Environment.CurrentDirectory +"\\clientcfgs";

            }
            else 
            {
                DirectoryInfo di = new DirectoryInfo(DefaultCodeFileDir);

                DefaultCodeFileDir = di.FullName;
            }
            */
        }

        // 刷新全部显示
        public void RefreshTree(TreeView treeView)
        {
            FillOneLevel(treeView, null, dom.DocumentElement);
        }

        // 填充treeview一级(以及以下全部级)的内容
        public void FillOneLevel(TreeView treeView,
            TreeNode treeNode,
            XmlNode node)
        {
            XmlNode nodeChild = null;

            // 清除以前残余的
            if (treeNode == null)
                treeView.Nodes.Clear();
            else
                treeNode.Nodes.Clear();

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                nodeChild = node.ChildNodes[i];
                if (nodeChild.NodeType != XmlNodeType.Element)
                    continue;

                if (nodeChild.Name == "dir")
                {
                    // 新node
                    string strDirName = DomUtil.GetAttr(nodeChild, "name");

                    TreeNode nodeNew = new TreeNode(strDirName, 0, 0);


                    if (treeNode == null)
                        treeView.Nodes.Add(nodeNew);
                    else
                        treeNode.Nodes.Add(nodeNew);

                    // 递归
                    FillOneLevel(treeView,
                        nodeNew,
                        nodeChild);
                }
                else if (nodeChild.Name == "project")
                {
                    // 新node
                    string strProjectName = DomUtil.GetAttr(nodeChild, "name");

                    TreeNode nodeNew = new TreeNode(strProjectName, 1, 1);


                    if (treeNode == null)
                        treeView.Nodes.Add(nodeNew);
                    else
                        treeNode.Nodes.Add(nodeNew);
                }
            }
        }

        public List<string> GetAllProjectNames(out string strError)
        {
            strError = "";

            List<string> results = new List<string>();

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//project");

            foreach (XmlElement node in nodes)
            {
                string strName = node.GetAttribute("name");
                // TODO: 要考虑名字前面的路径
                results.Add(strName);
            }

            return results;
        }

        // 列出全部已经安装的URL
        public int GetInstalledUrls(out List<string> urls,
            out string strError)
        {
            strError = "";
            urls = new List<string>();

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//project");

            foreach (XmlNode node in nodes)
            {
                string strLocate = DomUtil.GetAttr(node, "locate");
                strLocate = UnMacroPath(strLocate);

                XmlDocument metadata_dom = null;
                // 获得(一个已经安装的)方案元数据
                // parameters:
                //      dom 返回元数据XMLDOM
                // return:
                //      -1  出错
                //      0   没有找到元数据文件
                //      1   成功
                int nRet = ScriptManager.GetProjectMetadata(strLocate,
                out metadata_dom,
                out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 0)
                    continue;

                string strUpdateUrl = DomUtil.GetAttr(metadata_dom.DocumentElement,
        "updateUrl");
                if (string.IsNullOrEmpty(strUpdateUrl) == false)
                {
                    urls.Add(strUpdateUrl);
                }
            }

            return 0;
        }

        public static string GetFileNameFromUrl(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet != -1)
                strPath = strPath.Substring(nRet + 1);
            return strPath;
        }

        // 检查更新一个容器节点下的全部方案
        // parameters:
        //      dir_node    容器节点。如果 == null 检查更新全部方案
        //      strSource   "!url"或者磁盘目录。分别表示从网络检查更新，或者从磁盘检查更新
        // return:
        //      -2  全部放弃
        //      -1  出错
        //      0   成功
        public int CheckUpdate(
            IWin32Window owner,
            XmlNode dir_node,
            string strSource,
            ref bool bHideMessageBox,
            ref bool bDontUpdate,
            ref int nUpdateCount,
            ref string strUpdateInfo,
            ref string strWarning,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlNodeList nodes = null;
            if (dir_node == null)
                nodes = this.dom.DocumentElement.SelectNodes("//project");
            else
                nodes = dir_node.SelectNodes(".//project");

            foreach (XmlNode node in nodes)
            {
                // 方案节点
                // return:
                //      -2  放弃所有的更新
                //      -1  出错
                //      0   没有更新
                //      1   已经更新
                //      2   因为某些原因无法检查更新
                nRet = CheckUpdateOneProject(
                        owner,
                        node,
                        strSource,
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == -2)
                    return -2;
                if (nRet == 2)
                    strWarning += "方案 " + DomUtil.GetAttr(node, "name") + " " + strError + ";\r\n";

                if (nRet == 1)
                {
                    nUpdateCount++;
                    strUpdateInfo += DomUtil.GetAttr(node, "name") + "\r\n";
                }
            }

            return 0;
        }

        // 根据一个目录中的全部 .projpack 文件创建出 projects.xml 文件
        public static int BuildProjectsFile(string strDirectory,
            string strProjectsFilename,
            out string strError)
        {
            strError = "";

            List<ProjectInstallInfo> infos = new List<ProjectInstallInfo>();

            // 列出所有文件
            try
            {
                DirectoryInfo di = new DirectoryInfo(strDirectory);

                FileInfo[] fis = di.GetFiles("*.projpack");

                for (int i = 0; i < fis.Length; i++)
                {
                    string strFileName = fis[i].FullName;

                    List<ProjectInstallInfo> temp_infos = null;
                    // 获取projpack文件中的方案的名字和Host信息
                    int nRet = ScriptManager.GetInstallInfos(
                        strFileName,
                        out temp_infos,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 2013/5/11
                    // 设置 ProjectPath 
                    foreach (ProjectInstallInfo info in temp_infos)
                    {
                        info.ProjectPath = strFileName;
                    }

                    infos.AddRange(temp_infos);
                }

                // TODO: 对列表进行排序

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");

                foreach (ProjectInstallInfo info in infos)
                {
                    XmlNode node = dom.CreateElement("project");
                    dom.DocumentElement.AppendChild(node);

                    DomUtil.SetAttr(node, "name", info.ProjectName);
                    DomUtil.SetAttr(node, "host", info.Host);
                    DomUtil.SetAttr(node, "url", info.UpdateUrl);
                    DomUtil.SetAttr(node, "localFile", info.ProjectPath);   // 2013/5/11
                    DomUtil.SetAttr(node, "index", info.IndexInPack.ToString());
                }

                dom.Save(strProjectsFilename);
                return 0;
            }
            catch (Exception ex)
            {
                strError = "列出文件的过程中出错: " + ex.Message;
                return -1;
            }
        }

        // 检查更新一个方案
        // parameters:
        //      strSource   "!url"或者磁盘目录。分别表示从网络检查更新，或者从磁盘检查更新
        // return:
        //      -2  放弃所有的更新
        //      -1  出错
        //      0   没有更新
        //      1   已经更新
        //      2   因为某些原因无法检查更新
        public int CheckUpdateOneProject(
            IWin32Window owner,
            XmlNode node,
            string strSource,
            ref bool bHideMessageBox,
            ref bool bDontUpdate,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // string strProjectNamePath = node.FullPath;
            string strLocate = "";
            string strIfModifySince = "";

            strIfModifySince = DomUtil.GetAttr(node, "lastModified");

            strLocate = DomUtil.GetAttr(node, "locate");
            strLocate = UnMacroPath(strLocate);

            string strNamePath = GetNodePathName(node);

            // 获得下载URL

            XmlDocument metadata_dom = null;
            // 获得(一个已经安装的)方案元数据
            // parameters:
            //      dom 返回元数据XMLDOM
            // return:
            //      -1  出错
            //      0   没有找到元数据文件
            //      1   成功
            nRet = ScriptManager.GetProjectMetadata(strLocate,
            out metadata_dom,
            out strError);

            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                strError = "元数据文件不存在，因此无法检查更新";
                return 2;   // 没有元数据文件，无法更新
            }

            if (metadata_dom.DocumentElement == null)
            {
                strError = "元数据DOM的根元素不存在，因此无法检查更新";
                return 2;
            }

            string strUpdateUrl = DomUtil.GetAttr(metadata_dom.DocumentElement,
                "updateUrl");
            if (string.IsNullOrEmpty(strUpdateUrl) == true)
            {
                strError = "元数据D中没有定义updateUrl属性，因此无法检查更新";
                return 2;
            }

            List<string> protect_filenames = null;
            string strProtectList = DomUtil.GetAttr(metadata_dom.DocumentElement,
                "protectFiles");
            if (string.IsNullOrEmpty(strProtectList) == false)
            {
                protect_filenames = StringUtil.SplitList(strProtectList, ',');
            }

            string strLocalFileName = "";
            string strLastModified = "";
            // 尝试下载指定日期后更新过的文件
            if (strSource == "!url")
            {

                Debug.Assert(string.IsNullOrEmpty(this.DataDir) == false, "");

                strLocalFileName = PathUtil.MergePath(this.DataDir, "~temp_project.projpack");
                string strTempFileName = PathUtil.MergePath(this.DataDir, "~temp_download_webfile");


                try
                {
                    File.Delete(strLocalFileName);
                }
                catch
                {
                }
                try
                {
                    File.Delete(strTempFileName);
                }
                catch
                {
                }

                nRet = WebFileDownloadDialog.DownloadWebFile(
                    owner,
                    strUpdateUrl,
                    strLocalFileName,
                    strTempFileName,
                    strIfModifySince,
                    out strLastModified,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    return 0;
                }
            }
            else
            {
                // 磁盘目录中的文件

                string strPureFileName = GetFileNameFromUrl(strUpdateUrl);
                strLocalFileName = PathUtil.MergePath(strSource, strPureFileName);

                FileInfo fi = new FileInfo(strLocalFileName);
                if (fi.Exists == false)
                {
                    strError = "目录 '" + strSource + "' 中没有找到文件 '" + strPureFileName + "'";
                    return 0;
                }
                strLastModified = DateTimeUtil.Rfc1123DateTimeString(fi.LastWriteTimeUtc);
            }

            if (string.IsNullOrEmpty(strIfModifySince) == false
                && string.IsNullOrEmpty(strLastModified) == false)
            {
                DateTime ifmodifiedsince = DateTimeUtil.FromRfc1123DateTimeString(strIfModifySince);
                DateTime lastmodified = DateTimeUtil.FromRfc1123DateTimeString(strLastModified);
                if (ifmodifiedsince == lastmodified)
                    return 0;
            }

            // 询问是否更新
            if (bHideMessageBox == false)
            {
                DialogResult result = MessageDialog.Show(owner,
                    "是否要更新统计方案 '" + strNamePath + "' ?\r\n\r\n(是：更新; 否: 不更新，但继续检查其他方案; 取消: 中断更新检查过程)",
                    MessageBoxButtons.YesNoCancel,
                    bDontUpdate == true ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1,
                    "以后不再提示，按本次的选择处理",
                    ref bHideMessageBox);
                if (result == DialogResult.Cancel)
                {
                    strError = "放弃全部更新";
                    return -2;
                }
                if (result == DialogResult.No)
                {
                    bDontUpdate = true;
                    return 0;
                }
            }

            if (bDontUpdate == true)
                return 0;

            nRet = UpdateProject(
                strLocalFileName,
                strLocate,
                protect_filenames,
                out strError);
            if (nRet == -1)
                return -1;

            DomUtil.SetAttr(node, "lastModified",
    strLastModified);

            this.m_bChanged = true;
            this.Save();

            return 1;
        }

        // 获取projpack文件中的方案的名字和Host信息
        public static int GetInstallInfos(
            string strFilename,
            out List<ProjectInstallInfo> infos,
            out string strError)
        {
            strError = "";
            infos = null;

            ProjectCollection projects = null;
            try
            {
                using (Stream stream = File.Open(strFilename, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    try
                    {
                        projects = (ProjectCollection)formatter.Deserialize(stream);
                    }
                    catch (SerializationException ex)
                    {
                        strError = "装载打包文件出错：" + ex.Message;
                        return -1;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + "不存在...";
                return -1;
            }

            if (projects.Count == 0)
            {
                strError = ".projpack文件中没有包含任何Project";
                return -1;
            }

            infos = new List<ProjectInstallInfo>();

            for (int i = 0; i < projects.Count; i++)
            {
                Project project = (Project)projects[i];

                string strPath = "";
                string strName = "";
                ScriptManager.SplitProjectPathName(project.NamePath,
    out strPath,
    out strName);

                ProjectInstallInfo info = new ProjectInstallInfo();
                info.ProjectPath = strPath;
                info.ProjectName = strName;
                info.IndexInPack = i;

                info.Host = project.GetHostName();
                info.UpdateUrl = project.GetUpdateUrl();

                infos.Add(info);
            }

            return 0;
        }

        // 安装Project
        // return:
        //      -1  出错
        //      0   没有安装方案
        //      >0  安装的方案数
        public int InstallProject(
            IWin32Window owner,
            string strStatisWindowName,
            string strFilename,
            string strLastModified,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            int nCount = 0;
            ProjectCollection projects = null;

            try
            {
                using (Stream stream = File.Open(strFilename, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    try
                    {
                        projects = (ProjectCollection)formatter.Deserialize(stream);

                        if (projects.Count == 0)
                        {
                            strError = ".projpack文件中没有包含任何Project";
                            return -1;
                        }
                    }
                    catch (SerializationException ex)
                    {
                        strError = "装载打包文件出错：" + ex.Message;
                        return -1;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + "不存在...";
                return -1;
            }


            /*
            if (projects.Count > 1)
            {
                strError = ".projpack文件中包含了多个方案，目前暂不支持从其中获取并更新";
                return -1;
            }
             * */

            for (int i = 0; i < projects.Count; i++)
            {
                Project project = (Project)projects[i];

                string strPath = "";
                string strName = "";
                ScriptManager.SplitProjectPathName(project.NamePath,
    out strPath,
    out strName);

                // 创建目录
                if (string.IsNullOrEmpty(strPath) == false)
                {
                    XmlNode xmlNode = this.LocateDirNode(
                        strPath);
                    if (xmlNode == null)
                    {
                        xmlNode = this.NewDirNode(
                            strPath);
                    }
                }

                REDO_CHECKDUP:

                // 查重
                string strExistLocate;
                // 获得方案参数
                // strProjectNamePath	方案名，或者路径
                // return:
                //		-1	error
                //		0	not found project
                //		1	found
                nRet = this.GetProjectData(
                    ScriptManager.MakeProjectPathName(strPath, strName),
                    out strExistLocate);
                if (nRet == -1)
                {
                    strError = "GetProjectData " + ScriptManager.MakeProjectPathName(strPath, strName) + " error";
                    return -1;
                }

                bool bOverwrite = false;
                if (nRet == 1)
                {
                    string strDirName = "";
                    if (string.IsNullOrEmpty(strPath) == false)
                        strDirName = "目录 '" + strPath + "' 下";
                    DialogResult result = MessageBox.Show(owner,
    strStatisWindowName + " " + strDirName + "已经有名为 '" + strName + "' 的方案。\r\n\r\n请问是否要覆盖它?\r\n\r\n(Yes: 覆盖  No: 换名后安装; Cancel: 放弃安装此方案，但继续安装后面的方案)",
    "ScriptManager",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        strName = InputDlg.GetInput(owner,
                            "请指定一个新的方案名",
                            "方案名",
                            strName,
                            null);
                        if (strName == null)
                        {
                            continue;
                        }
                        goto REDO_CHECKDUP;
                    }

                    if (result == DialogResult.Cancel)
                        continue;

                    if (result == DialogResult.Yes)
                    {
                        bOverwrite = true;
                    }
                }

                if (bOverwrite == true)
                {
                    try
                    {
                        RemoveExistFiles(strExistLocate,
                            null);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    int nPrefixNumber = -1;

                    string strLocatePrefix = "";
                    if (project.Locate == "")
                    {
                        strLocatePrefix = strName;
                    }
                    else
                    {
                        strLocatePrefix = PathUtil.PureName(project.Locate);
                    }

                    strExistLocate = this.NewProjectLocate(
                        strLocatePrefix,
                        ref nPrefixNumber);
                }

                try
                {
                    // 直接paste
                    project.WriteToLocate(strExistLocate, true);
                }
                catch (Exception ex)
                {
                    strError = "拷贝文件进入方案目录 '" + strExistLocate + "' 时出错: " + ex.Message;
                    return -1;
                }

                string strNamePath = ScriptManager.MakeProjectPathName(strPath, strName);

                // 实际插入project参数
                XmlNode projNode = this.NewProjectNode(
                    strNamePath,
                    strExistLocate,
                    false);	// false表示不需要创建目录和缺省文件
                DomUtil.SetAttr(projNode, "lastModified",
    strLastModified);

                nCount++;
            }

            if (nCount > 0)
            {
                this.m_bChanged = true;
                try
                {
                    this.Save();
                }
                catch (Exception ex)
                {
                    strError = "保存方案配置文件时出现异常: " + ex.Message;
                    return -1;
                }
            }

            return nCount;
        }

        // 更新Project
        // parameters:
        //      reserve_filenames   需要保护的文件名列表。如果目录中已经有这些文件，则不要覆盖
        private int UpdateProject(
            string strFilename,
            string strExistLocate,
            List<string> protect_filenames,
            out string strError)
        {
            strError = "";

            ProjectCollection projects = null;
            Project project = null;
            try
            {
                using (Stream stream = File.Open(strFilename, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    try
                    {
                        projects = (ProjectCollection)formatter.Deserialize(stream);
                    }
                    catch (SerializationException ex)
                    {
                        strError = "装载打包文件出错：" + ex.Message;
                        return -1;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + "不存在...";
                return -1;
            }

            if (projects.Count == 0)
            {
                strError = ".projpack文件中没有包含任何Project";
                return -1;
            }
            if (projects.Count > 1)
            {
                strError = ".projpack文件中包含了多个方案，目前暂不支持从其中获取并更新";
                return -1;
            }

            project = (Project)projects[0];

            try
            {
                RemoveExistFiles(strExistLocate,
                    protect_filenames);
            }
            catch
            {
            }

            try
            {
                // 直接paste
                project.WriteToLocate(strExistLocate,
                    false);
            }
            catch (Exception ex)
            {
                strError = "拷贝文件进入方案目录 '" + strExistLocate + "' 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // parameters:
        //      reserve_filenames   需要保护的文件名列表。如果目录中已经有这些文件，则不要覆盖。要求里面的文件名都是小写字符
        static void RemoveExistFiles(string strExistLocate,
                    List<string> protect_filenames)
        {
            /*
            // 删除现有目录中的全部文件
            try
            {
                Directory.Delete(strExistLocate, true);
            }
            catch (Exception ex)
            {
                strError = "删除目录时出错: " + ex.Message;
                return -1;
            }
            PathUtil.CreateDirIfNeed(strExistLocate);
            */
            // 将目录中所有文件逐个打包
            DirectoryInfo di = new DirectoryInfo(strExistLocate);

            FileInfo[] afi = di.GetFiles();

            for (int i = 0; i < afi.Length; i++)
            {
                string strFileName = afi[i].Name;
                if (strFileName.Length > 0
                    && strFileName[0] == '~')
                    continue;	// 忽略临时文件

                if (protect_filenames != null
                    && protect_filenames.IndexOf(strFileName.ToLower()) != -1)
                    continue;

                try
                {
                    File.Delete(afi[i].FullName);
                }
                catch
                {
                }
            }
        }

        // 2015/8/6
        // 根据指定的路径解析 DLL Assembly
        public static Assembly ResolveAssembly(string strName, List<string> paths)
        {
            int nRet = strName.IndexOf(",");
            if (nRet != -1)
                strName = strName.Substring(0, nRet).Trim();

            foreach (string path in paths)
            {
                string strFileName = Path.Combine(path, strName + ".dll");
                if (File.Exists(strFileName) == true)
                    return Assembly.LoadFile(strFileName);
            }

            return null;
        }

        // 创建Assembly
        // parameters:
        //		saAddtionalRef	附加的refs文件路径。路径中可能包含宏%installdir%
        //		strLibPaths	附加的库搜索路径们，用','分隔的路径字符串
        // return:
        //		-2	出错，但是已经提示过错误信息了。
        //		-1	出错
        public int BuildAssembly(
            string strHostName,
            string strProjectNamePath,
            string strSourceFileName,	// 不包含目录部分的纯文件名
            string[] saAdditionalRef,	// 附加的refs
            string strLibPaths,
            string strOutputFileName,
            out string strError,
            out string strWarning)
        {
            strWarning = "";
            int nRet;
            string strLocate = "";
            string strCodeFileName;
            string[] saRef = null;

            // 获得方案参数
            // strProjectNamePath	方案名，或者路径
            // return:
            //		-1	error
            //		0	not found project
            //		1	found
            nRet = GetProjectData(strProjectNamePath,
                out strLocate);
            if (nRet == -1)
            {
                strError = "GetProjectData() error ...";
                return -1;
            }
            if (nRet == 0)
            {
                strError = "宿主 " + strHostName + " 方案 '" + strProjectNamePath + "' 没有找到 ...";
                return -1;
            }

            // refs
            string strRefFileName = strLocate + "\\" + "references.xml";

            nRet = GetRefs(strRefFileName,
                out saRef,
                out strError);
            if (nRet == -1)
                return -1;

            if (saAdditionalRef != null)
            {
                if (saRef == null)
                    saRef = new string[0];

                string[] saTemp = new string[saRef.Length + saAdditionalRef.Length];
                Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
                Array.Copy(saAdditionalRef, 0, saTemp, saRef.Length, saAdditionalRef.Length);
                saRef = saTemp;
            }

            // 替换%projectdir%宏
            RemoveRefsProjectDirMacro(ref saRef, strLocate);

            strCodeFileName = strLocate + "\\" + strSourceFileName;

            string strCode = "";
#if NO
            try
            {
                using (StreamReader sr = new StreamReader(strCodeFileName, true))
                {
                    strCode = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }
#endif
            nRet = LoadCode(strCodeFileName, out strCode, out strError);
            if (nRet == -1)
                return -1;

            nRet = CreateAssemblyFile(strCode,
                saRef,
                strLibPaths,
                strOutputFileName,
                out strError,
                out strWarning);

            if (strError != "")
            {
                strError = "宿主 " + strHostName + " 方案 '" + strProjectNamePath
                    + "' 中文件 '" + strSourceFileName + "' 编译发现错误或警告:\r\n" + strError;
                CompileErrorDlg dlg = new CompileErrorDlg();
                GuiUtil.AutoSetDefaultFont(dlg);
                dlg.applicationInfo = applicationInfo;
                dlg.Initial(strCodeFileName,
                    strError);
                {
                    string strTemp = strSourceFileName;
                    if (strTemp.IndexOf(".fltx.", 0) != -1)
                        dlg.IsFltx = true;
                }
                dlg.ShowDialog();
                strError = "编译执行被终止。请修改源代码后重新执行。";
                return -2;
            }

            if (strWarning != "")
            {
                strWarning = "宿主 " + strHostName + " 方案 '" + strProjectNamePath
                    + "' 中文件" + strSourceFileName + "' 编译发现警告:\r\n" + strWarning;
            }

            return nRet;
        }

        static bool IsUsing(string line)
        {
            if (string.IsNullOrEmpty(line) == true)
                return true;
            line = line.Trim();
            if (string.IsNullOrEmpty(line) == true)
                return true;

            if (line.StartsWith("using ") == true)
                return true;
            if (line.StartsWith("//") == true)
                return true;

            return false;
        }

        static bool IsEqual(string strText1, string strText2)
        {
            return string.Compare(strText1.Trim(), strText2.Trim(), true) == 0;
        }

        // 为了兼容以前代码，对 using 部分进行修改
        public static string ModifyCode(string strCode)
        {
            StringBuilder text = new StringBuilder();
            List<string> lines = StringUtil.SplitList(strCode, "\r\n");

            bool bHasCirculationClientUsing = false;
            bool bHasLibraryClientUsing = false;

            bool in_using = true;
            foreach (string line in lines)
            {
                if (IsUsing(line) == false)
                    in_using = false;

                if (in_using == false)
                    goto CONTINUE;

                if (IsEqual(line, "using DigitalPlatform.CirculationClient;"))
                    bHasCirculationClientUsing = true;
                if (IsEqual(line, "using DigitalPlatform.LibraryClient;"))
                    bHasLibraryClientUsing = true;
                if (IsEqual(line, "using DigitalPlatform.CirculationClient.localhost;"))
                {
                    text.Append("using DigitalPlatform.LibraryClient.localhost; // 为兼容而作的自动修改\r\n");
                    goto CONTINUE;
                }

                // 2018/8/18
                if (IsEqual(line, "using DigitalPlatform.GcatClient;"))
                    continue;
                CONTINUE:
                text.Append(line + "\r\n");
            }

            if (bHasCirculationClientUsing == true && bHasLibraryClientUsing == false)
            {
                return "using DigitalPlatform.LibraryClient; // 为兼容而作的自动修改\r\n\r\n"
                    + text.ToString();
            }
            else
                return text.ToString();
        }

        public static int LoadCode(string strCodeFileName,
    out string strCode,
    out string strError)
        {
            strCode = "";
            strError = "";

            try
            {
                StringBuilder text = new StringBuilder();
                using (StreamReader sr = new StreamReader(strCodeFileName, true))
                {
                    text.Append(sr.ReadToEnd());
                }

                strCode = ModifyCode(text.ToString());
                return 0;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

        }
#if NO
        public static int LoadCode(string strCodeFileName,
            out string strCode,
            out string strError)
        {
            strCode = "";
            strError = "";

            bool bHasCirculationClientUsing = false;
            bool bHasLibraryClientUsing = false;
            StringBuilder text = new StringBuilder();
            try
            {
                using (StreamReader sr = new StreamReader(strCodeFileName, true))
                {
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                            break;
                        if (IsUsing(line) == false)
                        {
                            text.Append(line + "\r\n");
                            break;
                        }

                        line = line.Trim();

                        if (line == "using DigitalPlatform.CirculationClient;")
                            bHasCirculationClientUsing = true;
                        if (line == "using DigitalPlatform.LibraryClient;")
                            bHasLibraryClientUsing = true;
                        if (line == "using DigitalPlatform.CirculationClient.localhost;")
                        {
                            text.Append("using DigitalPlatform.LibraryClient.localhost; // 为兼容而作的自动修改\r\n");
                            continue;
                        }

                        // 2018/8/18
                        if (line == "using DigitalPlatform.GcatClient;")
                            continue;

                        text.Append(line + "\r\n");
                    }
                    text.Append(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            if (bHasCirculationClientUsing == true && bHasLibraryClientUsing == false)
            {
                strCode = "using DigitalPlatform.LibraryClient; // 为兼容而作的自动修改\r\n\r\n"
                    + text.ToString();
            }
            else
                strCode = text.ToString();

            return 0;
        }
#endif

        // 从xml字符串中得到refs字符串数组
        // return:
        //		-1	error
        //		0	正确
        public static int GetRefsFromXml(string strRefXml,
            out string[] saRef,
            out string strError)
        {
            saRef = null;
            strError = "";
            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strRefXml);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            // 所有ref节点
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//ref");
            saRef = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                saRef[i] = DomUtil.GetNodeText(nodes[i]);
            }

            return 0;
        }

        // 从references.xml文件中得到refs字符串数组
        // return:
        //		-1	error
        //		0	not found file
        //		1	found file
        public static int GetRefs(string strXmlFileName,
            out string[] saRef,
            out string strError)
        {
            saRef = null;
            strError = "";
            XmlDocument dom = new XmlDocument();

            try
            {
                dom.Load(strXmlFileName);
            }
            catch (FileNotFoundException ex)
            {
                strError = ex.Message;
                return 0;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }


            // 所有ref节点
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//ref");
            saRef = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                saRef[i] = DomUtil.GetNodeText(nodes[i]);
            }


            return 1;
        }

        // 将refs字符串数组写入references.xml文件中
        // return:
        //		-1	error
        //		0	Suceed
        static int SaveRefs(string strXmlFileName,
            string[] saRef,
            out string strError)
        {
            strError = "";

            using (XmlTextWriter writer = new XmlTextWriter(strXmlFileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartDocument();

                writer.WriteStartElement("root");

                for (int i = 0; i < saRef.Length; i++)
                {
                    writer.WriteElementString("ref", saRef[i]);
                }

                writer.WriteEndElement();

                writer.WriteEndDocument();
            }
            return 0;
        }

        static int SaveMetadata(string strXmlFileName,
            string strUpdateUrl,
            string strHostName,
            out string strError)
        {
            strError = "";

            using (XmlTextWriter writer = new XmlTextWriter(strXmlFileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartDocument();

                writer.WriteStartElement("root");

                writer.WriteAttributeString("updateUrl", strUpdateUrl);
                writer.WriteAttributeString("host", strHostName);

                writer.WriteEndElement();

                writer.WriteEndDocument();
            }
            return 0;
        }

        // 定位Dir XmlNode节点
        public XmlNode LocateDirNode(string strDirNamePath)
        {
            string[] aName = strDirNamePath.Split(new Char[] { '/' });

            string strXPath = "";

            for (int i = 0; i < aName.Length; i++)
            {
                if (i != aName.Length - 1)
                {
                    strXPath += "dir[@name='" + aName[i] + "']";
                    if (strXPath != "")
                        strXPath += "/";
                }
                else
                    strXPath += "dir[@name='" + aName[i] + "']";

            }

            return dom.DocumentElement.SelectSingleNode(strXPath);
        }

        // 定位Dir XmlNode节点
        public XmlNode LocateAnyNode(string strNamePath)
        {
            string[] aName = strNamePath.Split(new Char[] { '/' });

            string strXPath = "";

            for (int i = 0; i < aName.Length; i++)
            {
                if (i != aName.Length - 1)
                {
                    strXPath += "dir[@name='" + aName[i] + "']";
                    if (strXPath != "")
                        strXPath += "/";
                }
                else
                    strXPath += "*[@name='" + aName[i] + "']";

            }

            return dom.DocumentElement.SelectSingleNode(strXPath);
        }


        // 将Dir XmlNode改名
        // return:
        //	0	not found
        //	1	found and changed
        public int RenameDir(string strDirNamePath,
            string strNewName)
        {
            XmlNode node = LocateDirNode(strDirNamePath);
            if (node == null)
                return 0;
            DomUtil.SetAttr(node, "name", strNewName);

            m_bChanged = true;

            return 1;
        }

        // 删除Dir XmlNode，包括其下全部节点
        // return:
        //	-1	error
        //	0	not found
        //	1	found and changed
        public int DeleteDir(string strDirNamePath,
            out XmlNode parentXmlNode,
            out string strError)
        {
            strError = "";
            parentXmlNode = null;

            XmlNode node = LocateDirNode(strDirNamePath);
            if (node == null)
                return 0;

            // 列出所有下级Project节点并删除
            //XmlNodeList nodes = node.SelectNodes("descendant::*");
            XmlNodeList nodes = node.SelectNodes("descendant::project");
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Name != "project")
                    continue;
                XmlNode parent;
                int nRet = this.DeleteProject(
                    this.GetNodePathName(nodes[i]),
                    false,
                    out parent,
                    out strError);
                if (nRet == -1)
                {
                    // return -1;
                }
            }

            parentXmlNode = node.ParentNode;
            parentXmlNode.RemoveChild(node);

            m_bChanged = true;

            return 1;
        }

        // 定位Project XmlNode节点
        public XmlNode LocateProjectNode(string strProjectNamePath)
        {
            if (this.dom == null || this.dom.DocumentElement == null)
                return null;    // 2011/10/5

            string[] aName = strProjectNamePath.Split(new Char[] { '/' });

            string strXPath = "";

            for (int i = 0; i < aName.Length; i++)
            {
                if (i != aName.Length - 1)
                {
                    strXPath += "dir[@name='" + aName[i] + "']";
                    if (strXPath != "")
                        strXPath += "/";
                }
                else
                    strXPath += "project[@name='" + aName[i] + "']";

            }

            return dom.DocumentElement.SelectSingleNode(strXPath);
        }

        // 修改project参数
        public int ChangeProjectData(string strProjectNamePath,
            string strProjectName,
            string strLocate,
            out string strError)
        {
            strError = "";
            XmlNode node = LocateProjectNode(strProjectNamePath);

            if (node == null)
            {
                strError = "方案 '" + strProjectNamePath + "' 不存在...";
                return -1;
            }

            if (strProjectName != null)
            {
                DomUtil.SetAttr(node, "name", strProjectName);
                Changed = true;
            }

            if (strLocate != null)
            {
                // 2007/1/24
                strLocate = MacroPath(strLocate);

                DomUtil.SetAttr(node, "locate", strLocate);
                Changed = true;
            }

            return 0;
        }

        // 从完整的方案"名字路径"中,析出路径和名
        public static void SplitProjectPathName(string strProjectNamePath,
            out string strPath,
            out string strName)
        {

            int nRet = strProjectNamePath.LastIndexOf("/");
            if (nRet == -1)
            {
                strName = strProjectNamePath;
                strPath = "";
            }
            else
            {
                strName = strProjectNamePath.Substring(nRet + 1);
                strPath = strProjectNamePath.Substring(0, nRet);
            }
        }

        // 根据路径和名字部分构造完整的方案名字路径
        public static string MakeProjectPathName(string strPath,
            string strName)
        {
            if (strPath == "")
                return strName;
            return strPath + "/" + strName;
        }

        // 包装后的版本
        // 获得方案参数
        // parameters:
        //      strProjectNamePath	方案名，或者路径
        //      strLastModified     返回方案最后修改时间。RFC1123格式
        // return:
        //		-1	error
        //		0	not found project
        //		1	found
        public int GetProjectData(string strProjectNamePath,
            out string strProjectLocate)
        {
            string strLastModified = "";
            return GetProjectData(strProjectNamePath,
                out strProjectLocate,
                out strLastModified);
        }

        // 获得方案参数
        // parameters:
        //      strProjectNamePath	方案名，或者路径
        //      strLastModified     返回方案最后修改时间。RFC1123格式
        // return:
        //		-1	error
        //		0	not found project
        //		1	found
        public int GetProjectData(string strProjectNamePath,
            out string strProjectLocate,
            out string strLastModified)
        {
            //saRef = null;
            strProjectLocate = "";
            strLastModified = "";

            XmlNode node = LocateProjectNode(strProjectNamePath);

            if (node == null)
                return 0;

            strProjectLocate = DomUtil.GetAttr(node, "locate");

            // 2011/11/5
            strLastModified = DomUtil.GetAttr(node, "lastModified");

            // 2007/1/24
            strProjectLocate = UnMacroPath(strProjectLocate);
            return 1;
        }

        // return:
        //		-1	error
        //		0	not found project
        //		1	found and set
        public int SetProjectData(string strProjectNamePath,
            string strLastModified)
        {
            XmlNode node = LocateProjectNode(strProjectNamePath);

            if (node == null)
                return 0;

            DomUtil.SetAttr(node, "lastModified",
                strLastModified);

            this.m_bChanged = true;
            return 1;
        }

        // 获得(一个已经安装的)方案元数据
        // parameters:
        //      dom 返回元数据XMLDOM
        // return:
        //      -1  出错
        //      0   没有找到元数据文件
        //      1   成功
        public static int GetProjectMetadata(string strLocate,
            out XmlDocument dom,
            out string strError)
        {
            strError = "";
            dom = null;

            string strFilePath = PathUtil.MergePath(strLocate, "metadata.xml");

            dom = new XmlDocument();
            try
            {
                dom.Load(strFilePath);
            }
            catch (FileNotFoundException /*ex*/)
            {
                strError = "元数据文件 " + strFilePath + " 不存在";
                return 0;
            }
            catch (Exception ex)
            {
                strError = "装入XML文件 " + strFilePath + " 到DOM时出错: " + ex.Message;
                return -1;
            }

            return 1;
        }

        /*
        // 获得方案参数
        // strProjectNamePath	方案名，或者路径
        // return:
        //		-1	error
        //		0	not found project
        //		1	found
        public int GetProjectData(string strProjectNamePath,
            out string strCodeFileName,
            out string [] saRef)
        {
            saRef = null;
            strCodeFileName = "";


            XmlNode node = LocateProjectNode(strProjectNamePath);

            if (node == null)
                return 0;

            strCodeFileName = DomUtil.GetAttr(node, "codeFileName");

            string strRef = DomUtil.GetAttr(node, "references");

            saRef = strRef.Split(new Char [] {','});

            return 1;
        }
        */

        // 删除一个方案
        // return:
        //	-1	error
        //	0	not found
        //	1	found and deleted
        //	2	canceld	因此project没有被删除
        public int DeleteProject(string strProjectNamePath,
            bool bWarning,
            out XmlNode parentXmlNode,
            out string strError)
        {
            strError = "";
            string strProjectLocate;
            parentXmlNode = null;

            XmlNode nodeThis = LocateProjectNode(strProjectNamePath);

            if (nodeThis == null)
                return 0;

            strProjectLocate = DomUtil.GetAttr(nodeThis, "locate");

            // 2007/1/24
            strProjectLocate = UnMacroPath(strProjectLocate);


            // 从Dom上删除节点
            parentXmlNode = nodeThis.ParentNode;
            parentXmlNode.RemoveChild(nodeThis);
            m_bChanged = true;

            if (strProjectLocate != "") // 删除目录
            {
                try
                {
                    Directory.Delete(strProjectLocate, true);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    //return -1; // 沉默
                }
            }

            return 1;
        }


        /*
        // 删除一个方案
        // return:
        //	0	not found
        //	1	found and deleted
        //	2	canceld	因此project没有被删除
        public int DeleteProject(string strProjectNamePath,
            bool bWarning,
            out XmlNode parentXmlNode)
        {
            string strCodeFileName;
            bool bDeleteCodeFile = true;
            parentXmlNode = null;

            XmlNode nodeThis = LocateProjectNode(strProjectNamePath);

            if (nodeThis == null)
                return 0;

            strCodeFileName = DomUtil.GetAttr(nodeThis, "codeFileName");

            if (strCodeFileName != "") 
            {

                // 看看源文件是否被多个project引用
                ArrayList aFound = new ArrayList();
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//project");
                for(int i=0;i<nodes.Count;i++) 
                {
                    string strFilePath = DomUtil.GetAttr(nodes[i], "codeFileName");

                    if (String.Compare(strCodeFileName, strFilePath, true) == 0)
                    {
                        if (nodes[i] != nodeThis)
                            aFound.Add(nodes[i]);
                    }
                }

                if (aFound.Count > 0) 
                {
                    if (bWarning == true) 
                    {
                        string strText = "系统发现，源代码文件 "
                            + strCodeFileName 
                            + " 除了被您即将删除的方案 "+
                            strProjectNamePath 
                            +" 使用外，还被下列方案引用：\r\n---\r\n";

                        for(int i=0;i<aFound.Count;i++)
                        {
                            strText += GetNodePathName((XmlNode)aFound[i]) + "\r\n";
                        }

                        strText += "---\r\n\r\n这意味着，如果删除这个源代码文件，上述方案将不能正常运行。\r\n\r\n请问，在删除方案" +strProjectNamePath+ "时，是否保留源代码文件 " + strCodeFileName + "?";

                        DialogResult msgResult = MessageBox.Show(//this,
                            strText,
                            "script",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);

                        if (msgResult == DialogResult.Yes)
                            bDeleteCodeFile = false;
                        if (msgResult == DialogResult.Cancel)
                            return 2;	// cancel
                    }
                    else 
                    {
                        bDeleteCodeFile = false;	// 自动采用最保险的方式
                    }

                }
            }

            // 从Dom上删除节点
            parentXmlNode = nodeThis.ParentNode;
            parentXmlNode.RemoveChild(nodeThis);

            if (bDeleteCodeFile == true
                && strCodeFileName != "")
                File.Delete(strCodeFileName);

            m_bChanged = true;

            return 1;
        }
        */


        // 上下移动节点
        // return:
        //	0	not found
        //	1	found and moved
        //	2	cant move
        public int MoveNode(string strNodeNamePath,
            bool bUp,
            out XmlNode parentXmlNode)
        {
            parentXmlNode = null;

            XmlNode nodeThis = LocateAnyNode(strNodeNamePath);

            if (nodeThis == null)
                return 0;

            XmlNode nodeInsert = null;


            if (bUp == true)
            {
                nodeInsert = nodeThis.PreviousSibling;
                if (nodeInsert == null)
                    return 2;
            }
            else
            {
                nodeInsert = nodeThis.NextSibling;
                if (nodeInsert == null)
                    return 2;
            }

            // 从Dom上删除节点
            parentXmlNode = nodeThis.ParentNode;
            parentXmlNode.RemoveChild(nodeThis);

            // 插入到特定位置
            if (bUp == true)
            {
                parentXmlNode.InsertBefore(nodeThis, nodeInsert);
            }
            else
            {
                parentXmlNode.InsertAfter(nodeThis, nodeInsert);
            }

            m_bChanged = true;

            return 1;
        }

        // 根据project或dir XmlNode得到路径名
        public string GetNodePathName(XmlNode nodeThis)
        {
            string strResult = "";

            strResult = DomUtil.GetAttr(nodeThis, "name");
            XmlNode node = nodeThis.ParentNode;

            while (node != null)
            {
                if (node == dom.DocumentElement)
                    break;
                strResult = DomUtil.GetAttr(node, "name") + "/" + strResult;

                node = node.ParentNode;
            }

            return strResult;
        }


        #region 和Assembly有关的几个函数

        /*
		 *
			string[] refs = new String[] {"System.dll",
											 "System.Xml.dll",
											 @"bin\DigitalPlatform.UI.dll",
											 @"bin\DigitalPlatform.Public.dll",
											 @"bin\DigitalPlatform.Xml_r.dll",
											 @"bin\DigitalPlatform.rms.db.dll"}; //加引用的dll
		  
		 * 
		 */


        // 创建Assembly
        // parameters:
        //	strCode:	脚本代码
        //	refs:	连接的外部assembly
        // strResult:处理信息
        // objDb:数据库对象，在出错调getErrorInfo用到
        // 返回值:创建好的Assembly
        public static Assembly CreateAssembly(string strCode,
            string[] refs,
            string strLibPaths,
            string strOutputFile,
            out string strErrorInfo,
            out string strWarningInfo)
        {
            // System.Reflection.Assembly compiledAssembly = null;
            strErrorInfo = "";
            strWarningInfo = "";

            // CompilerParameters对象
            System.CodeDom.Compiler.CompilerParameters compilerParams;
            compilerParams = new CompilerParameters();

            compilerParams.GenerateInMemory = true; //Assembly is created in memory
            compilerParams.IncludeDebugInformation = true;  // 2007/1/15

            if (strOutputFile != null && strOutputFile != "")
            {
                compilerParams.GenerateExecutable = false;
                compilerParams.OutputAssembly = strOutputFile;
                // compilerParams.CompilerOptions = "/t:library";
            }

            if (strLibPaths != null && strLibPaths != "")	// bug
                compilerParams.CompilerOptions = "/lib:" + strLibPaths;

            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.WarningLevel = 4;

            // 正规化路径，去除里面的宏字符串
            RemoveRefsBinDirMacro(ref refs);

            compilerParams.ReferencedAssemblies.AddRange(refs);


            CSharpCodeProvider provider;

            // System.CodeDom.Compiler.ICodeCompiler compiler;
            System.CodeDom.Compiler.CompilerResults results = null;
            try
            {
                provider = new CSharpCodeProvider();
                // compiler = provider.CreateCompiler();
                results = provider.CompileAssemblyFromSource(
                    compilerParams,
                    strCode);
            }
            catch (Exception ex)
            {
                strErrorInfo = "出错 " + ex.Message;
                return null;
            }

            int nErrorCount = 0;

            if (results.Errors.Count != 0)
            {
                string strErrorString = "";
                nErrorCount = getErrorInfo(results.Errors,
                    out strErrorString);

                strErrorInfo = "信息条数:" + Convert.ToString(results.Errors.Count) + "\r\n";
                strErrorInfo += strErrorString;

                if (nErrorCount == 0 && results.Errors.Count != 0)
                {
                    strWarningInfo = strErrorInfo;
                    strErrorInfo = "";
                }
            }

            if (nErrorCount != 0)
                return null;


            return results.CompiledAssembly;
        }

        int GetErrorCount(CompilerErrorCollection errors)
        {
            int nCount = 0;
            foreach (CompilerError oneError in errors)
            {
                if (oneError.IsWarning == false)
                    nCount++;
            }

            return nCount;
        }

        // 去除路径中的宏%projectdir%
        void RemoveRefsProjectDirMacro(ref string[] refs,
            string strProjectDir)
        {
            if (refs == null)
                return; // 2008/1/13

            Hashtable macroTable = new Hashtable();

            macroTable.Add("%projectdir%", strProjectDir);

            for (int i = 0; i < refs.Length; i++)
            {
                string strNew = PathUtil.UnMacroPath(macroTable,
                    refs[i],
                    false);	// 不要抛出异常，因为可能还有%binddir%宏现在还无法替换
                refs[i] = strNew;
            }

        }


        // 去除路径中的宏%bindir%
        public static void RemoveRefsBinDirMacro(ref string[] refs)
        {
            if (refs == null)
                return; // 2008/1/13

            Hashtable macroTable = new Hashtable();

            macroTable.Add("%bindir%", Environment.CurrentDirectory);

            for (int i = 0; i < refs.Length; i++)
            {
                string strNew = PathUtil.UnMacroPath(macroTable,
                    refs[i],
                    true);
                refs[i] = strNew;
            }

        }

        // parameters:
        //		refs	附加的refs文件路径。路径中可能包含宏%installdir%
        public static int CreateAssemblyFile(string strCode,
            string[] refs,
            string strLibPaths,
            string strOutputFile,
            out string strErrorInfo,
            out string strWarningInfo)
        {
            // System.Reflection.Assembly compiledAssembly = null;
            strErrorInfo = "";
            strWarningInfo = "";

            // CompilerParameters对象
            System.CodeDom.Compiler.CompilerParameters compilerParams;
            compilerParams = new CompilerParameters();

            compilerParams.GenerateInMemory = true; //Assembly is created in memory
            compilerParams.IncludeDebugInformation = true;

            if (strOutputFile != null && strOutputFile != "")
            {
                compilerParams.GenerateExecutable = false;
                compilerParams.OutputAssembly = strOutputFile;
                // compilerParams.CompilerOptions = "/t:library";
            }

            if (strLibPaths != null && strLibPaths != "")	// bug
                compilerParams.CompilerOptions = "/lib:" + strLibPaths;

            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.WarningLevel = 4;

            // 正规化路径，去除里面的宏字符串
            RemoveRefsBinDirMacro(ref refs);

            if (refs != null)
                compilerParams.ReferencedAssemblies.AddRange(refs);


            CSharpCodeProvider provider;

            // System.CodeDom.Compiler.ICodeCompiler compiler;
            System.CodeDom.Compiler.CompilerResults results = null;
            try
            {
                provider = new CSharpCodeProvider();
                // compiler = provider.CreateCompiler();
                results = provider.CompileAssemblyFromSource(
                    compilerParams,
                    strCode);
            }
            catch (Exception ex)
            {
                strErrorInfo = "出错 " + ex.Message;
                return -1;
            }

            int nErrorCount = 0;

            if (results.Errors.Count != 0)
            {
                string strErrorString = "";
                nErrorCount = getErrorInfo(results.Errors,
                    out strErrorString);

                strErrorInfo = "信息条数:" + Convert.ToString(results.Errors.Count) + "\r\n";
                strErrorInfo += strErrorString;

                if (nErrorCount == 0 && results.Errors.Count != 0)
                {
                    strWarningInfo = strErrorInfo;
                    strErrorInfo = "";
                }
            }

            if (nErrorCount != 0)
                return -1;


            return 0;
        }

        // 从 .ref 获取附加的库文件路径
        public static int GetRef(string strCsFileName,
            ref string[] refs,
            out string strError)
        {
            strError = "";

            string strRefFileName = strCsFileName + ".ref";

            // .ref文件可以缺省
            if (File.Exists(strRefFileName) == false)
                return 0;   // .ref 文件不存在

            string strRef = "";
            try
            {
                using (StreamReader sr = new StreamReader(strRefFileName, true))
                {
                    strRef = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            // 提前检查
            string[] add_refs = null;
            int nRet = ScriptManager.GetRefsFromXml(strRef,
                out add_refs,
                out strError);
            if (nRet == -1)
            {
                strError = strRefFileName + " 文件内容(应为XML格式)格式错误: " + strError;
                return -1;
            }

            // 兑现宏
            if (add_refs != null)
            {
                for (int i = 0; i < add_refs.Length; i++)
                {
                    add_refs[i] = add_refs[i].Replace("%bindir%", Environment.CurrentDirectory);
                }
            }

            refs = StringUtil.Append(refs, add_refs);
            return 1;
        }

        // (对refs中的宏不加以处理)
        // 直接编译到内存
        // parameters:
        //		refs	附加的refs文件路径。本函数对路径中可能包含宏%...%未加以处理，需要在函数调用前先处理好
        public static int CreateAssembly_1(string strCode,
            string[] refs,
            string strLibPaths,
            // AppDomain appDomain,
            out Assembly assembly,
            out string strErrorInfo,
            out string strWarningInfo)
        {
            assembly = null;
            strErrorInfo = "";
            strWarningInfo = "";

            // CompilerParameters对象
            System.CodeDom.Compiler.CompilerParameters compilerParams;
            compilerParams = new CompilerParameters();

            compilerParams.GenerateInMemory = true; //Assembly is created in memory
            compilerParams.IncludeDebugInformation = true;

            if (String.IsNullOrEmpty(strLibPaths) == false)
                compilerParams.CompilerOptions = "/lib:" + strLibPaths;

            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.WarningLevel = 4;


            /*
            // 2007/12/4 放开注释
            // 正规化路径，去除里面的宏字符串
            RemoveRefsBinDirMacro(ref refs);
             * */

            compilerParams.ReferencedAssemblies.AddRange(refs);


            CSharpCodeProvider provider;

            System.CodeDom.Compiler.CompilerResults results = null;
            try
            {
                Dictionary<string, string> options = new Dictionary<string, string>
                {
                {"CompilerVersion","v3.5"}
                };
                provider = new CSharpCodeProvider();
                // compiler = provider.CreateCompiler();
                results = provider.CompileAssemblyFromSource(
                    compilerParams,
                    strCode);
            }
            catch (Exception ex)
            {
                strErrorInfo = "出错 " + ex.Message;
                return -1;
            }

            int nErrorCount = 0;

            if (results.Errors.Count != 0)
            {
                string strErrorString = "";
                nErrorCount = getErrorInfo(results.Errors,
                    out strErrorString);

                strErrorInfo = "信息条数:" + Convert.ToString(results.Errors.Count) + "\r\n";
                strErrorInfo += strErrorString;

                if (nErrorCount == 0 && results.Errors.Count != 0)
                {
                    strWarningInfo = strErrorInfo;
                    strErrorInfo = "";
                }
            }

            if (nErrorCount != 0)
                return -1;

            assembly = results.CompiledAssembly;

            return 0;
        }

        // 构造出错信息字符串
        public static int getErrorInfo(CompilerErrorCollection errors,
            out string strResult)
        {
            strResult = "";
            int nCount = 0;

            if (errors == null)
            {
                strResult = "error参数为null";
                return 0;
            }


            foreach (CompilerError oneError in errors)
            {
                strResult += "(" + Convert.ToString(oneError.Line) + "," + Convert.ToString(oneError.Column) + ") ";
                strResult += (oneError.IsWarning) ? "warning " : "error ";
                strResult += oneError.ErrorNumber + " ";
                strResult += ": " + oneError.ErrorText + "\r\n";

                if (oneError.IsWarning == false)
                    nCount++;

            }
            return nCount;
        }

        // 获得从指定类或者接口派生的类
        public static Type GetDerivedClassType(Assembly assembly,
            string strBaseTypeFullName)
        {
            if (assembly == null)
                return null;

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsClass == false)
                    continue;

                // 2015/5/28
                Type[] interfaces = type.GetInterfaces();
                foreach (Type inter in interfaces)
                {
                    if (inter.FullName == strBaseTypeFullName)
                        return type;
                }

                if (IsDerivedFrom(type,
                    strBaseTypeFullName) == true)
                    return type;
            }

            return null;
        }


        public string GetDebugInfo(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            string strText = "";

            for (int i = 0; i < types.Length; i++)
            {
                strText += "Name:" + types[i].Name + "\r\n";
                strText += "baseClasses:" + GetBaseClasses(types[i]) + "\r\n";
                strText += "---\r\n";
            }


            return strText;
        }

        string GetBaseClasses(Type type)
        {
            // base type
            // StringCollection names = new StringCollection();

            string strText = "";

            Type curType = type;
            for (; ; )
            {
                if (curType == null
                    || curType.Name == "Object")
                    break;

                if (strText != "")
                    strText += ",";

                strText += curType.FullName;	//curType.Namespace + "|" + curType.Name;
                curType = curType.BaseType;
            }

            return strText;
        }


        // 观察type的基类中是否有类名为strBaseTypeFullName的类。
        public static bool IsDerivedFrom(Type type,
            string strBaseTypeFullName)
        {
            Type curType = type;
            for (; ; )
            {
                if (curType == null
                    || curType.FullName == "System.Object")
                    return false;

                if (curType.FullName == strBaseTypeFullName)
                    return true;

                curType = curType.BaseType;
            }

        }

        #endregion


        // *** 算法可以改进一下:如果发现原来的名字最后部分是数字,就先累加这个数字部分
        // 获得一个新的Project磁盘目录名。
        // 要满足：1) 在磁盘上不存在
        // 一旦获得这个目录名，就先创建一个空目录，以便占据这个目录名，
        // 避免被后来重复获得。但是，以后如果不使用这个目录，记得删除它，不然
        // 成为无人清理的垃圾目录
        public string NewProjectLocate(string strPrefix,
            ref int nPrefixNumber)
        {

            // 替换strPrefix中的某些字符
            strPrefix = strPrefix.Replace(" ", "_");


            string strNewPath = "";

            strNewPath = this.DefaultCodeFileDir
                + "\\"
                + strPrefix;
            int i;
            bool bFound = false;


            // 试探磁盘目录中同名文件
            PathUtil.TryCreateDir(this.DefaultCodeFileDir);
            DirectoryInfo di = new DirectoryInfo(this.DefaultCodeFileDir);

            FileSystemInfo[] fis = di.GetFileSystemInfos();

            for (; ; nPrefixNumber++)
            {
                string strName = "";

                if (nPrefixNumber == -1)
                {
                    strName = strPrefix;
                }
                else
                {
                    strName = strPrefix + Convert.ToString(nPrefixNumber);
                }

                bFound = false;
                for (i = 0; i < fis.Length; i++)
                {
                    string strExistName = fis[i].Name;

                    if (String.Compare(strName, strExistName, true) == 0)
                    {
                        bFound = true;
                        break;
                    }
                }
                if (bFound == false)
                    break;
            }

            if (nPrefixNumber == -1)
            {
                strNewPath = this.DefaultCodeFileDir
                    + "\\"
                    + strPrefix;
            }
            else
            {
                strNewPath = this.DefaultCodeFileDir
                    + "\\"
                    + strPrefix + Convert.ToString(nPrefixNumber);
            }

            PathUtil.TryCreateDir(strNewPath);

            return strNewPath;
        }


        // 获得一个新的源代码文件名。
        // 要满足：1) 在现有project元素的codeFileName参数中不重复
        //		2) 在磁盘上不存在
        // 一旦获得这个文件名，就先创建一个0byte的新文件，以便占据这个文件名，
        // 避免被后来重复获得。但是，以后如果不使用这个文件，记得删除它，不然
        // 成为无人清理的垃圾
        public string NewCodeFileName(string strPrefix,
            string strExt,	// 文件扩展名，带'.'
            ref int nPrefixNumber)
        {

            // 替换strPrefix中的某些字符
            strPrefix = strPrefix.Replace(" ", "_");


            string strNewPath = this.DefaultCodeFileDir
                + "\\"
                + strPrefix;
            int i;
            bool bFound = false;

            // 试探所有codeFileName参数
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//project");

            for (; ; nPrefixNumber++)
            {
                strNewPath = this.DefaultCodeFileDir + "\\" +
                    strPrefix + Convert.ToString(nPrefixNumber) + strExt;

                bFound = false;
                for (i = 0; i < nodes.Count; i++)
                {
                    string strExistPath = DomUtil.GetAttr(nodes[i],
                        "codeFileName");

                    if (strExistPath == "")
                        continue;

                    if (String.Compare(strExistPath, strNewPath) == 0)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                    break;
            }


            // 试探磁盘目录中同名文件
            PathUtil.TryCreateDir(this.DefaultCodeFileDir);
            DirectoryInfo di = new DirectoryInfo(this.DefaultCodeFileDir);

            FileInfo[] fis = di.GetFiles();

            for (; ; nPrefixNumber++)
            {
                string strName = strPrefix + Convert.ToString(nPrefixNumber) + strExt;

                bFound = false;
                for (i = 0; i < fis.Length; i++)
                {
                    string strExistName = fis[i].Name;


                    if (String.Compare(strName, strExistName, true) == 0)
                    {
                        bFound = true;
                        break;
                    }
                }
                if (bFound == false)
                    break;
            }

            return strNewPath;
        }

        // 创建一个新的Project XmlNode
        // parameters:
        //      bCreateDefault  是否要创建缺省的文件
        //      strHostName     宿主名。当bCreateDefault == true时才有意义
        public XmlNode NewProjectNode(string strProjectNamePath,
            string strLocate,
            bool bCreateDefault,
            string strHostName = "")
        {
#if DEBUG
            if (bCreateDefault == true
                && string.IsNullOrEmpty(strHostName) == true)
            {
                Debug.Assert(false, "当参数 bCreateDefault 为true的时候， strHostName不能为空");
            }
#endif

            string[] aName = strProjectNamePath.Split(new Char[] { '/' });

            string strXPath = "";

            XmlNode nodeCurrent = dom.DocumentElement;

            XmlNode projectNode = null;

            // string strRef = String.Join(",", saRef);

            for (int i = 0; i < aName.Length; i++)
            {
                if (i != aName.Length - 1)
                {
                    strXPath = "dir[@name='" + aName[i] + "']";

                    XmlNode node = nodeCurrent.SelectSingleNode(strXPath);

                    if (node == null) // 创建新下级目录
                    {
                        node = dom.CreateElement("dir");
                        nodeCurrent.AppendChild(node);
                        DomUtil.SetAttr(node, "name", aName[i]);
                    }

                    nodeCurrent = node;
                }
                else
                {
                    strXPath = "project[@name='" + aName[i] + "']";

                    XmlNode node = nodeCurrent.SelectSingleNode(strXPath);

                    if (node == null) // 创建新下级目录
                    {
                        node = dom.CreateElement("project");
                        nodeCurrent.AppendChild(node);
                        DomUtil.SetAttr(node, "name", aName[i]);

                        // 2007/1/24
                        strLocate = MacroPath(strLocate);

                        DomUtil.SetAttr(node, "locate", strLocate);


                        // 创建物理目录 
                        if (string.IsNullOrEmpty(strLocate) == false
                            && bCreateDefault == true)
                        {

                            CreateDefault(strLocate, strHostName);
                            /*
                            Directory.CreateDirectory(strLocate);

                            // 创建缺省文件

                            // main.cs
                            string strFileName = strLocate + "\\main.cs";

                            CreateDefaultMainCsFile(strFileName);

                            // reference.xml
                            strFileName = strLocate + "\\references.xml";

                            CreateDefaultReferenceXmlFile(strFileName);
                            */
                        }


                    }

                    projectNode = node;

                    nodeCurrent = node;
                }
            }

            m_bChanged = true;
            return projectNode;
        }

        public void OnCreateDefaultContent(string strFileName)
        {
            // 创建缺省文件
            if (this.CreateDefaultContent != null)
            {
                CreateDefaultContentEventArgs e = new CreateDefaultContentEventArgs();
                e.FileName = strFileName;
                this.CreateDefaultContent(this, e);
                if (e.Created == false)
                    CreateBlankContent(strFileName);
            }
            else
            {
                // 创建一个空文件
                CreateBlankContent(strFileName);
            }
        }

        // 创建一个方案目录中的若干缺省文件
        public int CreateDefault(string strLocate,
            string strHostName)
        {
            // 创建物理目录 
            if (strLocate == "")
                return -1;

            Directory.CreateDirectory(strLocate);

            // 创建缺省文件
            if (this.CreateDefaultContent != null)
            {

                // main.cs
                string strFileName = strLocate + "\\main.cs";

                // CreateDefaultMainCsFile(strFileName);
                CreateDefaultContentEventArgs e = new CreateDefaultContentEventArgs();
                e.FileName = strFileName;
                this.CreateDefaultContent(this, e);
                if (e.Created == false)
                    CreateBlankContent(strFileName);

                // reference.xml
                strFileName = strLocate + "\\references.xml";

                CreateDefaultReferenceXmlFile(strFileName);

                // metadata.xml
                strFileName = strLocate + "\\metadata.xml";

                CreateDefaultMetadataXmlFile(strFileName, strHostName);
            }

            return 0;
        }

        public void CreateBlankContent(string strFileName)
        {
            using (StreamWriter sw = new StreamWriter(strFileName))
            {
                sw.WriteLine("");
            }
        }

        // 创建缺省的references.xml文件
        // TODO: 应修改为事件驱动
        public static int CreateDefaultReferenceXmlFile(string strFileName)
        {
            string strError = "";
            //string strExe = Environment.CurrentDirectory + "\\dp1batch.exe";
            //string strDll = Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll";
            string[] saRef = { "system.dll", "system.windows.forms.dll", "system.xml.dll"/*, strExe, strDll*/};
            SaveRefs(strFileName, saRef, out strError);

            return 0;
        }

        // 创建缺省的metadata.xml文件
        // TODO: 应修改为事件驱动
        public static int CreateDefaultMetadataXmlFile(string strFileName,
            string strHostName)
        {
            string strError = "";
            SaveMetadata(strFileName,
                "",
                strHostName,
                out strError);

            return 0;
        }

        // 是否为缺省文件名?
        // strFileName	纯文件名
        public static bool IsReservedFileName(string strFileName)
        {
            if (String.Compare(strFileName,
                "main.cs", true) == 0)
                return true;
            if (String.Compare(strFileName,
                "references.xml", true) == 0)
                return true;
            if (String.Compare(strFileName,
                "marcfilter.fltx", true) == 0)
                return true;

            return false;
        }

        /*
        public XmlNode NewProjectNode(string strProjectNamePath,
            string strCodeFileName,
            string[] saRef)
        {
            string[] aName = strProjectNamePath.Split(new Char [] {'/'});

            string strXPath = "";

            XmlNode nodeCurrent = dom.DocumentElement;

            XmlNode projectNode = null;

            string strRef = String.Join(",", saRef);

            for(int i=0;i<aName.Length;i++) 
            {
                if (i != aName.Length - 1) 
                {
                    strXPath = "dir[@name='" + aName[i] + "']";

                    XmlNode node = nodeCurrent.SelectSingleNode(strXPath);

                    if (node == null) // 创建新下级目录
                    {
                        node = dom.CreateElement("dir");
                        nodeCurrent.AppendChild(node);
                        DomUtil.SetAttr(node, "name", aName[i]);
                    }

                    nodeCurrent = node;

                }
                else 
                {
                    strXPath = "project[@name='" + aName[i] + "']";

                    XmlNode node = nodeCurrent.SelectSingleNode(strXPath);

                    if (node == null) // 创建新下级目录
                    {
                        node = dom.CreateElement("project");
                        nodeCurrent.AppendChild(node);
                        DomUtil.SetAttr(node, "name", aName[i]);

                        DomUtil.SetAttr(node, "codeFileName", strCodeFileName);

                        DomUtil.SetAttr(node, "references", strRef);
                    }

                    projectNode = node;

                    nodeCurrent = node;

                }

            }

            m_bChanged = true;

            return projectNode;
        }
        */

        // 创建一个新的Dir xml节点
        public XmlNode NewDirNode(string strDirNamePath)
        {
            string[] aName = strDirNamePath.Split(new Char[] { '/' });

            string strXPath = "";

            XmlNode nodeCurrent = dom.DocumentElement;

            XmlNode dirNode = null;

            for (int i = 0; i < aName.Length; i++)
            {
                if (i != aName.Length - 1)
                {
                    strXPath = "dir[@name='" + aName[i] + "']";

                    XmlNode node = nodeCurrent.SelectSingleNode(strXPath);

                    if (node == null) // 创建新下级目录
                    {
                        node = dom.CreateElement("dir");
                        nodeCurrent.AppendChild(node);
                        DomUtil.SetAttr(node, "name", aName[i]);
                    }

                    nodeCurrent = node;

                }
                else
                {
                    strXPath = "dir[@name='" + aName[i] + "']";

                    XmlNode node = nodeCurrent.SelectSingleNode(strXPath);

                    if (node == null) // 创建新下级目录
                    {
                        node = dom.CreateElement("dir");
                        nodeCurrent.AppendChild(node);
                        DomUtil.SetAttr(node, "name", aName[i]);
                    }

                    dirNode = node;

                    nodeCurrent = node;

                }

            }

            m_bChanged = true;

            return dirNode;
        }

        // 创建一个缺省的projects.xml文件
        // parameters:
        //		strDefaultCodeFileSubDir	缺省的代码子目录。一般为clientcfgs
        public static int CreateDefaultProjectsXmlFile(string strFileName,
            string strDefaultCodeFileSubDir)
        {
            using (StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version='1.0' encoding='utf-8'?>");
                sw.WriteLine("<dir defaultCodeFileDir='" + strDefaultCodeFileSubDir + "'>");
                sw.WriteLine("</dir>");
            }
            return 0;
        }
    }

    // 创建文件缺省内容
    public delegate void CreateDefaultContentEventHandler(object sender,
    CreateDefaultContentEventArgs e);

    public class CreateDefaultContentEventArgs : EventArgs
    {
        public string FileName = "";
        public bool Created = false;
    }

    public class ProjectInstallInfo
    {
        public string ProjectPath = ""; // projpack 文件全路径
        public string ProjectName = "";
        public int IndexInPack = -1;    // 在projpack文件中的下标
        public string Host = "";
        public string UpdateUrl = "";

    }
}

