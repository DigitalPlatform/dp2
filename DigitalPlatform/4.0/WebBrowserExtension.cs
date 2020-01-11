using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DigitalPlatform
{
    public static class WebBrowserExtension
    {
        public static void ScrollToEnd(this WebBrowser webBrowser1)
        {
            if (webBrowser1.Document != null
                && webBrowser1.Document.Window != null
                && webBrowser1.Document.Body != null)
                webBrowser1.Document.Window.ScrollTo(
                    0,
                    webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        /// <summary>
        /// 对浏览器控件设置 HTML 字符串
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strHtml">HTML 字符串</param>
        /// <param name="strDataDir">数据目录。本函数将在其中创建一个临时文件</param>
        /// <param name="strTempFileType">临时文件类型。用于构造临时文件名</param>
        public static void SetHtmlString(this WebBrowser webBrowser,
            string strHtml,
            string strDataDir,
            string strTempFileType)
        {
            StopWebBrowser(webBrowser);

            strHtml = strHtml.Replace("%datadir%", strDataDir);
            strHtml = strHtml.Replace("%mappeddir%", Path.Combine(strDataDir, "servermapped"));

            string strTempFilename = Path.Combine(strDataDir, "~temp_" + strTempFileType + ".html");
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strHtml);
            }
            // webBrowser.Navigate(strTempFilename);
            NavigateTo(webBrowser, strTempFilename);  // 2015/7/28
        }

        // 把 XML 字符串装入一个Web浏览器控件
        // 这个函数能够适应"<root ... />"这样的没有prolog的XML内容
        /// <summary>
        /// 把 XML 字符串装入一个Web浏览器控件
        /// 本方法能够适应"&lt;root ... /&gt;"这样的没有 prolog 的 XML 内容
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strDataDir">数据目录。本函数将在其中创建一个临时文件</param>
        /// <param name="strTempFileType">临时文件类型。用于构造临时文件名</param>
        /// <param name="strXml">XML 字符串</param>
        public static void SetXmlToWebbrowser(this WebBrowser webBrowser,
            string strDataDir,
            string strTempFileType,
            string strXml)
        {
            if (string.IsNullOrEmpty(strXml) == true)
            {
                ClearHtmlPage(webBrowser,
                    strDataDir);
                return;
            }

            string strTargetFileName = strDataDir + "\\temp_" + strTempFileType + ".xml";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strTargetFileName = Path.Combine(strDataDir, "xml.txt");
                using (StreamWriter sw = new StreamWriter(strTargetFileName,
    false,	// append
    System.Text.Encoding.UTF8))
                {
                    sw.Write("XML内容装入DOM时出错: " + ex.Message + "\r\n\r\n" + strXml);
                }
                // webBrowser.Navigate(strTargetFileName);
                NavigateTo(webBrowser, strTargetFileName);  // 2015/7/28
                return;
            }

            dom.Save(strTargetFileName);
            // webBrowser.Navigate(strTargetFileName);
            NavigateTo(webBrowser, strTargetFileName);  // 2015/7/28
        }


        internal static bool StopWebBrowser(this WebBrowser webBrowser)
        {
            WebBrowserInfo info = null;
            if (webBrowser.Tag is WebBrowserInfo)
            {
                info = (WebBrowserInfo)webBrowser.Tag;
                if (info != null)
                {
                    if (info.Cleared == true)
                    {
                        webBrowser.Stop();
                        return true;
                    }
                    else
                    {
                        info.Cleared = true;
                        return false;
                    }
                }
            }

            return false;
        }

        // 包装后的版本
        public static void ClearHtmlPage(this WebBrowser webBrowser,
    string strDataDir)
        {
            ClearHtmlPage(webBrowser, strDataDir, SystemColors.Window);
        }

        /// <summary>
        /// 清空浏览器控件内容
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strDataDir">数据目录。本函数将在其中创建一个临时文件</param>
        /// <param name="backColor">背景色</param>
        public static void ClearHtmlPage(this WebBrowser webBrowser,
            string strDataDir,
            Color backColor)
        {
            StopWebBrowser(webBrowser);

            if (String.IsNullOrEmpty(strDataDir) == true)
            {
                webBrowser.DocumentText = "(空)";
                return;
            }
            string strImageUrl = Path.Combine(strDataDir, "page_blank_128.png");
            string strHtml = "<html><body style='background-color:" + Color2String(backColor) + ";'><img src='" + strImageUrl + "' width='64' height='64' alt='空'></body></html>";
            webBrowser.DocumentText = strHtml;
        }

        //根据#XXXXXX格式字符串得到Color
        public static Color String2Color(string strColor)
        {
            string strR = strColor.Substring(1, 2);
            int nR = ConvertUtil.S2Int32(strR, 16);

            string strG = strColor.Substring(3, 2);
            int nG = ConvertUtil.S2Int32(strG, 16);

            string strB = strColor.Substring(5, 2);
            int nB = ConvertUtil.S2Int32(strB, 16);

            return Color.FromArgb(nR, nG, nB);
        }

        //将Color转换成#XXXXXX格式字符串
        public static string Color2String(Color color)
        {
            int nR = color.R;
            string strR = Convert.ToString(nR, 16);
            if (strR == "0")
                strR = "00";

            int nG = color.G;
            string strG = Convert.ToString(nG, 16);
            if (strG == "0")
                strG = "00";

            int nB = color.B;
            string strB = Convert.ToString(nB, 16);
            if (strB == "0")
                strB = "00";

            return "#" + strR + strG + strB;
        }

        // 2015/7/28 
        // 能处理异常的 Navigate
        public static void NavigateTo(this WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
        REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents(); // 2015/8/13
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }

    }

    /// <summary>
    /// 浏览器控件信息
    /// </summary>
    internal class WebBrowserInfo
    {
        /// <summary>
        /// 是否至少使用过一次
        /// </summary>
        public bool Cleared = false;    // 是否被使用过
    }
}
