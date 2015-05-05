using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;

using System.Runtime.Serialization;

using System.Runtime.Serialization.Formatters.Binary;


using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;

namespace DigitalPlatform.Script
{
	/// <summary>
	/// Summary description for ProjectManageDlg.
	/// </summary>
	public class ProjectManageDlg : System.Windows.Forms.Form
	{
        // projects.xml文件URL
        // "http://dp2003.com/dp2circulation/projects/projects.xml"
        // "http://dp2003.com/dp2batch/projects/projects.xml"
        public string ProjectsUrl = "";

        public string HostName = "";

        public event AutoCreateProjectXmlFileEventHandle CreateProjectXmlFile = null;

		public ScriptManager scriptManager = null;

		public ApplicationInfo	AppInfo = null;

        public string DataDir = ""; // 数据目录


		string strRecentPackageFilePath = "";

		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.ImageList imageList_projectNodeType;
		private System.Windows.Forms.Button button_modify;
		private System.Windows.Forms.Button button_new;
		private System.Windows.Forms.Button button_delete;
		private System.Windows.Forms.Button button_down;
		private System.Windows.Forms.Button button_up;
		private System.Windows.Forms.Button button_import;
		private System.Windows.Forms.Button button_export;
        private Button button_updateProjects;
		private System.ComponentModel.IContainer components;

