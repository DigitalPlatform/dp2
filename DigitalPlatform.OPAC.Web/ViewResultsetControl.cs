using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Diagnostics;
using System.Collections;

using System.Threading;
using System.Resources;
using System.Globalization;
using System.Xml;

using DigitalPlatform.ResultSet;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.Xml;

using DigitalPlatform.OPAC.Server;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:ViewResultsetControl runat=server></{0}:ViewResultsetControl>")]
    public class ViewResultsetControl : WebControl, INamingContainer
    {
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        public string ResultSetName
        {
            get
            {
                this.EnsureChildControls();
                HiddenField resultsetname = (HiddenField)this.FindControl("resultsetname");
                if (resultsetname == null)
                    return "";
                return resultsetname.Value;
            }
            set
            {
                this.EnsureChildControls();
                HiddenField resultsetname = (HiddenField)this.FindControl("resultsetname");
                resultsetname.Value = value;
            }
        }

        public string FormatName
        {
            get
            {
                this.EnsureChildControls();
                HiddenField formatname = (HiddenField)this.FindControl("formatname");
                if (formatname == null)
                    return "";
                return formatname.Value;
            }
            set
            {
                this.EnsureChildControls();
                HiddenField formatname = (HiddenField)this.FindControl("formatname");
                formatname.Value = value;
            }
        }

        public int ResultCount
        {
            get
            {
                this.EnsureChildControls();
                HiddenField resultcount = (HiddenField)this.FindControl("resultcount");
                if (resultcount == null)
                    return 0;

                if (string.IsNullOrEmpty(resultcount.Value) == true)
                    return 0;

                return Convert.ToInt32(resultcount.Value);
            }
            set
            {
                this.EnsureChildControls();
                HiddenField resultcount = (HiddenField)this.FindControl("resultcount");
                resultcount.Value = value.ToString();
            }
        }

        public int StartIndex
        {
            get
            {
                this.EnsureChildControls();
                HiddenField startindex = (HiddenField)this.FindControl("startindex");
                if (startindex == null)
                    return 0;
                if (string.IsNullOrEmpty(startindex.Value) == true)
                    return 0;

                return Convert.ToInt32(startindex.Value);
            }
            set
            {
                this.EnsureChildControls();
                HiddenField startindex = (HiddenField)this.FindControl("startindex");
                startindex.Value = value.ToString();
            }
        }

        protected override void CreateChildControls()
        {
            HiddenField resultsetname = new HiddenField();
            resultsetname.ID = "resultsetname";
            this.Controls.Add(resultsetname);

            HiddenField formatname = new HiddenField();
            formatname.ID = "formatname";
            this.Controls.Add(formatname);

            HiddenField resultcount = new HiddenField();
            resultcount.ID = "resultcount";
            this.Controls.Add(resultcount);

            HiddenField startindex = new HiddenField();
            startindex.ID = "startindex";
            this.Controls.Add(startindex);
        }

        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        public int PageMaxLines = 10;

        // 获得当前渲染内容的 HTML 片断代码
        public static int GetContentText(
            OpacApplication app,
            SessionInfo sessioninfo,
            string strResultSetName,
            int nStart,
            int nCount,
            string strLang,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";
            long lRet = 0;

            StringBuilder text = new StringBuilder(4096);
            Record[] searchresults = null;
            long lStart = nStart;
            long lCount = nCount;
            long lTotalCount = 0;
            for (; ; )
            {
                lRet = sessioninfo.Channel.GetSearchResult(
        null,
        strResultSetName,
        lStart,
        lCount,
        "id",   // cols
        strLang,
        out searchresults,
        out strError);
                if (lRet == -1 && searchresults == null)
                    return -1;

                if (lRet != -1)
                    lTotalCount = lRet;

                int i = 0;
                foreach (Record record in searchresults)
                {
                    // data-index 书目记录位于结果集中的偏移量
                    // data_recpath 书目记录路径
                    text.Append("<div class='bibliorecord' data-index='"+(lStart+i).ToString()+"' data-recpath='"+HttpUtility.HtmlAttributeEncode(record.Path)+"'></div>\r\n");
                    i++;
                }

                lStart += searchresults.Length;
                lCount -= searchresults.Length;
                if (lStart >= lTotalCount)
                    break;
                if (lStart + lCount > lTotalCount)
                    lCount = lTotalCount - lStart;
                if (lCount <= 0)
                    break;
            }

            strResult = text.ToString();
            return 0;
        }
    }
}
