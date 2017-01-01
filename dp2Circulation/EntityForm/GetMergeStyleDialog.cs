using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 复制或移动书目记录的操作过程中，询问合并方式的对话框
    /// </summary>
    public partial class GetMergeStyleDialog : Form
    {
        /// <summary>
        /// 源书目记录路径
        /// </summary>
        public string SourceRecPath
        {
            get;
            set;
        }

        /// <summary>
        /// 目标书目记录路径
        /// </summary>
        public string TargetRecPath
        {
            get;
            set;
        }

        public GetMergeStyleDialog()
        {
            InitializeComponent();
        }

        private void GetMergeStyleDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            MergeStyle style = GetMergeStyle();
            // 如果 missing 和 reservetarget 同时出现，表示没有必要操作了
            if ((style & MergeStyle.MissingSourceSubrecord) != 0
                && (style & MergeStyle.ReserveTargetBiblio) != 0)
            {
                strError = "若不采纳来自源记录的书目和子记录，意味着没有必要进行此次合并操作";
                goto ERROR1;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Point pt = this.panel1.AutoScrollPosition;
            e.Graphics.TranslateTransform(pt.X, pt.Y);

            Size size = MergePicture.Paint(e,
                this.panel1.ClientSize,
                this.Font,
                "源 " + this.SourceRecPath,
                "目标 " + this.TargetRecPath,
                GetMergeStyle());
            this.panel1.AutoScrollMinSize = size;
            // this.panel1.SetBounds(0, 0, size.Width, size.Height, BoundsSpecified.All);
        }

        private void radioButton_biblio_reserveSource_CheckedChanged(object sender, EventArgs e)
        {
            this.panel1.Invalidate();
        }

        private void radioButton_biblio_reserveTarget_CheckedChanged(object sender, EventArgs e)
        {
            this.panel1.Invalidate();

        }

        private void radioButton_subrecord_combin_CheckedChanged(object sender, EventArgs e)
        {
            this.panel1.Invalidate();

        }

        private void radioButton_subrecord_source_CheckedChanged(object sender, EventArgs e)
        {
            this.panel1.Invalidate();

        }

        private void radioButton_subrecord_target_CheckedChanged(object sender, EventArgs e)
        {
            this.panel1.Invalidate();
        }

        public MergeStyle GetMergeStyle()
        {
            MergeStyle style = MergeStyle.None;

            if (radioButton_biblio_reserveSource.Checked)
                style |= MergeStyle.ReserveSourceBiblio;
            else
                style |= MergeStyle.ReserveTargetBiblio;

            if (radioButton_subrecord_combine.Checked)
                style |= MergeStyle.CombinSubrecord;
            else if (radioButton_subrecord_source.Checked)
                style |= MergeStyle.OverwriteSubrecord;
            else
                style |= MergeStyle.MissingSourceSubrecord;

            // 如果 missing 和 reservetarget 同时出现，表示没有必要操作了

            return style;
        }

        public void SetMergeStyle(MergeStyle style)
        {
            if ((style & MergeStyle.ReserveSourceBiblio) != 0)
            {
                this.radioButton_biblio_reserveSource.Checked = true;
                this.radioButton_biblio_reserveTarget.Checked = false;
            }
            else
            {
                // TODO: 是否验证 MergeStyle.ReserveTargetBiblio ?

                this.radioButton_biblio_reserveSource.Checked = false;
                this.radioButton_biblio_reserveTarget.Checked = true;
            }

            if ((style & MergeStyle.CombinSubrecord) != 0)
            {
                this.radioButton_subrecord_combine.Checked = true;
                this.radioButton_subrecord_source.Checked = false;
                this.radioButton_subrecord_target.Checked = false;
            }
            else if ((style & MergeStyle.OverwriteSubrecord) != 0)
            {
                this.radioButton_subrecord_combine.Checked = false;
                this.radioButton_subrecord_source.Checked = true;
                this.radioButton_subrecord_target.Checked = false;
            }
            else
            {
                // missing
                this.radioButton_subrecord_combine.Checked = false;
                this.radioButton_subrecord_source.Checked = false;
                this.radioButton_subrecord_target.Checked = true;
            }
        }

        public string MessageText
        {
            get
            {
                return this.textBox_messageText.Text;
            }
            set
            {
                this.textBox_messageText.Text = value;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.radioButton_subrecord_combine);
                controls.Add(this.radioButton_subrecord_source);
                controls.Add(this.radioButton_subrecord_target);

                controls.Add(this.radioButton_biblio_reserveSource);
                controls.Add(this.radioButton_biblio_reserveTarget);

                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.radioButton_subrecord_combine);
                controls.Add(this.radioButton_subrecord_source);
                controls.Add(this.radioButton_subrecord_target);

                controls.Add(this.radioButton_biblio_reserveSource);
                controls.Add(this.radioButton_biblio_reserveTarget);

                // 不让空字符串用于设置。因为空字符串会让所有 checked 都是 false。
                if (string.IsNullOrEmpty(value) == false)
                    GuiState.SetUiState(controls, value);

            }
        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            this.panel1.Invalidate();
        }

        bool _enableSubRecord = true;

        public bool EnableSubRecord
        {
            get
            {
                return this._enableSubRecord;
            }
            set
            {
                this._enableSubRecord = value;
                if (value == true)
                    groupBox_subRecord.Enabled = true;
                else
                {
                    this.radioButton_subrecord_target.Checked = true;
                    groupBox_subRecord.Enabled = false;
                }

            }
        }
    }
}
