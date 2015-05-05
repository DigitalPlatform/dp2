using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.CommonControl;
using System.Diagnostics;

namespace dp2Circulation
{
    /// <summary>
    /// 显示连续的出纳操作中前面的若干读者的摘要情况的窗口
    /// </summary>
    internal partial class PatronSummaryForm : Form
    {
        internal QuickChangeBiblioForm ChargingForm = null;
        internal List<PatronSummary> PatronSummaries = null;

        public PatronSummaryForm()
        {
            InitializeComponent();

            this.dpTable1.Columns.Visible = false;
        }

        private void PatronSummaryForm_Load(object sender, EventArgs e)
        {
            dpTable1_SizeChanged(this, null);
            FillList();
        }

        private void PatronSummaryForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        const int TOP_BLANK = 4;
        const int BAR_HEIGHT = 20;

        private void dpTable1_PaintRegion(object sender, DigitalPlatform.CommonControl.PaintRegionArgs e)
        {
            if (e.Action == "query")
            {
                e.Height = BAR_HEIGHT + TOP_BLANK;
                DpCell cell = e.Item as DpCell;
                DpRow row = cell.Container;

                int index = this.dpTable1.Rows.IndexOf(row);
                if (index == -1)
                {
                    Debug.Assert(false, "");
                    return;
                }

                if (index >= this.PatronSummaries.Count)
                {
                    Debug.Assert(false, "");
                    return;
                }

                PatronSummary summary = this.PatronSummaries[index];
                if (summary != null)
                    cell.Tag = summary;
                return;
            }


            // paint
            {
                DpCell cell = e.Item as DpCell;
                if (cell == null)
                {
                    Debug.Assert(false, "");
                    return;
                }
                PatronSummary summary = cell.Tag as PatronSummary;
                if (summary == null)
                {
                    Debug.Assert(false, "");
                    return;
                }

                ColorSummaryControl.DoPaint(e.pe.Graphics,
                    e.X,
                    e.Y + TOP_BLANK,
                    new Size(e.Width, e.Height - TOP_BLANK),
                    summary.ColorList);
            }
        }

        internal void FillList()
        {
            this.dpTable1.Rows.Clear();

            if (this.PatronSummaries == null)
                return;

            
            foreach(PatronSummary summary in this.PatronSummaries)
            {
                DpRow row = new DpRow();
                row.BackColor = SystemColors.Window;
                row.ForeColor = SystemColors.WindowText;

                DpCell cell = new DpCell();
                cell.Text = summary.Barcode + " " + summary.Name;
                cell.OwnerDraw = true;
                row.Add(cell);

                this.dpTable1.Rows.Add(row);
            }

            // 放最后一行可见
            if (this.dpTable1.Rows.Count > 0)
                this.dpTable1.EnsureVisible(this.dpTable1.Rows[dpTable1.Rows.Count - 1]);

        }

        private void dpTable1_SizeChanged(object sender, EventArgs e)
        {
            this.dpTable1.Columns[0].Width = Math.Max(100, this.ClientSize.Width - this.dpTable1.Padding.Horizontal - 4);
        }

        // 响应一个任务状态的改变
        public void OnTaskStateChanged(string strAction, ChargingTask task)
        {
            if (this.PatronSummaries == null)
                return;

            foreach (PatronSummary summary in this.PatronSummaries)
            {
                if (summary.Tasks.IndexOf(task) != -1)
                {
                    if (strAction == "remove")
                        summary.RemoveTask(task);
                    else if (strAction == "refresh"
                        || strAction == "refresh_and_visible")
                        summary.RefreshColorList();
                    return;
                }
            }
        }

        public string Comment
        {
            get
            {
                return this.label_comment.Text;
            }
            set
            {
                this.label_comment.Text = value;
            }
        }

        public void ShowComment()
        {
            this.label_comment.BringToFront();
            this.timer1.Start();
        }

        public void HideComment()
        {
            this.dpTable1.BringToFront();
        }

        private void label_comment_Click(object sender, EventArgs e)
        {
            this.HideComment();
            this.timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.HideComment();
        }
    }

    internal class PatronSummary
    {
        public string Name = "";
        public string Barcode = "";

        public string ColorList = "";

        public List<ChargingTask> Tasks = new List<ChargingTask>();   // 所包含的任务。第一个任务，也就是负责装载读者信息的那个任务

        // 根据任务的当前颜色刷新 this.ColorList
        public void RefreshColorList()
        {
            StringBuilder colorList = new StringBuilder(256);

            foreach (ChargingTask task in this.Tasks)
            {
                // 忽略状态读者信息的任务
                if (task.Action == "load_reader_info")
                    continue;

                string strColor = "";
                if (string.IsNullOrEmpty(task.Color) == false)
                    strColor = task.Color.Substring(0, 1).ToUpper();
                else
                    strColor = "B";
                colorList.Append(strColor);
            }

            this.ColorList = colorList.ToString();
        }

        public void RemoveTask(ChargingTask task)
        {
            this.Tasks.Remove(task);
            this.RefreshColorList();
        }
    }
}
