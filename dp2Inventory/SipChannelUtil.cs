using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;
using System.Deployment.Application;

using Microsoft.VisualStudio.Threading;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;
using DigitalPlatform.SIP2;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using static dp2Inventory.LibraryChannelUtil;
using DigitalPlatform.CirculationClient;

namespace dp2Inventory
{
    /// <summary>
    /// 和 SIP2 通道有关的功能
    /// </summary>
    public static class SipChannelUtil
    {
        static SipChannel _channel = new SipChannel(Encoding.UTF8);

        public static string DateFormat = "yyyy-MM-dd";

        static async Task<SipChannel> GetChannelAsync()
        {
            if (_channel.Connected == false)
            {
                _channel.Encoding = Encoding.GetEncoding(DataModel.sipEncoding);

                var result = await ConnectAsync();
                if (result.Value == -1) // 出错
                {
                    // TryDetectSipNetwork();
                    throw new Exception($"连接 SIP 服务器 {DataModel.sipServerAddr}:{DataModel.sipServerPort} 时出错: {result.ErrorInfo}");
                }

                if (string.IsNullOrEmpty(DataModel.sipUserName) == false)
                {
                    var login_result = await _channel.LoginAsync(DataModel.sipUserName,
                        DataModel.sipPassword);
                    if (login_result.Value == -1)
                        throw new Exception($"针对 SIP 服务器 {DataModel.sipServerAddr}:{DataModel.sipServerPort} 登录出错: {login_result.ErrorInfo}");
                }

                // TODO: ScStatus()

            }

            return _channel;
        }

        static void ReturnChannel(SipChannel channel)
        {

        }

        public static void CloseChannel()
        {
            _channel.Close();
        }

        static async Task<NormalResult> ConnectAsync()
        {
            string address = DataModel.sipServerAddr;
            int port_value = DataModel.sipServerPort;

            var result = await _channel.ConnectAsync(address,
    port_value);
            if (result.Value == -1) // 出错
            {
                // TryDetectSipNetwork();
                // throw new Exception($"连接 SIP 服务器 {App.SipServerUrl} 时出错: {result.ErrorInfo}");
            }

            return result;
        }

        static AsyncSemaphore _channelLimit = new AsyncSemaphore(1);

        public static async Task<NormalResult> UpdateItemStatusAsync(
            string oi,
            string pii,
            string location,
            string currentLocation,
            string shelfNo,
            string currentShelfNo)
        {
            string filter_oi = DataModel.sipInstitution;
            if (string.IsNullOrEmpty(filter_oi) == false)
            {
                if (oi != filter_oi)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"标签的 OI '{oi}' 不符合定义 '{filter_oi}'，修改册记录状态被(dp2ssl)拒绝",
                        ErrorCode = "oiMismatch"
                    };
            }

