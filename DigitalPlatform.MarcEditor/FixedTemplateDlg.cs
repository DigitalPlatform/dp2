using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

using DigitalPlatform.Text;

namespace DigitalPlatform.Marc
{
	/// <summary>
	/// 定长模板对话框
	/// </summary>
	public class FixedTemplateDlg : System.Windows.Forms.Form
	{
        public const int WM_FIRST_SETFOCUS = API.WM_USER + 201;

		public DigitalPlatform.Marc.MarcFixedFieldControl TemplateControl;

        public event GetTemplateDefEventHandler GetTemplateDef = null;  // 外部接口，获取一个特定模板的XML定义

		private System.Windows.Forms.Button button_ok;
		private System.Windows.Forms.Button button_cancel;

		public XmlNode m_fieldNode = null;
		public string m_strLang = "";
		public string m_strValue = "";


		public WaitDlg m_waitDlg = null;
		private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.ToolTip toolTip1;

		private System.ComponentModel.IContainer components;

		public FixedTemplateDlg()
		{
			//
			// Windows 窗体设计器支持所必需的
			//
			InitializeComponent();

			//
			// TODO: 在 InitializeComponent 调用后添加任何构造函数代码
			//
		}

		/// <summary>
		/// 清理所有正在使用的资源。
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

		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FixedTemplateDlg));
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.TemplateControl = new DigitalPlatform.Marc.MarcFixedFieldControl();
            this.SuspendLayout();
            // 
            // button_ok
            // 
            this.button_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ok.AutoSize = true;
            this.button_ok.Location = new System.Drawing.Point(425, 256);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(75, 23);
            this.button_ok.TabIndex = 2;
            this.button_ok.Text = "确定";
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.AutoSize = true;
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(506, 256);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(76, 23);
            this.button_cancel.TabIndex = 3;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(9, 261);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(413, 25);
            this.label_message.TabIndex = 1;
            this.label_message.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label_message_MouseMove);
            // 
            // TemplateControl
            // 
            this.TemplateControl.AdditionalValue = "";
            this.TemplateControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TemplateControl.AutoScroll = true;
            this.TemplateControl.BackColor = System.Drawing.SystemColors.Control;
            this.TemplateControl.Location = new System.Drawing.Point(11, 12);
            this.TemplateControl.Name = "TemplateControl";
            this.TemplateControl.Padding = new System.Windows.Forms.Padding(8);
            this.TemplateControl.Size = new System.Drawing.Size(567, 238);
            this.TemplateControl.TabIndex = 0;
            this.TemplateControl.Value = "";
            this.TemplateControl.GetTemplateDef += new DigitalPlatform.Marc.GetTemplateDefEventHandler(this.TemplateControl_GetTemplateDef);
            this.TemplateControl.ResetSize += new System.EventHandler(this.TemplateControl_ResetSize);
            this.TemplateControl.ResetTitle += new DigitalPlatform.Marc.ResetTitleEventHandler(this.TemplateControl_ResetTitle);
            this.TemplateControl.BeginGetValueList += new DigitalPlatform.Marc.BeginGetValueListEventHandle(this.marcFixedFieldControl1_BeginGetValueList);
            this.TemplateControl.EndGetValueList += new DigitalPlatform.Marc.EndGetValueListEventHandle(this.marcFixedFieldControl1_EndGetValueList);
            // 
            // FixedTemplateDlg
            // 
            this.AcceptButton = this.button_ok;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(590, 288);
            this.Controls.Add(this.TemplateControl);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(100, 300);
            this.Name = "FixedTemplateDlg";
            this.ShowInTaskbar = false;
            this.Text = "定义模板";
            this.Load += new System.EventHandler(this.MarcFixedFieldControlDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        public Font GetDefaultFont()
        {
            FontFamily family = null;
            try
            {
                family = new FontFamily("微软雅黑");
            }
            catch
            {
                return null;
            }
            float height = (float)9.0;
            if (this.Font != null)
                height = this.Font.SizeInPoints;
            return new Font(family, height, GraphicsUnit.Point);
        }

		private void SetInfo(XmlNode fieldNode,
			string strLang,
			string strValue)
		{
			this.m_fieldNode = fieldNode;
			this.m_strLang = strLang;
			this.m_strValue = strValue;
		}

		// return:
		//		-1	出错
		//		0	不是定长字段或子字段
		//		1	成功
		public int Initial(XmlNode fieldNode,
			string strLang,
			string strValue,
			out string strError)
		{
			strError = "";

            int nResultWidth = 0;
            int nResultHeight = 0;
			int nRet = this.TemplateControl.Initial(fieldNode,
				strLang,
                out nResultWidth,
                out nResultHeight,
				out strError);
			if (nRet == 0)
				return 0;

			if (nRet == -1)
				return -1;

			if (strValue == null)
				strValue = "";

			if (strValue != "")
				this.TemplateControl.Value = strValue;

            /*
            int nHeightDelta = this.Height - this.TemplateControl.Height;

            this.Size = new Size(nResultWidth + 25,
                nResultHeight + nHeightDelta);    // 
             * */

			return 1;
		}

		private void button_cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void button_ok_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void label_message_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.toolTip1.SetToolTip(this.label_message,
				this.label_message.Text);
		}

		private void marcFixedFieldControl1_BeginGetValueList(object sender, DigitalPlatform.Marc.BeginGetValueListEventArgs e)
		{
			this.label_message.Text = e.Ref;//StringUtil.GetShortString(e.Ref,18);//e.Ref;//"正在获取列表'" + e.Ref + "'";
			this.Update();
		}

		private void marcFixedFieldControl1_EndGetValueList(object sender, DigitalPlatform.Marc.EndGetValueListEventArgs e)
		{
			string strMessage = e.Ref;
			if (strMessage != "")
				strMessage = "出错：" + strMessage;
			this.label_message.Text = strMessage;//StringUtil.GetShortString(strMessage,18);
		}

        private void MarcFixedFieldControlDlg_Load(object sender, EventArgs e)
        {
            if (this.Owner != null)
                this.Font = this.Owner.Font;
            else
            {
                Font default_font = GetDefaultFont();
                if (default_font != null)
                    this.Font = default_font;
            }

            int nResultWidth = 0;
            int nResultHeight = 0;

            TemplateControl.AdjustTextboxSize(
                false,
                out nResultWidth,
                out nResultHeight);    // 2008/7/20 new add

            int nHeightDelta = this.Height - this.TemplateControl.Height;

            Size old_size = this.Size;

            this.Size = new Size(nResultWidth + 25,
                nResultHeight + nHeightDelta); // + 50 + 35

            int nXDelta = this.Size.Width - old_size.Width;
            int nYDelta = this.Size.Height - old_size.Height;
            this.Location = new Point(this.Location.X - nXDelta / 2,
                this.Location.Y - nYDelta / 2);

            this.TemplateControl.AdjuetListViewSize();
            /*
            TemplateControl.Size = new Size(nResultWidth,
                nResultHeight);
             * */
            API.PostMessage(this.Handle, WM_FIRST_SETFOCUS, 0, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_FIRST_SETFOCUS:
                    TemplateControl.FocusFirstLine();
                    return;
            }
            base.DefWndProc(ref m);
        }

        protected override void OnFontChanged(
    EventArgs e)
        {
            base.OnFontChanged(e);

            ResetSize();
        }

        public void ResetSize()
        {
            int nResultWidth = 0;
            int nResultHeight = 0;

            int nHeightDelta = this.Height - this.TemplateControl.Height;

            TemplateControl.AdjustTextboxSize(
                false,
                out nResultWidth,
                out nResultHeight);

            Size old_size = this.Size;

            this.Size = new Size(nResultWidth + 25,
                Math.Max(this.Height, nResultHeight + nHeightDelta));

            int nXDelta = this.Size.Width - old_size.Width;
            int nYDelta = this.Size.Height - old_size.Height;
            this.Location = new Point(this.Location.X - nXDelta / 2,
                this.Location.Y - nYDelta / 2);

            this.TemplateControl.AdjuetListViewSize();
        }

        private void TemplateControl_ResetSize(object sender, EventArgs e)
        {
            ResetSize();
        }

        private void TemplateControl_GetTemplateDef(object sender, GetTemplateDefEventArgs e)
        {
            if (this.GetTemplateDef != null)
            {
                this.GetTemplateDef(sender, e);
            }
        }

        private void TemplateControl_ResetTitle(object sender, ResetTitleEventArgs e)
        {
            this.Text = "定长模板: " + e.Title;
        }
		
	}
}
