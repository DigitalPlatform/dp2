using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.IO;
//using DigitalPlatform.CirculationClient;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:PersonalInfoControl runat=server></{0}:PersonalInfoControl>")]
    public class PersonalInfoControl : ReaderInfoBase
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.PersonalInfoControl.cs",
                typeof(PersonalInfoControl).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }

#if NO
        static string DarkOrLight(ref int index)
        {
            index++;
            if ((index % 2) == 0)
                return "dark";
            return "light";
        }
#endif

#if NO
        protected override void RenderContents(HtmlTextWriter output)
        {
            string strError = "";
            // return:
            //      -1  出错
            //      0   成功
            //      1   尚未登录
            int nRet = this.LoadReaderXml(out strError);
            if (nRet == -1)
            {
                output.Write(strError);
                return;
            }

            if (nRet == 1)
            {
                sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                this.Page.Response.Redirect("login.aspx", true);
                return;
            }

            int index = 0;

            string strResult = "";

            strResult += this.GetPrefixString(
                this.GetString("个人信息"),
                "personalinfo_wrapper");
            strResult += "<table class='personalinfo'>";
            // strResult += "<tr class='columntitle'><td>册条码号</td><td>书目摘要</td><td>借阅情况</td><td>续借注</td><td>操作者</td></tr>";

            // 显示名
            string strDisplayName = DomUtil.GetElementText(ReaderDom.DocumentElement,
    "displayName");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='displayname'>"
                + this.GetString("显示名")
                + "</td><td class='displayname value'>"
+ strDisplayName + "</td></tr>";

            // 姓名
            string strName = DomUtil.GetElementText(ReaderDom.DocumentElement, 
                "name");
            strResult += "<tr class='"+DarkOrLight(ref index)+"'><td class='name'>"
                + this.GetString("姓名")
                + "</td><td class='name value'>"
                + strName + "</td></tr>";

            // 性别
            string strGender = DomUtil.GetElementText(ReaderDom.DocumentElement, 
                "gender");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='gender'>"
                + this.GetString("性别")
                + "</td><td class='gender value'>"
                + strGender + "</td></tr>";

            // 生日
            string strBirthday = DomUtil.GetElementText(ReaderDom.DocumentElement, 
                "birthday");
            strBirthday = DateTimeUtil.LocalDate(strBirthday);
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='birthday'>"
                + this.GetString("生日")
                + "</td><td class='birthday value'>"
                + strBirthday + "</td></tr>";

            // 证号 2008/11/11
            string strCardNumber = DomUtil.GetElementText(ReaderDom.DocumentElement,
    "cardNumber");
            if (String.IsNullOrEmpty(strCardNumber) == false)
            {
                strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='cardnumber'>"
                + this.GetString("证号")
                + "</td><td class='cardnumber value'>"
        + strCardNumber + "</td></tr>";
            }

            // 身份证号
            string strIdCardNumber = DomUtil.GetElementText(ReaderDom.DocumentElement,
    "idCardNumber");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='idcardnumber'>"
                + this.GetString("身份证号")
                + "</td><td class='idcardnumber value'>"
    + strIdCardNumber + "</td></tr>";

            // 单位
            string strDepartment = DomUtil.GetElementText(ReaderDom.DocumentElement,
"department");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='department'>"
                + this.GetString("单位")
                + "</td><td class='department value'>"
+ strDepartment + "</td></tr>";

            // 职务
            string strPost = DomUtil.GetElementText(ReaderDom.DocumentElement,
"post");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='post'>"
                + this.GetString("职务")
                + "</td><td class='post value'>"
+ strPost + "</td></tr>";


            // 地址
            string strAddress = DomUtil.GetElementText(ReaderDom.DocumentElement,
"address");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='address'>"
                + this.GetString("地址")
                + "</td><td class='address value'>"
+ strAddress + "</td></tr>";

            // 电话
            string strTel = DomUtil.GetElementText(ReaderDom.DocumentElement,
"tel");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='tel'>"
                + this.GetString("电话")
                + "</td><td class='tel value'>"
+ strTel + "</td></tr>";

            // email
            string strEmail = DomUtil.GetElementText(ReaderDom.DocumentElement,
"email");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='email'>"
                + this.GetString("Email")
                + "</td><td class='email value'>"
+ strEmail + "</td></tr>";


            // 证条码号
            string strBarcode = DomUtil.GetElementText(ReaderDom.DocumentElement,
    "barcode");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='barcode'>"
                + this.GetString("证条码号")
                + "</td><td class='barcode value'>"
+ strBarcode + "</td></tr>";

            // 读者类型
            string strReaderType = DomUtil.GetElementText(ReaderDom.DocumentElement,
"readerType");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='readertype'>"
                + this.GetString("读者类型")
                + "</td><td class='readertype value'>"
+ strReaderType + "</td></tr>";


            // 证状态
            string strState = DomUtil.GetElementText(ReaderDom.DocumentElement,
"state");
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='state'>"
                + this.GetString("证状态")
                + "</td><td class='state value'>"
+ strState + "</td></tr>";

            // 发证日期
            string strCreateDate = DomUtil.GetElementText(ReaderDom.DocumentElement,
                "createDate");
            strCreateDate = DateTimeUtil.LocalDate(strCreateDate);
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='createdate'>"
                + this.GetString("发证日期")
                + "</td><td class='createdate value'>"
+ strCreateDate + "</td></tr>";


            // 证失效期
            string strExpireDate = DomUtil.GetElementText(ReaderDom.DocumentElement,
                "expireDate");
            strExpireDate = DateTimeUtil.LocalDate(strExpireDate);
            strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='expiredate'>"
                + this.GetString("证失效期")
                + "</td><td class='expiredate value'>"
+ strExpireDate + "</td></tr>";


            // 租金 2008/11/11
            string strHireExpireDate = "";
            string strHirePeriod = "";
            XmlNode nodeHire = ReaderDom.DocumentElement.SelectSingleNode("hire");
            if (nodeHire != null)
            {
                strHireExpireDate = DomUtil.GetAttr(nodeHire, "expireDate");
                strHirePeriod = DomUtil.GetAttr(nodeHire, "period");

                strHireExpireDate = DateTimeUtil.LocalDate(strHireExpireDate);
                strHirePeriod = app.GetDisplayTimePeriodStringEx(strHirePeriod);

                strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='hire'>"
                + this.GetString("租金")
                + "</td><td class='hire value'>"
                    + this.GetString("周期")
                + ": " + strHirePeriod + "; "
                + this.GetString("失效期")
                + ": " + strHireExpireDate + "</td></tr>";
            }

            // 押金 2008/11/11
            string strForegift = DomUtil.GetElementText(ReaderDom.DocumentElement,
                "foregift");
            if (String.IsNullOrEmpty(strForegift) == false)
            {
                strResult += "<tr class='" + DarkOrLight(ref index) + "'><td class='foregift'>"
                + this.GetString("押金")
                + "</td><td class='foregift value'>"
                    + strForegift + "</td></tr>";
            }

            strResult += "</table>";

            strResult += this.GetPostfixString();

            output.Write(strResult);

        }
