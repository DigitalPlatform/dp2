using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Xml;
using DigitalPlatform.Xml;

namespace StackRoomEditor
{
    /// <summary>
    /// TextPanelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TextPanelWindow : Window
    {
        public TextPanelWindow()
        {
            InitializeComponent();
        }

        public static int CreateTextPanel(Grid frame,
            string strXml,
            out string strError)
        {
            strError = "";

            frame.Children.Clear();
            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            Border border = new Border();
            border.HorizontalAlignment = HorizontalAlignment.Stretch;
            border.VerticalAlignment = VerticalAlignment.Stretch;
            frame.Children.Add(border);

            Grid grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.VerticalAlignment = VerticalAlignment.Stretch;
            border.Child = grid;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            // borderBrush
            string strBorderBrush = DomUtil.GetAttr(dom.DocumentElement, "borderBrush");
            if (string.IsNullOrEmpty(strBorderBrush) == false)
            {
                try
                {
                    border.BorderBrush = (Brush)new BrushConverter().ConvertFromString(strBorderBrush);
                }
                catch (Exception ex)
                {
                    strError = "边框颜色定义 '" + strBorderBrush + "' 格式不正确";
                    return -1;
                }
            }

            {
                // borderThickness
                string strBorderThickness = DomUtil.GetAttr(dom.DocumentElement, "borderThickness");
                if (string.IsNullOrEmpty(strBorderThickness) == false)
                {
                    try
                    {
                        border.BorderThickness = (Thickness)new ThicknessConverter().ConvertFromString(strBorderThickness);
                    }
                    catch (Exception ex)
                    {
                        strError = "边框厚度定义 '" + strBorderThickness + "' 格式不正确";
                        return -1;
                    }
                }
            }

            {
                // borderCornerRadius
                string strBorderCornerRadius = DomUtil.GetAttr(dom.DocumentElement, "borderCornerRadius");
                if (string.IsNullOrEmpty(strBorderCornerRadius) == false)
                {
                    try
                    {
                        border.CornerRadius = (CornerRadius)new CornerRadiusConverter().ConvertFromString(strBorderCornerRadius);
                    }
                    catch (Exception ex)
                    {
                        strError = "边框圆角定义 '" + strBorderCornerRadius + "' 格式不正确";
                        return -1;
                    }
                }
            }

            // width
            string strWidth = DomUtil.GetAttr(dom.DocumentElement, "width");
            if (string.IsNullOrEmpty(strWidth) == true)
                strWidth = "200";
            if (string.IsNullOrEmpty(strWidth) == false)
            {
                try
                {
                    frame.Width = (double)new LengthConverter().ConvertFromString(strWidth);
                }
                catch (Exception ex)
                {
                    strError = "宽度定义 '" + strWidth + "' 格式不正确";
                    return -1;
                }
            }

            {
                // margin
                string strMargin = DomUtil.GetAttr(dom.DocumentElement, "margin");
                if (string.IsNullOrEmpty(strMargin) == false)
                {
                    try
                    {
                        frame.Margin = (Thickness)new ThicknessConverter().ConvertFromString(strMargin);
                    }
                    catch (Exception ex)
                    {
                        strError = "外边距定义 '" + strMargin + "' 格式不正确";
                        return -1;
                    }
                }
            }

            // padding
            string strPadding = DomUtil.GetAttr(dom.DocumentElement, "padding");
            if (string.IsNullOrEmpty(strPadding) == true)
                strPadding = "20";
            if (string.IsNullOrEmpty(strPadding) == false)
            {
                try
                {
                    grid.Margin = (Thickness)new ThicknessConverter().ConvertFromString(strPadding);
                }
                catch (Exception ex)
                {
                    strError = "内边距定义 '" + strPadding + "' 格式不正确";
                    return -1;
                }
            }

            // background
            string strBackground = DomUtil.GetAttr(dom.DocumentElement, "background");
            if (string.IsNullOrEmpty(strBackground) == false)
            {
                try
                {
                    border.Background = (Brush)new BrushConverter().ConvertFromString(strBackground);
                }
                catch (Exception ex)
                {
                    strError = "背景定义 '" + strBackground + "' 格式不正确";
                    return -1;
                }
            }

            frame.Background = null;

            // position
            string strPosition = DomUtil.GetAttr(dom.DocumentElement, "position");
            if (string.IsNullOrEmpty(strPosition) == true)
                strPosition = "左下";

            if (strPosition == "左上")
            {
                frame.HorizontalAlignment = HorizontalAlignment.Left;
                frame.VerticalAlignment = VerticalAlignment.Top;
            }
            else if (strPosition == "右上")
            {
                frame.HorizontalAlignment = HorizontalAlignment.Right;
                frame.VerticalAlignment = VerticalAlignment.Top;
            }
            else if (strPosition == "左下")
            {
                frame.HorizontalAlignment = HorizontalAlignment.Left;
                frame.VerticalAlignment = VerticalAlignment.Bottom;
            }
            else if (strPosition == "右下")
            {
                frame.HorizontalAlignment = HorizontalAlignment.Right;
                frame.VerticalAlignment = VerticalAlignment.Bottom;
            }
            else
            {
                // 缺省为左下
                frame.HorizontalAlignment = HorizontalAlignment.Left;
                frame.VerticalAlignment = VerticalAlignment.Bottom;
            }

            
            /*
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("p or text()");
            int i = 0;
            foreach (XmlNode node in nodes)
            {
             * */
            int i = 0;
            foreach (XmlNode node in dom.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element
                    && node.Name == "p")
                {
                }
                else if (node.NodeType == XmlNodeType.Text)
                {
                }
                else
                    continue;

                RowDefinition d = new RowDefinition();
                d.Height = new GridLength(0, GridUnitType.Auto);
                grid.RowDefinitions.Add(d);

                TextBlock text = new TextBlock();
                text.Text = node.InnerText;
                text.TextWrapping = TextWrapping.Wrap;

                // fontFamily
                string strFontName = DomUtil.GetAttr(node, "fontFamily");
                if (string.IsNullOrEmpty(strFontName) == false)
                {
                    // text.FontFamily = new FontFamily(strFontName);
                    try
                    {
                        text.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString(strFontName);
                    }
                    catch (Exception ex)
                    {
                        strError = "fontFamily属性值 '" + strFontName + "' 格式不正确";
                        return -1;
                    }
                }

                // fontSize
                string strFontSize = DomUtil.GetAttr(node, "fontSize");
                if (string.IsNullOrEmpty(strFontSize) == false)
                {
#if NO
                    double v = 0;
                    if (double.TryParse(strFontSize, out v) == false)
                    {
                        strError = "fontSize属性值 '"+strFontSize+"' 格式不正确";
                        return -1;
                    }
                    text.FontSize = v;
#endif
                    try
                    {
                        text.FontSize = (double)new FontSizeConverter().ConvertFromString(strFontSize);
                    }
                    catch (Exception ex)
                    {
                        strError = "fontSize属性值 '" + strFontSize + "' 格式不正确";
                        return -1;
                    }
                }

                // fontStyle
                string strFontStyle = DomUtil.GetAttr(node, "fontStyle");
                if (string.IsNullOrEmpty(strFontStyle) == false)
                {
                    try
                    {
                        text.FontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(strFontStyle);
                    }
                    catch (Exception ex)
                    {
                        strError = "fontStyle属性值 '" + strFontStyle + "' 格式不正确";
                        return -1;
                    }
                }

                // margin
                string strMargin = DomUtil.GetAttr(node, "margin");
                if (string.IsNullOrEmpty(strMargin) == false)
                {
                    try
                    {
                        text.Margin = (Thickness)new ThicknessConverter().ConvertFromString(strMargin);
                    }
                    catch (Exception ex)
                    {
                        strError = "margin属性值 '" + strMargin + "' 格式不正确";
                        return -1;
                    }
                }

                // foreground
                string strForeground = DomUtil.GetAttr(node, "foreground");
                if (string.IsNullOrEmpty(strForeground) == false)
                {
                    try
                    {
                        text.Foreground = (Brush)new BrushConverter().ConvertFromString(strForeground);
                    }
                    catch (Exception ex)
                    {
                        strError = "foreground属性值 '" + strForeground + "' 格式不正确";
                        return -1;
                    }
                }

                // textAlignment
                string strTextAlignment = DomUtil.GetAttr(node, "textAlignment");
                if (string.IsNullOrEmpty(strTextAlignment) == false)
                {
                    try
                    {
                        text.TextAlignment = (TextAlignment)Enum.Parse(typeof(TextAlignment), strTextAlignment, true);
                    }
                    catch (Exception ex)
                    {
                        strError = "foreground属性值 '" + strForeground + "' 格式不正确";
                        return -1;
                    }
                }

                // text.Height = -1;
                grid.Children.Add(text);

                Grid.SetRow(text, i);
                i++;
            }

            {
                RowDefinition d = new RowDefinition();
                d.Height = new GridLength(0, GridUnitType.Auto);
                grid.RowDefinitions.Add(d);
            }

            return 0;
        }

