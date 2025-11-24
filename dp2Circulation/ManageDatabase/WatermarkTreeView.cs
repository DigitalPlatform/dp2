using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace dp2Circulation
{
    public class WatermarkTreeView : TreeView
    {
        public Image BgImage { get; set; }
        public bool TileBackground { get; set; } = true;
        public string WatermarkText { get; set; }
        public Font WatermarkFont { get; set; } = new Font("Microsoft YaHei", 14, FontStyle.Bold);
        public Color WatermarkColor { get; set; } = Color.FromArgb(60, Color.Black);

        public WatermarkTreeView()
        {
            this.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            this.HideSelection = false; // 便于在失去焦点时仍能看到选中效果
            // 减少闪烁
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

#if REMOVED
        // 在背景绘制阶段先绘制图片/水印
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (BgImage == null && string.IsNullOrEmpty(WatermarkText))
            {
                base.OnPaintBackground(pevent);
                return;
            }

            Graphics g = pevent.Graphics;
            Rectangle rc = this.ClientRectangle;

            // 填充底色
            using (var br = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(br, rc);
            }

            // 背景图片
            if (BgImage != null)
            {
                if (TileBackground)
                {
                    using (var tb = new TextureBrush(BgImage, WrapMode.Tile))
                    {
                        g.FillRectangle(tb, rc);
                    }
                }
                else
                {
                    g.DrawImage(BgImage, rc);
                }
            }

            // 水印文字（居中）
            if (!string.IsNullOrEmpty(WatermarkText))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using (var brush = new SolidBrush(WatermarkColor))
                {
                    g.DrawString(WatermarkText, WatermarkFont, brush, rc, sf);
                }
            }
        }
#endif

        // 完整 DrawNode 实现：绘制节点背景、图像、文本与焦点矩形
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawNode(e);

            // 在第一次 DrawNode 时绘制背景（可在 Paint 中做）
            if (e.Node.Bounds != Rectangle.Empty)
            {
                // 水印文字（居中）
                if (!string.IsNullOrEmpty(WatermarkText))
                {
                    Rectangle rc = this.ClientRectangle;

                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    using (var brush = new SolidBrush(WatermarkColor))
                    {
                        e.Graphics.DrawString(WatermarkText, WatermarkFont, brush, rc, sf);
                    }
                }
            }
            return;

#if REMOVED
            // 有时 e.Bounds 可能为 Empty（例如虚拟节点），退回到 Node.Bounds
            Rectangle nodeBounds = e.Bounds;
            if (nodeBounds == Rectangle.Empty)
                nodeBounds = e.Node.Bounds;

            // 计算图像区域宽度（若有 ImageList）
            int imageAreaWidth = 0;
            if (this.ImageList != null && this.ImageList.Images.Count > 0)
            {
                imageAreaWidth = this.ImageList.ImageSize.Width + 3; // 3px 间隔
            }

            // 文本绘制矩形（保留图像宽度）
            Rectangle textRect = new Rectangle(
                nodeBounds.X + imageAreaWidth,
                nodeBounds.Y,
                Math.Max(0, nodeBounds.Width - imageAreaWidth),
                nodeBounds.Height);

            bool selected = (e.State & TreeNodeStates.Selected) != 0;
            bool focused = (e.State & TreeNodeStates.Focused) != 0;

            // 背景颜色优先级：节点.BackColor (若设定) -> 选中时用 System highlight -> 控件 BackColor
            Color backColor = this.BackColor;
            if (!e.Node.BackColor.IsEmpty)
                backColor = e.Node.BackColor;
            if (selected)
                backColor = SystemColors.Highlight;

            var g = e.Graphics;
            using (var backBrush = new SolidBrush(backColor))
            {
                g.FillRectangle(backBrush, nodeBounds);
            }

            // 绘制节点图像（若存在）
            if (this.ImageList != null && this.ImageList.Images.Count > 0)
            {
                int idx = e.Node.ImageIndex;
                if (selected && e.Node.SelectedImageIndex >= 0)
                    idx = e.Node.SelectedImageIndex;

                if (idx >= 0 && idx < this.ImageList.Images.Count)
                {
                    Image img = this.ImageList.Images[idx];
                    // 垂直居中放置图像
                    int imgY = nodeBounds.Y + Math.Max(0, (nodeBounds.Height - img.Height) / 2);
                    int imgX = nodeBounds.X;
                    g.DrawImage(img, imgX, imgY, img.Width, img.Height);
                }
            }

            // 文本颜色：选中时用 highlight text，否则优先使用节点 ForeColor（若设定）
            Color foreColor = this.ForeColor;
            if (!e.Node.ForeColor.IsEmpty)
                foreColor = e.Node.ForeColor;
            if (selected)
                foreColor = SystemColors.HighlightText;

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
            TextRenderer.DrawText(g, e.Node.Text, this.Font, textRect, foreColor, flags);

            // 焦点矩形
            if (focused)
            {
                // 仅环绕文本区域绘制焦点矩形
                Rectangle focusRect = textRect;
                // 缩小焦点矩形宽度，避免覆盖折叠图标等
                focusRect.Width = Math.Max(0, TextRenderer.MeasureText(e.Node.Text, this.Font).Width);
                ControlPaint.DrawFocusRectangle(g, focusRect);
            }

            // 如果需要默认绘制（例如复杂情况无法覆盖），可以调用：
            // e.DrawDefault = true;
            // 但本实现已完整绘制节点内容
#endif
        }
    }
}