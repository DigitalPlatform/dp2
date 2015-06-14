using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// dp2library中调用C#脚本时, 用于转换册信息xml->html的脚本类的基类
    /// </summary>
    public class ItemConverter
    {
        public LibraryApplication App = null;

        public ItemConverter()
        {

        }

        public virtual void Begin(object sender,
    ItemConverterEventArgs e)
        {

        }

        public virtual void Item(object sender,
            ItemConverterEventArgs e)
        {

        }

        public virtual void End(object sender,
            ItemConverterEventArgs e)
        {

        }

        public static string LocalTime(string strRfc1123Time)
        {
            try
            {
                if (String.IsNullOrEmpty(strRfc1123Time) == true)
                    return "";
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123Time, "G");
            }
            catch (Exception /*ex*/)    // 2008/10/28
            {
                return "时间字符串 '" + strRfc1123Time + "' 格式错误，不是合法的RFC1123格式";
            }

        }

        /*
        public static string LocalDate(string strRfc1123Time)
        {
            if (String.IsNullOrEmpty(strRfc1123Time) == true)
                return "";
            return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123Time, "yyyy-MM-dd");
        }*/

        // 将RFC1123时间字符串转换为本地一般日期字符串
        public static string LocalDate(string strRfc1123Time)
        {
            try
            {
                if (String.IsNullOrEmpty(strRfc1123Time) == true)
                    return "";

                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123Time, "d"); // "yyyy-MM-dd"
            }
            catch (Exception /*ex*/)    // 2008/10/28
            {
                return "日期字符串 '" + strRfc1123Time + "' 格式错误，不是合法的RFC1123格式";
            }
        }
    }

    public class ItemConverterEventArgs : EventArgs
    {
        public string Xml = "";
        public string RecPath = ""; // 2009/10/18
        public int Index = -1;
        public int Count = 0;
        public string ActiveBarcode = "";

        public string ResultString = "";
        public Control ParentControl = null;
    }
}

