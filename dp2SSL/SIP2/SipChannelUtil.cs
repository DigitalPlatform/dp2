using DigitalPlatform;
using DigitalPlatform.SIP2;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using DigitalPlatform.Xml;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static dp2SSL.LibraryChannelUtil;

namespace dp2SSL
{
    /// <summary>
    /// 和 SIP2 通道有关的功能
    /// </summary>
    public static class SipChannelUtil
    {
        static SipChannel _channel = new SipChannel(Encoding.UTF8);

        static async Task<SipChannel> GetChannelAsync()
        {
            var parts = StringUtil.ParseTwoPart(App.SipServerUrl, ":");
            string address = parts[0];
            string port = parts[1];

            if (Int32.TryParse(port, out int port_value) == false)
                throw new Exception($"SIP 服务器和端口号字符串 '{App.SipServerUrl}' 中端口号部分 '{port}' 格式错误");

            var result = await _channel.ConnectionAsync(address,
                port_value);
            if (result.Value == -1) // 出错
                throw new Exception($"连接 SIP 服务器 {App.SipServerUrl} 时出错: {result.ErrorInfo}");

            // TODO: 按需登录？避免反复登录
            var login_result = await _channel.LoginAsync(App.SipUserName,
                App.SipPassword);
            if (login_result.Value == -1)
                throw new Exception($"针对 SIP 服务器 {App.SipServerUrl} 登录出错: {login_result.ErrorInfo}");

            return _channel;
        }

        static void ReturnChannel(SipChannel channel)
        {

        }

        static AsyncSemaphore _channelLimit = new AsyncSemaphore(1);


        // 获得册记录信息和书目摘要信息
        // parameters:
        //      style   风格。network 表示只从网络获取册记录；否则优先从本地获取，本地没有再从网络获取册记录。无论如何，书目摘要都是尽量从本地获取
        // .Value
        //      0   没有找到
        //      1   找到
        public static async Task<GetEntityDataResult> GetEntityDataAsync(string pii,
            string style)
        {
            bool network = StringUtil.IsInList("network", style);
            try
            {
                using (var releaser = await _channelLimit.EnterAsync())
                {

                    SipChannel channel = await GetChannelAsync();
                    try
                    {
                        GetEntityDataResult result = null;
                        List<NormalResult> errors = new List<NormalResult>();

                        EntityItem entity_record = null;

                        // ***
                        // 第一步：获取册记录

                        {
                            // 再尝试从 dp2library 服务器获取
                            // TODO: ItemXml 和 BiblioSummary 可以考虑在本地缓存一段时间
                            int nRedoCount = 0;
                        REDO_GETITEMINFO:
                            var get_result = await _channel.GetItemInfoAsync("", pii);
                            if (get_result.Value == -1)
                                errors.Add(new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = get_result.ErrorInfo,
                                    ErrorCode = get_result.ErrorCode
                                });
                            else
                            {
                                XmlDocument itemdom = new XmlDocument();
                                itemdom.LoadXml("<root />");
                                DomUtil.SetElementText(itemdom.DocumentElement,
                                    "barcode",
                                    get_result.Result.AB_ItemIdentifier_r);
                                DomUtil.SetElementText(itemdom.DocumentElement,
    "location",
    get_result.Result.AQ_PermanentLocation_o);
                                DomUtil.SetElementText(itemdom.DocumentElement,
"currentLocation",
get_result.Result.AP_CurrentLocation_o);

                                result = new GetEntityDataResult
                                {
                                    Value = 1,
                                    ItemXml = itemdom.OuterXml,
                                    ItemRecPath = get_result.Result.AB_ItemIdentifier_r,
                                    Title = get_result.Result.AJ_TitleIdentifier_r,
                                };

                                /*
                                // 保存到本地数据库
                                await AddOrUpdateAsync(context, new EntityItem
                                {
                                    PII = pii,
                                    Xml = item_xml,
                                    RecPath = item_recpath,
                                    Timestamp = timestamp,
                                });
                                */
                            }
                        }

                        // ***
                        /// 第二步：获取书目摘要

                        // 完全成功
                        if (result != null && errors.Count == 0)
                            return result;
                        if (result == null)
                            return new GetEntityDataResult
                            {
                                Value = errors[0].Value,
                                ErrorInfo = errors[0].ErrorInfo,
                                ErrorCode = errors[0].ErrorCode
                            };
                        result.ErrorInfo = errors[0].ErrorInfo;
                        result.ErrorCode = errors[0].ErrorCode;
                        return result;
                    }
                    finally
                    {
                        ReturnChannel(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"GetEntityDataAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"GetEntityDataAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }
    }
}