#endif
        /*
         * <personalInfoControl>
         *  <qrImage display="true"/>
         * </personalInfoControl>
         * */
        bool DisplayQrImage
        {
            get
            {
                bool bDefaultValue = false;
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                if (app == null)
                {
                    return bDefaultValue;
                }
                XmlNode nodeItem = app.WebUiDom.DocumentElement.SelectSingleNode(
                    "personalInfoControl/qrImage");

                if (nodeItem == null)
                    return bDefaultValue;

                string strError = "";
                bool bValue;
                DomUtil.GetBooleanParam(nodeItem,
                    "display",
                    bDefaultValue,
                    out bValue,
                    out strError);
                return bValue;
            }
        }


        protected override void CreateChildControls()
        {
            // int index = 0;

            LiteralControl literal = new LiteralControl();
            literal.Text = this.GetPrefixString(
                this.GetString("个人信息"),
                "personalinfo_wrapper");
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl(
                "<table class='personalinfo'>"
                ));

            // 读者识别码
            if (this.DisplayQrImage == true)
            {
                this.Controls.Add(new LiteralControl("<tr class='qrcode'><td class='name'>"
                    + this.GetString("读者识别码")
                    + "</td><td class='value'>"
                                                    ));
                literal = new LiteralControl();
                literal.ID = "qrcode";
                this.Controls.Add(literal);
            }

            // 显示名
            this.Controls.Add(new LiteralControl("<tr class='displayname'><td class='name'>"
                + this.GetString("显示名")
                + "</td><td class='value'>"
                                ));
            TextBox textbox = new TextBox();
            textbox.ID = "displayName";
            this.Controls.Add(textbox);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 姓名
            this.Controls.Add(new LiteralControl("<tr class='person_name'><td class='name'>"
                + this.GetString("姓名")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "name";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 性别
            this.Controls.Add(new LiteralControl("<tr class='gender'><td class='name'>"
                + this.GetString("性别")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "gender";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 出生日期
            this.Controls.Add(new LiteralControl("<tr class='dateOfBirth'><td class='name'>"
                + this.GetString("出生日期")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "dateOfBirth";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 证号 2008/11/11
            // 如果值为空，隐藏
            PlaceHolder holder = new PlaceHolder();
            holder.ID = "cardNumber_holder";
            this.Controls.Add(holder);

            holder.Controls.Add(new LiteralControl("<tr class='cardnumber'><td class='name'>"
                + this.GetString("证号")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "cardNumber";
            holder.Controls.Add(literal);

            holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 身份证号
            this.Controls.Add(new LiteralControl("<tr class='idcardnumber'><td class='name'>"
                + this.GetString("身份证号")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "idCardNumber";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 单位
            this.Controls.Add(new LiteralControl("<tr class='department'><td class='name'>"
                + this.GetString("单位")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "department";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 职务
            this.Controls.Add(new LiteralControl("<tr class='post'><td class='name'>"
                + this.GetString("职务")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "post";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 地址
            this.Controls.Add(new LiteralControl("<tr class='address'><td class='name'>"
                + this.GetString("地址")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "address";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 电话
            this.Controls.Add(new LiteralControl("<tr class='tel'><td class='name'>"
                + this.GetString("电话")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "tel";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // email
            this.Controls.Add(new LiteralControl("<tr class='email'><td class='name'>"
                + this.GetString("Email")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "email";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 证条码号
            this.Controls.Add(new LiteralControl("<tr class='barcode'><td class='name'>"
                + this.GetString("证条码号")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "barcode";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 读者类型
            this.Controls.Add(new LiteralControl("<tr class='readertype'><td class='name'>"
                + this.GetString("读者类型")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "readerType";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 证状态
            this.Controls.Add(new LiteralControl("<tr class='state'><td class='name'>"
                + this.GetString("证状态")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "state";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 发证日期
            this.Controls.Add(new LiteralControl("<tr class='createdate'><td class='name'>"
                + this.GetString("发证日期")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "createDate";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 证失效期
            this.Controls.Add(new LiteralControl("<tr class='expiredate'><td class='name'>"
                + this.GetString("证失效期")
                + "</td><td class='value'>"
                                                ));
            literal = new LiteralControl();
            literal.ID = "expireDate";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 租金 2008/11/11
            holder = new PlaceHolder();
            holder.ID = "hire_holder";
            this.Controls.Add(holder);

            holder.Controls.Add(new LiteralControl("<tr class='hire'><td class='name'>"
            + this.GetString("租金")
            + "</td><td class='value'>"
                                            ));
            literal = new LiteralControl();
            literal.ID = "hire";
            holder.Controls.Add(literal);

            holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 押金 2008/11/11
            holder = new PlaceHolder();
            holder.ID = "foregift_holder";
            this.Controls.Add(holder);

            holder.Controls.Add(new LiteralControl("<tr class='foregift'><td class='name'>"
            + this.GetString("押金")
            + "</td><td class='value'>"
                                        ));
            literal = new LiteralControl();
            literal.ID = "foregift";
            holder.Controls.Add(literal);

            holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 头像
            this.Controls.Add(new LiteralControl("<tr class='photo'><td class='name'>"
+ this.GetString("头像")
+ "</td><td class='value'>"
                            ));

            /*
            literal = new LiteralControl();
            literal.ID = "photo";
            holder.Controls.Add(literal);
             * */

            Image photo = new Image();
            photo.ID = "photo";
            photo.Width = 64;
            photo.Height = 64;
            this.Controls.Add(photo);

            PlaceHolder upload_photo_holder = new PlaceHolder();
            upload_photo_holder.ID = "upload_photo_holder";
            this.Controls.Add(upload_photo_holder);

            literal = new LiteralControl();
            literal.ID = "upload_photo_description";
            literal.Text = this.GetString("上传头像") + ": ";
            upload_photo_holder.Controls.Add(literal);


            FileUpload upload = new FileUpload();
            upload.ID = "upload";
            upload_photo_holder.Controls.Add(upload);

            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            this.Controls.Add(debugline);

            literal = new LiteralControl();
            literal.Text = "<tr class='debugline'><td colspan='2'>";
            debugline.Controls.Add(literal);

            literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            debugline.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            debugline.Controls.Add(literal);

            debugline = null;

            // 命令行
            this.Controls.Add(new LiteralControl("<tr class='cmdline'><td colspan='2'>"));

            // 提交按钮
            Button submit_button = new Button();
            submit_button.ID = "submit";
            submit_button.Text = this.GetString("保存");
            submit_button.CssClass = "submit";
            submit_button.Click += new EventHandler(submit_button_Click);
            this.Controls.Add(submit_button);
            submit_button = null;

            this.Controls.Add(new LiteralControl("</td></tr>"));

            this.Controls.Add(new LiteralControl("</table>" + this.GetPostfixString()));
        }

        static string FindNewFileID(XmlDocument dom)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            // 全部<dprms:file>元素
            XmlNodeList nodes = dom
                .DocumentElement.SelectNodes("//dprms:file", nsmgr);
            if (nodes.Count > 0)
            {
                List<string> ids = new List<string>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    ids.Add(DomUtil.GetAttr(nodes[i], "id"));
                }

                ids.Sort();
                string strLastID = ids[ids.Count - 1];
                try
                {

                    Int64 v = Convert.ToInt64(strLastID);
                    return (v + 1).ToString();
                }
                catch
                {
                    return strLastID + "_1";
                }
            }

            return "0";
        }

        void submit_button_Click(object sender, EventArgs e)
        {
            string strError = "";

            TextBox textbox = (TextBox)this.FindControl("displayName");
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            int nRedoCount = 0;
            bool bUploadSaved = false;

        REDO:
            XmlDocument readerdom = null;
            // 获得当前session中已经登录的读者记录DOM
            // return:
            //      -2  当前登录的用户不是reader类型
            //      -1  出错
            //      0   尚未登录
            //      1   成功
            int nRet = sessioninfo.GetLoginReaderDom(
                out readerdom,
                out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;

            if (nRet == 0)
                goto ERROR1;

            bool bXmlRecordChanged = false;
            FileUpload upload = (FileUpload)this.FindControl("upload");

            string strOldDisplayName = DomUtil.GetElementText(readerdom.DocumentElement,
                "displayName");
            if (textbox.Text == strOldDisplayName
                && upload.HasFile == false)
            {
                strError = "显示名没有发生修改并且也没有新上传头像，放弃保存";
                goto ERROR1;
            }

            if (strOldDisplayName != textbox.Text)
            {
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "displayName",
                    textbox.Text);
                bXmlRecordChanged = true;
            }


            // 如果有上传头像
            if (upload.HasFile == true
                && bUploadSaved == false)
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);

                // 全部<dprms:file>元素
                XmlNodeList nodes = sessioninfo.ReaderInfo.ReaderDom
                    .DocumentElement.SelectNodes("//dprms:file[@usage='photo']", nsmgr);
                string strResTimeStamp = "";
                string strFileID = "0";
                if (nodes.Count > 0)
                {
                    strFileID = DomUtil.GetAttr(nodes[0], "id");
                    if (String.IsNullOrEmpty(strFileID) == true)
                    {
                        strFileID = FindNewFileID(sessioninfo.ReaderInfo.ReaderDom);
                    }
                    strResTimeStamp = DomUtil.GetAttr(nodes[0], "__timestamp");
                }
                else
                {
                    strFileID = FindNewFileID(sessioninfo.ReaderInfo.ReaderDom);

                    XmlNode node = sessioninfo.ReaderInfo.ReaderDom.CreateElement(
                        "dprms:file", DpNs.dprms);
                    sessioninfo.ReaderInfo.ReaderDom.DocumentElement.AppendChild(node);

                    DomUtil.SetAttr(node, "id", strFileID);
                    DomUtil.SetAttr(node, "usage", "photo");
                    bXmlRecordChanged = true;
                }

                if (bXmlRecordChanged == true)
                {

                    sessioninfo.SetLoginReaderDomChanged();
                    // 先要保存一次XML记录，这样才有<dprms:file>元素，为上载资源作好准备
                    // return:
                    //      -2  时间戳冲突
                    //      -1  error
                    //      0   没有必要保存(changed标志为false)
                    //      1   成功保存
                    nRet = sessioninfo.SaveLoginReaderDom(
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == -2)
                    {
                        if (nRedoCount < 10)
                        {
                            nRedoCount++;
                            sessioninfo.ReaderInfo.ReaderDom = null;   // 强迫重新读入
                            goto REDO;
                        }
                        goto ERROR1;
                    }

                    bXmlRecordChanged = false;
                }


                // Stream stream = upload.FileContent;


                // 保存资源
                // 采用了代理帐户
                // return:
                //		-1	error
                //		0	发现上载的文件其实为空，不必保存了
                //		1	已经保存
                nRet = app.SaveUploadFile(
                    this.Page,
                    sessioninfo.ReaderInfo.ReaderDomPath,
                    strFileID,
                    strResTimeStamp,
                    upload.PostedFile,
                    64,
                    64,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                bUploadSaved = true;

                sessioninfo.ReaderInfo.ReaderDom = null;    // 迫使重新装载
                // 因为上载后时间戳改变，需要重新获取
                nRet = sessioninfo.GetLoginReaderDom(
    out readerdom,
    out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;

            }

            // 为显示名的修改而保存
            if (bXmlRecordChanged == true)
            {
                sessioninfo.SetLoginReaderDomChanged();


                // return:
                //      -2  时间戳冲突
                //      -1  error
                //      0   没有必要保存(changed标志为false)
                //      1   成功保存
                nRet = sessioninfo.SaveLoginReaderDom(
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == -2)
                {
                    if (nRedoCount < 10)
                    {
                        nRedoCount++;
                        sessioninfo.ReaderInfo.ReaderDom = null;   // 强迫重新读入
                        goto REDO;
                    }
                    goto ERROR1;
                }

                bXmlRecordChanged = false;
            }

            // 刷新显示
            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        protected override void Render(HtmlTextWriter output)
        {
            int nRet = 0;
            string strError = "";

            // return:
            //      -1  出错
            //      0   成功
            //      1   尚未登录
            nRet = this.LoadReaderXml(out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
            {
                sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                this.Page.Response.Redirect("login.aspx", true);
                return;
            }

            LiteralControl text = null;
            string strBarcode = DomUtil.GetElementText(ReaderDom.DocumentElement,
    "barcode");

            // 显示名
            string strDisplayName = DomUtil.GetElementText(ReaderDom.DocumentElement,
    "displayName");
            TextBox edit = (TextBox)this.FindControl("displayName");
            edit.Text = strDisplayName;

#if NO
            OpacApplication app = (OpacApplication)this.Page.Application["app"];

            if (string.IsNullOrEmpty(strBarcode) == false)
            {
                // 读者证号二维码
                string strCode = "";
                // 获得读者证号二维码字符串
                nRet = app.GetPatronTempId(
                    strBarcode,
                    out strCode,
                    out strError);
                if (nRet == -1)
                {
                    strCode = strError;
                }
                text = (LiteralControl)this.FindControl("qrcode");
                text.Text = strCode;
            }
#endif
            text = (LiteralControl)this.FindControl("qrcode");
            if (text != null)
                text.Text = "<img src='./getphoto.aspx?action=pqri&barcode=" + HttpUtility.UrlEncode(strBarcode) + "' alt='QRCode image'></img>";

            // 姓名
            string strName = DomUtil.GetElementText(ReaderDom.DocumentElement,
                "name");
            text = (LiteralControl)this.FindControl("name");
            text.Text = strName;

            // 性别
            string strGender = DomUtil.GetElementText(ReaderDom.DocumentElement,
                "gender");
            text = (LiteralControl)this.FindControl("gender");
            text.Text = strGender;

            // 出生日期
            string strDateOfBirth = DomUtil.GetElementText(ReaderDom.DocumentElement,
                "dateOfBirth");
            if (string.IsNullOrEmpty(strDateOfBirth) == true)
                strDateOfBirth = DomUtil.GetElementText(ReaderDom.DocumentElement,
   "birthday");

            strDateOfBirth = DateTimeUtil.LocalDate(strDateOfBirth);
            text = (LiteralControl)this.FindControl("dateOfBirth");
            text.Text = strDateOfBirth;

            // 证号 2008/11/11
            string strCardNumber = DomUtil.GetElementText(ReaderDom.DocumentElement,
    "cardNumber");
            if (String.IsNullOrEmpty(strCardNumber) == true)
            {
                PlaceHolder holder = (PlaceHolder)this.FindControl("cardNumber_holder");
                holder.Visible = false;
            }
            else
            {
                text = (LiteralControl)this.FindControl("cardNumber");
                text.Text = strCardNumber;
            }

            // 身份证号
            string strIdCardNumber = DomUtil.GetElementText(ReaderDom.DocumentElement,
    "idCardNumber");
            text = (LiteralControl)this.FindControl("idCardNumber");
            text.Text = strIdCardNumber;

            // 单位
            string strDepartment = DomUtil.GetElementText(ReaderDom.DocumentElement,
"department");
            text = (LiteralControl)this.FindControl("department");
            text.Text = strDepartment;

            // 职务
            string strPost = DomUtil.GetElementText(ReaderDom.DocumentElement,
"post");
            text = (LiteralControl)this.FindControl("post");
            text.Text = strPost;

            // 地址
            string strAddress = DomUtil.GetElementText(ReaderDom.DocumentElement,
"address");
            text = (LiteralControl)this.FindControl("address");
            text.Text = strAddress;

            // 电话
            string strTel = DomUtil.GetElementText(ReaderDom.DocumentElement,
"tel");
            text = (LiteralControl)this.FindControl("tel");
            text.Text = strTel;

            // email
            string strEmail = DomUtil.GetElementText(ReaderDom.DocumentElement,
"email");
            text = (LiteralControl)this.FindControl("email");
            text.Text = strEmail;

            // 证条码号

            text = (LiteralControl)this.FindControl("barcode");
            text.Text = strBarcode;

            // 读者类型
            string strReaderType = DomUtil.GetElementText(ReaderDom.DocumentElement,
"readerType");
            text = (LiteralControl)this.FindControl("readerType");
            text.Text = strReaderType;

            // 证状态
            string strState = DomUtil.GetElementText(ReaderDom.DocumentElement,
"state");
            text = (LiteralControl)this.FindControl("state");
            text.Text = strState;

            // 发证日期
            string strCreateDate = DomUtil.GetElementText(ReaderDom.DocumentElement,
                "createDate");
            strCreateDate = DateTimeUtil.LocalDate(strCreateDate);
            text = (LiteralControl)this.FindControl("createDate");
            text.Text = strCreateDate;

            // 证失效期
            string strExpireDate = DomUtil.GetElementText(ReaderDom.DocumentElement,
                "expireDate");
            strExpireDate = DateTimeUtil.LocalDate(strExpireDate);
            text = (LiteralControl)this.FindControl("expireDate");
            text.Text = strExpireDate;

            // 租金 2008/11/11
            string strHireExpireDate = "";
            string strHirePeriod = "";
            XmlNode nodeHire = ReaderDom.DocumentElement.SelectSingleNode("hire");
            if (nodeHire != null)
            {
                strHireExpireDate = DomUtil.GetAttr(nodeHire, "expireDate");
                strHirePeriod = DomUtil.GetAttr(nodeHire, "period");

                strHireExpireDate = DateTimeUtil.LocalDate(strHireExpireDate);
                strHirePeriod = app.GetDisplayTimePeriodStringEx(strHirePeriod);


                text = (LiteralControl)this.FindControl("hire");
                text.Text = this.GetString("周期")
                + ": " + strHirePeriod + "; "
                + this.GetString("失效期")
                + ": " + strHireExpireDate;
            }
            else
            {
                PlaceHolder holder = (PlaceHolder)this.FindControl("hire_holder");
                holder.Visible = false;
            }

            // 押金 2008/11/11
            string strForegift = DomUtil.GetElementText(ReaderDom.DocumentElement,
                "foregift");
            if (String.IsNullOrEmpty(strForegift) == false)
            {
                text = (LiteralControl)this.FindControl("foregift");
                text.Text = strForegift;
            }
            else
            {
                PlaceHolder holder = (PlaceHolder)this.FindControl("foregift_holder");
                holder.Visible = false;
            }

            Image photo = (Image)this.FindControl("photo");
            photo.ImageUrl = "./getphoto.aspx?barcode=" + strBarcode;

            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            Button submit_button = (Button)this.FindControl("submit");
            PlaceHolder upload_photo_holder = (PlaceHolder)this.FindControl("upload_photo_holder");


            if (loginstate == LoginState.Reader
                && sessioninfo.ReaderInfo.Barcode == strBarcode)
            {
                submit_button.Visible = true;
                upload_photo_holder.Visible = true;
            }
            else
            {
                submit_button.Visible = false;
                upload_photo_holder.Visible = false;
            }

            base.Render(output);
            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        void SetDebugInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)this.FindControl("debugtext");

            text.Text = strText;
        }

        void SetDebugInfo(string strSpanClass,
    string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("debugtext");
            if (strSpanClass == "errorinfo")
                text.Text = "<div class='errorinfo-frame'><div class='" + strSpanClass + "'>" + strText + "</div></div>";
            else
                text.Text = "<div class='" + strSpanClass + "'>" + strText + "</div>";
        }
    }
}
