using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.RFID;
using DigitalPlatform.RFID.UI;

namespace RfidCenter
{
    public partial class ChipDialog : Form
    {
        string _uid = "";
        public string UID
        {
            get { return _uid; }
            set
            {
                _uid = value;
                this.Text = "标签 " + _uid;
            }
        }

        public event EventHandler SaveTriggerd = null;
        public event EventHandler RefreshTriggerd = null;

        FloatingMessageForm _floatingMessage = null;

        public ChipDialog()
        {
            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }

            chipEditor1.PropertyValueChanged += ChipEditor1_PropertyValueChanged;
        }

        private void ChipEditor1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.toolStripButton_save.Enabled = true;
        }

        private void ChipDialog_Load(object sender, EventArgs e)
        {

        }

        private void ChipDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ChipDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        public LogicChipItem LogicChipItem
        {
            get
            {
                return chipEditor1.LogicChipItem;
            }
            set
            {
                chipEditor1.LogicChipItem = value;
                this.toolStripButton_save.Enabled = (value != null && value.Changed);
            }
        }

        private void toolStripButton_save_Click(object sender, EventArgs e)
        {
            this.SaveTriggerd?.Invoke(this, new EventArgs());
        }

        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            this.RefreshTriggerd?.Invoke(this, new EventArgs());
        }

        public void ShowMessage(string strMessage,
string strColor = "",
bool bClickClose = false)
        {
            if (this._floatingMessage == null)
                return;

            Color color = Color.FromArgb(80, 80, 80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        public void ClearMessage()
        {
            this.ShowMessage("");
        }
    }
}
