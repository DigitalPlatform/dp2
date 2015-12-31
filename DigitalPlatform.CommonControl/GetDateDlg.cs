using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    // 服务于DateControl
    public partial class GetDateDlg : Form
    {
        public DateTime DateTime = new DateTime((long)0);

        // public Point StartLocation = new Point(0, 0);

        public GetDateDlg()
        {
            InitializeComponent();
        }

        private void GetDateDlg_Load(object sender, EventArgs e)
        {
            this.monthCalendar1.SetDate(this.DateTime);

            Size size = new Size(this.monthCalendar1.Size.Width + SystemInformation.Border3DSize.Width * 2
                + 4,
                this.monthCalendar1.Size.Height
                + SystemInformation.CaptionHeight
                + SystemInformation.Border3DSize.Height * 2
                + 4);

            this.Size = size;
            // this.Location = this.StartLocation;

            // 调整到可见范围
            if (this.Location.Y + this.Size.Height > SystemInformation.WorkingArea.Height)
            {
                this.Location = new Point(this.Location.X,
                    SystemInformation.WorkingArea.Height - this.Size.Height);
            }
            if (this.Location.X + this.Size.Width > SystemInformation.WorkingArea.Width)
            {
                this.Location = new Point(
                    SystemInformation.WorkingArea.Width - this.Size.Width,
                    this.Location.Y);
            }

        }

        private void GetDateDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            this.DateTime = e.Start;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}