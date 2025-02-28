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
using static dp2SSL.LibraryChannelUtil;

using DigitalPlatform;
using DigitalPlatform.IO;
/*
using DigitalPlatform.RFID;
using DigitalPlatform.SIP2;
*/
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using DigitalPlatform.Xml;

namespace dp2SSL
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
                // 2021/1/19
                _channel.Encoding = Encoding.GetEncoding(App.SipEncoding);
                /*
                var parts = StringUtil.ParseTwoPart(App.SipServerUrl, ":");
                string address = parts[0];
                string port = parts[1];

                if (Int32.TryParse(port, out int port_value) == false)
                    throw new Exception($"SIP 服务器和端口号字符串 '{App.SipServerUrl}' 中端口号部分 '{port}' 格式错误");

                var result = await _channel.ConnectionAsync(address,
                    port_value);
                */
                var result = await ConnectAsync();
                if (result.Value == -1) // 出错
                {
                    TryDetectSipNetwork();
                    throw new Exception($"连接 SIP 服务器 {App.SipServerUrl} 时出错: {result.ErrorInfo}");
                }

                if (string.IsNullOrEmpty(App.SipUserName) == false)
                {
                    // 1 登录成功
                    // 0 登录失败
                    // -1 出错
                    var login_result = await _channel.LoginAsync(App.SipUserName,
                        App.SipPassword);
                    if (login_result.Value != 1)
                        throw new Exception($"针对 SIP 服务器 {App.SipServerUrl} 登录出错: {login_result.ErrorInfo}");
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
            var parts = StringUtil.ParseTwoPart(App.SipServerUrl, ":");
            string address = parts[0];
            string port = parts[1];

            if (Int32.TryParse(port, out int port_value) == false)
                throw new Exception($"SIP 服务器和端口号字符串 '{App.SipServerUrl}' 中端口号部分 '{port}' 格式错误");

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
            string filter_oi = App.SipInstitution;
            if (string.IsNullOrEmpty(filter_oi) == false)
            {
                if (oi != filter_oi)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"标签的 OI '{oi}' 不符合过滤定义 '{filter_oi}'，修改册记录状态被(dp2ssl)拒绝",
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
                WpfClientInfo.WriteErrorLog($"UpdateItemStatusAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"UpdateItemStatusAsync() 出现异常: {ex.Message}",
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
            string filter_oi = App.SipInstitution;
            if (string.IsNullOrEmpty(filter_oi) == false)
            {
                if (oi != filter_oi)
                    return new GetLocalEntityDataResult
                    {
                        Value = -1,
                        ErrorInfo = $"标签的 OI '{oi}' 不符合过滤定义 '{filter_oi}'，获取册记录被(dp2ssl)拒绝",
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

                        EntityItem entity_record = null;

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
                                    // 2025//28
                                    // 去掉右边的多余空白字符
                                    if (borrowDateString != null)
                                        borrowDateString = borrowDateString.TrimEnd(' ');
                                    if (string.IsNullOrEmpty(borrowDateString) == false)
                                    {
                                        if (DateTime.TryParseExact(borrowDateString,
                                            new string[] { 
                                                "yyyyMMdd    HHmmss",
                                                "yyyyMMdd", // 2025/2/28 兼容不太正规的用法
                                            },
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
                                            // 2025/2/28
                                            errors.Add(new NormalResult
                                            {
                                                Value = -1,
                                                ErrorInfo = $"SIP 消息 18 的 SM(HoldPickupDate) 字段内容 '{get_result.Result.CM_HoldPickupDate_18}' 不合法。应为 'yyyyMMdd    HHmmss' 格式",
                                                ErrorCode = "invalidSipFieldValue"
                                            });
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
                                            // 2025/2/28
                                            errors.Add(new NormalResult
                                            {
                                                Value = -1,
                                                ErrorInfo = $"SIP 消息 18 的 AH(DueDate) 字段内容 '{get_result.Result.AH_DueDate_o}' 不合法。应为 '{DateFormat}' 格式",
                                                ErrorCode = "invalidSipFieldValue"
                                            });
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
                                    book_item = await InventoryData.FindBookItemAsync(InventoryData.MakeOiPii(pii, oi));
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
                WpfClientInfo.WriteErrorLog($"GetEntityDataAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetLocalEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"GetEntityDataAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        // 讲 SIP2 的 GetItemInfo 的册状态翻译为 dp2 册记录的 state 值
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

        // return.Value:
        //      -1  出错
        //      0   读者记录没有找到
        //      1   成功
        public static async Task<GetReaderInfoResult> GetReaderInfoAsync(
            string patron_oi,
            string pii)
        {
            string filter_oi = App.SipInstitution;
            if (string.IsNullOrEmpty(filter_oi) == false)
            {
                if (string.IsNullOrEmpty(patron_oi) == false/*2024/12/27*/
                    && patron_oi != filter_oi)
                    return new GetReaderInfoResult
                    {
                        Value = -1,
                        ErrorInfo = $"读者证的 OI '{patron_oi}' 不符合过滤定义 '{filter_oi}'，获取读者记录被(dp2ssl)拒绝",
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
                        List<NormalResult> errors = new List<NormalResult>();

                        int nRedoCount = 0;
                    REDO_GETITEMINFO:
                        var get_result = await _channel.GetPatronInfoAsync(patron_oi, pii);
                        if (get_result.Value == -1)
                            return new GetReaderInfoResult
                            {
                                Value = -1,
                                ErrorInfo = get_result.ErrorInfo,
                                ErrorCode = get_result.ErrorCode
                            };
                        /*
                        else if (get_result.Result.CirculationStatus_2 == "01")
                        {
                            errors.Add(new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = get_result.Result.AF_ScreenMessage_o,
                                ErrorCode = get_result.Result.CirculationStatus_2
                            });
                        }
                        else if (get_result.Result.CirculationStatus_2 == "13")
                        {
                            errors.Add(new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = get_result.Result.AF_ScreenMessage_o,
                                ErrorCode = "itemNotFound"
                            });
                        }
                        */
                        else
                        {
                            if (string.IsNullOrEmpty(get_result.Result.AA_PatronIdentifier_r))
                                return new GetReaderInfoResult
                                {
                                    Value = -1,
                                    ErrorInfo = get_result.Result.AF_ScreenMessage_o,
                                    ErrorCode = "patronInfoError"
                                };

                            XmlDocument readerdom = new XmlDocument();
                            readerdom.LoadXml("<root />");

                            // 证状态
                            string state = "***";
                            if (get_result.Result.BL_ValidPatron_o == "Y")
                                state = "";
                            else
                                return new GetReaderInfoResult
                                {
                                    Value = -1,
                                    ErrorInfo = get_result.Result.AF_ScreenMessage_o,
                                    ErrorCode = "patronInfoError"
                                };

                            DomUtil.SetElementText(readerdom.DocumentElement,
                                "state",
                                state);

                            // 2021/4/2
                            if (string.IsNullOrEmpty(get_result.Result.AO_InstitutionId_r) == false)
                                DomUtil.SetElementText(readerdom.DocumentElement,
    "oi",
    get_result.Result.AO_InstitutionId_r);

                            // 读者证条码号
                            DomUtil.SetElementText(readerdom.DocumentElement,
                                "barcode",
                                get_result.Result.AA_PatronIdentifier_r);


                            // 姓名
                            DomUtil.SetElementText(readerdom.DocumentElement,
"name",
get_result.Result.AE_PersonalName_r);

                            // 可借册数
                            Patron.SetParamValue(readerdom.DocumentElement, "当前还可借", get_result.Result.BZ_HoldItemsLimit_o);
                            Patron.SetParamValue(readerdom.DocumentElement, "可借总册数", get_result.Result.CB_ChargedItemsLimit_o);

                            // TODO: 在借册可能一次返回不全。要和 get_result.Result.ChargedItemsCount_4 比较，看看是否获取完全了，如果不完全要继续获取直到完全。但有些 SIP Server 可能不支持按照偏移获取，要想办法判断这种情况(比如故意只获取一个，看看 SIP Server 是否能明白这个意图)
                            // 在借册
                            var root = readerdom.DocumentElement.AppendChild(readerdom.CreateElement("borrows")) as XmlElement;
                            var items = get_result.Result.AU_ChargedItems_o;
                            if (items != null)
                            {
                                foreach (var item in items)
                                {
                                    if (string.IsNullOrEmpty(item.Value))
                                        continue;

                                    var borrow = root.AppendChild(readerdom.CreateElement("borrow")) as XmlElement;
                                    InventoryData.ParseOiPii(item.Value, out string current_pii, out string current_oi);

                                    if (string.IsNullOrEmpty(current_oi) == false)
                                        borrow.SetAttribute("oi", current_oi);
                                    borrow.SetAttribute("barcode", current_pii);
                                }
                            }

                            return new GetReaderInfoResult
                            {
                                Value = 1,
                                ReaderXml = readerdom.OuterXml,
                                RecPath = "",
                                Timestamp = null
                            };
                        }
                    }
                    finally
                    {
                        ReturnChannel(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"GetEntGetReaderInfoAsyncityDataAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetReaderInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"GetReaderInfoAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        public static async Task<NormalResult> BorrowAsync(
            string patronBarcode,
            string itemBarcode)
        {
            string filter_oi = App.SipInstitution;
            if (string.IsNullOrEmpty(filter_oi) == false)
            {
                InventoryData.ParseOiPii(patronBarcode, out string patron_pii, out string patron_oi);
                InventoryData.ParseOiPii(itemBarcode, out string item_pii, out string item_oi);
                if (string.IsNullOrEmpty(patron_oi) == false/*2024/12/27*/
                    && patron_oi != filter_oi)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"读者证的 OI '{patron_oi}' 不符合过滤定义 '{filter_oi}'，借书被(dp2ssl)拒绝",
                        ErrorCode = "oiMismatch"
                    };
                if (item_oi != filter_oi)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"图书标签的 OI '{item_oi}' 不符合过滤定义 '{filter_oi}'，借书被(dp2ssl)拒绝",
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
                        var result = await channel.CheckoutAsync(patronBarcode,
                            itemBarcode,
                            filter_oi);
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
                WpfClientInfo.WriteErrorLog($"BorrowAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"BorrowAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        public static async Task<NormalResult> ReturnAsync(
            string itemBarcode)
        {
            string filter_oi = App.SipInstitution;
            if (string.IsNullOrEmpty(filter_oi) == false)
            {
                InventoryData.ParseOiPii(itemBarcode, out string item_pii, out string item_oi);
                if (item_oi != filter_oi)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"图书标签的 OI '{item_oi}' 不符合过滤定义 '{filter_oi}'，还书被(dp2ssl)拒绝",
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
                        var result = await channel.CheckinAsync(itemBarcode, filter_oi);
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
                WpfClientInfo.WriteErrorLog($"ReturnAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"ReturnAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        public static async Task<NormalResult> RenewAsync(
            string patronBarcode,
            string itemBarcode)
        {
            string filter_oi = App.SipInstitution;
            if (string.IsNullOrEmpty(filter_oi) == false)
            {
                InventoryData.ParseOiPii(patronBarcode, out string patron_pii, out string patron_oi);
                InventoryData.ParseOiPii(itemBarcode, out string item_pii, out string item_oi);
                if (string.IsNullOrEmpty(patron_oi) == false/*2024/12/27*/
                    && patron_oi != filter_oi)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"读者证的 OI '{patron_oi}' 不符合过滤定义 '{filter_oi}'，续借被(dp2ssl)拒绝",
                        ErrorCode = "oiMismatch"
                    };
                if (item_oi != filter_oi)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"图书标签的 OI '{item_oi}' 不符合过滤定义 '{filter_oi}'，续借被(dp2ssl)拒绝",
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
                        var result = await channel.RenewAsync(patronBarcode,
                            itemBarcode,
                            filter_oi);
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
                WpfClientInfo.WriteErrorLog($"RenewAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
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
                WpfClientInfo.WriteErrorLog($"DetectSipNetworkAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DetectSipNetworkAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }


        #region 监控

        // 可以适当降低探测的频率。比如每五分钟探测一次
        // 两次检测网络之间的间隔
        static TimeSpan _detectPeriod = TimeSpan.FromSeconds(100);
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
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"TryDetectSipNetwork() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
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
                        // _eventMonitor.WaitOne(_monitorIdleLength);
                        int index = WaitHandle.WaitAny(new WaitHandle[] {
                            _eventMonitor,
                            token.WaitHandle,
                            },
                            _monitorIdleLength);
                        if (index == 1)
                            return;

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
    }
}