            try
            {
                using (var releaser = await _channelLimit.EnterAsync())
                {
                    SipChannel channel = await GetChannelAsync();
                    try
                    {
                        var result = await _channel.ItemStatusUpdateAsync(
    oi,
    pii,
    location,
    currentLocation,
    shelfNo,
    currentShelfNo);
                        if (result.Value == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = result.ErrorCode
                            };
                        if (result.Result.ItemPropertiesOk_1 != "1")
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.Result.AF_ScreenMessage_o,
                                ErrorCode = "sipErrorCode:" + result.Result.ItemPropertiesOk_1
                            };
                        }
                        return new NormalResult();
                    }
                    finally
                    {
                        ReturnChannel(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"UpdateItemStatusAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"GetEntityDataAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        public class GetLocalEntityDataResult : GetEntityDataResult
        {
            // 返回本地数据库记录对象
            public BookItem BookItem { get; set; }
        }

        // 获得册记录信息和书目摘要信息
        // parameters:
        //      style   风格。network 表示只从网络获取册记录；否则优先从本地获取，本地没有再从网络获取册记录。无论如何，书目摘要都是尽量从本地获取
        //              updateItemTitle 表示用从 SIP 服务器获得的题名刷新(从本地数据库中获得的) BookItem 中的 Title 成员
        // .Value
        //      0   没有找到
        //      1   找到
        public static async Task<GetLocalEntityDataResult> GetEntityDataAsync(
            string pii,
            string oi,
            string style)
        {
            string filter_oi = DataModel.sipInstitution;
            if (string.IsNullOrEmpty(filter_oi) == false)
            {
                if (oi != filter_oi)
                    return new GetLocalEntityDataResult
                    {
                        Value = -1,
                        ErrorInfo = $"标签的 OI '{oi}' 不符合定义 '{filter_oi}'，获取册记录被(dp2Inventory)拒绝",
                        ErrorCode = "oiMismatch"
                    };
            }

            bool network = StringUtil.IsInList("network", style);
            bool updateItemTitle = StringUtil.IsInList("updateItemTitle", style);
            try
            {
                using (var releaser = await _channelLimit.EnterAsync())
                {

                    SipChannel channel = await GetChannelAsync();
                    try
                    {
                        BookItem book_item = null;
                        GetLocalEntityDataResult result = null;
                        List<NormalResult> errors = new List<NormalResult>();

                        // ***
                        // 第一步：获取册记录

                        {
                            // 再尝试从 dp2library 服务器获取
                            // TODO: ItemXml 和 BiblioSummary 可以考虑在本地缓存一段时间
                            int nRedoCount = 0;
                        REDO_GETITEMINFO:
                            var get_result = await _channel.GetItemInfoAsync(oi, pii);
                            if (get_result.Value == -1)
                                errors.Add(new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = get_result.ErrorInfo,
                                    ErrorCode = get_result.ErrorCode
                                });
#if NO
                            else if (get_result.Result.CirculationStatus_2 == "01")
                            {
                                errors.Add(new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = get_result.Result.AF_ScreenMessage_o,
                                    ErrorCode = get_result.Result.CirculationStatus_2
                                });
                            }
                            else if (get_result.Result.CirculationStatus_2 == "13"
                                || string.IsNullOrEmpty(get_result.Result.AB_ItemIdentifier_r))
                            {
                                errors.Add(new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = get_result.Result.AF_ScreenMessage_o,
                                    ErrorCode = "itemNotFound"
                                });
                            }
#endif
                            else if (string.IsNullOrEmpty(get_result.Result.AB_ItemIdentifier_r))
                            {
                                errors.Add(new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = get_result.Result.AF_ScreenMessage_o,
                                    ErrorCode = "itemNotFound"
                                });
                            }
                            else
                            {
                                XmlDocument itemdom = new XmlDocument();
                                itemdom.LoadXml("<root />");

                                string state = ConvertItemState(get_result.Result.CirculationStatus_2, get_result.Result.AF_ScreenMessage_o);

                                // 册状态
                                DomUtil.SetElementText(itemdom.DocumentElement,
                                    "state",
                                    state);

                                // 册条码号
                                DomUtil.SetElementText(itemdom.DocumentElement,
                                    "barcode",
                                    get_result.Result.AB_ItemIdentifier_r);

                                // 2021/1/31
                                // OI
                                if (string.IsNullOrEmpty(oi) == false)
                                    DomUtil.SetElementText(itemdom.DocumentElement,
    "oi",
    oi);

                                // 永久馆藏地
                                DomUtil.SetElementText(itemdom.DocumentElement,
                                    "location",
                                    get_result.Result.AQ_PermanentLocation_o);
                                // 永久架位
                                if (string.IsNullOrEmpty(get_result.Result.KQ_PermanentShelfNo_o) == false)
                                    DomUtil.SetElementText(itemdom.DocumentElement,
                                        "shelfNo",
                                        get_result.Result.KQ_PermanentShelfNo_o);
                                // 当前位置
                                string currentLocation = BuildCurrentLocation(
        get_result.Result.AP_CurrentLocation_o,
        get_result.Result.KP_CurrentShelfNo_o);
                                if (string.IsNullOrEmpty(currentLocation) == false)
                                    DomUtil.SetElementText(itemdom.DocumentElement,
        "currentLocation",
        currentLocation);

                                // 索取号
                                DomUtil.SetElementText(itemdom.DocumentElement,
                                    "accessNo",
                                    get_result.Result.KC_CallNo_o);    // 原来是 .CH_ItemProperties_o

                                // 借书时间
                                {
                                    string borrowDateString = get_result.Result.CM_HoldPickupDate_18;
                                    if (string.IsNullOrEmpty(borrowDateString) == false)
                                    {
                                        if (DateTime.TryParseExact(borrowDateString,
                                        "yyyyMMdd    HHmmss",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out DateTime borrowDate))
                                        {
                                            DomUtil.SetElementText(itemdom.DocumentElement,
            "borrowDate",
            DateTimeUtil.Rfc1123DateTimeStringEx(borrowDate));

                                            DomUtil.SetElementText(itemdom.DocumentElement,
"borrower",
"***");
                                        }
                                        else
                                        {
                                            // 报错，时间字符串格式错误，无法解析
                                        }
                                    }
                                }

                                // 应还书时间
                                {
                                    string returnningDateString = get_result.Result.AH_DueDate_o;
                                    if (string.IsNullOrEmpty(returnningDateString) == false)
                                    {
                                        if (DateTime.TryParseExact(returnningDateString,
                                        DateFormat,
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out DateTime returningDate))
                                        {
                                            DomUtil.SetElementText(itemdom.DocumentElement,
            "returningDate",
            DateTimeUtil.Rfc1123DateTimeStringEx(returningDate));
                                        }
                                        else
                                        {
                                            // 报错，时间字符串格式错误，无法解析
                                        }
                                    }
                                }

                                // 2020/1/19
                                // 题名也放入 ItemXml 中
                                DomUtil.SetElementText(itemdom.DocumentElement,
                                    "title",
                                    get_result.Result.AJ_TitleIdentifier_r);

                                // 2021/1/17
                                // 用本地数据记录修正盘点部分字段
                                if (StringUtil.IsInList("localInventory", style))
                                {
                                    book_item = await DataModel.FindBookItemAsync(DataModel.MakeOiPii(pii, oi));
                                    if (book_item != null)
                                    {
                                        if (book_item.Location != null)
                                            DomUtil.SetElementText(itemdom.DocumentElement,
                                                "location",
                                                book_item.Location);
                                        if (book_item.ShelfNo != null)
                                            DomUtil.SetElementText(itemdom.DocumentElement,
                                                "shelfNo",
                                                book_item.ShelfNo);

                                        string old_currentLocation = DomUtil.GetElementText(itemdom.DocumentElement,
                                            "currentLocation");
                                        string oldLeft = null;
                                        string oldRight = null;
                                        if (string.IsNullOrEmpty(old_currentLocation) == false)
                                        {
                                            var parts = StringUtil.ParseTwoPart(old_currentLocation, ":");
                                            oldLeft = parts[0];
                                            oldRight = parts[1];
                                        }

                                        DomUtil.SetElementText(itemdom.DocumentElement,
                                            "currentLocation",
                                            BuildCurrentLocation(book_item.CurrentLocation == null ? oldLeft : book_item.CurrentLocation,
                                            book_item.CurrentShelfNo == null ? oldRight : book_item.CurrentShelfNo));

                                        if (updateItemTitle)
                                            book_item.Title = get_result.Result.AJ_TitleIdentifier_r;
                                    }
                                }

                                result = new GetLocalEntityDataResult
                                {
                                    Value = 1,
                                    ItemXml = itemdom.OuterXml,
                                    ItemRecPath = get_result.Result.AB_ItemIdentifier_r,
                                    Title = get_result.Result.AJ_TitleIdentifier_r,
                                    BookItem = book_item,
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
                            return new GetLocalEntityDataResult
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
                ClientInfo.WriteErrorLog($"GetEntityDataAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetLocalEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"GetEntityDataAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        // 将 SIP2 的 GetItemInfo 的册状态翻译为 dp2 册记录的 state 值
        static string ConvertItemState(string circulationStatus,
            string message)
        {
            switch (circulationStatus)
            {
                case "01":
                    if (string.IsNullOrEmpty(message) == false)
                        return $"其它({message})";
                    return "其它";
                case "02":
                    return "订购中";
                case "03":
                    return "";  // 在架、可借
                case "04":
                    // https://goldenwestcollege.libguides.com/english100
                    // Charged (also Not Charged) – In the catalog, charged books are not available. Not Charged means they are available.
                    return "不可供";
                case "05":
                    return "不可供";  // charged; not to be recalled until earliest recall date
                case "06":
                    return "加工中";
                case "07":
                    return "recalled";
                case "08":
                    return "等待放到预约架";
                case "09":
                    return "等待重新上架";
                case "10":
                    return "在图书馆馆藏地之间运输中";
                case "11":
                    return "声明退货";
                case "12":
                    return "丢失";
                case "13":
                    return "声明丢失";
            }

            return "sip2_" + circulationStatus;
        }

        // 根据馆藏地和架号字符串构造册记录 currentLocation 元素值
        static string BuildCurrentLocation(string location, string shelfNo)
        {
            if (string.IsNullOrEmpty(shelfNo) == false)
                return location + ":" + shelfNo;
            return location;
        }

        public static async Task<NormalResult> BorrowAsync(string patronBarcode,
            string itemBarcode)
        {
            try
            {
                using (var releaser = await _channelLimit.EnterAsync())
                {

                    SipChannel channel = await GetChannelAsync();
                    try
                    {
                        var result = await channel.CheckoutAsync(patronBarcode, itemBarcode, null);
                        if (result.Value == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = result.ErrorCode
                            };
                        if (result.Value == 0)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = result.ErrorCode
                            };
                        return new NormalResult();
                    }
                    finally
                    {
                        ReturnChannel(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"BorrowAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"BorrowAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        public static async Task<NormalResult> ReturnAsync(string itemBarcode)
        {
            try
            {
                using (var releaser = await _channelLimit.EnterAsync())
                {

                    SipChannel channel = await GetChannelAsync();
                    try
                    {
                        var result = await channel.CheckinAsync(itemBarcode, null);
                        if (result.Value == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = result.ErrorCode
                            };
                        if (result.Value == 0)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = result.ErrorCode
                            };
                        return new NormalResult();
                    }
                    finally
                    {
                        ReturnChannel(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"ReturnAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"ReturnAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        public static async Task<NormalResult> RenewAsync(string patronBarcode,
    string itemBarcode)
        {
            try
            {
                using (var releaser = await _channelLimit.EnterAsync())
                {

                    SipChannel channel = await GetChannelAsync();
                    try
                    {
                        var result = await channel.RenewAsync(patronBarcode, itemBarcode, null);
                        if (result.Value == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = result.ErrorCode
                            };
                        if (result.Value == 0)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = result.ErrorCode
                            };
                        return new NormalResult();
                    }
                    finally
                    {
                        ReturnChannel(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"RenewAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"RenewAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        static async Task<NormalResult> DetectSipNetworkAsync()
        {
            /*
            // testing
            return new NormalResult { Value = 1 };
            */
            try
            {
                using (var releaser = await _channelLimit.EnterAsync())
                {
                    SipChannel channel = await GetChannelAsync();
                    try
                    {
                        // -1出错，0不在线，1正常
                        var result = await channel.ScStatusAsync();
                        if (result.Value == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = result.ErrorCode
                            };
                        if (result.Value == 0)
                            return new NormalResult
                            {
                                Value = 0,
                                ErrorInfo = result.ErrorInfo,
                                ErrorCode = result.ErrorCode
                            };
                        return new NormalResult
                        {
                            Value = result.Value,
                            ErrorInfo = result.ErrorInfo,
                            ErrorCode = result.ErrorCode
                        };
                    }
                    finally
                    {
                        ReturnChannel(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"DetectSipNetworkAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"DetectSipNetworkAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

#if REMOVED
        #region 监控

        // 可以适当降低探测的频率。比如每五分钟探测一次
        // 两次检测网络之间的间隔
        static TimeSpan _detectPeriod = TimeSpan.FromMinutes(5);
        // 最近一次检测网络的时间
        static DateTime _lastDetectTime;

        static Task _monitorTask = null;

        // 是否已经(升级)更新了
        static bool _updated = false;
        // 最近一次检查升级的时刻
        static DateTime _lastUpdateTime;
        // 检查升级的时间间隔
        static TimeSpan _updatePeriod = TimeSpan.FromMinutes(2 * 60); // 2*60 两个小时

        // 监控间隔时间
        static TimeSpan _monitorIdleLength = TimeSpan.FromSeconds(10);

        static AutoResetEvent _eventMonitor = new AutoResetEvent(false);

        // 激活 Monitor 任务
        public static void ActivateMonitor()
        {
            _eventMonitor.Set();
        }

        static Task _delayTry = null;

        // 立即安排一次检测 SIP 网络
        public static void TryDetectSipNetwork(bool delay = true)
        {
            /*
            // testing
            return;
            */

            if (_delayTry != null)
                return;

            _delayTry = Task.Run(async () =>
            {
                try
                {
                    if (delay)
                        await Task.Delay(TimeSpan.FromSeconds(10));
                    _lastDetectTime = DateTime.MinValue;
                    ActivateMonitor();
                    _delayTry = null;
                }
                catch
                {

                }
            });
        }

        // 启动一般监控任务
        public static void StartMonitorTask()
        {
            if (_monitorTask != null)
                return;

            CancellationToken token = App.CancelToken;

            token.Register(() =>
            {
                _eventMonitor.Set();
            });

            _monitorTask = Task.Factory.StartNew(async () =>
            {
                WpfClientInfo.WriteInfoLog("SIP 监控专用线程开始");
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        // await Task.Delay(TimeSpan.FromSeconds(10));
                        _eventMonitor.WaitOne(_monitorIdleLength);

                        token.ThrowIfCancellationRequested();

                        if (DateTime.Now - _lastDetectTime > _detectPeriod)
                        {
                            var detect_result = await DetectSipNetworkAsync();
                            _lastDetectTime = DateTime.Now;

                            // testing
                            //detect_result.Value = -1;
                            //detect_result.ErrorInfo = "测试文字";

                            if (detect_result.Value != 1)
                                App.OpenErrorWindow(detect_result.ErrorInfo);
                            else
                                App.CloseErrorWindow();
                        }
                    }
                    _monitorTask = null;

                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"SIP 监控专用线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.SetError("monitor", $"SIP 监控专用线程出现异常: {ex.Message}");
                }
                finally
                {
                    WpfClientInfo.WriteInfoLog("SIP 监控专用线程结束");
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        #endregion

#endif

    }
}
