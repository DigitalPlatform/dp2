using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.VisualStudio.Threading;

using DigitalPlatform;
using DigitalPlatform.WPF;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using Microsoft.Data.Sqlite;

namespace dp2SSL
{
    public static class LibraryChannelUtil
    {
        public class GetEntityDataResult : NormalResult
        {
            public string Title { get; set; }
            public string ItemXml { get; set; }
            public string ItemRecPath { get; set; }
        }

        static bool _cacheDbCreated = false;

        static AsyncSemaphore _channelLimit = new AsyncSemaphore(2);

        // 获得册记录信息和书目摘要信息
        // .Value
        //      0   没有找到
        //      1   找到
        public static async Task<GetEntityDataResult> GetEntityDataAsync(string pii)
        {
            try
            {
                using (var releaser = await _channelLimit.EnterAsync())
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    if (_cacheDbCreated == false)
                    {
                        context.Database.EnsureCreated();
                        _cacheDbCreated = true;
                    }

                    LibraryChannel channel = App.CurrentApp.GetChannel();
                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = TimeSpan.FromSeconds(10);
                    try
                    {
                        GetEntityDataResult result = null;
                        List<NormalResult> errors = new List<NormalResult>();

                        // ***
                        // 第一步：获取册记录

                        // 先尝试从本地实体库中获得记录
                        var entity_record = context.Entities.Where(o => o.PII == pii).FirstOrDefault();
                        // EntityItem entity_record = null;   // testing

                        if (entity_record != null)
                            result = new GetEntityDataResult
                            {
                                Value = 1,
                                ItemXml = entity_record.Xml,
                                ItemRecPath = entity_record.RecPath,
                                Title = "",
                            };
                        else
                        {
                            // 再尝试从 dp2library 服务器获取
                            // TODO: ItemXml 和 BiblioSummary 可以考虑在本地缓存一段时间
                            int nRedoCount = 0;
                        REDO_GETITEMINFO:
                            long lRet = channel.GetItemInfo(null,
                                "item",
                                pii,
                                "",
                                "xml",
                                out string item_xml,
                                out string item_recpath,
                                out byte[] timestamp,
                                "",
                                out _,
                                out _,
                                out string strError);
                            if (lRet == -1)
                            {
                                if ((channel.ErrorCode == ErrorCode.RequestError ||
                                    channel.ErrorCode == ErrorCode.RequestTimeOut)
                                    && nRedoCount < 2)
                                {
                                    nRedoCount++;
                                    goto REDO_GETITEMINFO;
                                }
                                // TODO: 这里不着急返回，还需要尝试获得书目摘要
                                errors.Add(new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString()
                                });
                                /*
                                return new GetEntityDataResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString()
                                };
                                */
                            }
                            else
                            {
                                result = new GetEntityDataResult
                                {
                                    Value = 1,
                                    ItemXml = item_xml,
                                    ItemRecPath = item_recpath,
                                    Title = "",
                                };

                                // 保存到本地数据库
                                await AddOrUpdateAsync(context, new EntityItem
                                {
                                    PII = pii,
                                    Xml = item_xml,
                                    RecPath = item_recpath,
                                    Timestamp = timestamp,
                                });
#if NO
                                context.Entities.Add(new EntityItem
                                {
                                    PII = pii,
                                    Xml = item_xml,
                                    RecPath = item_recpath,
                                    Timestamp = timestamp,
                                });
                                try
                                {
                                    await context.SaveChangesAsync();
                                }
                                catch (Exception ex)
                                {
                                    SqliteException sqlite_exception = ex.InnerException as SqliteException;
                                    if (sqlite_exception != null && sqlite_exception.SqliteErrorCode == 19)
                                    {
                                        // PII 发生重复了
                                    }
                                    else
                                        throw ex;
                                }
#endif
                            }
                        }

                        // ***
                        /// 第二步：获取书目摘要

                        // 先尝试从本地书目库中获取书目摘要

                        var item = context.BiblioSummaries.Where(o => o.PII == pii).FirstOrDefault();
                        if (item != null
                            && string.IsNullOrEmpty(item.BiblioSummary) == false)
                        {
                            if (result == null)
                                result = new GetEntityDataResult();

                            result.Title = item.BiblioSummary;
                        }
                        else
                        {
                            // 从 dp2library 服务器获取书目摘要
                            int nRedoCount = 0;
                        REDO_GETBIBLIOSUMMARY:
                            long lRet = channel.GetBiblioSummary(
                null,
                pii,
                "", // strConfirmItemRecPath,
                null,
                out _,
                out string strSummary,
                out string strError);
                            if (lRet == -1)
                            {
                                if ((channel.ErrorCode == ErrorCode.RequestError ||
                channel.ErrorCode == ErrorCode.RequestTimeOut)
                && nRedoCount < 2)
                                {
                                    nRedoCount++;
                                    goto REDO_GETBIBLIOSUMMARY;
                                }

                                errors.Add(new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString()
                                });
                                /*
                                return new GetEntityDataResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString(),
                                };
                                */
                            }
                            else
                            {
                                strSummary = strSummary?.Replace(". -- ", "\r\n");   // .Replace("/", "\r\n");

                                if (result == null)
                                    result = new GetEntityDataResult();

                                result.Title = strSummary;

                                // 存入数据库备用
                                if (lRet == 1 && string.IsNullOrEmpty(strSummary) == false)
                                {
                                    try
                                    {
                                        var exist_item = context.BiblioSummaries.Where(o => o.PII == pii).FirstOrDefault();

                                        if (exist_item != null)
                                        {
                                            if (exist_item.BiblioSummary != strSummary)
                                            {
                                                exist_item.BiblioSummary = strSummary;
                                                context.BiblioSummaries.Update(exist_item);
                                            }
                                        }
                                        else
                                            context.BiblioSummaries.Add(new BiblioSummaryItem
                                            {
                                                PII = pii,
                                                BiblioSummary = strSummary
                                            });
                                        await context.SaveChangesAsync();
                                    }
                                    catch (Exception ex)
                                    {
                                        WpfClientInfo.WriteErrorLog($"GetEntityDataAsync() 中保存 summary 时(PII 为 '{pii}')出现异常:{ExceptionUtil.GetDebugText(ex)}");
                                    }
                                }
                            }

                            /*
                            return new GetEntityDataResult
                            {
                                Value = (int)lRet,
                                ItemXml = item_xml,
                                ItemRecPath = item_recpath,
                                Title = strSummary,
                                ErrorInfo = strError,
                                ErrorCode = channel.ErrorCode.ToString()
                            };
                            */
                        }

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
                        channel.Timeout = old_timeout;
                        App.CurrentApp.ReturnChannel(channel);
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

        static async Task AddOrUpdateAsync(BiblioCacheContext context,
            EntityItem item)
        {
            try
            {
                // 保存到本地数据库
                context.Entities.Add(item);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                SqliteException sqlite_exception = ex.InnerException as SqliteException;
                if (sqlite_exception != null && sqlite_exception.SqliteErrorCode == 19)
                {
                    // PII 发生重复了
                    goto UPDATE;
                }
                else
                    throw ex;
            }

        UPDATE:
            // 更新到本地数据库
            context.Entities.Update(item);
            await context.SaveChangesAsync();
        }

        // 探测和 dp2library 服务器的通讯是否正常
        // return.Value
        //      -1  本函数执行出现异常
        //      0   网络不正常
        //      1   网络正常
        public static NormalResult DetectLibraryNetwork()
        {
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(5);  // 设置 5 秒超时，避免等待太久
            try
            {
                int nRedoCount = 0;
            REDO:
                long lRet = channel.GetClock(null,
                    out string _,
                    out string strError);
                if (lRet == -1)
                {
                    // 一次重试机会
                    if (lRet == -1
                        && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                        && nRedoCount < 2)
                    {
                        nRedoCount++;
                        goto REDO;
                    }

                    return new NormalResult
                    {
                        Value = 0,
                        ErrorInfo = strError
                    };
                }

                return new NormalResult { Value = 1 };
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DetectNetwork() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        // result.Value
        //      0   没有找到记录。没有发生更新
        //      1   成功更新
        public static async Task<NormalResult> UpdateEntityXmlAsync(string pii,
            string item_xml,
            byte[] timestamp)
        {
            try
            {
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    if (_cacheDbCreated == false)
                    {
                        context.Database.EnsureCreated();
                        _cacheDbCreated = true;
                    }

                    // 先尝试从本地实体库中获得原记录
                    var entity_record = context.Entities.Where(o => o.PII == pii).FirstOrDefault();
                    if (entity_record == null)
                        return new NormalResult { Value = 0 };

                    entity_record.Xml = item_xml;
                    entity_record.Timestamp = timestamp;

                    // 保存到本地数据库
                    context.Entities.Update(entity_record);
                    await context.SaveChangesAsync();
                    return new NormalResult { Value = 1 };
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"UpdateEntityXmlAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"UpdateEntityXmlAsync() 出现异常: {ex.Message}"
                };
            }
        }

        public class SetReaderInfoResult : NormalResult
        {
            public byte[] NewTimestamp { get; set; }
        }

        public static Task<SetReaderInfoResult> SetReaderInfoAsync(string recpath,
            string xml,
            string old_xml,
            byte[] timestamp)
        {
            return Task<SetReaderInfoResult>.Run(() =>
            {
                LibraryChannel channel = App.CurrentApp.GetChannel();
                TimeSpan old_timeout = channel.Timeout;
                channel.Timeout = TimeSpan.FromSeconds(10);
                try
                {
                    long lRet = channel.SetReaderInfo(null,
                        "change",
                        recpath,
                        xml,
                        old_xml,
                        timestamp,
                        out string existing_xml,
                        out string saved_xml,
                        out string saved_recpath,
                        out byte[] new_timestamp,
                        out ErrorCodeValue error_code,
                        out string strError);
                    if (lRet == -1)
                        return new SetReaderInfoResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            NewTimestamp = new_timestamp
                        };
                    if (lRet == 0)
                        return new SetReaderInfoResult
                        {
                            Value = 0,
                            ErrorInfo = strError,
                            NewTimestamp = new_timestamp
                        };
                    return new SetReaderInfoResult
                    {
                        Value = 1,
                        NewTimestamp = new_timestamp
                    };
                }
                finally
                {
                    channel.Timeout = old_timeout;
                    App.CurrentApp.ReturnChannel(channel);
                }
            });
        }

        public class GetReaderInfoResult : NormalResult
        {
            public string RecPath { get; set; }
            public string ReaderXml { get; set; }
            public byte[] Timestamp { get; set; }
        }

        // 从本地数据库获取读者记录
        // return.Value:
        //      -1  出错
        //      0   读者记录没有找到
        //      1   成功
        public static GetReaderInfoResult GetReaderInfoFromLocal(string pii)
        {
            try
            {
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    if (_cacheDbCreated == false)
                    {
                        context.Database.EnsureCreated();
                        _cacheDbCreated = true;
                    }

                    pii = pii?.ToUpper();

                    string query = $",{pii},";
                    var patrons = context.Patrons
                        .Where(o => o.PII == pii || o.Bindings.Contains(query))
                        .ToList();
                    if (patrons.Count == 0)
                        return new GetReaderInfoResult
                        {
                            Value = 0,
                            ErrorInfo = $"PII 为 '{pii}' 的本地读者记录没有找到"
                        };

                    // 命中读者记录多于一条
                    if (patrons.Count > 1)
                    {
                        return new GetReaderInfoResult
                        {
                            Value = -1,
                            ErrorInfo = $"装载本地读者记录失败：'{pii}' 检索命中读者记录 {patrons.Count} 条"
                        };
                    }

                    var patron = patrons[0];
                    return new GetReaderInfoResult
                    {
                        Value = 1,
                        RecPath = patron.RecPath,
                        ReaderXml = patron.Xml,
                        Timestamp = patron.Timestamp
                    };
                }
            }
            catch (Exception ex)
            {
                return new GetReaderInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"装载本地读者记录(PII 为 '{pii}')时出现异常: {ex.Message}"
                };
            }
        }

