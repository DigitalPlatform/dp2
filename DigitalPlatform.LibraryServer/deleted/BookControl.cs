using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;

namespace DigitalPlatform.LibraryServer
{
#if NO
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:BookControl runat=server></{0}:BookControl>")]
    public class BookControl : WebControl, INamingContainer
    {
        public string Barcode = "";

        public string BiblioRecPath = "";

        //public SessionInfo SessionInfo = null;
        //public CirculationApplication App = null;

        public DispStyle DispStyle = DispStyle.Biblio | DispStyle.Items;

        string tempOutput = "";
        ItemConverter ItemConverter = null;
        ItemConverterEventArgs ItemConverterEventArgs = null;

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }


        /*
        Control FindControl(string strID)
        {
            foreach (Control control in this.Controls)
            {
                if (control.ID == strID)
                    return control;
            }

            return null;
        }
         */

        protected override void CreateChildControls()
        {

            // 种
            LiteralControl literal = new LiteralControl();
            literal.ID = "biblio";
            literal.Text = "";
            this.Controls.Add(literal);

            // 册
            literal = new LiteralControl();
            literal.ID = "items";
            literal.Text = "";
            this.Controls.Add(literal);

        }

        protected override void Render(HtmlTextWriter output)
        {
            int nRet = 0;
            long lRet = 0;
            string strError = "";
            string strOutputPath = "";
            string strItemXml = "";

            LibraryApplication app = (LibraryApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strItemRecPath = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "channel == null";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(this.Barcode) == false)
            {

                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = app.GetItemRecXml(
                    channel,
                    this.Barcode,
                    out strItemXml,
                    out strOutputPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "册条码号为 '" + this.Barcode + "' 的册记录没有找到";
                    goto ERROR1;
                }

                if (nRet == -1)
                    goto ERROR1;

                strItemRecPath = strOutputPath;
            }


            string strItemDbName = "";
            string strBiblioRecID = "";
            string strBiblioDbName = "";


            // 若需要同时取得种记录
            if ( (this.DispStyle & DispStyle.Biblio ) == DispStyle.Biblio)
            {
                string strBiblioRecPath = "";

                if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
                {
                    /*
                    // 准备工作: 映射数据库名
                    nRet = app.GetGlobalCfg(sessioninfo.Channels,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                     * */

                    strItemDbName = ResPath.GetDbName(strOutputPath);
                    // string strBiblioDbName = "";

                    // 根据实体库名, 找到对应的书目库名
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = app.GetBiblioDbNameByItemDbName(strItemDbName,
                        out strBiblioDbName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "实体库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                        goto ERROR1;
                    }


                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strItemXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "册记录XML装载到DOM出错:" + ex.Message;
                        goto ERROR1;
                    }

                    strBiblioRecID = DomUtil.GetElementText(dom.DocumentElement, "parent"); //
                    if (String.IsNullOrEmpty(strBiblioRecID) == true)
                    {
                        strError = "册记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                        goto ERROR1;
                    }

                    strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
                }
                else
                {
                    strBiblioRecPath = this.BiblioRecPath;

                    strBiblioDbName = ResPath.GetDbName(this.BiblioRecPath);

                    if (String.IsNullOrEmpty(strItemDbName) == true)
                    {
                        // 根据书目库名, 找到对应的实体库名
                        // return:
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到
                        nRet = app.GetItemDbName(strBiblioDbName,
                            out strItemDbName,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                    }

                    strBiblioRecID = ResPath.GetRecordId(this.BiblioRecPath);
                }

                string strBiblioXml = "";


                string strMetaData = "";
                byte[] timestamp = null;
                lRet = channel.GetRes(strBiblioRecPath,
                    out strBiblioXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "获得种记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                    goto ERROR1;
                }

                // 需要从内核映射过来文件
                string strLocalPath = "";
                nRet = app.MapKernelScriptFile(
                    sessioninfo,
                    strBiblioDbName,
                    "./cfgs/loan_biblio.fltx",
                    out strLocalPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 将种记录数据从XML格式转换为HTML格式
                string strBiblio = "";
                // 2006/11/28 changed
                string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                if (string.IsNullOrEmpty(strBiblioXml) == false)
                {
                    nRet = app.ConvertBiblioXmlToHtml(
                    strFilterFileName,
                        strBiblioXml,
                        strBiblioRecPath,
                        out strBiblio,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                    strBiblio = "";
                // output.Write(strBiblio);
                LiteralControl literal = (LiteralControl)FindControl("biblio");
                literal.Text = strBiblio;
            }

            if ((this.DispStyle & DispStyle.Items) == DispStyle.Items)
            {

                this.ItemConverter = app.NewItemConverter(
                    app.CfgDir + "\\itemopac.cs",
                    app.CfgDir + "\\itemopac.cs.ref",
                    out strError);
                if (this.ItemConverter == null)
                    goto ERROR1;
                this.ItemConverter.App = app;


                // 检索出该种的所有册
                sessioninfo.ItemLoad += new ItemLoadEventHandler(SessionInfo_ItemLoad);
                tempOutput = "";
                // tempOutput = output;
                try
                {
                    nRet = sessioninfo.SearchItems(
                        app,
                        strItemDbName,
                        strBiblioRecID,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                finally
                {
                    sessioninfo.ItemLoad -= new ItemLoadEventHandler(SessionInfo_ItemLoad);
                    //tempOutput = null;

                    this.ItemConverter = null;
                }

                LiteralControl literal = (LiteralControl)FindControl("items");
                literal.Text = tempOutput;
                tempOutput = "";

            }

            else if ((this.DispStyle & DispStyle.Item) == DispStyle.Item)
            {

                string strResult = "";
                // 取得册信息
                // 将册记录数据从XML格式转换为HTML格式
                nRet = app.ConvertItemXmlToHtml(
                    app.CfgDir + "\\itemxml2html.cs",
                    app.CfgDir + "\\itemxml2html.cs.ref",
                    strItemXml,
                    strItemRecPath, // 2009/10/18 new add
                    out strResult,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // output.Write(strResult);
                LiteralControl literal = (LiteralControl)FindControl("items");
                literal.Text = strResult;
            }


            base.Render(output);

            return;

        ERROR1:
            output.Write(strError);
         
        }

        void SessionInfo_ItemLoad(object sender, ItemLoadEventArgs e)
        {
            int nRet = 0;
            string strError = "";
            // string strResult = "";

            // 连带第一次
            if (e.Index == 0)
            {
                this.ItemConverterEventArgs = new ItemConverterEventArgs();
                this.ItemConverterEventArgs.Count = e.Count;
                this.ItemConverterEventArgs.Index = -1;
                this.ItemConverterEventArgs.ActiveBarcode = "";

                nRet = LibraryApplication.RunItemConverter(
                    "begin",
                    this.ItemConverter,
                    this,
                    this.ItemConverterEventArgs,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                //tempOutput.Write(this.ItemConverterEventArgs.ResultString);
                tempOutput += this.ItemConverterEventArgs.ResultString;
                this.ItemConverterEventArgs.ResultString = "";

            }

            this.ItemConverterEventArgs.Index = e.Index;
            this.ItemConverterEventArgs.ActiveBarcode = Barcode;
            this.ItemConverterEventArgs.Count = e.Count;
            this.ItemConverterEventArgs.Xml = e.Xml;
            nRet = LibraryApplication.RunItemConverter(
                "item",
                ItemConverter,
                this,
                this.ItemConverterEventArgs,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // tempOutput.Write(this.ItemConverterEventArgs.ResultString);
            tempOutput += this.ItemConverterEventArgs.ResultString;
            this.ItemConverterEventArgs.ResultString = "";

            // 连带最后一次
            if (e.Index == e.Count - 1)
            {
                this.ItemConverterEventArgs.Count = e.Count;
                this.ItemConverterEventArgs.Index = e.Count;
                this.ItemConverterEventArgs.ActiveBarcode = "";

                nRet = LibraryApplication.RunItemConverter(
                    "end",
                    this.ItemConverter,
                    this,
                    this.ItemConverterEventArgs,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                // tempOutput.Write(this.ItemConverterEventArgs.ResultString);
                tempOutput += this.ItemConverterEventArgs.ResultString;
                this.ItemConverterEventArgs.ResultString = "";
            }

            return;
        ERROR1:
            tempOutput += strError;
            // tempOutput.Write(strError);
        }
    }

    public enum DispStyle
    {
        Biblio = 0x01,  // 显示种
        Item = 0x02,    // 显示册
        Items = 0x04,   // 显示全部册
    }
#endif
}
