using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.IO;

namespace dp2Installer
{
    /// <summary>
    /// 下载文件实用函数
    /// </summary>
    public static class DownloadUtility
    {
        /*
        static string GetUtilDir(string strBinDir)
        {
            return Path.Combine(Path.GetDirectoryName(strBinDir), "~" + Path.GetFileName(strBinDir) + "_greenutility");
        }
        */

        public class GetServerFileInfoResult : NormalResult
        {
            public long ContentLength { get; set; }
            // 2020/9/1
            public DateTime LastModified { get; set; }

            // 2020/9/1
            public string DebugInfo { get; set; }

            public override string ToString()
            {
                return base.ToString() + $",ContentLength={ContentLength}, LastModified={LastModified.ToString()}, DebugInfo={DebugInfo}";
            }
        }

        // 判断 http 服务器上一个文件是否已经更新
        // parameters:
        //      local_lastmodify    本地文件最后修改时间
        //      local_filelength    本地文件尺寸
        // return:
        //      -1  出错
        //      0   没有更新
        //      1   已经更新
        public static async Task<GetServerFileInfoResult> GetServerFileInfo(string strUrl,
            DateTime local_lastmodify,
            long local_fileLength)
        {
            StringBuilder debugInfo = new StringBuilder();

            debugInfo?.AppendLine($"调用参数: strUrl={strUrl},local_lastmodify={local_lastmodify.ToString()}");

            var webRequest = System.Net.WebRequest.Create(strUrl);

            // 2020/8/27
            // Define a cache policy for this request only.
            HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            webRequest.CachePolicy = noCachePolicy;

            webRequest.Method = "HEAD";
            webRequest.Timeout = 5000;
            try
            {
                using (var response = await webRequest.GetResponseAsync() as HttpWebResponse)
                {
                    string strContentLength = response.GetResponseHeader("Content-Length");
                    debugInfo?.AppendLine($"strContentLength={strContentLength}");

                    if (Int64.TryParse(strContentLength, out long contentLength) == false)
                        contentLength = -1;

                    string strLastModified = response.GetResponseHeader("Last-Modified");
                    debugInfo?.AppendLine($"strLastModified={strLastModified}");
                    if (string.IsNullOrEmpty(strLastModified) == true)
                    {
                        return new GetServerFileInfoResult
                        {
                            Value = -1,
                            ContentLength = contentLength,
                            ErrorInfo = "header 中无法获得 Last-Modified 值"
                        };
                    }

                    if (DateTimeUtil.TryParseRfc1123DateTimeString(strLastModified, out DateTime time) == false)
                    {
                        return new GetServerFileInfoResult
                        {
                            Value = -1,
                            ContentLength = contentLength,
                            ErrorInfo = $"从响应中取出的 Last-Modified 字段值 '{ strLastModified}' 格式不合法"
                        };
                    }

                    if (time > local_lastmodify)
                    {
                        debugInfo?.AppendLine($"服务器一端的文件时间 {time.ToString()} 晚于本地文件时间 {local_lastmodify.ToString()}");
                        return new GetServerFileInfoResult
                        {
                            Value = 1,
                            ContentLength = contentLength,
                            LastModified = time,
                            DebugInfo = debugInfo?.ToString()
                        };
                    }

                    if (contentLength != local_fileLength)
                    {
                        debugInfo?.AppendLine($"服务器一端的文件尺寸 {contentLength} 不同于本地文件尺寸 {local_fileLength}");
                        return new GetServerFileInfoResult
                        {
                            Value = 1,
                            ContentLength = contentLength,
                            LastModified = time,
                            DebugInfo = debugInfo?.ToString()
                        };
                    }

                    debugInfo?.AppendLine($"服务器一端的文件和本地文件的最后修改时间、尺寸均未发现不同");

                    return new GetServerFileInfoResult
                    {
                        Value = 0,
                        ContentLength = contentLength,
                        LastModified = time,
                        DebugInfo = debugInfo?.ToString()
                    };
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return new GetServerFileInfoResult
                        {
                            Value = -1,
                            ErrorInfo = ex.Message,
                            ErrorCode = "notFound"
                        };
                    }
                }
                return new GetServerFileInfoResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = ex.GetType().ToString()
                };
            }
            catch (Exception ex)
            {
                return new GetServerFileInfoResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message, // ExceptionUtil.GetAutoText(ex),
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        class MyWebClient : WebClient
        {
            public int Timeout = -1;
            public int ReadWriteTimeout = -1;

            HttpWebRequest _request = null;

            protected override WebRequest GetWebRequest(Uri address)
            {
                _request = (HttpWebRequest)base.GetWebRequest(address);

#if NO
            this.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
#endif
                if (this.Timeout != -1)
                    _request.Timeout = this.Timeout;
                if (this.ReadWriteTimeout != -1)
                    _request.ReadWriteTimeout = this.ReadWriteTimeout;
                return _request;
            }

            public void Cancel()
            {
                if (this._request != null)
                    this._request.Abort();
            }
        }

        public delegate void Delegate_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e);
        public delegate void Delegate_Abort();

        // 从 http 服务器下载一个文件
        // 阻塞式
        public static async Task<NormalResult> DownloadFileAsync(string strUrl,
    string strLocalFileName,
    CancellationToken token,
    Delegate_DownloadProgressChanged progressChanged,
    Delegate_Abort abort)
        {
            using (MyWebClient webClient = new MyWebClient())
            {
                if (progressChanged != null)
                    webClient.DownloadProgressChanged += (o, e) =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            webClient.Cancel();
                            abort?.Invoke();
                        }
                        progressChanged(o, e);
                    };

                webClient.ReadWriteTimeout = 30 * 1000; // 30 秒，在读写之前 - 2015/12/3
                webClient.Timeout = 30 * 60 * 1000; // 30 分钟，整个下载过程 - 2015/12/3
                webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                string strTempFileName = strLocalFileName + ".temp";

                // 2020/6/4
                PathUtil.TryCreateDir(Path.GetDirectoryName(strTempFileName));

                // TODO: 先下载到临时文件，然后复制到目标文件
                try
                {
                    await webClient.DownloadFileTaskAsync(new Uri(strUrl, UriKind.Absolute), strTempFileName);

                    File.Delete(strLocalFileName);
                    File.Move(strTempFileName, strLocalFileName);
                    return new NormalResult();
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
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = $"{strUrl} 不存在 ({ex.Message})",
                                    ErrorCode = "notFound"
                                };
                            }
                        }
                    }

                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = ex.Message, //ExceptionUtil.GetDebugText(ex),
                        ErrorCode = ex.GetType().ToString()
                    };
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = ex.Message, // ExceptionUtil.GetDebugText(ex),
                        ErrorCode = ex.GetType().ToString()
                    };
                }
            }
        }

        private static void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
