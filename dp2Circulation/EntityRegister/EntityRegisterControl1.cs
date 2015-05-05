using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 册登记控件
    /// 负责一种图书的册登记
    /// </summary>
    public partial class EntityRegisterControlOld : UserControl
    {
        public EntityRegisterControlOld()
        {
            InitializeComponent();

        }

        public void SetMarc(string strMarc)
        {
            this.easyMarcControl1.SetMarc(strMarc);
        }

        // 添加一个新的册对象
        public int NewEntity(string strItemBarcode,
            out string strError)
        {
            strError = "";

            EntityEditControl control = new EntityEditControl();
            control.Barcode = strItemBarcode;
            control.AutoScroll = false;
            control.DisplayMode = "simple_register";
            control.Width = 120;
            control.AutoSize = true;
            control.Font = this.Font;
            control.Initializing = false;
            control.PaintContent += new PaintEventHandler(control_PaintContent);
            this.flowLayoutPanel_entities.Controls.Add(control);
            this.flowLayoutPanel_entities.ScrollControlIntoView(control);

            return 0;
        }

        void control_PaintContent(object sender, PaintEventArgs e)
        {
#if NO
            Control control = sender as Control;

            string strText = "test";
            Brush brush = new SolidBrush(SystemColors.GrayText);

            Font font = new Font(this.Font.Name, control.Height / 4, FontStyle.Bold, GraphicsUnit.Pixel);
            SizeF size = e.Graphics.MeasureString(strText, font);
            PointF start = new PointF(control.Width / 2, control.Height / 2 - size.Height / 2);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(strText, font, brush, start, format);
#endif
        }
    }
}
