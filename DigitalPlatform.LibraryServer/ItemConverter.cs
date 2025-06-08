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
    public class ItemConverter : ReaderItemConvertorBase
    {

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