        string m_strXml = "";
        public string Xml
        {
            get
            {
                return this.m_strXml;
            }
            set
            {
                this.m_strXml = value;

                // 兑现到界面
                string strError = "";
                int nRet = Initial(value,
                        out strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }
        }

        int Initial(string strXml,
            out string strError)
        {
            strError = "";
            this.Clear();

            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            this.textBox_width.Text = DomUtil.GetAttr(dom.DocumentElement, "width");
            this.textBox_padding.Text = DomUtil.GetAttr(dom.DocumentElement, "padding");
            this.textBox_background.Text = DomUtil.GetAttr(dom.DocumentElement, "background");
            this.comboBox_position.Text = DomUtil.GetAttr(dom.DocumentElement, "position");
            this.textBox_margin.Text = DomUtil.GetAttr(dom.DocumentElement, "margin");

            this.textBox_borderBrush.Text = DomUtil.GetAttr(dom.DocumentElement, "borderBrush");
            this.textBox_borderCornerRadius.Text = DomUtil.GetAttr(dom.DocumentElement, "borderCornerRadius");
            this.textBox_borderThickness.Text = DomUtil.GetAttr(dom.DocumentElement, "borderThickness");

            this.textBox_def.Text = dom.DocumentElement.InnerXml;

            return 0;
        }

        void Clear()
        {
            this.textBox_width.Text = "";
            this.textBox_padding.Text = "";
            this.textBox_background.Text = "";
            this.comboBox_position.Text = "";
            this.textBox_margin.Text = "";
            this.textBox_borderBrush.Text = "";
            this.textBox_borderCornerRadius.Text = "";
            this.textBox_borderThickness.Text = "";


            this.textBox_def.Text = "";
        }

        int CreateXml(out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<textPanel />");

            try
            {
                dom.DocumentElement.InnerXml = this.textBox_def.Text;
            }
            catch (Exception ex)
            {
                strError = "文字定义格式不正确: " + ex.Message;
                return -1;
            }

            DomUtil.SetAttr(dom.DocumentElement, "width", this.textBox_width.Text);
            DomUtil.SetAttr(dom.DocumentElement, "padding", this.textBox_padding.Text);
            DomUtil.SetAttr(dom.DocumentElement, "background", this.textBox_background.Text);
            DomUtil.SetAttr(dom.DocumentElement, "position", this.comboBox_position.Text);
            DomUtil.SetAttr(dom.DocumentElement, "margin", this.textBox_margin.Text);

            DomUtil.SetAttr(dom.DocumentElement, "borderBrush", this.textBox_borderBrush.Text);
            DomUtil.SetAttr(dom.DocumentElement, "borderCornerRadius", this.textBox_borderCornerRadius.Text);
            DomUtil.SetAttr(dom.DocumentElement, "borderThickness", this.textBox_borderThickness.Text);

            this.m_strXml = dom.DocumentElement.OuterXml;
            return 0;
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            string strError = "";
            if (CreateXml(out strError) == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
