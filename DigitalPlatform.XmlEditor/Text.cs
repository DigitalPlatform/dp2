using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.IO;

namespace DigitalPlatform.Xml
{
    public class XmlText : TextVisual
    {
        public override ItemRegion GetRegionName()
        {
            return ItemRegion.Text;
        }

        public override int GetWidth()
        {
            int nWidth = 0;
            nWidth = this.TotalRestWidth;
            nWidth += 22;
            return nWidth;
        }

        public override int GetHeight(int nWidth)
        {
            Item item = this.GetItem();
            Graphics g = Graphics.FromHwnd(item.m_document.Handle);

            StringFormat sf = new StringFormat();
            sf.Trimming = StringTrimming.None;
            SizeF size = g.MeasureString(Text + "\r\n",   //给最后加一个'\r\n'以保证算出最后空行的高度
                GetFont(),
                nWidth,
                sf);

            int nTempHeight = (int)size.Height;

            //？注意这里是否有疑问
            if (nTempHeight <= 0)
                nTempHeight = 20;
            return nTempHeight + this.TotalRestHeight;
        }
    }
}
