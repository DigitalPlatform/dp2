using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.GUI;

namespace DigitalPlatform.Script
{
	/// <summary>
	/// 管理一个 Project 的对话框
	/// </summary>
	public class OneProjectDialog : System.Windows.Forms.Form
	{
        public string HostName = "";
		public ScriptManager scriptManager = null;
		bool m_bNew = false;

		public string strTempLocate = "";

		public string ResultProjectNamePath = "";
		public string ResultLocate = "";
		//public string[] ResultRefs = null;

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_projectName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Label label5;
        private DigitalPlatform.GUI.ListViewNF listView_files;
		private System.Windows.Forms.ColumnHeader columnHeader_fileName;
		private System.Windows.Forms.ColumnHeader columnHeader_comment;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox_projectPathOfName;
		private System.Windows.Forms.TextBox textBox_projectLocate;
		private System.Windows.Forms.Button button_editFile;
		private System.Windows.Forms.Button button_newFile;
		private System.Windows.Forms.Button button_deleteFile;
		private System.Windows.Forms.Button button_changeProjectLocation;
		private System.Windows.Forms.Button button_changeProjectName;
		private System.Windows.Forms.CheckBox checkBox_displayTempFile;
        private Button button_openProjectFolder;
        private Button button_openInCode;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OneProjectDialog()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OneProjectDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_projectName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_projectPathOfName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.listView_files = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_fileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox_projectLocate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_editFile = new System.Windows.Forms.Button();
            this.button_newFile = new System.Windows.Forms.Button();
            this.button_deleteFile = new System.Windows.Forms.Button();
            this.button_changeProjectLocation = new System.Windows.Forms.Button();
            this.button_changeProjectName = new System.Windows.Forms.Button();
            this.checkBox_displayTempFile = new System.Windows.Forms.CheckBox();
            this.button_openProjectFolder = new System.Windows.Forms.Button();
            this.button_openInCode = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "方案名:";
            // 
            // textBox_projectName
            // 
            this.textBox_projectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_projectName.Location = new System.Drawing.Point(121, 9);
            this.textBox_projectName.Name = "textBox_projectName";
            this.textBox_projectName.ReadOnly = true;
            this.textBox_projectName.Size = new System.Drawing.Size(282, 21);
            this.textBox_projectName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "构成文件:";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(408, 322);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(74, 22);
            this.button_Cancel.TabIndex = 17;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(328, 322);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 22);
            this.button_OK.TabIndex = 16;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_projectPathOfName
            // 
            this.textBox_projectPathOfName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_projectPathOfName.Location = new System.Drawing.Point(121, 33);
            this.textBox_projectPathOfName.Name = "textBox_projectPathOfName";
            this.textBox_projectPathOfName.ReadOnly = true;
            this.textBox_projectPathOfName.Size = new System.Drawing.Size(282, 21);
            this.textBox_projectPathOfName.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "方案名所在位置:";
            // 
            // listView_files
            // 
            this.listView_files.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_files.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_fileName,
            this.columnHeader_comment});
            this.listView_files.FullRowSelect = true;
            this.listView_files.HideSelection = false;
            this.listView_files.Location = new System.Drawing.Point(9, 138);
            this.listView_files.Name = "listView_files";
            this.listView_files.Size = new System.Drawing.Size(394, 179);
            this.listView_files.TabIndex = 11;
            this.listView_files.UseCompatibleStateImageBehavior = false;
            this.listView_files.View = System.Windows.Forms.View.Details;
            this.listView_files.SelectedIndexChanged += new System.EventHandler(this.listView_files_SelectedIndexChanged);
            this.listView_files.DoubleClick += new System.EventHandler(this.listView_files_DoubleClick);
            this.listView_files.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_files_MouseUp);
            // 
            // columnHeader_fileName
            // 
            this.columnHeader_fileName.Text = "文件名";
            this.columnHeader_fileName.Width = 204;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "注释";
            this.columnHeader_comment.Width = 288;
            // 
            // textBox_projectLocate
            // 
            this.textBox_projectLocate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_projectLocate.Location = new System.Drawing.Point(121, 58);
            this.textBox_projectLocate.Name = "textBox_projectLocate";
            this.textBox_projectLocate.ReadOnly = true;
            this.textBox_projectLocate.Size = new System.Drawing.Size(361, 21);
            this.textBox_projectLocate.TabIndex = 6;
            this.textBox_projectLocate.TextChanged += new System.EventHandler(this.textBox_projectLocate_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "方案文件目录:";
            // 
            // button_editFile
            // 
            this.button_editFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editFile.Location = new System.Drawing.Point(408, 138);
            this.button_editFile.Name = "button_editFile";
            this.button_editFile.Size = new System.Drawing.Size(74, 21);
            this.button_editFile.TabIndex = 12;
            this.button_editFile.Text = "编辑(&E)";
            this.button_editFile.Click += new System.EventHandler(this.button_editFile_Click);
            // 
            // button_newFile
            // 
            this.button_newFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_newFile.Location = new System.Drawing.Point(408, 164);
            this.button_newFile.Name = "button_newFile";
            this.button_newFile.Size = new System.Drawing.Size(74, 22);
            this.button_newFile.TabIndex = 13;
            this.button_newFile.Text = "新增(&N)";
            this.button_newFile.Click += new System.EventHandler(this.button_newFile_Click);
            // 
            // button_deleteFile
            // 
            this.button_deleteFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_deleteFile.Location = new System.Drawing.Point(408, 205);
            this.button_deleteFile.Name = "button_deleteFile";
            this.button_deleteFile.Size = new System.Drawing.Size(74, 22);
            this.button_deleteFile.TabIndex = 14;
            this.button_deleteFile.Text = "删除(&D)";
            this.button_deleteFile.Click += new System.EventHandler(this.button_deleteFile_Click);
            // 
            // button_changeProjectLocation
            // 
            this.button_changeProjectLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_changeProjectLocation.Location = new System.Drawing.Point(408, 82);
            this.button_changeProjectLocation.Name = "button_changeProjectLocation";
            this.button_changeProjectLocation.Size = new System.Drawing.Size(74, 21);
            this.button_changeProjectLocation.TabIndex = 9;
            this.button_changeProjectLocation.Text = "改名(&R)";
            this.button_changeProjectLocation.Click += new System.EventHandler(this.button_changeProjectLocation_Click);
            // 
            // button_changeProjectName
            // 
            this.button_changeProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_changeProjectName.Location = new System.Drawing.Point(408, 9);
            this.button_changeProjectName.Name = "button_changeProjectName";
            this.button_changeProjectName.Size = new System.Drawing.Size(74, 22);
            this.button_changeProjectName.TabIndex = 2;
            this.button_changeProjectName.Text = "改名(&R)";
            this.button_changeProjectName.Click += new System.EventHandler(this.button_changeProjectName_Click);
            // 
            // checkBox_displayTempFile
            // 
            this.checkBox_displayTempFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_displayTempFile.AutoSize = true;
            this.checkBox_displayTempFile.Location = new System.Drawing.Point(9, 328);
            this.checkBox_displayTempFile.Name = "checkBox_displayTempFile";
            this.checkBox_displayTempFile.Size = new System.Drawing.Size(114, 16);
            this.checkBox_displayTempFile.TabIndex = 15;
            this.checkBox_displayTempFile.Text = "显示临时文件(&T)";
            this.checkBox_displayTempFile.CheckedChanged += new System.EventHandler(this.checkBox_displayTempFile_CheckedChanged);
            // 
            // button_openProjectFolder
            // 
            this.button_openProjectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_openProjectFolder.Location = new System.Drawing.Point(270, 82);
            this.button_openProjectFolder.Name = "button_openProjectFolder";
            this.button_openProjectFolder.Size = new System.Drawing.Size(133, 21);
            this.button_openProjectFolder.TabIndex = 8;
            this.button_openProjectFolder.Text = "打开文件夹(&F)...";
            this.button_openProjectFolder.Click += new System.EventHandler(this.button_openProjectFolder_Click);
            // 
            // button_openInCode
            // 
            this.button_openInCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_openInCode.Location = new System.Drawing.Point(121, 82);
            this.button_openInCode.Name = "button_openInCode";
            this.button_openInCode.Size = new System.Drawing.Size(143, 21);
            this.button_openInCode.TabIndex = 7;
            this.button_openInCode.Text = "在 Code 中打开(&C)...";
            this.button_openInCode.Click += new System.EventHandler(this.button_openInCode_Click);
            // 
            // OneProjectDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(492, 353);
            this.Controls.Add(this.button_openInCode);
            this.Controls.Add(this.button_openProjectFolder);
            this.Controls.Add(this.checkBox_displayTempFile);
            this.Controls.Add(this.button_changeProjectName);
            this.Controls.Add(this.button_changeProjectLocation);
            this.Controls.Add(this.button_deleteFile);
            this.Controls.Add(this.button_newFile);
            this.Controls.Add(this.button_editFile);
            this.Controls.Add(this.textBox_projectLocate);
            this.Controls.Add(this.textBox_projectPathOfName);
            this.Controls.Add(this.textBox_projectName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listView_files);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OneProjectDialog";
            this.ShowInTaskbar = false;
            this.Text = "脚本代码管理";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ScriptDlg_Closing);
            this.Closed += new System.EventHandler(this.ScriptDlg_Closed);
            this.Load += new System.EventHandler(this.ScriptDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void ScriptDlg_Load(object sender, System.EventArgs e)
		{
			// textBox_scriptCode.Font = new Font("Courier New",9);
			listView_files_SelectedIndexChanged(null, null);
		
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (textBox_projectName.Text == "")
			{
				MessageBox.Show("尚未指定方案名");
				return;
			}

			// 方案名 + 路径
			ResultProjectNamePath = textBox_projectPathOfName.Text;

			if (ResultProjectNamePath != "")
				ResultProjectNamePath += "/";
			
			ResultProjectNamePath += textBox_projectName.Text;

			ResultLocate = textBox_projectLocate.Text;
			// 避免有用目录和文件被OnClosed()自动删除
			this.strTempLocate = "";	

			/*
			// 源代码文件名
			ResultCodeFileName = textBox_codeFileName.Text;

			// refs
			string strTemp = textBox_refs.Text;
			strTemp = strTemp.Replace("\r\n", ",");	// 注意去掉空行?
			ResultRefs = strTemp.Split(new Char [] {','});

			// 避免有用文件被OnClosed()自动删除
			this.strTempCodeFileName = "";	
			*/

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
			this.DialogResult = DialogResult.Cancel;
		}

		// 初始化参数
		public void Initial(string strProjectNamePath,
			string strLocate)
		{
			// 析出路径和名

			/*
			int nRet = strProjectNamePath.LastIndexOf("/");
			if (nRet == -1) 
			{
				this.textBox_projectName.Text = strProjectNamePath;
				this.textBox_projectPathOfName.Text = "";
			}
			else 
			{
				this.textBox_projectName.Text = strProjectNamePath.Substring(nRet+1);
				this.textBox_projectPathOfName.Text = strProjectNamePath.Substring(0,nRet);
			}
			*/
			string strPath;
			string strName;
			ScriptManager.SplitProjectPathName(strProjectNamePath,
				out strPath,
				out strName);

			this.textBox_projectName.Text = strName;
			this.textBox_projectPathOfName.Text = strPath;

			// 文件目录
			textBox_projectLocate.Text = strLocate;

			LoadFileInfo();

			/*
			// refs
			//string[] aName = strList.Split(new Char [] {','});
			// 每行一个ref事项
			textBox_refs.Text = "";
			for(int i=0;i<refs.Length;i++)
			{
				if (textBox_refs.Text != "")
					textBox_refs.Text += "\r\n";
				textBox_refs.Text += refs[i];
			}
			*/

		}

		class FileInfoCompare : IComparer  
		{

			// Calls CaseInsensitiveComparer.Compare with the parameters reversed.
			int IComparer.Compare( Object x, Object y )  
			{
				return( (new CaseInsensitiveComparer()).Compare( ((FileInfo)x).Name, ((FileInfo)y).Name     ) );
			}

		}

		// 填充文件信息
		void LoadFileInfo()
		{

			listView_files.Items.Clear();

			DirectoryInfo di = null;
			
			try 
			{
				di = new DirectoryInfo(textBox_projectLocate.Text);
			}
			catch (Exception ex) 
			{
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
				return;
			}

			FileInfo[] afi = null;
			try 
			{
				afi = di.GetFiles();
			}
			catch (DirectoryNotFoundException ex)
			{
				MessageBox.Show(this, ex.Message);
				return;
			}

			Array.Sort(afi, new FileInfoCompare());

			for(int i=0;i<afi.Length;i++) 
			{
				string strName = afi[i].Name;

				if (strName.Length == 0)
					continue;
				if (strName[0] == '~'
					&& checkBox_displayTempFile.Checked == false)
					continue;	// 不显示 '~'打头的临时文件

				ListViewItem item =
					new ListViewItem(strName,
					0);

				item.SubItems.Add(GetComment(strName));

				listView_files.Items.Add(item);

			}

		}

		string GetComment(string strFileName)
		{
			string strResult = "";

			if (String.Compare(strFileName, 
				"main.cs",
				true) == 0)
				strResult = "主程序";
			else if (String.Compare(strFileName, 
				"marcfilter.fltx",
				true) == 0)
				strResult = "MARC记录过滤器";
			else if (String.Compare(strFileName, 
				"references.xml",
				true) == 0)
				strResult = "references配置";
            else if (String.Compare(strFileName,
    "metadata.xml",
    true) == 0)
                strResult = "元数据";

			return strResult;
		}


		/*
		void LoadCode()
		{
			// 源代码本身
			try 
			{
				StreamReader sr = new StreamReader(textBox_codeFileName.Text, true);
				textBox_scriptCode.Text = sr.ReadToEnd();
				sr.Close();
			}
			catch
			{
				textBox_scriptCode.Text = "";
			}
		}
		*/

		// 为新建一个Script准备参数
		public void New(string strProjectPath,
			string strTempName,
			string strNewLocate)
		{
			m_bNew = true;

			this.textBox_projectName.Text = strTempName;
			this.textBox_projectPathOfName.Text = strProjectPath;

			this.strTempLocate = strNewLocate;

			textBox_projectLocate.Text = strNewLocate;

			if (strNewLocate != null)
				scriptManager.CreateDefault(strNewLocate, this.HostName);

			LoadFileInfo();
		}

		private void ScriptDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{

			if (this.DialogResult != DialogResult.OK
				&& m_bNew == true) 
			{
				DialogResult msgResult = MessageBox.Show(this,
					"确实要放弃保存新方案 '" + textBox_projectName.Text + "' ?",
					"script",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button2);
				if (msgResult == DialogResult.No) 
				{
					e.Cancel = true;
					return;
				}
			}
		}

		private void ScriptDlg_Closed(object sender, System.EventArgs e)
		{
			// 删除临时代码文件
			if (this.strTempLocate != "") 
			{
				try 
				{
					Directory.Delete(this.strTempLocate, true);
				}
				catch
				{
				}
			}
		
		}

		private void listView_files_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			bool bSelected = listView_files.SelectedItems.Count > 0;

			//
			menuItem = new MenuItem("编辑(&E)");
			menuItem.Click += new System.EventHandler(this.button_editFile_Click);
			if (bSelected == false) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);

			//
			menuItem = new MenuItem("新增(&N)");
			menuItem.Click += new System.EventHandler(this.button_newFile_Click);
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			//
			menuItem = new MenuItem("删除(&D)");
			menuItem.Click += new System.EventHandler(this.button_deleteFile_Click);
			if (bSelected == false) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);

			contextMenu.Show(listView_files, new Point(e.X, e.Y) );		
			
		}

		private void button_editFile_Click(object sender, System.EventArgs e)
		{
			foreach(ListViewItem item in listView_files.SelectedItems) 
			{
				string strFileName = textBox_projectLocate.Text + "\\" + item.Text;

				System.Diagnostics.Process.Start("notepad.exe", strFileName);
			}
		}

		private void button_newFile_Click(object sender, System.EventArgs e)
		{
			FileNameDlg dlg = new FileNameDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			// 看看文件名是否重复创建
			if (GetFileNameItemIndex(dlg.textBox_fileName.Text) != -1) 
			{
				MessageBox.Show(this, "文件" + dlg.textBox_fileName.Text + "已经存在，不能重复创建...");
				return ;
			}


			string strFileName = textBox_projectLocate.Text + "\\" + dlg.textBox_fileName.Text;

			if (String.Compare(dlg.textBox_fileName.Text,
				"main.cs", true) == 0)
			{
				// ScriptManager.CreateDefaultMainCsFile(strFileName);
				scriptManager.OnCreateDefaultContent(strFileName);
			}
			else if (String.Compare(dlg.textBox_fileName.Text,
				"marcfilter.fltx", true) == 0)
			{
				// ScriptManager.CreateDefaultMarcFilterFile(strFileName);
				scriptManager.OnCreateDefaultContent(strFileName);
			}
			else if (String.Compare(dlg.textBox_fileName.Text,
				"references.xml", true) == 0)
			{
                // TODO: 应修改为事件驱动
				ScriptManager.CreateDefaultReferenceXmlFile(strFileName);
			}
            else if (String.Compare(dlg.textBox_fileName.Text,
                "metadata.xml", true) == 0)
            {
                Debug.Assert(string.IsNullOrEmpty(this.HostName) == false, "");
                // TODO: 应修改为事件驱动
                ScriptManager.CreateDefaultMetadataXmlFile(strFileName, this.HostName);
            }
            else 
			{
				StreamWriter sw = new StreamWriter(strFileName);
				sw.WriteLine("");
				sw.Close();
			}

			// 装入listview
			LoadFileInfo();

			int nIndex = GetFileNameItemIndex(dlg.textBox_fileName.Text);
			if (nIndex != -1) 
			{

				listView_files.SelectedItems.Clear();
				listView_files.Items[nIndex].Selected = true;
			}
		}

		int GetFileNameItemIndex(string strFileName)
		{
			for(int i=0;i<listView_files.Items.Count;i++)
			{
				if (String.Compare(strFileName, 
					listView_files.Items[i].Text,
					true) == 0)
					return i;
			}

			return -1;
		}

		private void button_deleteFile_Click(object sender, System.EventArgs e)
		{
			if (listView_files.SelectedItems.Count == 0) 
			{
				MessageBox.Show(this, "尚未选择要删除的文件...");
				return;
			}

            string strFileNames = ListViewUtil.GetItemNameList(listView_files.SelectedItems, "\r\n");

            /*
			foreach(ListViewItem item in listView_files.SelectedItems)
			{
				strFileNames += item.Text + "\r\n";
			}
             * */

			// 警告
			DialogResult msgResult = MessageBox.Show(this,
				"确实要删除下列文件?\r\n---\r\n" + strFileNames + "---",
				"script",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
			if (msgResult == DialogResult.No) 
			{
				return;
			}

			// 删除实际文件
			foreach(ListViewItem item in listView_files.SelectedItems)
			{
				string strFileName = textBox_projectLocate.Text + "\\" 
					+item.Text;

				try 
				{
					File.Delete(strFileName);
				}
				catch (Exception ex)
				{
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
				}
			}

			// 删除listview中事项
			for(int i=listView_files.SelectedIndices.Count-1;i>=0;i--)
			{
				listView_files.Items.RemoveAt(listView_files.SelectedIndices[i]);
			}
		
		}

		private void listView_files_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (listView_files.SelectedItems.Count == 0) 
			{
				button_editFile.Enabled = false;
				button_newFile.Enabled = true;
				button_deleteFile.Enabled = false;
			}
			else 
			{
				button_editFile.Enabled = true;
				button_newFile.Enabled = true;
				button_deleteFile.Enabled = true;
			}

		}

		private void button_changeProjectLocation_Click(object sender, System.EventArgs e)
		{
			DirNameDlg dlg = new DirNameDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

			DirectoryInfo di = new DirectoryInfo(textBox_projectLocate.Text);

			dlg.textBox_dirName.Text = di.Name;
			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			// 改名
			string strNewLocation = di.Parent.FullName + "\\" + dlg.textBox_dirName.Text;

			if (di.Exists == true) 
			{
				try 
				{
					Directory.Move(textBox_projectLocate.Text, strNewLocation);
				}
				catch (IOException ex)
				{
					MessageBox.Show(this, ex.Message);
					return;
				}
				catch (Exception ex)
				{
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
					return;
				}
			}
		

			// 方案名 + 路径
			ResultProjectNamePath = textBox_projectPathOfName.Text;

			if (ResultProjectNamePath != "")
				ResultProjectNamePath += "/";

			ResultProjectNamePath += textBox_projectName.Text;

			string strError;

			Debug.Assert(scriptManager != null, "调用本对话框以前，scriptManager指针应初始化...");

			if (m_bNew == false) 
			{
				int nRet = scriptManager.ChangeProjectData(
					ResultProjectNamePath,
					null,
					strNewLocation,
					out strError);
				if (nRet == -1) 
				{
					MessageBox.Show(this, strError);
					return;
				}

				scriptManager.Save();
			}

			textBox_projectLocate.Text = strNewLocation;

			// 重新装载文件名?
			LoadFileInfo();

		}

		private void listView_files_DoubleClick(object sender, System.EventArgs e)
		{
			button_editFile_Click(null, null);
		
		}

		private void button_changeProjectName_Click(object sender, System.EventArgs e)
		{

			ProjectNameDlg dlg = new ProjectNameDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

			dlg.textBox_projectName.Text = textBox_projectName.Text;
            dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			if (dlg.textBox_projectName.Text == textBox_projectName.Text)
				return;	// 没有必要修改

			if (m_bNew == true) 
			{
				textBox_projectName.Text = dlg.textBox_projectName.Text;
				return;
			}

			// 方案名 + 路径
			ResultProjectNamePath = textBox_projectPathOfName.Text;

			if (ResultProjectNamePath != "")
				ResultProjectNamePath += "/";
			
			ResultProjectNamePath += textBox_projectName.Text;

			string strError;

			int nRet = scriptManager.ChangeProjectData(ResultProjectNamePath,
				dlg.textBox_projectName.Text,
				null,
				out strError);
			if (nRet == -1) 
			{
				MessageBox.Show(this, strError);
			}
			else 
			{
				// 兑现显示遗留给对话框退出以后再做
				// node.Text = dlg.textBox_projectName.Text;
				textBox_projectName.Text = dlg.textBox_projectName.Text;
				scriptManager.Save();
			}

		}

		private void checkBox_displayTempFile_CheckedChanged(object sender, System.EventArgs e)
		{
			LoadFileInfo();
		}

        private void button_openProjectFolder_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.textBox_projectLocate.Text) == true)
            {
                MessageBox.Show(this, "尚未指定方案目录");
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(this.textBox_projectLocate.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }

        }

        private void textBox_projectLocate_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox_projectLocate.Text == "")
            {
                this.button_openProjectFolder.Enabled = false;
                this.button_changeProjectLocation.Enabled = false;
                this.button_openInCode.Enabled = false;
            }
            else
            {
                this.button_openProjectFolder.Enabled = true;
                this.button_changeProjectLocation.Enabled = true;
                this.button_openInCode.Enabled = true;
            }
        }

        // 在 Visual Studio Code 中打开
        private void button_openInCode_Click(object sender, EventArgs e)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("code");
                startInfo.Arguments = ".";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                // startInfo.UseShellExecute = false;
                startInfo.WorkingDirectory = this.textBox_projectLocate.Text;
                Process p = Process.Start(startInfo);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "启动 Vusual Studio Code 时发生异常(可能是 Code 尚未安装):" + ex.Message);
            }
        }
	}
}
