using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform
{
    /// <summary>
    /// 绘制 ToolStrip 控件的背景。绘制为控件 BackColor 所指定的颜色
    /// </summary>
    public class TransparentToolStripRenderer : ToolStripSystemRenderer
    {
        public ToolStrip ToolStrip = null;

        public TransparentToolStripRenderer(ToolStrip toolstrip)
        {
            this.ToolStrip = toolstrip;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (Brush brush = new SolidBrush(this.ToolStrip == null ? e.BackColor : this.ToolStrip.BackColor))  // SystemColors.ControlDark
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
            // base.OnRenderToolStripBackground(e);
        }

        // 去掉下面那根线
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
        }
    }
}
