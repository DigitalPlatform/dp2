using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Drawing;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 一个标签的尺寸定义信息
    /// </summary>
    public class LabelParam
    {
        // 单位：百分之一英寸

        // 标签高度
        public double LabelHeight = 0;
        // 标签宽度
        public double LabelWidth = 0;

        // 行间距 2014/2/23
        public double LineSep = 0;


        // 标签内文字区的边距
        public DecimalPadding LabelPaddings = new DecimalPadding(0, 0, 0, 0);

        public List<LineFormat> LineFormats = new List<LineFormat>();

        public Font Font = new Font("Arial", 10, FontStyle.Regular, GraphicsUnit.Point);
        public bool IsBarcodeFont = false;  // 是否为条码字体？ 如果是条码字体，则要在文字左右加上 *

        public double PageWidth = 0;
        public double PageHeight = 0;

        // public bool Landscape = false;
        public int RotateDegree = 0;

        // 一页纸张的宏观定义信息
        // 页边距 单位为百分之一英寸
        public DecimalPadding PageMargins = new DecimalPadding(0, 0, 0, 0);

        public string DefaultPrinter = "";  // 缺省的打印机参数。包括打印机名和纸张名。如果没有定义页面尺寸，则用纸张的尺寸作为页面尺寸

        public static int Build(string strLabelDefFilename,
            out LabelParam label_param,
            out string strError)
        {
            label_param = null;
            strError = "";
            //int nRet = 0;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.Load(strLabelDefFilename);
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strLabelDefFilename + "' 到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            return Build(dom, out label_param, out strError);
        }

        static int GetMarginValue(
            string strPaddings,
            out DecimalPadding margins,
            out string strError)
        {
            strError = "";
            margins = new DecimalPadding();
            try
            {
                string[] parts = strPaddings.Split(new char[] { ',' });
                if (parts.Length > 0)
                    margins.Left = Convert.ToDecimal(parts[0]);
                if (parts.Length > 1)
                    margins.Top = Convert.ToDecimal(parts[1]);
                if (parts.Length > 2)
                    margins.Right = Convert.ToDecimal(parts[2]);
                if (parts.Length > 3)
                    margins.Bottom = Convert.ToDecimal(parts[3]);
            }
            catch (Exception ex)
            {
                strError = "值格式错误: " + ex.Message;
                return -1;
            }

            return 0;
        }

        public static int Build(XmlDocument dom,
    out LabelParam label_param,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            label_param = new LabelParam();

            XmlNode label = dom.DocumentElement.SelectSingleNode("label");
            if (label != null)
            {
                // int nValue = 0;
                double fValue = 0;

                // width
                nRet = DomUtil.GetDoubleParam(label,
                    "width",
                    0,
                    out fValue,
                    out strError);
                if (nRet == -1)
                    return -1;

                label_param.LabelWidth = fValue;

                // height
                nRet = DomUtil.GetDoubleParam(label,
                    "height",
                    0,
                    out fValue,
                    out strError);
                if (nRet == -1)
                    return -1;

                label_param.LabelHeight = fValue;

                // lineSep
                nRet = DomUtil.GetDoubleParam(label,
                    "lineSep",
                    0,
                    out fValue,
                    out strError);
                if (nRet == -1)
                    return -1;

                label_param.LineSep = fValue;

                {
                    DecimalPadding margins;
                    string strPaddings = DomUtil.GetAttr(label, "paddings");
                    nRet = GetMarginValue(
                        strPaddings,
                        out margins,
                out strError);
                    if (nRet == -1)
                    {
                        strError = "<label>元素paddings属性值格式错误: " + strError;
                        return -1;
                    }
                    label_param.LabelPaddings = margins;
                }
#if NO
                try
                {
                    string strPaddings = DomUtil.GetAttr(label, "paddings");

                    string[] parts = strPaddings.Split(new char[] { ',' });
                    if (parts.Length > 0)
                        label_param.LabelPaddings.Left = Convert.ToInt32(parts[0]);
                    if (parts.Length > 1)
                        label_param.LabelPaddings.Top = Convert.ToInt32(parts[1]);
                    if (parts.Length > 2)
                        label_param.LabelPaddings.Right = Convert.ToInt32(parts[2]);
                    if (parts.Length > 3)
                        label_param.LabelPaddings.Bottom = Convert.ToInt32(parts[3]);
                }
                catch (Exception ex)
                {
                    strError = "<label>元素paddings属性值格式错误: " + ex.Message;
                    return -1;
                }
#endif

                string strFont = DomUtil.GetAttr(label, "font");
                if (String.IsNullOrEmpty(strFont) == false)
                {
                    if (Global.IsVirtualBarcodeFont(ref strFont) == true)
                        label_param.IsBarcodeFont = true;

                    try
                    {
                        label_param.Font = Global.BuildFont(strFont);
                    }
                    catch (Exception ex)
                    {
                        strError = "<label>元素 font 属性值格式错误: " + ex.Message;
                        return -1;
                    }
                }
            }


            XmlNode page = dom.DocumentElement.SelectSingleNode("page");
            if (page != null)
            {
                {
                    // int nValue = 0;
                    double fValue = 0;

                    // width
                    nRet = DomUtil.GetDoubleParam(page,
                        "width",
                        0,
                        out fValue,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    label_param.PageWidth = fValue;

                    // height
                    nRet = DomUtil.GetDoubleParam(page,
                        "height",
                        0,
                        out fValue,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    label_param.PageHeight = fValue;
                }

                {
                    DecimalPadding margins;
                    string strMargins = DomUtil.GetAttr(page, "margins");
                    nRet = GetMarginValue(
        strMargins,
        out margins,
    out strError);
                    if (nRet == -1)
                    {
                        strError = "<page>元素margins属性值格式错误: " + strError;
                        return -1;
                    }
                    label_param.PageMargins = margins;
                }
#if NO
                try
                {
                    string strMargins = DomUtil.GetAttr(page, "margins");
                    string[] parts = strMargins.Split(new char[] { ',' });
                    if (parts.Length > 0)
                        label_param.PageMargins.Left = Convert.ToInt32(parts[0]);
                    if (parts.Length > 1)
                        label_param.PageMargins.Top = Convert.ToInt32(parts[1]);
                    if (parts.Length > 2)
                        label_param.PageMargins.Right = Convert.ToInt32(parts[2]);
                    if (parts.Length > 3)
                        label_param.PageMargins.Bottom = Convert.ToInt32(parts[3]);
                }
                catch (Exception ex)
                {
                    strError = "<page>元素margins属性值格式错误: " + ex.Message;
                    return -1;
                }
#endif

                label_param.DefaultPrinter = DomUtil.GetAttr(page, "defaultPrinter");

#if NO
                bool bValue = false;
                nRet = DomUtil.GetBooleanParam(page,
                    "landscape",
                    false,
                    out bValue,
                    out strError);
                if (nRet == -1)
                {
                    strError = "<page> 元素的 landscape 属性错误: " + strError;
                    return -1;
                }
                label_param.Landscape = bValue;
#endif
                int nValue = 0;
                DomUtil.GetIntegerParam(page,
                    "rotate",
                    0,
                    out nValue,
                    out strError);
                if (nRet == -1)
                {
                    strError = "<page> 元素的 rotate 属性错误: " + strError;
                    return -1;
                }
                label_param.RotateDegree = nValue;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("lineFormats/line");
            label_param.LineFormats = new List<LineFormat>();
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strFont = DomUtil.GetAttr(node, "font");

                LineFormat format = new LineFormat();

                if (Global.IsVirtualBarcodeFont(ref strFont) == true)
                    format.IsBarcodeFont = true;

                if (string.IsNullOrEmpty(strFont) == false)
                {
                    try
                    {
                        format.Font = Global.BuildFont(strFont);
                    }
                    catch (Exception ex)
                    {
                        strError = "<line>元素font属性值格式错误: " + ex.Message;
                        return -1;
                    }
                }
                else
                    format.Font = null; // 继承页面的字体

                format.Align = DomUtil.GetAttr(node, "align");
                format.Style = DomUtil.GetAttr(node, "style");

                string strOffset = DomUtil.GetAttr(node, "offset");
                if (string.IsNullOrEmpty(strOffset) == false)
                {
                    try
                    {
                        double left = 0;
                        double right = 0;
                        ParsetTwoDouble(strOffset,
                            false,
                            out left,
                            out right);
                        format.OffsetX = left;
                        format.OffsetY = right;
                    }
                    catch (Exception ex)
                    {
                        strError = "<line>元素offset属性值格式错误: " + ex.Message;
                        return -1;
                    }
                }

                string strStart = DomUtil.GetAttr(node, "start");
                if (string.IsNullOrEmpty(strStart) == false)
                {
                    try
                    {
                        double left = double.NaN;
                        double right = double.NaN;
                        ParsetTwoDouble(strStart,
                            true,
                            out left,
                            out right);
                        format.StartX = left;
                        format.StartY = right;
                    }
                    catch (Exception ex)
                    {
                        strError = "<line>元素start属性值格式错误: " + ex.Message;
                        return -1;
                    }
                }

                string strSize = DomUtil.GetAttr(node, "size");
                if (string.IsNullOrEmpty(strSize) == false)
                {
                    try
                    {
                        double left = double.NaN;
                        double right = double.NaN;
                        ParsetTwoDouble(strSize,
                            true,
                            out left,
                            out right);
                        format.Width = left;
                        format.Height = right;
                    }
                    catch (Exception ex)
                    {
                        strError = "<line>元素size属性值格式错误: " + ex.Message;
                        return -1;
                    }
                }

                format.ForeColor = DomUtil.GetAttr(node, "foreColor");
                format.BackColor = DomUtil.GetAttr(node, "backColor");

                label_param.LineFormats.Add(format);
            }

            return 0;
        }

        public static void ParsetTwoDouble(string strText,
            bool bEmptyAsNaN,
            out double left,
            out double right)
        {
            if (bEmptyAsNaN == true)
            {
                left = double.NaN;
                right = double.NaN;
            }
            else
            {
                left = 0;
                right = 0;
            }

            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strText, ",", out strLeft, out strRight);
            if (string.IsNullOrEmpty(strLeft) == false)
            {
                if (double.TryParse(strLeft, out left) == false)
                {
                    throw new Exception("字符串 '" + strLeft + "' 格式错误。逗号左侧应该是一个数字");
                }
            }
            if (string.IsNullOrEmpty(strRight) == false)
            {
                if (double.TryParse(strRight, out right) == false)
                {
                    throw new Exception("字符串 '" + strRight + "' 格式错误。逗号右侧应该是一个数字");
                }
            }
        }

        static string ToString(DecimalPadding margin)
        {
            return margin.Left.ToString() + ","
                + margin.Top.ToString() + ","
                + margin.Right.ToString() + ","
                + margin.Bottom.ToString();
        }

        // 将内容创建为 XmlDocument
        public int ToXmlDocument(out XmlDocument dom,
            out string strError)
        {
            strError = "";
            dom = new XmlDocument();

            dom.LoadXml("<root />");

            // <page>
            XmlElement page = dom.CreateElement("page");
            dom.DocumentElement.AppendChild(page);

            DomUtil.SetAttr(page, "width", this.PageWidth.ToString());
            DomUtil.SetAttr(page, "height", this.PageHeight.ToString());
            DomUtil.SetAttr(page, "margins", ToString(this.PageMargins));

            DomUtil.SetAttr(page, "defaultPrinter", this.DefaultPrinter);
#if NO
            if (this.Landscape == true)
                DomUtil.SetAttr(page, "landscape", "yes");
#endif
            if (this.RotateDegree != 0)
                DomUtil.SetAttr(page, "rotate", this.RotateDegree.ToString());

            // <label>
            XmlElement label = dom.CreateElement("label");
            dom.DocumentElement.AppendChild(label);

            DomUtil.SetAttr(label, "width", this.LabelWidth.ToString());
            DomUtil.SetAttr(label, "height", this.LabelHeight.ToString());
            DomUtil.SetAttr(label, "paddings", ToString(this.LabelPaddings));

            if (this.IsBarcodeFont == true)
                DomUtil.SetAttr(label, "font", Global.GetBarcodeFontString(this.Font));
            else
                DomUtil.SetAttr(label, "font", FontUtil.GetFontString(this.Font));
            DomUtil.SetAttr(label, "lineSep", this.LineSep.ToString());

            // <lineFormats>
            if (this.LineFormats.Count > 0)
            {
                XmlElement lineFormats = dom.CreateElement("lineFormats");
                dom.DocumentElement.AppendChild(lineFormats);

                foreach (LineFormat format in this.LineFormats)
                {
                    XmlElement line = dom.CreateElement("line");
                    lineFormats.AppendChild(line);

                    if (format.Font != null)
                    {
                        if (format.IsBarcodeFont == true)
                            DomUtil.SetAttr(line, "font", Global.GetBarcodeFontString(format.Font));
                        else
                            DomUtil.SetAttr(line, "font", FontUtil.GetFontString(format.Font));
                    }

                    DomUtil.SetAttr(line, "align", format.Align);
                    DomUtil.SetAttr(line, "style", format.Style);

                    Debug.Assert(double.IsNaN(format.OffsetX) == false, "OffsetX 不可能为 NaN");
                    Debug.Assert(double.IsNaN(format.OffsetY) == false, "OffsetY 不可能为 NaN");

                    if (format.OffsetX != 0 || format.OffsetY != 0)
                        line.SetAttribute("offset", format.OffsetX + "," + format.OffsetY);

                    if (double.IsNaN(format.StartX) == false || double.IsNaN(format.StartY) == false)
                        line.SetAttribute("start", ToString(format.StartX) + "," + ToString(format.StartY));

                    if (double.IsNaN(format.Width) == false || double.IsNaN(format.Height) == false)
                        line.SetAttribute("size", ToString(format.Width) + "," + ToString(format.Height));

                    if (string.IsNullOrEmpty(format.ForeColor) == false)
                        line.SetAttribute("foreColor", format.ForeColor);

                    if (string.IsNullOrEmpty(format.BackColor) == false)
                        line.SetAttribute("backColor", format.BackColor);

                }
            }

            return 0;
        }

        public static string ToString(double v)
        {
            if (double.IsNaN(v) == true)
                return "";
            return v.ToString();
        }
    }

    // 一行的格式
    public class LineFormat
    {
        public Font Font = null;    // 如果为空，则表示继承页面的字体

        public string Align = "left";

        public bool IsBarcodeFont = false;  // 是否为条码字体？ 如果是条码字体，则要在文字左右加上 *

        // 左上角绝对位置
        public double StartX = Double.NaN;
        public double StartY = Double.NaN;

        // 相对偏移位置
        public double OffsetX = 0;
        public double OffsetY = 0;

        // 2017/2/26
        // 宽高
        public double Width = Double.NaN;
        public double Height = Double.NaN;


        // 前景颜色
        public string ForeColor = "";   // 缺省为黑色
        // 背景颜色
        public string BackColor = "";   // 缺省为透明

        public string Style = "";   // 风格
    }

}
