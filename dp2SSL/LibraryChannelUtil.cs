using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Runtime.Remoting.Activation;
using System.Threading;

using Microsoft.VisualStudio.Threading;
using Microsoft.Data.Sqlite;

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.WPF;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2SSL
{
    /// <summary>
    /// 和 dp2library 通道有关的功能
    /// </summary>
    public static class LibraryChannelUtil
    {
        public class GetEntityDataResult : NormalResult
        {
            public string Title { get; set; }
            public string ItemXml { get; set; }
            public string ItemRecPath { get; set; }

            // 2021/4/1
            public byte[] ItemTimestamp { get; set; }
        }

        static bool _cacheDbCreated = false;

        static AsyncSemaphore _channelLimit = new AsyncSemaphore(2);

        // 获得册记录信息和书目摘要信息
        // parameters:
        //      style   风格。
        //              network 表示只从网络获取册记录；否则优先从本地获取，本地没有再从网络获取册记录。无论如何，书目摘要都是尽量从本地获取
        //              offline 表示指从本地获取册记录和书目记录
        // .Value
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public static async Task<GetEntityDataResult> GetEntityDataAsync(string pii,
            string style)
        {
            bool network = StringUtil.IsInList("network", style);

            // 2021/5/17
            bool offline = StringUtil.IsInList("offline", style);

            bool skip_biblio = StringUtil.IsInList("skip_biblio", style);

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

                        EntityItem entity_record = null;

                        // ***
                        // 第一步：获取册记录

                        if (network == false)
                        {
                            // 先尝试从本地实体库中获得记录
                            entity_record = context.Entities.Where(o => o.PII == pii).FirstOrDefault();
                            // 2020/8/27
                            if (entity_record == null && pii.Contains(".") == false)
                            {
                                // 如果 pii 是不含有点的，则尝试后方一致匹配数据库
                                entity_record = context.Entities.Where(o => o.PII.EndsWith("." + pii)).FirstOrDefault();
                            }
                            else if (entity_record == null && pii.Contains(".") == true)
                            {
                                // 如果 pii 是含有点的，则改为用纯净 PII 部分进行匹配
                                string pure_pii = GetPurePII(pii);
                                entity_record = context.Entities.Where(o => o.PII == pure_pii).FirstOrDefault();
                            }

                            // EntityItem entity_record = null;   // testing
                        }

                        if (entity_record != null)
                            result = new GetEntityDataResult
                            {
                                Value = 1,
                                ItemXml = entity_record.Xml,
                                ItemRecPath = entity_record.RecPath,
                                Title = "",
                            };
                        else if (offline == false)  //
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
                            else if (lRet == 0)
                                errors.Add(new NormalResult
                                {
                                    Value = 0,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString()
                                });
                            else
                            {
                                result = new GetEntityDataResult
                                {
                                    Value = 1,
                                    ItemXml = item_xml,
                                    ItemRecPath = item_recpath,
                                    Title = "",
                                    ItemTimestamp = timestamp,
                                };

                                // TODO: item_xml 里面最好包含 OI 字符串，便于建立本地缓存
                                // TODO: 如何做到尽量保存 xxx.xxx 形态的 PII 作为 key?
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

                        if (skip_biblio == false)
                        {
                            // 先尝试从本地书目库中获取书目摘要

                            var item = context.BiblioSummaries.Where(o => o.PII == pii).FirstOrDefault();
                            if (item != null
                                && string.IsNullOrEmpty(item.BiblioSummary) == false)
                            {
                                if (result == null)
                                    result = new GetEntityDataResult();

                                result.Title = item.BiblioSummary;
                            }
                            else if (offline == false)
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
                                            /*
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
                                            */
                                            // 2020/9/23
                                            await AddOrUpdateAsync(context, new BiblioSummaryItem
                                            {
                                                PII = pii,
                                                BiblioSummary = strSummary
                                            });
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

                        }

                        // 完全成功
                        if (result != null && errors.Count == 0)
                            return result;

                        // 2021/5/17
                        if (errors.Count == 0)
                            errors.Add(new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = offline ? "本机没有此册信息" : "册信息没有找到",
                                ErrorCode = "NotFound"
                            });

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

        public static async Task AddOrUpdateAsync(BiblioCacheContext context,
    BiblioSummaryItem item)
        {
            try
            {
                // 保存到本地数据库
                context.BiblioSummaries.Add(item);
                await context.SaveChangesAsync();
                return; // 2021/12/22
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
            context.BiblioSummaries.Update(item);
            await context.SaveChangesAsync();
        }


        // 从 OI.PII 中获得 PII 部分
        static string GetPurePII(string text)
        {
            if (text == null)
                return "";
            if (text.Contains(".") == false)
                return text;
            return StringUtil.ParseTwoPart(text, ".")[1];
        }

        // 从本地数据库获得册记录信息和书目摘要信息
        // .Value
        //      0   没有找到
        //      1   找到一种
        //      2   两种都找到了
        public static GetEntityDataResult LocalGetEntityData(string pii)
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

                    GetEntityDataResult result = null;

                    // ***
                    // 第一步：获取册记录

                    // 从本地实体库中获得记录
                    var entity_record = context.Entities.Where(o => o.PII == pii).FirstOrDefault();

                    // 2020/9/3
                    // 对没有点的 PII 字符串尝试后方一致匹配
                    if (entity_record == null && pii.IndexOf(".") == -1)
                        entity_record = context.Entities.Where(o => o.PII.EndsWith("." + pii)).FirstOrDefault();

                    // EntityItem entity_record = null;   // testing

                    if (entity_record != null)
                        result = new GetEntityDataResult
                        {
                            Value = 1,
                            ItemXml = entity_record.Xml,
                            ItemRecPath = entity_record.RecPath,
                            Title = "",
                        };

                    // ***
                    /// 第二步：获取书目摘要

                    // 从本地书目库中获取书目摘要

                    var item = context.BiblioSummaries.Where(o => o.PII == pii).FirstOrDefault();
                    if (item != null
                        && string.IsNullOrEmpty(item.BiblioSummary) == false)
                    {
                        if (result == null)
                            result = new GetEntityDataResult();

                        result.Title = item.BiblioSummary;
                        result.Value++;
                    }

                    if (result == null)
                        return new GetEntityDataResult { Value = 0 };

                    return result;
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"LocalGetEntityDataAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"LocalGetEntityDataAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        static void SetPII(EntityItem item)
        {
            try
            {
                XmlDocument itemdom = new XmlDocument();
                itemdom.LoadXml(item.Xml);

                string oi = DomUtil.GetElementText(itemdom.DocumentElement, "oi");
                string barcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

                if (string.IsNullOrEmpty(oi))
                    item.PII = barcode;
                else
                    item.PII = oi + "." + barcode;
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"SetPII() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        public static async Task AddOrUpdateAsync(BiblioCacheContext context,
            EntityItem item)
        {
            // 调整 PII 字段，尽量规整为 OI.PII 形态
            SetPII(item);

            try
            {
                // 保存到本地数据库
                context.Entities.Add(item);
                await context.SaveChangesAsync();
                return; // 2021/12/22
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

        public static string GetBiblioSummaryFromLocal(string pii)
        {
            try
            {
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    var item = context.BiblioSummaries.Where(o => o.PII == pii).FirstOrDefault();
                    if (item != null
                        && string.IsNullOrEmpty(item.BiblioSummary) == false)
                        return item.BiblioSummary;
                    return "";
                }
            }
            catch
            {
                return null;
            }
        }

        // 2020/9/23
        // 从 dp2library 服务器获得书目摘要
        public static async Task<string> GetBiblioSummaryFromNetworkAsync(string pii)
        {
            using (var releaser = await _channelLimit.EnterAsync())
            {
                LibraryChannel channel = App.CurrentApp.GetChannel();
                TimeSpan old_timeout = channel.Timeout;
                channel.Timeout = TimeSpan.FromSeconds(10);
                try
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

                        return null;
                    }

                    return strSummary?.Replace(". -- ", "\r\n");
                }
                finally
                {
                    channel.Timeout = old_timeout;
                    App.CurrentApp.ReturnChannel(channel);
                }
            }
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
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString(),
                    };
                }

                return new NormalResult { Value = 1 };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"DetectLibraryNetwork() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DetectNetwork() 出现异常：{ex.Message}",
                    ErrorCode = ex.GetType().ToString(),
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public class GetVersionResult : NormalResult
        {
            public string Version { get; set; }
            public string ServerUid { get; set; }
        }

        // 2021/11/22
        public static GetVersionResult GetVersion()
        {
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(5);  // 设置 5 秒超时，避免等待太久
            try
            {
                int nRedoCount = 0;
            REDO:
                long lRet = channel.GetVersion(null,
                    out string version,
                    out string uid,
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

                    return new GetVersionResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString(),
                    };
                }

                return new GetVersionResult
                {
                    Value = 1,
                    Version = version,
                    ServerUid = uid,
                };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"GetVersion() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetVersionResult
                {
                    Value = -1,
                    ErrorInfo = $"GetVersion() 出现异常：{ex.Message}",
                    ErrorCode = ex.GetType().ToString(),
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

                    if (string.IsNullOrEmpty(item_xml) == true)
                    {
                        context.Remove(entity_record);
                    }
                    else
                    {
                        entity_record.Xml = item_xml;
                        entity_record.Timestamp = timestamp;

                        // 保存到本地数据库
                        context.Entities.Update(entity_record);
                    }
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
        // parameters:
        //      buildBorrowInfo 是否根据本地动作信息合成 borrows/borrow 元素
        // return.Value:
        //      -1  出错
        //      0   读者记录没有找到
        //      1   成功
        public static GetReaderInfoResult GetReaderInfoFromLocal(string pii,
            bool buildBorrowInfo)
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
                    {
                        // 再尝试后方一致匹配一次
                        if (pii != null && pii.Contains(".") == false)
                        {
                            patrons = context.Patrons
                                .Where(o => o.PII.EndsWith($".{pii}"))
                                .ToList();
                        }

                        if (patrons.Count == 0)
                            return new GetReaderInfoResult
                            {
                                Value = 0,
                                ErrorInfo = $"PII 为 '{pii}' 的本地读者记录没有找到"
                            };
                    }

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

                    if (buildBorrowInfo)
                    {
                        // 2020/5/8
                        // 添加用本地信息模拟出来的 borrows/borrow 元素
                        XmlDocument patron_dom = new XmlDocument();
                        patron_dom.LoadXml(patron.Xml);
                        SetBorrowInfo(patron_dom, patron.LastWriteTime);

                        patron.Xml = patron_dom.OuterXml;
                    }

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
                WpfClientInfo.WriteErrorLog($"GetReaderInfoFromLocal() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetReaderInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"装载本地读者记录(PII 为 '{pii}')时出现异常: {ex.Message}"
                };
            }
        }

        static string GetPii(XmlDocument dom)
        {
            string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(pii))
                pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
            return pii;
        }

        // 获得读者的 PII。注意包含了 OI 部分
        static string GetPatronOiPii(XmlDocument dom)
        {
            string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(pii))
            {
                pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
                return pii;
            }

            string oi = DomUtil.GetElementText(dom.DocumentElement, "oi");
            if (oi == null)
                oi = "";

            // 2020/9/25
            // 如果读者记录中没有 oi 元素，则从 libraryCode 元素推导
            if (string.IsNullOrEmpty(oi))
            {
                string libraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
                var ret = ShelfData.GetOwnerInstitution(libraryCode + "/", out string isil, out string alternative);
                if (ret == true)
                {
                    if (string.IsNullOrEmpty(isil) == false)
                        oi = isil;
                    else if (string.IsNullOrEmpty(alternative) == false)
                        oi = alternative;
                }
            }

            // 注意返回的是严格形态
            return oi + "." + pii;
        }


#if REMOVED
        // 获得读者的 PII。注意包含了 OI 部分
        static string GetPatronPii(XmlDocument dom)
        {
            string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(pii))
            {
                pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
                return pii;
            }

            // 2020/7/17
            // 加上 OI 部分
            string libraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
            var ret = ShelfData.GetOwnerInstitution(libraryCode + "/", out string isil, out string alternative);
            if (ret == true)
            {
                // 应该是 xxx.xxx 形态
                if (string.IsNullOrEmpty(isil) == false)
                    pii = isil + "." + pii;
                else if (string.IsNullOrEmpty(alternative) == false)
                    pii = alternative + "." + pii;
            }

            return pii;
        }

