using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.rms.Client.rmsws_localhost;


namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// Summary description for ViewAccessPointDlg.
	/// </summary>
	public class ViewAccessPointForm : System.Windows.Forms.Form
	{
		/*
		public Channel channel = null;
		public string RecPath = "";
		public string XmlBody = "";
		public Stop stop = null;
		*/

		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader_key;
		private System.Windows.Forms.ColumnHeader columnHeader_keyOrigin;
		private System.Windows.Forms.ColumnHeader columnHeader_number;
		private System.Windows.Forms.ColumnHeader columnHeader_id;
		private System.Windows.Forms.ColumnHeader columnHeader_fromName;
		private System.Windows.Forms.ColumnHeader columnHeader_fromValue;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ViewAccessPointForm()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewAccessPointForm));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader_key = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_keyOrigin = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_number = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_fromName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_fromValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_key,
            this.columnHeader_keyOrigin,
            this.columnHeader_number,
            this.columnHeader_fromName,
            this.columnHeader_fromValue,
            this.columnHeader_id});
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(9, 9);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(470, 257);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader_key
            // 
            this.columnHeader_key.Text = "Key";
            this.columnHeader_key.Width = 171;
            // 
            // columnHeader_keyOrigin
            // 
            this.columnHeader_keyOrigin.Text = "原始Key";
            this.columnHeader_keyOrigin.Width = 199;
            // 
            // columnHeader_number
            // 
            this.columnHeader_number.Text = "数值形态的 Key";
            this.columnHeader_number.Width = 150;
            // 
            // columnHeader_fromName
            // 
            this.columnHeader_fromName.Text = "检索途径";
            this.columnHeader_fromName.Width = 150;
            // 
            // columnHeader_fromValue
            // 
            this.columnHeader_fromValue.Text = "来源";
            this.columnHeader_fromValue.Width = 150;
            // 
            // columnHeader_id
            // 
            this.columnHeader_id.Text = "记录ID";
            this.columnHeader_id.Width = 100;
            // 
            // ViewAccessPointForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(488, 276);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewAccessPointForm";
            this.ShowInTaskbar = false;
            this.Text = "观察检索点";
            this.Load += new System.EventHandler(this.ViewAccessPointDlg_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void ViewAccessPointDlg_Load(object sender, System.EventArgs e)
		{
			/*
			if (channel != null)
			{
				string strError;
				long lRet = channel.DoGetKeys(
					this.RecPath,
					this.XmlBody,
					this,
					this.stop,
					out strError);
				if (lRet == -1) 
				{
					MessageBox.Show(this, strError);
				}
			}
			*/

		}

		public void Clear()
		{
			listView1.Items.Clear();
		}

		// 在listview最后追加一行
		public void NewLine(KeyInfo keyInfo)
		{
			ListViewItem item = new ListViewItem(keyInfo.Key, 0);

			listView1.Items.Add(item);

			item.SubItems.Add(keyInfo.KeyNoProcess);
			item.SubItems.Add(keyInfo.Num);
			item.SubItems.Add(keyInfo.FromName);
			item.SubItems.Add(keyInfo.FromValue);
			item.SubItems.Add(keyInfo.ID);

		}



	}
}
