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
    // 尚在封面图像文件的控件
    [ToolboxData("<{0}:UploadCoverImageControl runat=server></{0}:UploadCoverImageControl>")]
    public class UploadCoverImageControl : WebControl, INamingContainer
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.UploadCoverImageControl.cs",
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

        protected override void CreateChildControls()
        {
            Image photo = new Image();
            photo.ID = "photo";
            photo.Width = 64;
            photo.Height = 64;
            this.Controls.Add(photo);

            PlaceHolder upload_photo_holder = new PlaceHolder();
            upload_photo_holder.ID = "upload_photo_holder";
            this.Controls.Add(upload_photo_holder);

            LiteralControl literal = new LiteralControl();
            literal.ID = "upload_photo_description";
            literal.Text = this.GetString("上传头像") + ": ";
            upload_photo_holder.Controls.Add(literal);


            FileUpload upload = new FileUpload();
            upload.ID = "upload";
            upload_photo_holder.Controls.Add(upload);


        }
    }
}