#endif

        // 获得 oi.pii 的 oi 部分
        public static string GetOiPart(string oi_pii, bool return_null)
        {
            if (oi_pii.IndexOf(".") == -1)
            {
                if (return_null)
                    return null;
                return "";
            }
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[0];
        }

        // 获得 oi.pii 的 pii 部分
        public static string GetPiiPart(string oi_pii)
        {
            if (oi_pii.IndexOf(".") == -1)
                return oi_pii;
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[1];
        }

        // 根据本地历史记录，在读者记录中添加 borrows/borrow 元素
        // parameters:
        //      lastWriteTime   读者 XML 记录最近更新时间。只取这个时间以后的本地未还借书动作
        static NormalResult SetBorrowInfo(XmlDocument patron_dom,
            DateTime lastWriteTime)
        {
            string pii = GetPii(patron_dom);

            bool changed = false;
            XmlElement root = patron_dom.DocumentElement.SelectSingleNode("borrows") as XmlElement;
            if (root == null)
            {
                root = patron_dom.CreateElement("borrows");
                patron_dom.DocumentElement.AppendChild(root);
            }

            /*
            // 删除原有 borrows/borrow 元素
            if (root.ChildNodes.Count > 0)
            {
                root.RemoveAll();
                changed = true;
            }
            */


            using (var context = new RequestContext())
            {
                // 显示该读者的在借册情况
                var borrows = context.Requests
                    .Where(o => o.OperatorID == pii && o.Action == "borrow" && o.LinkID == null
                    && o.OperTime > lastWriteTime)
                    .OrderBy(o => o.ID).ToList();
                /* testing
                var borrows = context.Requests
    .Where(o => o.OperatorID == pii && o.Action == "borrow")
    .OrderBy(o => o.ID).ToList();
                */
                foreach (var item in borrows)
                {
                    // 2020/6/20
                    // 查重 合并 barcode 相同的 borrow 元素
                    // 注意，XPath 中 and 不能用大写，只能用小写
                    var dup = root.SelectSingleNode($"borrow[@barcode='{GetPiiPart(item.PII)}' and @oi='{GetOiPart(item.PII, false)}']") as XmlElement;
                    if (dup != null)
                        continue;

                    var borrow_info = JsonConvert.DeserializeObject<BorrowInfo>(item.ActionString);

                    XmlElement new_borrow = patron_dom.CreateElement("borrow");
                    root.AppendChild(new_borrow);
                    // var title = GetEntityTitle(item.EntityString);

                    new_borrow.SetAttribute("barcode", GetPiiPart(item.PII));
                    new_borrow.SetAttribute("oi", GetOiPart(item.PII, false));
                    new_borrow.SetAttribute("borrowDate", DateTimeUtil.Rfc1123DateTimeStringEx(item.OperTime));
                    if (borrow_info != null)
                    {
                        /*
{"BorrowCount":0,
"BorrowOperator":"supervisor",
"DenyPeriod":"",
"ItemBarcode":"T0000131",
"LatestReturnTime":"Mon, 08 Jun 2020 12:00:00 +0800",
"Overflows":null,
"Period":"31day"}
* */
                        new_borrow.SetAttribute("returningDate", borrow_info.LatestReturnTime);
                        new_borrow.SetAttribute("borrowPeriod", borrow_info.Period);
                        if (borrow_info.Overflows != null)
                            new_borrow.SetAttribute("overflow", string.Join("; ", borrow_info.Overflows));
                        new_borrow.SetAttribute("no", borrow_info.BorrowCount.ToString());
                    }
                    changed = true;
                }
            }

            if (changed)
                return new NormalResult { Value = 1 };
            return new NormalResult();
        }


        // 把读者记录保存(更新)到本地数据库
        // parameters:
        //          lastWriteTime   最后写入时间。采用服务器时间
        // result.Value
        //      -1  出错
        //      0   没有发生修改
        //      1   发生了创建或者修改
        public static NormalResult UpdateLocalPatronRecord(
            GetReaderInfoResult get_result,
            DateTime lastWriteTime)
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
                /*
                string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(pii))
                    pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
                    */
                string oi_pii = GetPatronOiPii(dom);
                var patron = context.Patrons
    .Where(o => o.PII == oi_pii)
    .FirstOrDefault();
                if (patron != null)
                {
                    // 如果已经存在的读者记录比打算写入的要新，则放弃写入
                    if (patron.LastWriteTime > lastWriteTime)
                        return new NormalResult { Value = 0 };
                    Set(patron, dom);
                    context.Patrons.Update(patron);
                }
                else
                {
                    patron = new PatronItem
                    {
                        PII = oi_pii?.ToUpper(),
                    };
                    Set(patron, dom);
                    context.Patrons.Add(patron);
                }

                context.SaveChanges();
                return new NormalResult { Value = 1 };
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
                patron.LastWriteTime = lastWriteTime;

