using DigitalPlatform.Core;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace DigitalPlatform.DataMining
{
    /// <summary>
    /// 龙源期刊
    /// </summary>
    public class LongyuanQikan
    {
                public static int GetCoverImageToClipboard(
            IWin32Window owner,
string strISSN,
string strYear,
string strIssueNo,
out string strError)
        {
            strError = "";
            int nRet = 0;

            using (BrowserDialog dlg = new BrowserDialog())
            {
                dlg.Show(owner);

#if NO
                int nRet = dlg.LoadPage("http://xxdy.qikan.com/", out strError);
                if (nRet == -1)
                    return -1;

                MessageBox.Show(owner, "homepage OK");
#endif

                //         http://www.qikan.com.cn/searchmagazine.html?k=%e4%b8%89%e8%81%94%e7%94%9f%e6%b4%bb%e5%91%a8%e5%88%8a&t=0
                // string strUrl = "http://xxdy.qikan.com/MagInfo.aspx?issn=1674-3121&year=2015&periodNum=1";
                string strUrl = "http://www.qikan.com.cn/searchmagazine.html?k=" + HttpUtility.UrlEncode(strISSN) + "&t=0";
                nRet = dlg.LoadPage(strUrl, out strError);
                if (nRet == -1)
                    return -1;

#if NO
                for(int i=0;i<1000;i++)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }
#endif

                string guid = dlg.GetQikanGuid();
                if (string.IsNullOrEmpty(guid))
                {
                    strError = "没有找到期刊 GUID";
                    return -1;
                }

                strUrl = "http://www.qikan.com.cn/magdetails/" +guid + "/" + strYear + "/" + strIssueNo + ".html";
                nRet = dlg.LoadPage(strUrl, out strError);
                if (nRet == -1)
                    return -1;

                // MessageBox.Show(owner, "detail url OK");

                if (dlg.CopyImageToClipboard1() == false)
                {
                    strError = "没有找到 img 对象";
                    return -1;
                }
                return 1;
            }
        }

#if NO
        public static int GetCoverImageToClipboard(
            IWin32Window owner,
string strISSN,
string strYear,
string strIssueNo,
out string strError)
        {
            strError = "";
            int nRet = 0;

            using (BrowserDialog dlg = new BrowserDialog())
            {
                dlg.Show(owner);

#if NO
                int nRet = dlg.LoadPage("http://xxdy.qikan.com/", out strError);
                if (nRet == -1)
                    return -1;

                MessageBox.Show(owner, "homepage OK");
#endif

                // string strUrl = "http://xxdy.qikan.com/MagInfo.aspx?issn=1674-3121&year=2015&periodNum=1";
                string strUrl = "http://xxdy.qikan.com/MagInfo.aspx?issn=" + strISSN + "&year=" + strYear + "&periodNum=" + strIssueNo;
                nRet = dlg.LoadPage(strUrl, out strError);
                if (nRet == -1)
                    return -1;

                // MessageBox.Show(owner, "long url OK");
                if (dlg.CopyImageToClipboard() == false)
                {
                    strError = "没有找到 img 对象";
                    return -1;
                }
                return 1;
            }
        }

#endif


        public static int GetCoverImageUrl(
            IWin32Window owner,
            string strISSN,
            string strYear,
            string strIssueNo,
            ref CookieContainer cookie,
            out string strImageUrl,
            out string strError)
        {
            strError = "";
            strImageUrl = "";

            string strUrl = "http://xxdy.qikan.com/MagInfo.aspx?issn=" + strISSN + "&year=" + strYear + "&periodNum=" + strIssueNo;

            /*
   Connection: Keep-Alive
   Cookie: xxdy=Default|Default_blue|1674-3121|%e4%b8%ad%e5%b0%8f%e5%ad%a6%e5%be%b7%e8%82%b2|2016|8|xxdy|459|||%e5%b0%8f%e5%ad%a6%e5%be%b7%e8%82%b2%e7%bd%91%e7%ab%99%3ahttp%3a%2f%2fxxdy.qikan.com|2016|9|True|ãä¸­å°å­¦å¾·è²ãæ¯ç±å½å®¶æè²é¨å§æååå¸èå¤§å­¦ä¸»åçä¸æ¬å
¨é¢åæ ä¸­å°å­¦å¾·è²å·¥ä½çä¸ä¸æåï¼åæ¶ä½ä¸ºä¸­å½æè²å­¦ä¼ä¸­å°å­¦å¾·è²ç ç©¶åä¼ä¼åãæ¬åè´åäºä¸ºä¸­å°å­¦å¾·è²çè®ºç ç©¶ä¸å®è·µå·¥ä½è
æä¾ææ°çå¾·è²æ¹é©å¨åãæå¨çå¾·è²æ¿ç­æå¼ãæ°éçå¾·è²ç ç©¶ææãé²æ´»çå¾·è²å®è·µç»éªï¼åäºæä¸ºä¸­å°å­¦å¾·è²æ¹é©çâé£åæ âï¼éææ¹é©çâæå¤´å
µâï¼å¾·è²å·¥ä½è
äº¤æµç»éªãæ¢ç´¢å¾·è²è§å¾çâå¤§èå°âï¼å¼é¢å¾·è²è¡æ¿é¨é¨ãæç é¨é¨åå¹¿å¤§å¾·è²æå¸çâåè°é¨âã%0d%0aç¼ è¾ é¨ï¼020-85215129  85211209%0d%0aå è¡ é¨ï¼020-85215179  85211443ï¼ä¼ çï¼%0d%0açµå­é®ç®±ï¼zxxdy@vip.163.comï¼æç¨¿ï¼%0d%0a          zxxdydingyue@163.comï¼è®¢é
ï¼%0d%0aåå®¢ï¼http://blog.sina.com.cn/s/articlelist_2734759432_0_1.html
            */
            if (cookie == null)
                cookie = new CookieContainer();
            WebClientEx webClient = new WebClientEx(cookie);

#if NO
            {
                byte[] byteArray = x.DownloadData(new Uri("http://xxdy.qikan.com"));
            }

            x = new WebClientEx(cookie);
#endif

            webClient.Headers.Add("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
            webClient.Headers.Add("Accept-Encoding", "gzip, deflate");

            webClient.Headers.Add("Accept-Language", "zh-Hans-CN, zh-Hans; q=0.8, en-US; q=0.5, en; q=0.3");
            //    Host: xxdy.qikan.com
            webClient.Headers.Add("Host", "xxdy.qikan.com");
            // x.BaseAddress = "xxdy.qikan.com";
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393");
            try
            {
#if NO
                byte[] byteArray = x.DownloadData(new Uri(strUrl));
                Stream stream = new MemoryStream(byteArray);
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.OptionFixNestedTags = true;
                htmlDoc.Load(stream, true);


                // ParseErrors is an ArrayList containing any errors from the Load statement
                if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                {
                    // Handle any parse errors as required
                    strError = "parse html error: " + htmlDoc.ParseErrors.ToString();
                    return -1;
                }
#endif
                byte[] byteArray = webClient.DownloadData(new Uri(strUrl));
                string strContent = Encoding.UTF8.GetString(byteArray);
                // string strContent = x.DownloadString(new Uri(strUrl));
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.OptionFixNestedTags = true;
                htmlDoc.LoadHtml(strContent);

#if NO
                // ParseErrors is an ArrayList containing any errors from the Load statement
                if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                {
                    // Handle any parse errors as required
                    strError = "parse html error: " + htmlDoc.ParseErrors.ToString();
                    return -1;
                }
#endif

                if (htmlDoc.DocumentNode == null)
                {
                    strError = "htmlDoc.DocumentNode == null";
                    return -1;
                }

                /*
          <div class="left1">
              <!--最新封面开始-->
          

             <div class="cover1">
                 <h1>封面</h1>
                 <div class="cover1_box">
                     <a href="../../MagInfo.aspx?issn=1674-3121&year=2013&periodNum=7"><img src="http://img.qikan.com.cn/qkimages/xxdy/xxdy201307-l.jpg" width="190" height="270" border="0" alt="2013年第7期" /></a>
                     <span class="f14 fBold"><a href="MagInfo.aspx?issn=1674-3121&year=2013&periodNum=7" title="2013年第7期">2013年第7期</a>
                     </span>
                 * 
                 * */

#if NO
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//img");
                foreach(HtmlNode node in nodes)
                {
                    string src1 = node.GetAttributeValue("src", "");
                    int i = 0;
                    i++;
                }
#endif

                HtmlAgilityPack.HtmlNode cover1_box = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='cover1_box']");

                if (cover1_box == null)
                {
                    strError = "cover1_box 没有找到";
                    return -1;
                }

                HtmlNode img = cover1_box.SelectSingleNode("*/img");
                string src = img.GetAttributeValue("src", "");
                strImageUrl = src;
                return 1;
            }
            catch (Exception ex)
            {
                strError = "异常: " + ex.Message;
                return -1;
            }
        }

        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public static int DownloadImageFile(string url,
            string strLocalFileName,
            ref CookieContainer cookie,
            out string strError)
        {
            strError = "";
            if (cookie == null)
                cookie = new CookieContainer();

            using (MyWebClient webClient = new MyWebClient())
            {
#if NO
                webClient.Headers.Add("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
                webClient.Headers.Add("Accept-Encoding", "gzip, deflate");
                webClient.Headers.Add("Accept-Language", "zh-Hans-CN, zh-Hans; q=0.8, en-US; q=0.5, en; q=0.3");
#endif
                webClient.CookieContainer = cookie;

                webClient.Headers.Add("Host", "img.qikan.com.cn");
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393");

                webClient.Timeout = 5000;
                // webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);

                try
                {
                    webClient.DownloadFile(new Uri(url, UriKind.Absolute), strLocalFileName);
                    return 1;
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                strError = ex.Message;
                                return 0;
                            }
                        }
                    }

                    strError = ex.Message;
                    return -1;
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }
        }

    }

    class MyWebClient : WebClient
    {
        public int Timeout = -1;

        HttpWebRequest _request = null;

        public CookieContainer CookieContainer
        {
            get { return container; }
            set { container = value; }
        }

        private CookieContainer container = new CookieContainer();


        protected override WebRequest GetWebRequest(Uri address)
        {
            _request = (HttpWebRequest)base.GetWebRequest(address);

#if NO
            this.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
#endif
            if (this.Timeout != -1)
                _request.Timeout = this.Timeout;

                _request.CookieContainer = this.container;

            return _request;
        }

        public void Cancel()
        {
            if (this._request != null)
                this._request.Abort();
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }

}