		public ProjectManageDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectManageDlg));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.button_OK = new System.Windows.Forms.Button();
            this.imageList_projectNodeType = new System.Windows.Forms.ImageList(this.components);
            this.button_modify = new System.Windows.Forms.Button();
            this.button_new = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            this.button_down = new System.Windows.Forms.Button();
            this.button_up = new System.Windows.Forms.Button();
            this.button_import = new System.Windows.Forms.Button();
            this.button_export = new System.Windows.Forms.Button();
            this.button_updateProjects = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(9, 9);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(358, 268);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.DoubleClick += new System.EventHandler(this.treeView1_DoubleClick);
            this.treeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseDown);
            this.treeView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseUp);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(375, 287);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(72, 23);
            this.button_OK.TabIndex = 9;
            this.button_OK.Text = "关闭";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // imageList_projectNodeType
            // 
            this.imageList_projectNodeType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_projectNodeType.ImageStream")));
            this.imageList_projectNodeType.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_projectNodeType.Images.SetKeyName(0, "");
            this.imageList_projectNodeType.Images.SetKeyName(1, "");
            // 
            // button_modify
            // 
            this.button_modify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_modify.Location = new System.Drawing.Point(372, 12);
            this.button_modify.Name = "button_modify";
            this.button_modify.Size = new System.Drawing.Size(75, 21);
            this.button_modify.TabIndex = 1;
            this.button_modify.Text = "修改(&M)";
            this.button_modify.Click += new System.EventHandler(this.button_modify_Click);
            // 
            // button_new
            // 
            this.button_new.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_new.Location = new System.Drawing.Point(372, 38);
            this.button_new.Name = "button_new";
            this.button_new.Size = new System.Drawing.Size(75, 22);
            this.button_new.TabIndex = 2;
            this.button_new.Text = "新增(&N)";
            this.button_new.Click += new System.EventHandler(this.button_newProject_Click);
            // 
            // button_delete
            // 
            this.button_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_delete.Location = new System.Drawing.Point(372, 149);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(75, 22);
            this.button_delete.TabIndex = 5;
            this.button_delete.Text = "删除(&E)";
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // button_down
            // 
            this.button_down.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_down.Location = new System.Drawing.Point(372, 109);
            this.button_down.Name = "button_down";
            this.button_down.Size = new System.Drawing.Size(75, 22);
            this.button_down.TabIndex = 4;
            this.button_down.Text = "下移(&D)";
            this.button_down.Click += new System.EventHandler(this.button_down_Click);
            // 
            // button_up
            // 
            this.button_up.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_up.Location = new System.Drawing.Point(372, 82);
            this.button_up.Name = "button_up";
            this.button_up.Size = new System.Drawing.Size(75, 22);
            this.button_up.TabIndex = 3;
            this.button_up.Text = "上移(&U)";
            this.button_up.Click += new System.EventHandler(this.button_up_Click);
            // 
            // button_import
            // 
            this.button_import.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_import.Location = new System.Drawing.Point(9, 287);
            this.button_import.Name = "button_import";
            this.button_import.Size = new System.Drawing.Size(132, 23);
            this.button_import.TabIndex = 6;
            this.button_import.Text = "导入[当前目录](&I)...";
            this.button_import.Click += new System.EventHandler(this.button_import_Click);
            // 
            // button_export
            // 
            this.button_export.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_export.Location = new System.Drawing.Point(148, 287);
            this.button_export.Name = "button_export";
            this.button_export.Size = new System.Drawing.Size(99, 23);
            this.button_export.TabIndex = 7;
            this.button_export.Text = "导出(&E)...";
            this.button_export.Click += new System.EventHandler(this.button_export_Click);
            // 
            // button_updateProjects
            // 
            this.button_updateProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_updateProjects.Location = new System.Drawing.Point(253, 287);
            this.button_updateProjects.Name = "button_updateProjects";
            this.button_updateProjects.Size = new System.Drawing.Size(99, 23);
            this.button_updateProjects.TabIndex = 8;
            this.button_updateProjects.Text = "检查更新(&U)";
            this.button_updateProjects.UseVisualStyleBackColor = true;
            this.button_updateProjects.Visible = false;
            this.button_updateProjects.Click += new System.EventHandler(this.button_updateProjects_Click);
            // 
            // ProjectManageDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 318);
            this.Controls.Add(this.button_updateProjects);
            this.Controls.Add(this.button_export);
            this.Controls.Add(this.button_import);
            this.Controls.Add(this.button_down);
            this.Controls.Add(this.button_up);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.button_new);
            this.Controls.Add(this.button_modify);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.treeView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProjectManageDlg";
            this.ShowInTaskbar = false;
            this.Text = "方案管理";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ProjectManageDlg_Closing);
            this.Closed += new System.EventHandler(this.ProjectManageDlg_Closed);
            this.Load += new System.EventHandler(this.ProjectManageDlg_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void ProjectManageDlg_Load(object sender, System.EventArgs e)
		{
            Debug.Assert(string.IsNullOrEmpty(this.HostName) == false, "");

			if (AppInfo != null) 
			{
				AppInfo.LoadFormStates(this,
					"projectman");
			}
			/*
			if (applicationInfo != null) 
			{

				this.Width = applicationInfo.GetInt(
					"projectman", "width", 640);
				this.Height = applicationInfo.GetInt(
					"projectman", "height", 500);

				this.Location = new Point(
					applicationInfo.GetInt("projectman", "x", 0),
					applicationInfo.GetInt("projectman", "y", 0));

				this.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), applicationInfo.GetString(
					"projectman", "window_state", "Normal"));
			}
			*/


			treeView1.ImageList = imageList_projectNodeType;
			treeView1.PathSeparator = "/";

			if (scriptManager != null)
			{
				bool bDone = false;
			REDO:
				try 
				{
					scriptManager.FillTree(this.treeView1);
				}
				catch(System.IO.FileNotFoundException ex) 
				{
					/*
					MessageBox.Show("装载" + scriptManager.CfgFilePath + "文件失败，原因:"
						+ ex.Message);
					*/
					// MessageBox.Show(ex.Message);
					//return;
					if (bDone == false) 
					{
						MessageBox.Show(this, "自动创建新文件 " + scriptManager.CfgFilePath);

                        // 触发事件
                        if (this.CreateProjectXmlFile != null)
                        {
                            AutoCreateProjectXmlFileEventArgs e1 = new AutoCreateProjectXmlFileEventArgs();
                            e1.Filename = scriptManager.CfgFilePath;
                            this.CreateProjectXmlFile(this, e1);
                        }

						ScriptManager.CreateDefaultProjectsXmlFile(scriptManager.CfgFilePath,
							"clientcfgs");
						bDone = true;
						goto REDO;
					}
					else 
					{
						MessageBox.Show(this, ex.Message);
						return;
					}
				}
				catch(System.Xml.XmlException ex)
				{
					MessageBox.Show("装载" + scriptManager.CfgFilePath + "文件失败，原因:"
						+ ex.Message);
					return;
				}
			}
			treeView1_AfterSelect(null,null);

			TreeViewUtil.SelectTreeNode(treeView1, 
				AppInfo.GetString(
				"projectman",
				"lastUsedProject",
				""),
                '/');

		}

		private void ProjectManageDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			/*
			if (scriptManager.Changed == true)
			{
				DialogResult msgResult = MessageBox.Show(this,
					"是否放弃先前所作的修改而立即退出?",
					"script",
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button2);
				if (msgResult == DialogResult.Cancel) 
				{
					e.Cancel = true;
					return;
				}
			}
			*/



		}

		private void ProjectManageDlg_Closed(object sender, System.EventArgs e)
		{

			if (AppInfo != null) 
			{
				AppInfo.SetString(
					"projectman",
					"lastUsedProject",
					TreeViewUtil.GetSelectedTreeNodePath(treeView1, '/')); // 2007/8/2 changed
			}

			if (AppInfo != null) 
			{
				AppInfo.SaveFormStates(this,
					"projectman");
			}

			/*
			if (applicationInfo != null) 
			{
				applicationInfo.SetString(
					"projectman", "window_state", 
					Enum.GetName(typeof(FormWindowState), this.WindowState));
			}

			if (applicationInfo != null) 
			{
				WindowState = FormWindowState.Normal;	// 是否先隐藏窗口?
				applicationInfo.SetInt(
					"projectman", "width", this.Width);
				applicationInfo.SetInt(
					"projectman", "height", this.Height);

				applicationInfo.SetInt("projectman", "x", this.Location.X);
				applicationInfo.SetInt("projectman", "y", this.Location.Y);
			}
			*/


		}

		// 修改方案
		private void button_modify_Click(object sender, System.EventArgs e)
		{
			int nRet;
			// 当前已选择的node
			if (treeView1.SelectedNode == null) 
			{
				MessageBox.Show("尚未选择方案或者目录");
				return;
			}

			TreeNode node = treeView1.SelectedNode;
			if (node.ImageIndex == 0) 
			{
				// 修改目录名
				DirNameDlg namedlg = new DirNameDlg();
                GuiUtil.AutoSetDefaultFont(namedlg);

				namedlg.textBox_dirName.Text = node.Text;
				namedlg.StartPosition = FormStartPosition.CenterScreen;
				namedlg.ShowDialog(this);

				if (namedlg.DialogResult == DialogResult.OK) 
				{
					// return:
					//	0	not found
					//	1	found and changed
					nRet = scriptManager.RenameDir(node.FullPath,
						namedlg.textBox_dirName.Text);
					if (nRet == 1) 
					{
						node.Text = namedlg.textBox_dirName.Text;	// 兑现视觉
						scriptManager.Save();
					}
				}

				return ;
			}

			string strProjectNamePath = node.FullPath;

			string strLocate = "";

			// 获得方案参数
			// strProjectNamePath	方案名，或者路径
			// return:
			//		-1	error
			//		0	not found project
			//		1	found
			nRet = scriptManager.GetProjectData(
				strProjectNamePath,
				out strLocate);
			if (nRet != 1) 
			{
				MessageBox.Show("方案 "+ strProjectNamePath + " 在ScriptManager中没有找到");
				return ;
			}


			ScriptDlg dlg = new ScriptDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.HostName = this.HostName;
			dlg.scriptManager = scriptManager;
			dlg.Initial(strProjectNamePath,
				strLocate);

			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);


			if (dlg.DialogResult == DialogResult.OK) 
			{
				if (dlg.ResultProjectNamePath != strProjectNamePath) 
				{
					/*
					// 修改显示的Project名字
					string strPath;
					string strName;
					ScriptManager.SplitProjectPathName(dlg.ResultProjectNamePath,
						out strPath,
						out strName);

					string strError;

					nRet = scriptManager.ChangeProjectData(strProjectNamePath,
						strName,
						null,
						out strError);
					if (nRet == -1) 
					{
						MessageBox.Show(this, strError);
					}
					else 
					{
						// 兑现显示?
					}
					*/
					// XML DOM已经在ScriptDlg中修改，这里只是兑现显示
					string strPath;
					string strName;
					ScriptManager.SplitProjectPathName(dlg.ResultProjectNamePath,
						out strPath,
						out strName);

					node.Text = strName;

				}

				scriptManager.Save();
			}

		
		}

		// 在儿子中找到一个不重复的名字
		string GetTempProjectName(TreeView treeView,
			TreeNode parent,
			string strPrefix,
			ref int nPrefixNumber)
		{
			TreeNodeCollection nodes = null;

			if (parent != null) 
				nodes = parent.Nodes;
			else
				nodes = treeView.Nodes;

			string strName = strPrefix;

			for(;;nPrefixNumber ++) 
			{
				if (nPrefixNumber == -1)
					strName = strPrefix;
				else
					strName = strPrefix + " " + Convert.ToString(nPrefixNumber);

				bool bFound = false;
				for(int i=0;i<nodes.Count; i++) 
				{
					string strText = nodes[i].Text;

					if (String.Compare(strText, strName, true) == 0) 
					{
						bFound = true;
						break;
					}
				}

				if (bFound == false)
					break;
			}

			return strName;

		}

		// 在儿子中找到一个不重复的名字
		string GetTempDirName(TreeView treeView,
			TreeNode parent,
			string strPrefix,
			ref int nPrefixNumber)
		{
			TreeNodeCollection nodes = null;

			if (parent != null) 
				nodes = parent.Nodes;
			else
				nodes = treeView.Nodes;

			string strName = strPrefix;

			for(;;nPrefixNumber ++) 
			{
				strName = strPrefix + " " +Convert.ToString(nPrefixNumber);

				bool bFound = false;
				for(int i=0;i<nodes.Count; i++) 
				{
					string strText = nodes[i].Text;

					if (String.Compare(strText, strName, true) == 0) 
					{
						bFound = true;
						break;
					}
				}

				if (bFound == false)
					break;
			}

			return strName;

		}

		// 新方案
		private void button_newProject_Click(object sender, System.EventArgs e)
		{
			// 当前所在路径
			string strProjectPath = "";
			string strTempName = "new project 1";

			TreeNode parent = null;

			int nPrefixNumber = -1;	// 1

			// 当前已选择的node
			if (treeView1.SelectedNode == null) 
			{
				strProjectPath = "";
			}
			else 
			{
				// 如果当前选择的是dir类型节点，就在其下创建新project
				// 否则，就在同一级创建新project
				if (treeView1.SelectedNode.ImageIndex == 0)
					parent = treeView1.SelectedNode;
				else
					parent = treeView1.SelectedNode.Parent;

				strProjectPath = parent != null ? parent.FullPath : "";

				strTempName = GetTempProjectName(treeView1,
					parent,
					"new project",
					ref nPrefixNumber);

				scriptManager.Save();

			}

			/*
			StreamReader sr = new StreamReader(strTempName, true);
			string strCode =sr.ReadToEnd();
			sr.Close();
			*/

			string strNewLocate = scriptManager.NewProjectLocate(
				"new project",
				ref nPrefixNumber);

			ScriptDlg dlg = new ScriptDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.HostName = this.HostName;
            dlg.scriptManager = scriptManager;
			dlg.New(strProjectPath,
				strTempName,
				strNewLocate);

			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			// 实际插入project参数
			XmlNode projNode = scriptManager.NewProjectNode(
				dlg.ResultProjectNamePath,
				dlg.ResultLocate,
				false);	// false表示不需要创建目录和缺省文件

			// 兑现显示?
			scriptManager.FillOneLevel(treeView1, 
				parent, 
				projNode.ParentNode);
			TreeViewUtil.SelectTreeNode(treeView1, 
				scriptManager.GetNodePathName(projNode),
                '/');

			/*
			if (parent != null) 
			{
				parent.Expand();
			}
			*/

			scriptManager.Save();
		}

		// 新目录
		private void button_newDir_Click(object sender, System.EventArgs e)
		{
			// 当前所在路径
			string strDirPath = "";

			TreeNode parent = null;

			int nPrefixNumber = 1;

			// 当前已选择的node
			if (treeView1.SelectedNode == null) 
			{
				strDirPath = "";
			}
			else 
			{
				// project下不能创建子节点，但是可以创建兄弟?

				if (treeView1.SelectedNode.ImageIndex == 0)
					parent = treeView1.SelectedNode;
				else
					parent = treeView1.SelectedNode.Parent;

				strDirPath = parent != null ? parent.FullPath : "";
			}

			string strTempName = GetTempDirName(treeView1,
				parent,
				"new dir",
				ref nPrefixNumber);

			DirNameDlg namedlg = new DirNameDlg();
            GuiUtil.AutoSetDefaultFont(namedlg);

			namedlg.textBox_dirName.Text = strTempName;
			namedlg.StartPosition = FormStartPosition.CenterScreen;
			namedlg.ShowDialog(this);

			if (namedlg.DialogResult == DialogResult.OK) 
			{
				string strDirNamePath = (strDirPath!="" ? strDirPath + "/" : "")
					+ namedlg.textBox_dirName.Text;

				XmlNode dirNode = scriptManager.NewDirNode(strDirNamePath);

				if (dirNode != null) 
				{
					scriptManager.Save();

					// 兑现显示?
					scriptManager.FillOneLevel(treeView1, 
						parent, 
						dirNode.ParentNode);

					TreeViewUtil.SelectTreeNode(treeView1, 
						scriptManager.GetNodePathName(dirNode),
                        '/');

					/*
					if (parent != null) 
						parent.Expand();
						*/

				}
			}

		}

		// 删除方案
		private void button_delete_Click(object sender, System.EventArgs e)
		{
			string strError;

			// 当前已选择的node
			if (treeView1.SelectedNode == null) 
			{
				MessageBox.Show("尚未选择方案或目录");
				return ;
			}

			TreeNode parent = treeView1.SelectedNode.Parent;

			int nRet;
			XmlNode parentXmlNode = null;
			DialogResult msgResult;

			TreeNode node = treeView1.SelectedNode;
			if (node.ImageIndex == 0) 
			{
				node.ExpandAll();
				msgResult = MessageBox.Show(this,
					"确实要删除目录 " + node.FullPath + "和下级包含的全部目录和方案么?",
					"script",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button2);
				if (msgResult == DialogResult.No)
					return;

				// return:
				//	-1	error
				//	0	not found
				//	1	found and changed
				nRet = scriptManager.DeleteDir(node.FullPath,
					out parentXmlNode,
					out strError);
				if (nRet == -1) 
				{
					MessageBox.Show(strError);
					// return ;
				}

				if (nRet == 1) 
				{
					if (parentXmlNode != null)
					{
						// 兑现显示?
						scriptManager.FillOneLevel(treeView1, 
							parent, 
							parentXmlNode);
					}
					scriptManager.Save();
				}
				return ;
			}


			string strProjectNamePath = node.FullPath;

			msgResult = MessageBox.Show(this,
				"确实要删除方案 '" + node.FullPath + "' ?",
				"script",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
			if (msgResult == DialogResult.No)
				return;

			// 准备删除后，selection停靠的节点路径
			TreeNode nodeNear = null;
			if (node.PrevNode != null)
				nodeNear = node.PrevNode;
			else if (node.NextNode != null)
				nodeNear = node.NextNode;
			else 
				nodeNear = parent;
			string strPath = "";
			if (nodeNear != null)
				strPath = nodeNear.FullPath;

			// 删除一个方案
			// return:
			// -1	error
			//	0	not found
			//	1	found and deleted
			//	2	canceld	因此project没有被删除
			nRet = scriptManager.DeleteProject(
				strProjectNamePath,
				true,
				out parentXmlNode,
				out strError);
			if (nRet == -1)
				goto ERROR1;

			if (nRet == 0) 
			{
				strError = "方案 "+ strProjectNamePath + " 在ScriptManager中没有找到";
				goto CANCEL1;
			}

			if (nRet == 2)
			{
				strError = "方案 "+ strProjectNamePath + " 放弃删除";
				goto CANCEL1;
			}

			if (parentXmlNode != null)
			{
				// 兑现显示?
				scriptManager.FillOneLevel(treeView1, 
					parent, 
					parentXmlNode);

				TreeViewUtil.SelectTreeNode(treeView1, 
					strPath,
                    '/');

			}


			scriptManager.Save();
			return;
			CANCEL1:
				if (strError != "")
					MessageBox.Show(strError);
			return;

			ERROR1:
				MessageBox.Show(strError);
			return;
		}


		private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			// 当前已选择的node
			if (treeView1.SelectedNode == null) 
			{
				button_modify.Enabled = false;
				button_new.Enabled = true;
				button_delete.Enabled = false;
				button_up.Enabled = false;
				button_down.Enabled = false;
				button_export.Enabled = false;
				return ;
			}
			

			if (treeView1.SelectedNode.ImageIndex == 0) // 目录
			{
				button_modify.Enabled = true;
				button_new.Enabled = true;
				button_delete.Enabled = true;

				if (treeView1.SelectedNode.PrevNode == null)
					button_up.Enabled = false;
				else
					button_up.Enabled = true;
				if (treeView1.SelectedNode.NextNode == null)
					button_down.Enabled = false;
				else
					button_down.Enabled = true;

				button_export.Enabled = false;
				return;
			}

			if (treeView1.SelectedNode.ImageIndex == 1) // project
			{
				button_modify.Enabled = true;
				button_new.Enabled = true;
				button_delete.Enabled = true;
				if (treeView1.SelectedNode.PrevNode == null)
					button_up.Enabled = false;
				else
					button_up.Enabled = true;
				if (treeView1.SelectedNode.NextNode == null)
					button_down.Enabled = false;
				else
					button_down.Enabled = true;

				button_export.Enabled = true;
			}
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{

			this.Close();
			this.DialogResult = DialogResult.OK;
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{

			this.Close();
			this.DialogResult = DialogResult.Cancel;
		}



		private void treeView1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			TreeNode node = treeView1.SelectedNode;

			//
			menuItem = new MenuItem("修改(&M)");
			menuItem.Click += new System.EventHandler(this.button_modify_Click);
			if (node == null) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			string strText;

		{
			TreeNode parent = null;

			if (treeView1.SelectedNode != null) 
			{
				if (treeView1.SelectedNode.ImageIndex == 0)
					parent = treeView1.SelectedNode;
				else
					parent = treeView1.SelectedNode.Parent;
			}

			if (parent == null)
				strText = "在根下";
			else
				strText = "在目录 " + parent.Text + "下";
		}

			//
			menuItem = new MenuItem("新增方案(" + strText + ") (&N)");
			menuItem.Click += new System.EventHandler(this.button_newProject_Click);
			contextMenu.MenuItems.Add(menuItem);


		{
			TreeNode parent = null;

			if (treeView1.SelectedNode != null) 
			{
				if (treeView1.SelectedNode.ImageIndex == 0)
					parent = treeView1.SelectedNode;
				else
					parent = treeView1.SelectedNode.Parent;
			}

			if (parent == null)
				strText = "在根下";
			else
				strText = "在目录 " + parent.Text + "下";
		}


			//
			menuItem = new MenuItem("新增目录(" + strText + ") (&A)");
			menuItem.Click += new System.EventHandler(this.button_newDir_Click);
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			// 
			menuItem = new MenuItem("上移(&U)");
			menuItem.Click += new System.EventHandler(this.button_up_Click);
			if (treeView1.SelectedNode == null
				|| treeView1.SelectedNode.PrevNode == null)
				menuItem.Enabled = false;
			else
				menuItem.Enabled = true;
			contextMenu.MenuItems.Add(menuItem);



			// 
			menuItem = new MenuItem("下移(&D)");
			menuItem.Click += new System.EventHandler(this.button_down_Click);
			if (treeView1.SelectedNode == null
				|| treeView1.SelectedNode.NextNode == null)
				menuItem.Enabled = false;
			else
				menuItem.Enabled = true;
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			//
			menuItem = new MenuItem("删除(&E)");
			menuItem.Click += new System.EventHandler(this.button_delete_Click);
			if (node == null) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);


			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("复制(&C)");
			menuItem.Click += new System.EventHandler(this.button_CopyToClipboard_Click);
			if (node == null || node.ImageIndex == 0) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);

			bool bHasClipboardObject = false;
			IDataObject iData = Clipboard.GetDataObject();
			if (iData == null
				|| iData.GetDataPresent(typeof(Project)) == false)
				bHasClipboardObject = false;
			else
				bHasClipboardObject = true;



			menuItem = new MenuItem("粘贴到当前目录 '" + GetCurTreeDir() + "' (&P)");
			menuItem.Click += new System.EventHandler(this.button_PasteFromClipboard_Click);
			if (bHasClipboardObject== false)
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("粘贴到原目录 '" + GetClipboardProjectDir() + "' (&O)");
			menuItem.Click += new System.EventHandler(this.button_PasteFromClipboardToOriginDir_Click);

			if (bHasClipboardObject== false)
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);


			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("导出(&E)");
			menuItem.Click += new System.EventHandler(this.button_CopyToFile_Click);
			if (node == null || node.ImageIndex == 0) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("导入(&I)");
			menuItem.Click += new System.EventHandler(this.button_PasteFromFile_Click);
			/*
			if (node == null || node.ImageIndex == 0) 
			{
				menuItem.Enabled = false;
			}
			*/
			contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("从 dp2003.com 安装方案(&I)");
            menuItem.Click += new System.EventHandler(this.menu_installProjects_Click);
            if (string.IsNullOrEmpty(this.ProjectsUrl) == true)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("从 dp2003.com 检查更新(&U)");
            menuItem.Click += new System.EventHandler(this.button_updateProjects_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("从磁盘目录安装方案(&D)");
            menuItem.Click += new System.EventHandler(this.menu_installProjectsFromDisk_Click);
            if (string.IsNullOrEmpty(this.ProjectsUrl) == true)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("从磁盘目录检查更新(&P)");
            menuItem.Click += new System.EventHandler(this.button_updateProjectsFromDisk_Click);
            contextMenu.MenuItems.Add(menuItem);

			contextMenu.Show(treeView1, new Point(e.X, e.Y) );		
		}

        // 从磁盘目录安装方案
        void menu_installProjectsFromDisk_Click(object sender, System.EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            int nInstallCount = 0;

            bool bDebugger = false;
            if (Control.ModifierKeys == Keys.Control)
                bDebugger = true;

            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定方案所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            // 寻找 projects.xml 文件
            string strProjectsFileName = PathUtil.MergePath(dir_dlg.SelectedPath, "projects.xml");
            if (File.Exists(strProjectsFileName) == false)
            {
                // strError = "您所指定的目录 '" + dir_dlg.SelectedPath + "' 中并没有包含 projects.xml 文件，无法进行安装";
                // goto ERROR1;

                // 如果没有 projects.xml 文件，则搜索全部 *.projpack 文件，并创建好一个临时的 ~projects.xml文件
                Debug.Assert(string.IsNullOrEmpty(this.DataDir) == false, "");
                strProjectsFileName = PathUtil.MergePath(this.DataDir, "~projects.xml");
                nRet = ScriptManager.BuildProjectsFile(dir_dlg.SelectedPath,
                    strProjectsFileName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }


            this.EnableControls(false);
            try
            {
                // 列出已经安装的方案的URL
                List<string> installed_urls = new List<string>();

                nRet = this.scriptManager.GetInstalledUrls(out installed_urls,
                out strError);
                if (nRet == -1)
                    goto ERROR1;

                SelectInstallProjectsDialog dlg = new SelectInstallProjectsDialog();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.FilterHosts.Clear();
                Debug.Assert(string.IsNullOrEmpty(this.HostName) == false, "");
                dlg.FilterHosts.Add(this.HostName);
                dlg.XmlFilename = strProjectsFileName;
                dlg.InstalledUrls = installed_urls;
                if (bDebugger == true)
                    dlg.Category = "debugger";
                dlg.StartPosition = FormStartPosition.CenterScreen;

                if (this.AppInfo != null)
                    this.AppInfo.LinkFormState(dlg,
                        "SelectInstallProjectsDialog_state");
                dlg.ShowDialog(this);
                if (this.AppInfo != null)
                    this.AppInfo.UnlinkFormState(dlg);
                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                foreach (ProjectItem item in dlg.SelectedProjects)
                {
                    string strLastModified = "";
                    string strLocalFileName1 = "";

                    if (string.IsNullOrEmpty(item.FilePath) == false)
                    {
                        strLocalFileName1 = item.FilePath;
                    }
                    else
                    {
                        string strPureFileName = ScriptManager.GetFileNameFromUrl(item.Url);

                        strLocalFileName1 = PathUtil.MergePath(dir_dlg.SelectedPath, strPureFileName);
                    }

                    FileInfo fi = new FileInfo(strLocalFileName1);
                    if (fi.Exists == false)
                    {
                        strError = "没有找到文件 '" + strLocalFileName1 + "'";
                        //    strError = "目录 '" + dir_dlg.SelectedPath + "' 中没有找到文件 '" + strPureFileName + "'";
                        goto ERROR1;
                    }


                    strLastModified = DateTimeUtil.Rfc1123DateTimeString(fi.LastWriteTimeUtc);

                    // 安装Project
                    // return:
                    //      -1  出错
                    //      0   没有安装方案
                    //      >0  安装的方案数
                    nRet = this.scriptManager.InstallProject(
                        this,
                        "当前统计窗",
                        strLocalFileName1,
                        strLastModified,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    nInstallCount += nRet;
                }

                // 刷新树的显示
                if (nInstallCount > 0)
                {
                    this.treeView1.Nodes.Clear();
                    scriptManager.FillTree(this.treeView1);
                    treeView1_AfterSelect(null, null);
                }
            }
            finally
            {
                this.EnableControls(true);
            }

            MessageBox.Show(this, "共安装方案 " + nInstallCount.ToString() + " 个");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从 dp2003.com 安装方案
        void menu_installProjects_Click(object sender, System.EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            int nInstallCount = 0;

            bool bDebugger = false;
            if (Control.ModifierKeys == Keys.Control)
                bDebugger = true;


            this.EnableControls(false);
            try
            {
                Debug.Assert(string.IsNullOrEmpty(this.DataDir) == false, "");

                // 下载projects.xml文件
                string strLocalFileName = PathUtil.MergePath(this.DataDir, "~temp_projects.xml");
                string strTempFileName = PathUtil.MergePath(this.DataDir, "~temp_download_projects.xml");

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
                    this,
                    this.ProjectsUrl,   // "http://dp2003.com/dp2batch/projects/projects.xml"
                    strLocalFileName,
                    strTempFileName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 列出已经安装的方案的URL
                List<string> installed_urls = new List<string>();

                nRet = this.scriptManager.GetInstalledUrls(out installed_urls,
                out strError);
                if (nRet == -1)
                    goto ERROR1;

                SelectInstallProjectsDialog dlg = new SelectInstallProjectsDialog();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.FilterHosts.Clear();
                Debug.Assert(string.IsNullOrEmpty(this.HostName) == false, "");
                dlg.FilterHosts.Add(this.HostName);
                dlg.XmlFilename = strLocalFileName;
                dlg.InstalledUrls = installed_urls;
                if (bDebugger == true)
                    dlg.Category = "debugger";
                dlg.StartPosition = FormStartPosition.CenterScreen;

                if (this.AppInfo != null)
                    this.AppInfo.LinkFormState(dlg,
                        "SelectInstallProjectsDialog_state");
                dlg.ShowDialog(this);
                if (this.AppInfo != null)
                    this.AppInfo.UnlinkFormState(dlg);
                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                foreach (ProjectItem item in dlg.SelectedProjects)
                {
                    string strLocalFileName1 = this.DataDir + "\\~install_project.projpack";
                    string strTempFileName1 = this.DataDir + "\\~temp_download_webfile";
                    string strLastModified = "";

                    nRet = WebFileDownloadDialog.DownloadWebFile(
                        this,
                        item.Url,
                        strLocalFileName1,
                        strTempFileName1,
                        "",
                        out strLastModified,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 安装Project
                    // return:
                    //      -1  出错
                    //      0   没有安装方案
                    //      >0  安装的方案数
                    nRet = this.scriptManager.InstallProject(
                        this,
                        "当前统计窗",
                        strLocalFileName1,
                        strLastModified,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    nInstallCount += nRet;
                }

                // 刷新树的显示
                if (nInstallCount > 0)
                {
                    this.treeView1.Nodes.Clear();
                    scriptManager.FillTree(this.treeView1);
                    treeView1_AfterSelect(null, null);
                }
            }
            finally
            {
                this.EnableControls(true);
            }


            MessageBox.Show(this, "共安装方案 " + nInstallCount.ToString() + " 个");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

		private void treeView1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			TreeNode curSelectedNode = treeView1.GetNodeAt(e.X, e.Y);

			if (treeView1.SelectedNode != curSelectedNode) 
			{
				treeView1.SelectedNode = curSelectedNode;

				if (treeView1.SelectedNode == null)
					treeView1_AfterSelect(null, null);	// 补丁
			}

		}

		void MoveUpDown(bool bUp)
		{
			// 当前已选择的node
			if (treeView1.SelectedNode == null) 
			{
				MessageBox.Show("尚未选择方案或目录");
				return ;
			}

			TreeNode parent = treeView1.SelectedNode.Parent;

			string strPath = treeView1.SelectedNode.FullPath;

			int nRet;
			XmlNode parentXmlNode = null;

			TreeNode node = treeView1.SelectedNode;

			// 上下移动节点
			// return:
			//	0	not found
			//	1	found and moved
			//	2	cant move
			nRet =  scriptManager.MoveNode(node.FullPath,
				bUp,
				out parentXmlNode);

			if (nRet == 1) 
			{
				if (parentXmlNode != null)
				{
					// 兑现显示?
					scriptManager.FillOneLevel(treeView1, 
						parent, 
						parentXmlNode);

					TreeViewUtil.SelectTreeNode(treeView1, 
						strPath,
                        '/');
				}
				scriptManager.Save();
			}

			if (nRet == 2) 
			{
				MessageBox.Show("已经到头了，不能移动了...");
			}
			return ;
		}

		private void button_up_Click(object sender, System.EventArgs e)
		{
			MoveUpDown(true);
		}

		private void button_down_Click(object sender, System.EventArgs e)
		{
			MoveUpDown(false);
		}

		/*
		static void SelectTreeNode(TreeView treeView, 
			string strPath)
		{
			string[] aName = strPath.Split(new Char [] {'/'});

			TreeNode node = null;
			TreeNode nodeThis = null;
			for(int i=0;i<aName.Length;i++)
			{
				TreeNodeCollection nodes = null;

				if (node == null)
					nodes = treeView.Nodes;
				else 
					nodes = node.Nodes;

				bool bFound = false;
				for(int j=0;j<nodes.Count;j++)
				{
					if (aName[i] == nodes[j].Text) 
					{
						bFound = true;
						nodeThis = nodes[j];
						break;
					}
				}
				if (bFound == false)
					break;

				node = nodeThis;

			}

			if (nodeThis!= null && nodeThis.Parent != null)
				nodeThis.Parent.Expand();

			treeView.SelectedNode = nodeThis;
		}
		*/

		private void treeView1_DoubleClick(object sender, System.EventArgs e)
		{
			// 当前已选择的node
			if (treeView1.SelectedNode == null) 
				return ;

			TreeNode node = treeView1.SelectedNode;
			if (node.ImageIndex == 0) // 目录
				return;

			button_modify_Click(null, null);
		}

		// 导入
		private void button_import_Click(object sender, System.EventArgs e)
		{
			// 未按下Control键, 一般导入功能 -- 导入当前目录
			if (!(Control.ModifierKeys == Keys.Control))
			{
				button_PasteFromFile_Click(null, null);
				return;
			}

			int nRet ;
			string strError = "";

			// 询问project*.xml文件全路径
			OpenFileDialog projectDefFileDlg = new OpenFileDialog();

			projectDefFileDlg.FileName = "outer_projects.xml";
			projectDefFileDlg.InitialDirectory = Environment.CurrentDirectory;
			projectDefFileDlg.Filter = "projects files (outer*.xml)|outer*.xml|All files (*.*)|*.*" ;
			//dlg.FilterIndex = 2 ;
			projectDefFileDlg.RestoreDirectory = true ;

			if(projectDefFileDlg.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			ScriptManager newScriptManager = new ScriptManager();
			newScriptManager.applicationInfo = null;	//applicationInfo;
			newScriptManager.CfgFilePath = projectDefFileDlg.FileName;
			newScriptManager.Load();

			// 选取要Import的Project名

			GetProjectNameDlg nameDlg = new GetProjectNameDlg();
            GuiUtil.AutoSetDefaultFont(nameDlg);

			nameDlg.Text = "请选定要导入的外部方案名";
			nameDlg.scriptManager = newScriptManager;
			/*
			nameDlg.textBox_projectName.Text = applicationInfo.GetString(
				"projectmanagerdlg_import",
				"lastUsedProject",
				"");
			*/

			nameDlg.StartPosition = FormStartPosition.CenterScreen;
			nameDlg.ShowDialog(this);

			if (nameDlg.DialogResult != DialogResult.OK)
				return;

			string strSourceProjectName = nameDlg.ProjectName;

			string strSourceLocate = "";

			// 获得源方案参数
			// strProjectNamePath	方案名，或者路径
			// return:
			//		-1	error
			//		0	not found project
			//		1	found
			nRet = newScriptManager.GetProjectData(
				strSourceProjectName,
				out strSourceLocate);
			if (nRet == -1) 
			{
				MessageBox.Show(this, "source GetProjectData() "+ strSourceProjectName + " error...");
				return ;
			}
			if (nRet == 0)
			{
				MessageBox.Show(this, "source project "+ strSourceProjectName + " not found error...");
				return ;
			}


			/*
			applicationInfo.SetString(
				"projectmanagerdlg_import",
				"lastUsedProject",
				nameDlg.textBox_projectName.Text);
			*/

			REDOEXPORT:

				string strTargetLocate = "";
			// 获得目标方案参数
			// strProjectNamePath	方案名，或者路径
			// return:
			//		-1	error
			//		0	not found project
			//		1	found
			nRet = this.scriptManager.GetProjectData(
				strSourceProjectName,
				out strTargetLocate);
			if (nRet == -1) 
			{
				MessageBox.Show(this, "target GetProjectData() "+ strSourceProjectName + " error...");
				return ;
			}

			// 发现重名，询问是否覆盖
			if (nRet == 1) 
			{
				string strText = "当前已经存在与源 '"
					+ strSourceProjectName + "' 同名的目标方案(磁盘目录位于'"
					+ strTargetLocate + "')。\r\n\r\n" 
					+ "请问是否覆盖已有的目标方案?\r\n(Yes=覆盖; No=改名后导入；Cancel=放弃操作)";


				DialogResult msgResult = MessageBox.Show(this,
					strText,
					"script",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button2);

				
				if (msgResult == DialogResult.Cancel) 
					return;


				if (msgResult == DialogResult.Yes) 
				{	// 覆盖
					// 拷贝目录
					nRet = PathUtil.CopyDirectory(strSourceLocate,
						strTargetLocate,
						true,
						out strError);
					if (nRet == -1)
						goto ERROR1;
					goto END1;
				}
				else 
				{	// 改名


					// 询问新名字
					nameDlg = new GetProjectNameDlg();
                    GuiUtil.AutoSetDefaultFont(nameDlg);

					nameDlg.Text = "请制定目标(系统内)新方案名";
					nameDlg.scriptManager = this.scriptManager;
					nameDlg.ProjectName = strSourceProjectName;

					nameDlg.StartPosition = FormStartPosition.CenterScreen;
					nameDlg.ShowDialog(this);

					if (nameDlg.DialogResult != DialogResult.OK)
						goto END2;

					strSourceProjectName = nameDlg.ProjectName;
					goto REDOEXPORT;

				}



			}
			else // 不重名，直接复制
			{
				// 创建一个新的project，获得strTargetLocate
				int nPrefixNumber = -1;	// 0
				strTargetLocate = this.scriptManager.NewProjectLocate(
					PathUtil.PureName(strSourceLocate),	// 尽量取和源相同的末级目录名
					ref nPrefixNumber);

				// 拷贝目录
				nRet = PathUtil.CopyDirectory(strSourceLocate,
					strTargetLocate,
					true,
					out strError);
				if (nRet == -1)
					goto ERROR1;

				// 实际插入project参数
				XmlNode projNode = this.scriptManager.NewProjectNode(
					strSourceProjectName,	// 沿用原来的名字
					strTargetLocate,
					false);	// false表示不需要创建目录和缺省文件

				// 兑现显示?
				scriptManager.RefreshTree(treeView1);
			}

			END1:

				this.scriptManager.Save();



			TreeViewUtil.SelectTreeNode(treeView1, 
				strSourceProjectName,
                '/');

			MessageBox.Show(this, "外部方案 '" + strSourceProjectName + "' 已经成功导入本系统。");

			return;
			END2:
				return;
			ERROR1:
				MessageBox.Show(this, strError);
			return ;
		}

		// 导出
		private void button_export_Click(object sender, System.EventArgs e)
		{
			// 未按下Control键, 一般导出功能
			if (!(Control.ModifierKeys == Keys.Control))
			{
				button_CopyToFile_Click(null, null);
				return;
			}

			// 特殊导出功能
			int nRet ;
			string strError = "";

			TreeNode node = treeView1.SelectedNode;

			if (node == null) 
			{
				MessageBox.Show(this,"请先选定要导出的方案...");
				return;
			}

			string strSourceProjectName = node.FullPath;

			string strSourceLocate = "";

			// 获得源方案参数
			// strProjectNamePath	方案名，或者路径
			// return:
			//		-1	error
			//		0	not found project
			//		1	found
			nRet = scriptManager.GetProjectData(
				strSourceProjectName,
				out strSourceLocate);
			if (nRet == -1) 
			{
				MessageBox.Show(this, "source GetProjectData() "+ strSourceProjectName + " error...");
				return ;
			}
			if (nRet == 0)
			{
				MessageBox.Show(this, "source project "+ strSourceProjectName + " not found error...");
				return ;
			}



			// 询问project*.xml文件全路径
			SaveFileDialog projectDefFileDlg = new SaveFileDialog();

			projectDefFileDlg.CreatePrompt = false;
			projectDefFileDlg.FileName = "outer_projects.xml";
			projectDefFileDlg.InitialDirectory = Environment.CurrentDirectory;
			projectDefFileDlg.Filter = "projects files (outer*.xml)|outer*.xml|All files (*.*)|*.*" ;
			//dlg.FilterIndex = 2 ;
			projectDefFileDlg.RestoreDirectory = true ;

			if(projectDefFileDlg.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			// 如果文件不存在，则创建之
			if (File.Exists(projectDefFileDlg.FileName) == false)
				ScriptManager.CreateDefaultProjectsXmlFile(projectDefFileDlg.FileName,
					"outercfgs");

			// 创建ScriptManager对象
			ScriptManager newScriptManager = new ScriptManager();
			newScriptManager.applicationInfo = null;	//applicationInfo;
			newScriptManager.CfgFilePath = projectDefFileDlg.FileName;
			newScriptManager.Load();

			// 查询Project路径+名是否已经在输出的projects.xml已经存在

			REDOEXPORT:

				string strTargetLocate = "";
			// 获得方案参数
			// strProjectNamePath	方案名，或者路径
			// return:
			//		-1	error
			//		0	not found project
			//		1	found
			nRet = newScriptManager.GetProjectData(
				strSourceProjectName,
				out strTargetLocate);
			if (nRet == -1) 
			{
				MessageBox.Show(this, "target GetProjectData() "+ strSourceProjectName + " error...");
				return ;
			}

			// 发现重名，询问是否覆盖
			if (nRet == 1) 
			{
				string strText = "外部方案集\r\n  (由文件 '" + projectDefFileDlg.FileName + "' 管理)\r\n已经存在一个与源 \r\n'"
					+ strSourceProjectName + "'\r\n 同名的方案\r\n  (其磁盘目录位于 '"
					+ strTargetLocate + "')。\r\n\r\n" 
					+ "请问是否覆盖此方案?\r\n(Yes=覆盖; No=改名后导出；Cancel=放弃操作)\r\n\r\n注意：覆盖后无法还原。";


				DialogResult msgResult = MessageBox.Show(this,
					strText,
					"script",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button2);

				
				if (msgResult == DialogResult.Cancel) 
					return;


				if (msgResult == DialogResult.Yes) 
				{	// 覆盖
					// 拷贝目录
					nRet = PathUtil.CopyDirectory(strSourceLocate,
						strTargetLocate,
						true,
						out strError);
					if (nRet == -1)
						goto ERROR1;
					goto END1;
				}
				else 
				{	// 改名


					// 询问新名字
					GetProjectNameDlg nameDlg = new GetProjectNameDlg();
                    GuiUtil.AutoSetDefaultFont(nameDlg);

					nameDlg.Text = "请选定目标(外部)新方案名";
					nameDlg.scriptManager = newScriptManager;
					nameDlg.ProjectName = strSourceProjectName;

					nameDlg.StartPosition = FormStartPosition.CenterScreen;
					nameDlg.ShowDialog(this);

					if (nameDlg.DialogResult != DialogResult.OK)
						goto END2;

					strSourceProjectName = nameDlg.ProjectName;
					goto REDOEXPORT;

				}



			}
			else // 不重名，直接复制
			{
				// 创建一个新的project，获得strTargetLocate
				int nPrefixNumber = -1;	// 0
				strTargetLocate = newScriptManager.NewProjectLocate(
					PathUtil.PureName(strSourceLocate),	// 尽量取和源相同的末级目录名
					ref nPrefixNumber);

				// 拷贝目录
				nRet = PathUtil.CopyDirectory(strSourceLocate,
					strTargetLocate,
					true,
					out strError);
				if (nRet == -1)
					goto ERROR1;

				// 实际插入project参数
				XmlNode projNode = newScriptManager.NewProjectNode(
					strSourceProjectName,	// 沿用原来的名字
					strTargetLocate,
					false);	// false表示不需要创建目录和缺省文件

			}

			END1:

				newScriptManager.Save();
			MessageBox.Show(this, "方案 '" + strSourceProjectName
				+ "' \r\n已经成功导出到文件 \r\n'" 
				+ newScriptManager.CfgFilePath + "' \r\n所管理的外部方案集内。");

			return;
			END2:
				return;
			ERROR1:
				MessageBox.Show(this, strError);
			return ;
		}

		// 复制
		private void button_CopyToClipboard_Click(object sender, System.EventArgs e)
		{

			int nRet;
			// 当前已选择的node
			if (treeView1.SelectedNode == null) 
			{
				MessageBox.Show("尚未选择方案或者目录");
				return ;
			}

			TreeNode node = treeView1.SelectedNode;
			if (node.ImageIndex == 0) // 目录
			{

			}
			else 
			{
				string strProjectNamePath = node.FullPath;

				string strLocate = "";

				// 获得方案参数
				// strProjectNamePath	方案名，或者路径
				// return:
				//		-1	error
				//		0	not found project
				//		1	found
				nRet = scriptManager.GetProjectData(
					strProjectNamePath,
					out strLocate);
				if (nRet != 1) 
				{
					MessageBox.Show("方案 "+ strProjectNamePath + " 在ScriptManager中没有找到");
					return ;
				}

                Project project = null;
                try
                {

                    project = Project.MakeProject(
                        strProjectNamePath,
                        strLocate);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "MakeProject error : " + ex.Message);
                    return;
                }

				Clipboard.SetDataObject(project);

			}


		}

		// 打包, 即复制到文件
		private void button_CopyToFile_Click(object sender, System.EventArgs e)
		{
			int nRet;
			// 当前已选择的node
			if (treeView1.SelectedNode == null) 
			{
				MessageBox.Show("尚未选择方案或者目录");
				return ;
			}

			TreeNode node = treeView1.SelectedNode;
			if (node.ImageIndex == 0) // 目录
			{

			}
			else 
			{
				string strProjectNamePath = node.FullPath;

				string strLocate = "";

				// 获得方案参数
				// strProjectNamePath	方案名，或者路径
				// return:
				//		-1	error
				//		0	not found project
				//		1	found
				nRet = scriptManager.GetProjectData(
					strProjectNamePath,
					out strLocate);
				if (nRet != 1) 
				{
					MessageBox.Show("方案 "+ strProjectNamePath + " 在ScriptManager中没有找到");
					return ;
				}

				string strPath;
				string strName;

				// 从完整的方案"名字路径"中,析出路径和名
				ScriptManager.SplitProjectPathName(strProjectNamePath,
					out strPath,
					out strName);

                Project project = null;

                try
                {
                    project = Project.MakeProject(
                         strProjectNamePath,
                         strLocate);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "MakeProject error : " + ex.Message);
                    return;
                }

                // 目前还允许host参数为空，这样软件不会加以检查
                string strHostName = project.GetHostName();
                if (string.IsNullOrEmpty(strHostName) == false
                    && strHostName != this.HostName)
                {
                    string strError = "拟导出的方案其(在metadata.xml定义的)宿主名为 '" + strHostName + "', 不符合当前窗口的宿主名 '" + this.HostName + "'。拒绝导出";
                    MessageBox.Show(strError);
                    return;
                }

				// 询问包文件全路径
				SaveFileDialog dlg = new SaveFileDialog();

				dlg.Title = "导出方案 -- 请指定要保存的文件名";
				dlg.CreatePrompt = true;
				dlg.FileName = strName + ".projpack";
				dlg.InitialDirectory = strRecentPackageFilePath == "" ?
					Environment.GetFolderPath(Environment.SpecialFolder.Personal)
					: strRecentPackageFilePath; //Environment.CurrentDirectory;
				dlg.Filter = "方案打包文件 (*.projpack)|*.projpack|All files (*.*)|*.*" ;
				dlg.RestoreDirectory = true ;

				if(dlg.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				strRecentPackageFilePath = dlg.FileName;


				// Clipboard.SetDataObject(project);
				ProjectCollection array = new ProjectCollection();

				array.Add(project);

				///
				//Opens a file and serializes the object into it in binary format.
				Stream stream = File.Open(dlg.FileName, FileMode.Create);
				BinaryFormatter formatter = new BinaryFormatter();

				formatter.Serialize(stream, array);
				stream.Close();
			}


		}


		// 解包, 即从文件复制到当前窗口 进入当前选定的目录
		private void button_PasteFromFile_Click(object sender, System.EventArgs e)
		{
            string strError = "";
            // 询问包文件全路径
			OpenFileDialog dlg = new OpenFileDialog();

			dlg.Title = "导入方案 -- 请指定要打开的文件名";
			dlg.FileName = "";	// strName + ".projpack";
			dlg.InitialDirectory = strRecentPackageFilePath == "" ?
				Environment.GetFolderPath(Environment.SpecialFolder.Personal)
				: strRecentPackageFilePath; //Environment.CurrentDirectory;
			dlg.Filter = "方案打包文件 (*.projpack)|*.projpack|All files (*.*)|*.*";	// projects package files
			dlg.RestoreDirectory = true ;

			if(dlg.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			strRecentPackageFilePath = dlg.FileName;

			Stream stream = null;
			try 
			{
				stream = File.Open(dlg.FileName, FileMode.Open);
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show(this, "文件 " + dlg.FileName + "不存在...");
				return;
			}

			BinaryFormatter formatter = new BinaryFormatter();

			ProjectCollection projects = null;
			try 
			{
				projects = (ProjectCollection)formatter.Deserialize(stream);
			}
			catch (SerializationException ex) 
			{
				MessageBox.Show("装载打包文件出错：" + ex.Message);
				return;
			}
			finally  
			{
				stream.Close();
			}

            FileInfo fi = new FileInfo(dlg.FileName);
            string strLastModified = DateTimeUtil.Rfc1123DateTimeString(fi.LastWriteTimeUtc);

			for(int i=0;i<projects.Count;i++)
			{
				Project project = (Project)projects[i];

                string strHostName = project.GetHostName();
                if (string.IsNullOrEmpty(strHostName) == false
                && strHostName != this.HostName)
                {
                    strError = "拟导入方案 '"+project.NamePath+"' 其宿主为 '" + strHostName + "', 不符合当前窗口的宿主名 '" + this.HostName + "'。被拒绝导入。";
                    MessageBox.Show(strError);
                    continue;
                }

				int nRet = PasteProject(project,
                    strLastModified,
                    false,
                    out strError);
				if (nRet == -1) 
				{
					MessageBox.Show(strError);
					return;
				}
			}
		}



		// 将Project对象Paste到管理界面中
		// bRestoreOriginNamePath	是否恢复到原始名字路径。==false，表示恢复到treeview当前目录
		private int PasteProject(Project project,
            string strLastModified,
			bool bRestoreOriginNamePath,
			out string strError)
		{
			strError = "";

			string strPath;
			string strName;

			// 纯Project名
			ScriptManager.SplitProjectPathName(project.NamePath,
				out strPath,
				out strName);

			string strCurPath = "";

			// 插入的目录

			int nRet;
			TreeNode node = null;
			TreeNode parent = null;


			// 恢复原始名字路径
			if (bRestoreOriginNamePath == true)
			{
				if (strPath == "")
					parent = null;
				XmlNode xmlNode = scriptManager.LocateDirNode(
					strPath);
				if (xmlNode == null) 
				{
					xmlNode = scriptManager.NewDirNode(
						strPath);
					// 兑现显示?
					scriptManager.RefreshTree(treeView1);
					TreeViewUtil.SelectTreeNode(treeView1, 
						strPath,
                        '/');
				}
				else 
				{
					TreeViewUtil.SelectTreeNode(treeView1, 
						strPath,
                        '/');
				}

			}

			// 恢复到当前目录
		{
			node = treeView1.SelectedNode;
			// 当前已选择的node
			if (node == null) 
			{
				// 根
			}
			else 
			{

				if (node.ImageIndex == 0) // 目录
				{
					parent = node;
					strCurPath = node.FullPath;
				}
				else 
				{
					parent = node.Parent;
					if (parent != null)
						strCurPath = parent.FullPath;
				}
			}
		}

			// 看看当前目录下是否已经存在同名Project

			string strLocate;
			// 获得方案参数
			// strProjectNamePath	方案名，或者路径
			// return:
			//		-1	error
			//		0	not found project
			//		1	found
			nRet = scriptManager.GetProjectData(
				ScriptManager.MakeProjectPathName(strCurPath, strName),
				out strLocate);
			if (nRet == -1) 
			{
				strError = "GetProjectData "+ ScriptManager.MakeProjectPathName(strCurPath, strName) + " error";
				return -1;
			}

			int nPrefixNumber = 0;

			if (nRet == 0) 
			{
				nPrefixNumber = -1;
			}
			else 
			{
				// 换名paste进入

				// 在儿子中找到一个不重复的名字
				strName = GetTempProjectName(treeView1,
					parent,
					strName,
					ref nPrefixNumber);
			}

			string strLocatePrefix = "";
			if (project.Locate == "") 
			{
				strLocatePrefix = strName;
			}
			else 
			{
				strLocatePrefix = PathUtil.PureName(project.Locate);
			}

			strLocate = scriptManager.NewProjectLocate(
				strLocatePrefix,
				ref nPrefixNumber);

			string strNamePath = ScriptManager.MakeProjectPathName(strCurPath, strName);

			// 直接paste
			project.WriteToLocate(strLocate,
                true);

			// 实际插入project参数
			XmlNode projNode = scriptManager.NewProjectNode(
				strNamePath,
				strLocate,
				false);	// false表示不需要创建目录和缺省文件

            DomUtil.SetAttr(projNode, "lastModified",
strLastModified);

			// 兑现显示?
			scriptManager.FillOneLevel(treeView1, 
				parent, 
				projNode.ParentNode);
			TreeViewUtil.SelectTreeNode(treeView1, 
				scriptManager.GetNodePathName(projNode),
                '/');

			scriptManager.Save();

			TreeViewUtil.SelectTreeNode(treeView1, 
				strNamePath,
                '/');

			return 0;
		}

		// 粘贴，到当前目录
		private void button_PasteFromClipboard_Click(object sender, System.EventArgs e)
		{
            string strError = "";

			IDataObject iData = Clipboard.GetDataObject();
			if (iData == null
				|| iData.GetDataPresent(typeof(Project)) == false)
			{
				strError = "剪贴板中尚不存在Project类型数据";
                goto ERROR1;
			}

			Project project = (Project)iData.GetData(typeof(Project));

			if (project == null) 
			{
				strError = "GetData error";
				goto ERROR1;
			}

            string strHostName = project.GetHostName();
            if (string.IsNullOrEmpty(strHostName) == false 
                && strHostName != this.HostName)
            {
                strError = "警告：拟粘贴的方案其宿主为 '" + strHostName + "', 不符合当前窗口的宿主名 '" + this.HostName + "'。请注意在粘贴完成后修改其宿主名(位于metadata.xml中)";
                MessageBox.Show(strError);
            }

			int nRet = PasteProject(project,
                "",
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(strError);
            return;
        }

        // 粘贴，到原始目录
        private void button_PasteFromClipboardToOriginDir_Click(object sender, System.EventArgs e)
        {
            string strError = "";
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(Project)) == false)
            {
                MessageBox.Show(this, "剪贴板中尚不存在Project类型数据");
                return;
            }

            Project project = (Project)iData.GetData(typeof(Project));
            if (project == null)
            {
                strError = "GetData error";
                goto ERROR1;
            }

            // TODO: 是否可以允许粘贴，但是警告一下即可？
            string strHostName = project.GetHostName();
            if (string.IsNullOrEmpty(strHostName) == false
                && strHostName != this.HostName)
            {
                strError = "警告：拟粘贴的方案其宿主为 '" + strHostName + "', 不符合当前窗口的宿主名 '"+this.HostName+"'。请注意在粘贴完成后修改其宿主名(位于metadata.xml中)";
                MessageBox.Show(strError);
            }

            int nRet = PasteProject(project, 
                "",
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(strError);
            return;
        }

		// 得到剪贴板中Project对象的原始名字目录
		string GetClipboardProjectDir()
		{
			IDataObject iData = Clipboard.GetDataObject();
			if (iData == null
				|| iData.GetDataPresent(typeof(Project)) == false)
			{
				// "剪贴板中尚不存在Project类型数据"
				return "";
			}

			Project project = (Project)iData.GetData(typeof(Project));

			if (project == null) 
			{
				// GetData error;
				return "";
			}

			string strPath;
			string strName;
			ScriptManager.SplitProjectPathName(project.NamePath, out strPath, out strName);
			return strPath;
		}

		string GetCurTreeDir()
		{
			TreeNode node = treeView1.SelectedNode;
			// 当前已选择的node
			if (node == null) 
			{
				// 根
				return "";
			}
			else 
			{

				if (node.ImageIndex == 0) // 目录
				{
					return node.FullPath;
				}
				else 
				{
					TreeNode parent = node.Parent;
					if (parent != null)
						return parent.FullPath;
					return "";
				}
			}
		}

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.treeView1.Enabled = bEnable;

            if (bEnable == false)
            {
                this.button_modify.Enabled = bEnable;
                this.button_new.Enabled = bEnable;
                this.button_delete.Enabled = bEnable;
                this.button_up.Enabled = bEnable;
                this.button_down.Enabled = bEnable;
                this.button_export.Enabled = bEnable;
            }
            if (bEnable == true)
            {
                treeView1_AfterSelect(null, null);
            }

            this.button_import.Enabled = bEnable;
            this.button_updateProjects.Enabled = bEnable;
            this.button_OK.Enabled = bEnable;
        }

        // 从磁盘目录检查更新
        private void button_updateProjectsFromDisk_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strWarning = "";
            string strUpdateInfo = "";
            int nUpdateCount = 0;

            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定方案所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;


            this.EnableControls(false);
            try
            {
                bool bHideMessageBox = false;
                bool bDontUpdate = false;
                // 检查更新一个容器节点下的全部方案
                // parameters:
                //      dir_node    容器节点。如果 == null 检查更新全部方案
                //      strSource   "!url"或者磁盘目录。分别表示从网络检查更新，或者从磁盘检查更新
                // return:
                //      -1  出错
                //      0   成功
                int nRet = this.scriptManager.CheckUpdate(
                    this,
                    null,
                    dir_dlg.SelectedPath,
                    ref bHideMessageBox,
                    ref bDontUpdate,
                    ref nUpdateCount,
                    ref strUpdateInfo,
                    ref strWarning,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);
            }

            if (string.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);

            if (string.IsNullOrEmpty(strUpdateInfo) == false)
                MessageBox.Show(this, "下列方案已经更新:\r\n" + strUpdateInfo);
            else
                MessageBox.Show(this, "没有发现更新");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从 dp2003.com 检查更新
        private void button_updateProjects_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strWarning = "";
            string strUpdateInfo = "";
            int nUpdateCount = 0;

            this.EnableControls(false);
            try
            {
                bool bHideMessageBox = false;
                bool bDontUpdate = false;
                // 检查更新一个容器节点下的全部方案
                // parameters:
                //      dir_node    容器节点。如果 == null 检查更新全部方案
                //      strSource   "!url"或者磁盘目录。分别表示从网络检查更新，或者从磁盘检查更新
                // return:
                //      -1  出错
                //      0   成功
                int nRet = this.scriptManager.CheckUpdate(
                    this,
                    null,
                    "!url",
                    ref bHideMessageBox,
                    ref bDontUpdate,
                    ref nUpdateCount,
                    ref strUpdateInfo,
                    ref strWarning,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);
            }

            if (string.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);

            if (string.IsNullOrEmpty(strUpdateInfo) == false)
                MessageBox.Show(this, "下列方案已经更新:\r\n" + strUpdateInfo);
            else
                MessageBox.Show(this, "没有发现更新");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        // 检查更新一个容器节点下的全部方案
        // parameters:
        //      dir_node    容器节点。如果 == null 检查更新全部方案
        // return:
        //      -1  出错
        //      0   成功
        public int CheckUpdate(
            TreeNode dir_node,
            ref int nUpdateCount,
            ref string strUpdateInfo,
            ref string strWarning,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            TreeNodeCollection nodes = null;
            if (dir_node == null)
                nodes = this.treeView1.Nodes;
            else
                nodes = dir_node.Nodes;

            foreach (TreeNode node in nodes)
            {
                if (node.ImageIndex == 0)
                {
                    // 目录节点
                    nRet = CheckUpdate(node, 
                        ref nUpdateCount,
                        ref strUpdateInfo,
                        ref strWarning,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                else
                {
                    // 方案节点
                    // return:
                    //      -1  出错
                    //      0   没有更新
                    //      1   已经更新
                    //      2   因为某些原因无法检查更新
                    nRet = CheckUpdateOneProject(node,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 2)
                        strWarning += "方案 " + node.FullPath + " "+strError+";\r\n";

                    if (nRet == 1)
                    {
                        nUpdateCount++;
                        strUpdateInfo += node.FullPath + "\r\n";
                    }
                }
            }

            return 0;
        }

        // 检查更新一个方案
        // return:
        //      -1  出错
        //      0   没有更新
        //      1   已经更新
        //      2   因为某些原因无法检查更新
        public int CheckUpdateOneProject(TreeNode node,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strProjectNamePath = node.FullPath;
            string strLocate = "";
            string strIfModifySince = "";

            // 获得方案参数
            // strProjectNamePath	方案名，或者路径
            // return:
            //		-1	error
            //		0	not found project
            //		1	found
            nRet = scriptManager.GetProjectData(
                strProjectNamePath,
                out strLocate,
                out strIfModifySince);
            if (nRet != 1)
            {
                strError = "方案 " + strProjectNamePath + " 在ScriptManager中没有找到";
                return -1;
            }

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

            // 尝试下载指定日期后更新过的文件

            Debug.Assert(string.IsNullOrEmpty(this.DataDir) == false, "");

            string strLocalFileName = PathUtil.MergePath(this.DataDir, "~temp_project.projpack");
            string strTempFileName = PathUtil.MergePath(this.DataDir, "~temp_download_webfile");

            string strLastModified = "";
            nRet = WebFileDownloadDialog.DownloadWebFile(
                this,
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

            if (string.IsNullOrEmpty(strIfModifySince) == false
                && string.IsNullOrEmpty(strLastModified) == false)
            {
                DateTime ifmodifiedsince = DateTimeUtil.FromRfc1123DateTimeString(strIfModifySince);
                DateTime lastmodified = DateTimeUtil.FromRfc1123DateTimeString(strLastModified);
                if (ifmodifiedsince == lastmodified)
                    return 0;
            }

            nRet = UpdateProject(
            strLocalFileName,
            strLocate,
            out strError);
            if (nRet == -1)
                return -1;

            nRet = scriptManager.SetProjectData(
    strProjectNamePath,
    strLastModified);
            scriptManager.Save();

            return 1;
        }

        // 更新Project
        private int UpdateProject(
            string strFilename,
            string strExistLocate,
            out string strError)
        {
            strError = "";

            Project project = null;
            Stream stream = null;
            try
            {
                stream = File.Open(strFilename, FileMode.Open);
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + "不存在...";
                return -1;
            }

            BinaryFormatter formatter = new BinaryFormatter();

            ProjectCollection projects = null;
            try
            {
                projects = (ProjectCollection)formatter.Deserialize(stream);
            }
            catch (SerializationException ex)
            {
                strError = "装载打包文件出错：" + ex.Message;
                return -1;
            }
            finally
            {
                stream.Close();
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

            // 直接paste
            project.WriteToLocate(strExistLocate);
            return 0;
        }

#endif
	}

    public delegate void AutoCreateProjectXmlFileEventHandle(object sender,
AutoCreateProjectXmlFileEventArgs e);

    public class AutoCreateProjectXmlFileEventArgs : EventArgs
    {
        public string Filename = "";
    }
}
