using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace dp2rms
{
	/// <summary>
	/// Summary description for SelPinyinDlg.
	/// </summary>
	public class SelPinyinDlg : System.Windows.Forms.Form
	{
		public string SampleText = "";
		public int    Offset = -1;	// Hanzi这个汉字在SampleText中所在的偏移
		public string Pinyins = "";
		public string Hanzi = "";
		public string ResultPinyin = "";


		private System.Windows.Forms.TextBox textBox_sampleText;
		private System.Windows.Forms.Label label_largeHanzi;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
        private Button button_stop;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SelPinyinDlg()
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
            this.textBox_sampleText = new System.Windows.Forms.TextBox();
            this.label_largeHanzi = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_sampleText
            // 
            this.textBox_sampleText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sampleText.Location = new System.Drawing.Point(16, 15);
            this.textBox_sampleText.Name = "textBox_sampleText";
            this.textBox_sampleText.ReadOnly = true;
            this.textBox_sampleText.Size = new System.Drawing.Size(392, 25);
            this.textBox_sampleText.TabIndex = 0;
            this.textBox_sampleText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_largeHanzi
            // 
            this.label_largeHanzi.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_largeHanzi.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_largeHanzi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label_largeHanzi.Font = new System.Drawing.Font("SimSun", 90F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_largeHanzi.Location = new System.Drawing.Point(16, 51);
            this.label_largeHanzi.Name = "label_largeHanzi";
            this.label_largeHanzi.Size = new System.Drawing.Size(40, 114);
            this.label_largeHanzi.TabIndex = 1;
            this.label_largeHanzi.Text = "汉";
            this.label_largeHanzi.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label_largeHanzi.SizeChanged += new System.EventHandler(this.label_largeHanzi_SizeChanged);
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.IntegralHeight = false;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(64, 51);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(192, 114);
            this.listBox1.TabIndex = 2;
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(308, 36);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(100, 33);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.Location = new System.Drawing.Point(308, 74);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(100, 32);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_stop
            // 
            this.button_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_stop.Location = new System.Drawing.Point(308, 135);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(100, 30);
            this.button_stop.TabIndex = 5;
            this.button_stop.Text = "停止(&S)";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // SelPinyinDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(8, 18);
            this.ClientSize = new System.Drawing.Size(424, 180);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.label_largeHanzi);
            this.Controls.Add(this.textBox_sampleText);
            this.Name = "SelPinyinDlg";
            this.Text = "选择拼音";
            this.Load += new System.EventHandler(this.SelPinyinDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void SelPinyinDlg_Load(object sender, System.EventArgs e)
		{
			this.label_largeHanzi.Font = new Font("Arial", 
				Math.Min(label_largeHanzi.Width,
				label_largeHanzi.Height) - 8,
				GraphicsUnit.Pixel);
			
			this.textBox_sampleText.Text = MakeShorterSampleText(this.SampleText,
				this.Offset);
			this.label_largeHanzi.Text = Hanzi;
		
			FillList();

			this.listBox1.Focus();
		}

		void FillList()
		{
			listBox1.Items.Clear();

			if (Pinyins == "")
				return;

			string[] aPart = Pinyins.Split(new char[]{';'});
			for(int i=0;i<aPart.Length; i++)
			{
				string strOnePinyin = aPart[i].Trim();
				if (strOnePinyin == "")
					continue;
				listBox1.Items.Add(strOnePinyin);
			}

			listBox1.SelectedIndex = 0;

		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (listBox1.SelectedIndex == -1) 
			{
				MessageBox.Show(this, "尚未选择事项...");
				return;
			}

			this.ResultPinyin = (string)listBox1.Items[listBox1.SelectedIndex];
		
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();

		}

		private void label_largeHanzi_SizeChanged(object sender, System.EventArgs e)
		{
			this.label_largeHanzi.Font = new Font("Arial", 
				Math.Min(label_largeHanzi.Width,
				label_largeHanzi.Height) - 8,
				GraphicsUnit.Pixel);

		}
		
		string MakeShorterSampleText(string strText,
			int nOffset)
		{
			if (strText == "")
				return "";
			int nHalf = 20;

			if (nOffset == -1)
				return strText;

			int nLeft = nOffset;
			int nRight = strText.Length - nOffset - 1;
			int nCenter = nOffset;

			strText = strText.Insert(nCenter+1, "*");
			strText = strText.Insert(nCenter, "*");

			nCenter ++;
			nLeft ++;
			nRight ++;

			bool bLeftTruncated = false;
			bool bRightTruncated = false;
			if (nLeft > nHalf)
			{
				strText = strText.Remove(0, nLeft - nHalf);
				nCenter -= nLeft - nHalf;
				bLeftTruncated = true;
			}

			if (nRight > nHalf)
			{
				strText = strText.Substring(0, nCenter + nHalf);
				bRightTruncated = true;
			}

			if (bLeftTruncated == true)
				strText = "... " + strText;
			if (bRightTruncated == true)
				strText =  strText + " ...";

			return strText;
		}

		private void listBox1_DoubleClick(object sender, System.EventArgs e)
		{
			button_OK_Click(null, null);
		}

        private void button_stop_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

	}
}