        // 把读者记录保存(更新)到本地数据库
        public static NormalResult UpdateLocalPatronRecord(
            GetReaderInfoResult get_result)
        {
            using (BiblioCacheContext context = new BiblioCacheContext())
            {
                if (_cacheDbCreated == false)
                {
                    context.Database.EnsureCreated();
                    _cacheDbCreated = true;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(get_result.ReaderXml);
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"读者记录装载进入 XMLDOM 时出错:{ex.Message}",
                        ErrorCode = "loadXmlError"
                    };
                }
                string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(pii))
                    pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
                var patron = context.Patrons
    .Where(o => o.PII == pii)
    .FirstOrDefault();
                if (patron != null)
                {
                    Set(patron, dom);
                    context.Patrons.Update(patron);
                }
                else
                {
                    patron = new PatronItem
                    {
                        PII = pii?.ToUpper(),
                    };
                    Set(patron, dom);
                    context.Patrons.Add(patron);
                }

                context.SaveChanges();
                return new NormalResult { Value = 0 };
            }

            void Set(PatronItem patron, XmlDocument dom)
            {
                string cardNumber = DomUtil.GetElementText(dom.DocumentElement, "cardNumber");
                cardNumber = cardNumber.ToUpper();
                if (string.IsNullOrEmpty(cardNumber) == false)
                    cardNumber = "," + cardNumber + ",";

                if (get_result.RecPath != null)
                    patron.RecPath = get_result.RecPath;
                patron.Bindings = cardNumber;
                patron.Xml = get_result.ReaderXml;
                patron.Timestamp = get_result.Timestamp;
            }
        }

        public static NormalResult DeleteLocalPatronRecord(string strXml)
        {
            using (BiblioCacheContext context = new BiblioCacheContext())
            {
                if (_cacheDbCreated == false)
                {
                    context.Database.EnsureCreated();
                    _cacheDbCreated = true;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"读者记录装载进入 XMLDOM 时出错:{ex.Message}",
                        ErrorCode = "loadXmlError"
                    };
                }
                string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(pii))
                    pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
                var patron = context.Patrons
    .Where(o => o.PII == pii)
    .FirstOrDefault();
                if (patron != null)
                {
                    context.Patrons.Remove(patron);
                    context.SaveChanges();
                }

                return new NormalResult { Value = 0 };
            }
        }


        // return.Value:
        //      -1  出错
        //      0   读者记录没有找到
        //      1   成功
        public static GetReaderInfoResult GetReaderInfo(string pii)
        {
            /*
            reader_xml = "";
            recpath = "";
            timestamp = null;
            */
            if (string.IsNullOrEmpty(App.dp2ServerUrl) == true)
                return new GetReaderInfoResult
                {
                    Value = -1,
                    ErrorInfo = "dp2library 服务器 URL 尚未配置，无法获得读者信息"
                };
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(5);  // 设置 5 秒超时，避免等待太久
            try
            {
                int nRedoCount = 0;
            REDO:
                long lRet = channel.GetReaderInfo(null,
                    pii,
                    "advancexml", // "xml",
                    out string[] results,
                    out string recpath,
                    out byte[] timestamp,
                    out string strError);
                if (lRet == -1 || lRet == 0)
                {
                    // 2020/4/24 增加一次重试机会
                    if (lRet == -1
                        && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                        && nRedoCount < 2)
                    {
                        nRedoCount++;
                        goto REDO;
                    }

                    // 如果发生通讯失败，则主动重新探测一次网络状况
                    if (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                    {
                        ShelfData.DetectLibraryNetwork();
                    }

                    return new GetReaderInfoResult
                    {
                        Value = (int)lRet,
                        ErrorInfo = strError,
                        RecPath = recpath,
                        Timestamp = timestamp
                    };
                }

                // 2019/12/19
                // 命中读者记录多于一条
                if (lRet > 1)
                {
                    return new GetReaderInfoResult
                    {
                        Value = -1,
                        ErrorInfo = $"装载读者记录失败：'{pii}' 检索命中读者记录 {lRet} 条"
                    };
                }

                string reader_xml = "";
                if (results != null && results.Length > 0)
                    reader_xml = results[0];
                return new GetReaderInfoResult
                {
                    Value = 1,
                    RecPath = recpath,
                    Timestamp = timestamp,
                    ReaderXml = reader_xml
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public class LoginResult : NormalResult
        {
            public string OutputUserName { get; set; }
            public string Rights { get; set; }
            public string LibraryCode { get; set; }
        }

        // result.Value
        //      -1:   出错
        //      0:    登录未成功
        //      1:    登录成功
        public static LoginResult WorkerLogin(string userName, string password)
        {
            if (string.IsNullOrEmpty(App.dp2ServerUrl) == true)
                return new LoginResult
                {
                    Value = -1,
                    ErrorInfo = "dp2library 服务器 URL 尚未配置，无法进行工作人员登录"
                };
            LibraryChannel channel = App.CurrentApp.GetChannel(userName);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                // -1:   出错
                // 0:    登录未成功
                // 1:    登录成功
                long lRet = channel.Login(userName,
                    password,
                    "type=worker,client=dp2ssl|" + WpfClientInfo.ClientVersion,
                    out string strOutputUserName,
                    out string strRights,
                    out string strLibraryCode,
                    out string strError);
                if (lRet == -1 || lRet == 0)
                    return new LoginResult
                    {
                        Value = (int)lRet,
                        ErrorInfo = strError,
                    };

                // testing
                // channel.Logout(out strError);

                return new LoginResult
                {
                    Value = 1,
                    OutputUserName = strOutputUserName,
                    Rights = strRights,
                    LibraryCode = strLibraryCode,
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public class GetLocationListResult : NormalResult
        {
            public List<string> List { get; set; }
        }

        // 获得馆藏地列表
        public static GetLocationListResult GetLocationList()
        {
            string strOutputInfo = "";
            LibraryChannel channel = App.CurrentApp.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                long lRet = channel.GetSystemParameter(
null,
"circulation",
"locationTypes",
out strOutputInfo,
out string strError);
                if (lRet == -1)
                    return new GetLocationListResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }

            // 
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strOutputInfo;
            }
            catch (Exception ex)
            {
                return new GetLocationListResult
                {
                    Value = -1,
                    ErrorInfo = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message
                };
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
<locationTypes>
    <item canborrow="yes" itembarcodeNullable="yes">流通库</item>
    <item>阅览室</item>
    <library code="分馆1">
        <item canborrow="yes">流通库</item>
        <item>阅览室</item>
    </library>
</locationTypes>
*/

            List<string> results = new List<string>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//item");
            foreach (XmlElement node in nodes)
            {
                string strText = node.InnerText;

                // 
                string strLibraryCode = "";
                XmlElement parent = node.ParentNode as XmlElement;
                if (parent.Name == "library")
                {
                    strLibraryCode = parent.GetAttribute("code");
                }

                results.Add(string.IsNullOrEmpty(strLibraryCode) ? strText : strLibraryCode + "/" + strText);
            }

            return new GetLocationListResult { List = results };
        }
    }
}
