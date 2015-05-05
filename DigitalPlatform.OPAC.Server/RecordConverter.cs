using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// dp2opac中调用C#脚本时, 用于转换一般记录信息xml->html的脚本类的基类
    /// </summary>
    public class RecordConverter
    {
        public OpacApplication App = null;

        public string RecPath = ""; // 2009/10/18 new add

        public RecordConverter()
        {

        }

        public virtual string Convert(string strXml)
        {

            return strXml;
        }
    }
}