#if REMOVED
                // 2020/9/25
                // 把 PII 规整为包含 OI 的形态
                if (patron.PII == null
                    || patron.PII?.IndexOf(".") == -1 || patron.PII.StartsWith("."))
                {
                    string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    if (string.IsNullOrEmpty(pii))
                        pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");

                    if (pii.StartsWith("@") == false)
                    {
                        string libraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
                        var ret = ShelfData.GetOwnerInstitution(libraryCode + "/", out string isil, out string alternative);
                        if (ret == true)
                        {
                            // 应该是 xxx.xxx 形态
                            if (string.IsNullOrEmpty(isil) == false)
                                pii = isil + "." + pii;
                            else if (string.IsNullOrEmpty(alternative) == false)
                                pii = alternative + "." + pii;
                        }
                        WpfClientInfo.WriteInfoLog($"写入本地读者缓存以前，修正 PII '{patron.PII}' 为 '{pii}' (UpdateLocalPatronRecord())");
                        patron.PII = pii;
                    }
                }
#endif
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
                /*
                string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(pii))
                    pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
                    */
                string oi_pii = GetPatronOiPii(dom);
                var patron = context.Patrons
    .Where(o => o.PII == oi_pii)
    .FirstOrDefault();
                if (patron != null)
                {
                    context.Patrons.Remove(patron);
                    context.SaveChanges();
                }

                return new NormalResult { Value = 0 };
            }
        }

        public static PatronItem GetPatronItem(string pii)
        {
            using (BiblioCacheContext context = new BiblioCacheContext())
            {
                if (_cacheDbCreated == false)
                {
                    context.Database.EnsureCreated();
                    _cacheDbCreated = true;
                }

                var patron = context.Patrons
    .Where(o => o.PII == pii)
    .FirstOrDefault();
                return patron;
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

        static bool _localAccountsCleared = false;
        // 禁止用本地缓存账户信息登录直到 ... 时间
        static DateTime _denyLocalLoginUntil = DateTime.MinValue;
        static string[] special_usernames = new string[] { "public", "reader", "opac", "图书馆" };

        public class LoginResult : NormalResult
        {
            public string OutputUserName { get; set; }
            public string Rights { get; set; }
            public string LibraryCode { get; set; }
        }

        // parameters:
        //      style   风格。local,network
        // result.Value
        //      -1:   出错
        //      0:    登录未成功
        //      1:    登录成功
        public static LoginResult WorkerLogin(string userName,
            string password,
            string style)
        {

            // 如果当前参数是不缓存账户，则要主动清除以前缓存的全部账户记录
            if (ShelfData.CacheWorkerAccount() == "false"
                && _localAccountsCleared == false)
            {
                ClearLocalAccounts();
                _localAccountsCleared = true;
            }

            if (Array.IndexOf(special_usernames, userName) != -1)
            {
                return new LoginResult
                {
                    Value = -1,
                    ErrorInfo = "不允许使用特殊账户登录"
                };
            }

            bool local = StringUtil.IsInList("local", style);
            bool network = StringUtil.IsInList("network", style);

            if (local == false && network == false)
                return new LoginResult
                {
                    Value = -1,
                    ErrorInfo = "style 参数至少要包含 local 和 network 中的一个值"
                };

            // 检查 App.dp2ServerUrl
            if (network)
            {
                if (string.IsNullOrEmpty(App.dp2ServerUrl) == true)
                    return new LoginResult
                    {
                        Value = -1,
                        ErrorInfo = "dp2library 服务器 URL 尚未配置，无法进行工作人员登录"
                    };
            }

            LibraryChannel channel = App.CurrentApp.GetChannel(userName);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                string strOutputUserName = "";
                string strRights = "";
                string strLibraryCode = "";
                string strError = "";

                bool local_succeed = false;
                if (network)
                {
                    // -1:   出错
                    // 0:    登录未成功
                    // 1:    登录成功
                    long lRet = channel.Login(userName,
                        password,
                        "type=worker,client=dp2ssl|" + WpfClientInfo.ClientVersion,
                        out strOutputUserName,
                        out strRights,
                        out strLibraryCode,
                        out strError);
                    if (lRet == -1 || lRet == 0)
                        return new LoginResult
                        {
                            Value = (int)lRet,
                            ErrorInfo = strError,
                        };
                }
                else if (local)
                {
                    // TODO: 登录失败则把失败的账户名记住，后面禁止登录操作一段时间
                    if (DateTime.Now < _denyLocalLoginUntil)
                        return new LoginResult
                        {
                            Value = -1,
                            ErrorInfo = "工作人员登录被暂时禁用"
                        };


                    // 尝试用本地缓存的读者记录登录
                    using (var context = new AccountCacheContext())
                    {
                        context.Database.EnsureCreated();

                        var account = context.Accounts.Where(o => o.UserName == userName).FirstOrDefault();
                        if (account == null)
                            return new LoginResult
                            {
                                Value = 0,
                                ErrorInfo = $"账户 {userName} 不存在"
                            };
                        if (account.HashedPassword != Cryptography.GetSHA1(password))
                        {
                            _denyLocalLoginUntil = DateTime.Now + TimeSpan.FromMinutes(5);
                            return new LoginResult
                            {
                                Value = 0,
                                ErrorInfo = $"账户 {userName} 密码不正确"
                            };
                        }

                        strOutputUserName = account.UserName;
                        strRights = account.Rights;
                        strLibraryCode = account.LibraryCodeList;
                    }

                    local_succeed = true;
                }

                if (StringUtil.IsInList("manageshelf", strRights) == false)
                    return new LoginResult
                    {
                        Value = -1,
                        ErrorInfo = $"工作人员账户 '{strOutputUserName}' 不具备 manageshelf 权限",
                        ErrorCode = "AccessDenied"
                    };

                // testing
                // channel.Logout(out strError);
                // 顺便保存到本地
                if (local_succeed == false
                    && ShelfData.CacheWorkerAccount() == "true")
                {
                    AccountItem account = new AccountItem();
                    account.UserName = strOutputUserName;
                    account.HashedPassword = Cryptography.GetSHA1(password);
                    account.Rights = strRights;
                    account.LibraryCodeList = strLibraryCode;
                    using (var context = new AccountCacheContext())
                    {
                        context.Database.EnsureCreated();

                        AddOrUpdate(context, account);
                    }
                }

                return new LoginResult
                {
                    Value = 1,
                    OutputUserName = strOutputUserName,
                    Rights = strRights,
                    LibraryCode = strLibraryCode,
                };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"WorkerLogin() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new LoginResult
                {
                    Value = -1,
                    ErrorInfo = $"WorkerLogin() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        // 清除本地缓存的所有账户
        public static void ClearLocalAccounts()
        {
            using (var context = new AccountCacheContext())
            {
                context.Database.EnsureCreated();

                var list = context.Accounts.ToList();
                context.Accounts.RemoveRange(list);
                context.SaveChanges();
            }
        }

        public static void AddOrUpdate(AccountCacheContext context,
AccountItem item)
        {
            try
            {
                // 保存到本地数据库
                context.Accounts.Add(item);
                context.SaveChanges();
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
            context.Accounts.Update(item);
            context.SaveChanges();
        }


        #region GetRfidCfg

        public class GetRfidCfgResult : NormalResult
        {
            public string Xml { get; set; }
            public string LibraryName { get; set; }
            // 2021/11/22
            public string ServerUid { get; set; }

            public override string ToString()
            {
                return $"Xml='{Xml}',LibraryName='{LibraryName}'," + base.ToString();
            }
        }

        // 获得 RFID 配置信息
        public static GetRfidCfgResult GetRfidCfg()
        {
            string strOutputInfo = "";
            string libraryName = "";
            string serverUid = "";

            LibraryChannel channel = App.CurrentApp.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                long lRet = channel.GetSystemParameter(
                    null,
                    "system",
                    "rfid",
                    out strOutputInfo,
                    out string strError);
                if (lRet == -1)
                    return new GetRfidCfgResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };

                lRet = channel.GetSystemParameter(
                    null,
                    "library",
                    "name",
                    out libraryName,
                    out strError);

                // 2021/11/22
                lRet = channel.GetVersion(null,
                    out string version,
                    out serverUid,
                    out strError);
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }

            // 
            /*
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
            */

            // 顺便保存到本地
            WpfClientInfo.Config.Set("cache",
                "rfidCfg",
                strOutputInfo);

            WpfClientInfo.Config.Set("cache",
                "libraryName",
                libraryName);

            WpfClientInfo.Config.Set("cache",
                "serverUid",
                serverUid);

            return new GetRfidCfgResult
            {
                Value = 1,
                Xml = strOutputInfo,
                LibraryName = libraryName,
                ServerUid = serverUid,
            };
        }

        // 从本地获得 RFID 配置信息(不访问 dp2library 服务器)
        public static GetRfidCfgResult GetRfidCfgFromLocal()
        {
            string value = WpfClientInfo.Config.Get("cache",
    "rfidCfg",
    null);
            if (value == null)
                return new GetRfidCfgResult();

            return new GetRfidCfgResult
            {
                Value = 1,
                Xml = value,
                LibraryName = WpfClientInfo.Config.Get("cache",
    "libraryName",
    null),
                ServerUid = WpfClientInfo.Config.Get("cache",
    "serverUid",
    null),
            };
        }

        #endregion

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
            channel.Timeout = TimeSpan.FromSeconds(30); // dp2library 刚启动后，第一次响应 GetSystemParameter() API 可能比较慢

            try
            {
                int nRedoCount = 0;
            REDO:
                long lRet = channel.GetSystemParameter(
null,
"circulation",
"locationTypes",
out strOutputInfo,
out string strError);
                if (lRet == -1)
                {
                    if ((channel.ErrorCode == ErrorCode.RequestTimeOut
                        || channel.ErrorCode == ErrorCode.ServerTimeout)
                        && nRedoCount < 3)
                    {
                        nRedoCount++;
                        goto REDO;
                    }

                    return new GetLocationListResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }
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

            // 顺便保存到本地
            WpfClientInfo.Config.Set("cache",
                "locationList",
                JsonConvert.SerializeObject(results));

            return new GetLocationListResult
            {
                Value = 1,
                List = results
            };
        }

        // 从本地获得馆藏地列表(不访问 dp2library 服务器)
        public static GetLocationListResult GetLocationListFromLocal()
        {
            string value = WpfClientInfo.Config.Get("cache",
    "locationList",
    null);
            if (value == null)
                return new GetLocationListResult();

            return new GetLocationListResult
            {
                Value = 1,
                List = JsonConvert.DeserializeObject<List<string>>(value)
            };
        }

        public class GetRightsTableResult : NormalResult
        {
            public string Xml { get; set; }
        }

        // 获得读者权限定义 XML
        public static GetRightsTableResult GetRightsTable()
        {
            LibraryChannel channel = App.CurrentApp.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                long lRet = channel.GetSystemParameter(
        null,
        "circulation",
        "rightsTable",
        out string strOutputInfo,
        out string strError);
                if (lRet == -1)
                    return new GetRightsTableResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };

                if (string.IsNullOrEmpty(strOutputInfo) == false)
                    strOutputInfo = "<rightsTable>" + strOutputInfo + "</rightsTable>";

                return new GetRightsTableResult
                {
                    Value = (int)lRet,
                    Xml = strOutputInfo
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public delegate void delegate_showText(string text);

        public class SimuTagInfo
        {
            public string PII { get; set; }
            public string OI { get; set; }
            public string UID { get; set; }
            public string AccessNo { get; set; }
        }

        public class TagsInfoResult : NormalResult
        {
            public List<SimuTagInfo> TagInfos { get; set; }
        }

        // 从 dp2library 服务器检索获得模拟 RFID 图书标签所需的数据
        // parameters:
        //
        public static TagsInfoResult DownloadTagsInfo(
            List<string> item_dbnames,
            int max_count,
            delegate_showText func_showProgress,
            CancellationToken token)
        {
            WpfClientInfo.WriteInfoLog($"开始 DownloadTagsInfo()");
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为册记录检索需要一定时间
            try
            {
                List<SimuTagInfo> infos = new List<SimuTagInfo>();

                if (item_dbnames == null)
                {
                    long lRet = channel.GetSystemParameter(
    null,
    "item",
    "dbnames",
    out string strValue,
    out string strError);
                    if (lRet == -1)
                        return new TagsInfoResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    item_dbnames = StringUtil.SplitList(strValue);
                    StringUtil.RemoveBlank(ref item_dbnames);
                }

                foreach (string dbName in item_dbnames)
                {
                    func_showProgress?.Invoke($"正在从 {dbName} 获取信息 ...");

                    int nRedoCount = 0;
                REDO:
                    if (token.IsCancellationRequested)
                        return new TagsInfoResult
                        {
                            Value = -1,
                            ErrorInfo = "用户中断"
                        };
                    // 检索全部读者库记录
                    long lRet = channel.SearchItem(null,
    dbName, // "<all>",
    "",
    -1,
    "__id",
    "left",
    "zh",
    null,   // strResultSetName
    "", // strSearchStyle
    "", // strOutputStyle
    out string strError);
                    if (lRet == -1)
                    {
                        WpfClientInfo.WriteErrorLog($"SearchItem() 出错, strError={strError}, channel.ErrorCode={channel.ErrorCode}");

                        // 一次重试机会
                        if (lRet == -1
                            && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                            && nRedoCount < 2)
                        {
                            nRedoCount++;
                            goto REDO;
                        }

                        return new TagsInfoResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    }

                    long hitcount = lRet;

                    WpfClientInfo.WriteInfoLog($"{dbName} 共检索命中册记录 {hitcount} 条");


                    DateTime search_time = DateTime.Now;

                    int skip_count = 0;
                    int error_count = 0;

                    if (hitcount > 0)
                    {
                        // string strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/uid";

                        // 把超时时间改短一点
                        var timeout0 = channel.Timeout;
                        channel.Timeout = TimeSpan.FromSeconds(20);

                        try
                        {
                            // 获取和存储记录
                            ResultSetLoader loader = new ResultSetLoader(channel,
                null,
                null,
                "id,xml",
                "zh");

                            // loader.Prompt += this.Loader_Prompt;
                            int i = 0;
                            foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                            {
                                if (token.IsCancellationRequested)
                                    return new TagsInfoResult
                                    {
                                        Value = -1,
                                        ErrorInfo = "用户中断"
                                    };

                                var xml = record.RecordBody.Xml;

                                XmlDocument dom = new XmlDocument();
                                dom.LoadXml(xml);

                                var info = new SimuTagInfo();
                                info.PII = DomUtil.GetElementText(dom.DocumentElement, "barcode");

                                if (string.IsNullOrEmpty(info.PII))
                                    continue;

                                if (info.PII.Contains("_"))
                                    continue;

                                {
                                    string oi = "";
                                    string location = DomUtil.GetElementText(dom.DocumentElement, "location");
                                    location = StringUtil.GetPureLocation(location);
                                    var ret = ShelfData.GetOwnerInstitution(location, out string isil, out string alternative);
                                    if (ret == true)
                                    {
                                        if (string.IsNullOrEmpty(isil) == false)
                                            oi = isil;
                                        else if (string.IsNullOrEmpty(alternative) == false)
                                            oi = alternative;
                                    }
                                    info.OI = oi;
                                }

                                // info.OI = DomUtil.GetElementText(dom.DocumentElement, "oi");

                                if (string.IsNullOrEmpty(info.OI))
                                    continue;

                                info.UID = DomUtil.GetElementText(dom.DocumentElement, "uid");
                                info.AccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
                                infos.Add(info);
                                i++;

                                if (i >= max_count)
                                    break;
                            }

                        }
                        finally
                        {
                            channel.Timeout = timeout0;
                        }
                    }

                    WpfClientInfo.WriteInfoLog($"dbName='{dbName}'。skip_count={skip_count}, error_count={error_count}");

                }
                return new TagsInfoResult
                {
                    TagInfos = infos,
                };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"DownloadTagsInfo() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new TagsInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadTagsInfo() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);

                WpfClientInfo.WriteInfoLog($"结束 DownloadTagsInfo()");
            }
        }

        // 对照服务器时钟和本地时钟
        // parameters:
        //      length  时间差异范围。大于这个范围就会返回出错
        public static NormalResult CheckServerClock(TimeSpan length)
        {
            LibraryChannel channel = App.CurrentApp.GetChannel();
            try
            {
                string strTime = "";
                DateTime start = DateTime.Now;
                long lRet = channel.GetClock(
                    null,
                    out strTime,
                    out string strError);
                DateTime end = DateTime.Now;
                if (lRet == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };

                DateTime server_time = DateTimeUtil.FromRfc1123DateTimeString(strTime);
                server_time = server_time.ToLocalTime();

                // API 调用折返中途的前端本地时间
                DateTime local_time = start + TimeSpan.FromTicks((end - start).Ticks);

                TimeSpan delta = server_time - local_time;
                if (server_time < local_time)
                    delta = local_time - server_time;

                if (delta > length)
                {
                    strError = $"本地时钟和服务器时钟差异过大(超过 {length.ToString()})，为 "
                        + delta.ToString()
                        + "。测试时的服务器时间为: " + server_time.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "  本地时间为: " + local_time.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = "outOfRange"
                    };
                }
                return new NormalResult
                {
                    Value = 0,
                    ErrorInfo = $"时间正常。测试时的服务器时间为: { server_time.ToString("yyyy-MM-dd HH:mm:ss.ffff")}  本地时间为: { local_time.ToString("yyyy-MM-dd HH:mm:ss.ffff")}"
                };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"CheckServerClock() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"CheckServerClock() 出现异常：{ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public class VerifyClockResult : NormalResult
        {
            // 前端硬件时钟和服务器时钟的差额 ticks
            public long DeltaTicks { get; set; }
        }

        // 依据 dp2library 校验本地时间
        public static VerifyClockResult VerifyClock()
        {
            LibraryChannel channel = App.CurrentApp.GetChannel();
            try
            {
                string strTime = "";
                DateTime start = DateTime.Now;
                long lRet = channel.GetClock(
                    null,
                    out strTime,
                    out string strError);
                DateTime end = DateTime.Now;
                if (lRet == -1)
                    return new VerifyClockResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };

                DateTime server_time = DateTimeUtil.FromRfc1123DateTimeString(strTime);
                server_time = server_time.ToLocalTime();

                // API 调用折返中途的前端本地时间
                DateTime local_time = start + TimeSpan.FromTicks((end - start).Ticks);

                TimeSpan delta = server_time - local_time;

                return new VerifyClockResult { DeltaTicks = delta.Ticks };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"VerifyClock() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new VerifyClockResult
                {
                    Value = -1,
                    ErrorInfo = $"VerifyClock() 出现异常：{ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
            }
        }


        // 清除前端缓存的所有册记录和书目摘要
        public static void ClearCachedEntities()
        {
            using (BiblioCacheContext context = new BiblioCacheContext())
            {
                if (_cacheDbCreated == false)
                {
                    context.Database.EnsureCreated();
                    _cacheDbCreated = true;
                }

                {
                    var list = context.Entities.ToList();
                    if (list.Count > 0)
                    {
                        context.Entities.RemoveRange(list);
                        context.SaveChanges();
                    }
                }

                {
                    var list = context.BiblioSummaries.ToList();
                    if (list.Count > 0)
                    {
                        context.BiblioSummaries.RemoveRange(list);
                        context.SaveChanges();
                    }
                }
            }
        }

    }
}
