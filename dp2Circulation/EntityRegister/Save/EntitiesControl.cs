using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.CommonControl;
using System.Diagnostics;

namespace dp2Circulation
{
    /// <summary>
    /// 负责显示和编辑若干册记录信息的控件
    /// 被册登记控件内嵌使用
    /// </summary>
    public partial class EntitiesControl : TableControlBase
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        public EntitiesControl()
        {
            InitializeComponent();

            this.TableLayoutPanel = this.tableLayoutPanel1;

            List<string> titles = new List<string> {"","册条码号", "馆藏地", "图书类型", "索取号", "价格", "参考ID", "批次号" };
            this.SetTitleLine(titles);

            this.RESERVE_LINES = 1;
        }


    }

    /// <summary>
    /// 视觉行基类
    /// </summary>
    public class EntityLine : TableItemBase 
    {
        // public EntitiesControl Container = null;

        public Splitter splitter = null;

        // 册条码号
        public TextBox textBox_barcode = null;

        // 馆藏地
        public ComboBox comboBox_location = null;

        // 图书类型
        public ComboBox comboBox_bookType = null;

        // 索取号
        public TextBox textBox_accessNo = null;

        // 册价格
        public TextBox textBox_price = null;

        // 参考 ID
        public Label label_refID = null;

        // 批次号
        public Label label_batchNo = null;

        public override void DisposeChildControls()
        {
            splitter.Dispose();
            textBox_barcode.Dispose();
            comboBox_location.Dispose();
            comboBox_bookType.Dispose();
            textBox_accessNo.Dispose();
            textBox_price.Dispose();
            label_refID.Dispose();
            label_batchNo.Dispose();
            base.DisposeChildControls();
        }

        public EntityLine(EntitiesControl container)
        {
            this.Container = container;

            label_color = new Label();
            label_color.Dock = DockStyle.Fill;
            label_color.Size = new Size(6, 23);
            label_color.Margin = new Padding(0, 0, 0, 0);

            //label_color.ImageList = this.Container.ImageListIcons;
            //label_color.ImageIndex = -1;

#if NO
            splitter = new MySplitter();
            // splitter.Dock = DockStyle.Fill;
            splitter.Size = new Size(8, 23);
            splitter.Width = 8;
            splitter.Margin = new Padding(0, 0, 0, 0);
            splitter.BackColor = Color.Transparent;
#endif

            // barcode
            this.textBox_barcode = new TextBox();
            textBox_barcode.BorderStyle = BorderStyle.None;
            textBox_barcode.Dock = DockStyle.Fill;
            textBox_barcode.MinimumSize = new Size(20, 21); // 23
            textBox_barcode.Size = new Size(20, 21); // 23
            textBox_barcode.Margin = new Padding(8, 4, 0, 0);

            // location
            this.comboBox_location = new ComboBox();
            comboBox_location.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_location.FlatStyle = FlatStyle.Flat;
            comboBox_location.Dock = DockStyle.Fill;
            comboBox_location.MaximumSize = new Size(150, 28);
            comboBox_location.Size = new Size(100, 28);
            comboBox_location.MinimumSize = new Size(50, 28);
            comboBox_location.DropDownHeight = 300;
            comboBox_location.DropDownWidth = 300;
            comboBox_location.ForeColor = this.Container.TableLayoutPanel.ForeColor;
            comboBox_location.Text = "";
            comboBox_location.Margin = new Padding(6, 6, 6, 0);

            // bookType
            this.comboBox_bookType = new ComboBox();
            comboBox_bookType.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_bookType.FlatStyle = FlatStyle.Flat;
            comboBox_bookType.Dock = DockStyle.Fill;
            comboBox_bookType.MaximumSize = new Size(150, 28);
            comboBox_bookType.Size = new Size(100, 28);
            comboBox_bookType.MinimumSize = new Size(50, 28);
            comboBox_bookType.DropDownHeight = 300;
            comboBox_bookType.DropDownWidth = 300;
            comboBox_bookType.ForeColor = this.Container.TableLayoutPanel.ForeColor;
            comboBox_bookType.Text = "";
            comboBox_bookType.Margin = new Padding(6, 6, 6, 0);

            // accessNo
            this.textBox_accessNo = new TextBox();
            textBox_accessNo.BorderStyle = BorderStyle.None;
            textBox_accessNo.Dock = DockStyle.Fill;
            textBox_accessNo.MinimumSize = new Size(20, 21); // 23
            textBox_accessNo.Size = new Size(20, 21); // 23
            textBox_accessNo.Margin = new Padding(8, 4, 0, 0);

            // price
            this.textBox_price = new TextBox();
            textBox_price.BorderStyle = BorderStyle.None;
            textBox_price.Dock = DockStyle.Fill;
            textBox_price.MinimumSize = new Size(20, 21); // 23
            textBox_price.Size = new Size(20, 21); // 23
            textBox_price.Margin = new Padding(8, 4, 0, 0);

            // refID
            label_refID = new Label();
            label_refID.Dock = DockStyle.Fill;
            label_refID.Size = new Size(6, 23);
            label_refID.AutoSize = true;
            label_refID.Margin = new Padding(4, 2, 4, 0);
            // label_caption.BackColor = SystemColors.Control;

            // batchNo
            this.label_batchNo = new Label();
            label_batchNo.Dock = DockStyle.Fill;
            label_batchNo.Size = new Size(6, 23);
            label_batchNo.AutoSize = true;
            label_batchNo.Margin = new Padding(4, 2, 4, 0);
            // label_caption.BackColor = SystemColors.Control;

        }

            // 移除本行相关的控件
        public override void RemoveControls(TableLayoutPanel table)
        {
            table.Controls.Remove(this.label_color);
            if (this.splitter != null)
                table.Controls.Remove(this.splitter);
            table.Controls.Remove(this.textBox_barcode);
            table.Controls.Remove(this.comboBox_location);
            table.Controls.Remove(this.comboBox_bookType);
            table.Controls.Remove(this.textBox_accessNo);
            table.Controls.Remove(this.textBox_price);
            table.Controls.Remove(this.label_refID);
            table.Controls.Remove(this.label_batchNo);
        }

        public override void AddControls(TableLayoutPanel table, int nRow)
        {
            int nColumnIndex = 0;
            int nRowIndex = nRow + this.Container.RESERVE_LINES;
            table.Controls.Add(this.label_color, nColumnIndex++, nRowIndex);
            if (this.splitter != null)
                table.Controls.Add(this.splitter, nColumnIndex++, nRowIndex);
            table.Controls.Add(this.textBox_barcode, nColumnIndex++, nRowIndex);
            table.Controls.Add(this.comboBox_location, nColumnIndex++, nRowIndex);
            table.Controls.Add(this.comboBox_bookType, nColumnIndex++, nRowIndex);
            table.Controls.Add(this.textBox_accessNo, nColumnIndex++, nRowIndex);
            table.Controls.Add(this.textBox_price, nColumnIndex++, nRowIndex);
            table.Controls.Add(this.label_refID, nColumnIndex++, nRowIndex);
            table.Controls.Add(this.label_batchNo, nColumnIndex++, nRowIndex);
        }

        public override void SetReaderOnly(bool bReadOnly)
        {
            // 
            this.textBox_barcode.ReadOnly = bReadOnly;
        }
    }
}
