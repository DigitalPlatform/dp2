using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DigitalPlatform.OPAC.Web;

public partial class ViewPdf : MyWebPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;
        string strPageNo = Request.QueryString["page"];
        if (string.IsNullOrEmpty(strPageNo))
            strPageNo = "1";

        Int32.TryParse(strPageNo, out int nPageNo);
        SetPageNo(nPageNo);
#if NO
        this.PageNo.Value = strPageNo;

        string strURI = Request.QueryString["uri"]; // uri 参数里面是到对象这一级的 URI。更深部分由程序自动生成
        this.Image1.ImageUrl = "./getobject.aspx?uri=" + strURI + "/page:"+strPageNo+",format:png,dpi:75";

        Int32.TryParse(strPageNo, out int nPageNo);
        this.PrevPage.PostBackUrl = "./viewpdf.aspx?uri=" + strURI + "&page=" + (nPageNo - 1);
        this.NextPage.PostBackUrl = "./viewpdf.aspx?uri=" + strURI + "&page=" + (nPageNo - 1);
#endif
    }

    protected void PrevPage_Click(object sender, EventArgs e)
    {
        string strPageNo = this.PageNo.Value;
        Int32.TryParse(strPageNo, out int nPageNo);
        SetPageNo(nPageNo);
    }

    protected void NextPage_Click(object sender, EventArgs e)
    {
        string strPageNo = this.PageNo.Value;
        Int32.TryParse(strPageNo, out int nPageNo);
        SetPageNo(nPageNo);
    }

    protected void FirstPage_Click(object sender, EventArgs e)
    {
        SetPageNo(1);
    }

    protected void TailPage_Click(object sender, EventArgs e)
    {
        string strPageNo = this.PageCount.Value;
        Int32.TryParse(strPageNo, out int nPageNo);
        SetPageNo(0);
    }

    void SetPageNo(int nPageNo)
    {
        this.PageNo.Value = nPageNo.ToString();
        this.LabelPageNo.Text = nPageNo.ToString();

        string strURI = Request.QueryString["uri"]; // uri 参数里面是到对象这一级的 URI。更深部分由程序自动生成
        this.Image1.ImageUrl = "./getobject.aspx?uri=" + strURI + "/page:" + nPageNo.ToString() + ",format:png,dpi:75";

        if (nPageNo > 1)
            this.PrevPage.PostBackUrl = "./viewpdf.aspx?uri=" + strURI + "&page=" + (nPageNo - 1);
        else
            this.PrevPage.Enabled = false;

        this.NextPage.PostBackUrl = "./viewpdf.aspx?uri=" + strURI + "&page=" + (nPageNo + 1);
    }


}
