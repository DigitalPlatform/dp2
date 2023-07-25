using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.Text;

namespace CompactShelfDriver.First
{
    public static class Driver
    {
        // parameters:
        //      addr    地址。至少 4 字符。例如 "101B" 表示一区，第一架，A 面
        // result.Value:
        //      -1  出错
        //      0   厂家报错失败
        //      1   厂家成功
        public static async Task<NormalResult> OpenColumn(
    string serverUrl,
    string addr
    /*
    string area,
    string column*/)
        {
            // 检查输入参数
            if (string.IsNullOrEmpty(serverUrl))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "服务器地址不允许为空",
                    ErrorCode = "argumentError"
                };
            }

            if (string.IsNullOrEmpty(addr))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "地址不允许为空",
                    ErrorCode = "argumentError"
                };
            }

            /*
            if (string.IsNullOrEmpty(area))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "区编号不允许为空",
                    ErrorCode = "argumentError"
                };
            }
            if (string.IsNullOrEmpty(column))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "架编号不允许为空",
                    ErrorCode = "argumentError"
                };
            }
            */

            if (addr.Length < 3)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "地址格式不合法。应为 4 字符(或以上)",
                    ErrorCode = "argumentError"
                };
            }

            // 解析 addr
            string area = addr.Substring(0, 1);

            if (StringUtil.IsPureNumber(area) == false)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"地址 '{addr}' 中区号部分 '{area}' 不合法。应为纯数字",
                    ErrorCode = "argumentError"
                };
            }

            string shelf = addr.Substring(1, 2);

            if (StringUtil.IsPureNumber(shelf) == false)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"地址 '{addr}' 中架号部分 '{shelf}' 不合法。应为纯数字",
                    ErrorCode = "argumentError"
                };
            }

            string side = addr.Substring(3, 1).ToUpper();

            if (side != "A" && side != "B")
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"地址 '{addr}' 中 A B 面部分 '{side}' 不合法。应为 A 或者 B",
                    ErrorCode = "argumentError"
                };
            }

            // 把 shelf 和 side 换算为通道号。通道号从 1 开始计数
            // 1B-->1 2A-->1 2B-->2 3A-->2 3B-->3 ...
            var shelf_number = Convert.ToInt32(shelf);
            if (side.ToUpper() == "A")
                shelf_number --;

            // 2023/7/7
            // 如果是 1 区或者 3 区，则还需要把通道号倒转过来。1-12 倒转为 12-1
            if (area == "3" || area == "1")
            {
                if (shelf_number < 1 || shelf_number > 12)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"shelf_number '{shelf_number}' 越出合法范围 1~12 (addr='{addr}')",
                        ErrorCode = "argumentError"
                    };

                shelf_number = 13 - shelf_number;
            }

            return await Command(serverUrl,
                "OpenColumn",
                area + "," + shelf_number.ToString());
        }

        // result.Value:
        //      -1  出错
        //      0   厂家报错失败
        //      1   厂家成功
        public static async Task<NormalResult> CloseArea(
string serverUrl,
string addr)
        {
            // 检查输入参数
            if (string.IsNullOrEmpty(serverUrl))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "服务器地址不允许为空",
                    ErrorCode = "argumentError"
                };
            }
            if (string.IsNullOrEmpty(addr))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "区编号不允许为空",
                    ErrorCode = "argumentError"
                };
            }

            string area = addr.Substring(0, 1);

            return await Command(serverUrl,
                "CloseArea",
                area);
        }

        public static async Task<NormalResult> Command(
            string serverUrl,
            string api,
            string location)
        {
            try
            {
                // 检查输入参数
                if (string.IsNullOrEmpty(serverUrl))
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "服务器地址不允许为空",
                        ErrorCode = "argumentError"
                    };
                }
                if (string.IsNullOrEmpty(api))
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "API 名称不允许为空",
                        ErrorCode = "argumentError"
                    };
                }

                // 调接口

                using (HttpClient httpClient = new HttpClient())
                {
                    param p = new param()
                    {
                        location = location,
                    };
                    string strquest = JsonConvert.SerializeObject(p);

                    // this.textBox_result.Text = "OpenColumn请求信息:" + strquest + "\r\n\r\n";
                    HttpContent content = new StringContent(strquest);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(
                        GetMethodUrl(serverUrl, api),
                        content);   // .Result;

                    response.EnsureSuccessStatusCode();//用来抛异常的

                    //===
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    // this.textBox_result.Text += "返回结果:[" + responseBody + "]\r\n";
                    /*
                    this.textBox_result.Text += "\r\n比较==字符串1的结果";
                    if (responseBody == "1")
                        this.textBox_result.Text += "\r\n成功。";
                    else
                        this.textBox_result.Text += "\r\n失败。";
                    //====
                    this.textBox_result.Text += "\r\n比较==字符串\"1\"的结果";
                    if (responseBody == "\"1\"")
                        this.textBox_result.Text += "\r\n成功。";
                    else
                        this.textBox_result.Text += "\r\n失败。";
                    */

                    return new NormalResult
                    {
                        Value = responseBody == "\"1\"" ? 1 : 0,
                        ErrorInfo = responseBody == "\"1\"" ? "设备成功" : "设备失败",
                        ErrorCode = responseBody
                    };
                }
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"请求服务器 '{serverUrl}' 发生异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString(),
                };
            }
        }

        public class param
        {
            // 区
            public string location { get; set; }
        }

        static string GetMethodUrl(string strServerUrl, string strMethod)
        {
            if (string.IsNullOrEmpty(strServerUrl) == true)
                return strMethod;

            if (strServerUrl[strServerUrl.Length - 1] == '/')
                return strServerUrl + strMethod;

            return strServerUrl + "/" + strMethod;
        }
    }
}
