using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

// using DigitalPlatform.Text;

namespace DigitalPlatform.DTLP
{
    public partial class GetDtlpResDialog : Form
    {
        public DtlpChannelArray DtlpChannels = null;
        public DtlpChannel DtlpChannel = null;	// 

        public int[] EnabledIndices = null;	// null表示全部发黑。如果对象存在，但是元素个数为0，表示全部发灰

        public GetDtlpResDialog()
        {
            InitializeComponent();

            this.dtlpResDirControl1.PathSeparator = "/";
        }

        public int Initial(DtlpChannelArray channels,
            DtlpChannel channel)
        {
            this.dtlpResDirControl1.channelarray = channels;
            this.dtlpResDirControl1.Channel = channel;

            return 0;
        }

        private void GetDtlpResDialog_Load(object sender, EventArgs e)
        {
            this.dtlpResDirControl1.FillSub(null);

            if (this.textBox_path.Text != "")
                this.dtlpResDirControl1.SelectedPath1 = this.textBox_path.Text;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_path.Text == "")
            {
                MessageBox.Show(this, "尚未选择对象");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        private void dtlpResDirControl1_ItemSelected(object sender, 
            DigitalPlatform.DTLP.ItemSelectedEventArgs e)
        {
            if (EnabledIndices != null
                && IsMaskInList(e.Mask, EnabledIndices) == false)
                this.textBox_path.Text = "";
            else
                this.textBox_path.Text = e.Path;



            /*
            if ((e.Mask & DtlpChannel.TypeStdbase) != 0)
                this.textBox_dtlpRecPath.Text = e.Path;
            else
                this.textBox_dtlpRecPath.Text = "";
             * */
        }

        private void dtlpResDirControl1_GetItemTextStyle(object sender, GetItemTextStyleEventArgs e)
        {
            e.FontFace = "";
            e.FontSize = 0;
            e.FontStyle = FontStyle.Regular;

            if (EnabledIndices != null
               && IsMaskInList(e.Mask, EnabledIndices) == false)
            {
                e.ForeColor = ControlPaint.LightLight(ForeColor);
                e.Result = 1;
            }
            else
            {
                e.Result = 0;
            }
        }

        static bool IsMaskInList(int nMask, int [] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if ((nMask & list[i]) != 0)
                    return true;
            }

            return false;
        }

        public string Path
        {
            get
            {
                return this.textBox_path.Text;
            }
            set
            {
                this.textBox_path.Text = value;
            }
        }
    }
}