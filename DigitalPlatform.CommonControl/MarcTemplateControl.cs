using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// MARC 模板输入界面
    /// </summary>
    public partial class MarcTemplateControl : UserControl
    {
        public MarcTemplateControl()
        {
            InitializeComponent();
        }

        int m_nInSuspend = 0;

        public List<TemplateLine> Items = new List<TemplateLine>();

        bool m_bChanged = false;

        public void DisableUpdate()
        {
            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_content.SuspendLayout();
            }

            this.m_nInSuspend++;
        }

        // parameters:
        //      bOldVisible 如果为true, 表示真的要结束
        public void EnableUpdate()
        {
            this.m_nInSuspend--;


            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_content.ResumeLayout(false);
                this.tableLayoutPanel_content.PerformLayout();
            }
        }


        public TemplateLine InsertNewItem(int index)
        {
            this.DisableUpdate();   // 防止闪动

            try
            {
                this.tableLayoutPanel_content.RowCount += 1;
                this.tableLayoutPanel_content.RowStyles.Insert(index + 1, new System.Windows.Forms.RowStyle());

                TemplateLine item = new TemplateLine(this);

                item.InsertToTable(this.tableLayoutPanel_content, index);

                this.Items.Insert(index, item);

                item.State = ItemState.New;

                return item;
            }
            finally
            {
                this.EnableUpdate();
            }
        }

    }


    public class TemplateLine // TODO: IDisposeable
    {
        public MarcTemplateControl Container = null;

        public object Tag = null;   // 用于存放需要连接的任意类型对象

        // 颜色、popupmenu
        public Label label_color = null;

        public Label label_fieldName = null;

        public Label label_subfieldName = null;

        public TextBox textBox_subfieldContent = null;

        ItemState m_state = ItemState.Normal;

        public TemplateLine(MarcTemplateControl container)
        {
            this.Container = container;
            int nTopBlank = (int)this.Container.Font.GetHeight() + 2;

            label_color = new Label();
            label_color.Dock = DockStyle.Fill;
            label_color.Size = new Size(6, 28);
            label_color.Margin = new Padding(1, 0, 1, 0);

            label_fieldName = new Label();
            label_fieldName.Dock = DockStyle.Fill;
            label_fieldName.Size = new Size(6, 28);
            label_fieldName.Margin = new Padding(1, 0, 1, 0);

            label_subfieldName = new Label();
            label_subfieldName.Dock = DockStyle.Fill;
            label_subfieldName.Size = new Size(6, 28);
            label_subfieldName.Margin = new Padding(1, 0, 1, 0);

            // 子字段内容
            this.textBox_subfieldContent = new TextBox();
            textBox_subfieldContent.BorderStyle = BorderStyle.None;
            textBox_subfieldContent.Dock = DockStyle.Fill;
            textBox_subfieldContent.MinimumSize = new Size(80, 28);
            // textBox_price.Multiline = true;
            textBox_subfieldContent.Margin = new Padding(6, nTopBlank + 6, 6, 0);

        }

        // 插入本Line到某行。调用前，table.RowCount已经增量
        // parameters:
        //      nRow    从0开始计数
        internal void InsertToTable(TableLayoutPanel table,
            int nRow)
        {
            this.Container.DisableUpdate();

            try
            {

                Debug.Assert(table.RowCount == this.Container.Items.Count + 3, "");

                // 先移动后方的
                for (int i = (table.RowCount - 1) - 3; i >= nRow; i--)
                {
                    TemplateLine line = this.Container.Items[i];

                    // color
                    Label label = line.label_color;
                    table.Controls.Remove(label);
                    table.Controls.Add(label, 0, i + 1 + 1);

                    // fieldname
                    Label fieldName = line.label_fieldName;
                    table.Controls.Remove(fieldName);
                    table.Controls.Add(fieldName, 0, i + 1 + 1);

                    // subfieldname
                    Label subfieldName = line.label_subfieldName;
                    table.Controls.Remove(subfieldName);
                    table.Controls.Add(subfieldName, 0, i + 1 + 1);

                    // subfield content
                    TextBox subfieldContent = line.textBox_subfieldContent;
                    table.Controls.Remove(subfieldContent);
                    table.Controls.Add(subfieldContent, 1, i + 1 + 1);
                }

                table.Controls.Add(this.label_color, 0, nRow + 1);
                table.Controls.Add(this.textBox_subfieldContent, 1, nRow + 1);

            }
            finally
            {
                this.Container.EnableUpdate();
            }

            // events
            AddEvents();
        }


        void AddEvents()
        {
        }

        // 事项状态
        public ItemState State
        {
            get
            {
                return this.m_state;
            }
            set
            {
                if (this.m_state != value)
                {
                    this.m_state = value;

                    // SetLineColor();

                    bool bOldReadOnly = this.ReadOnly;
                    if ((this.m_state & ItemState.ReadOnly) != 0)
                    {
                        this.ReadOnly = true;
                    }
                    else
                    {
                        this.ReadOnly = false;
                    }
                }
            }
        }

        bool m_bReadOnly = false;

        public bool ReadOnly
        {
            get
            {
                return this.m_bReadOnly;
            }
            set
            {
                bool bOldValue = this.m_bReadOnly;
                if (bOldValue != value)
                {
                    this.m_bReadOnly = value;

                    // 
                    this.textBox_subfieldContent.ReadOnly = value;
                }
            }
        }

        public string SubfieldContent
        {
            get
            {
                return this.textBox_subfieldContent.Text;
            }
            set
            {
                this.textBox_subfieldContent.Text = value;
            }
        }
    }
}
