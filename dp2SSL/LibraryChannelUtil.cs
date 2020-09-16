﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Runtime.Remoting.Activation;

using Microsoft.VisualStudio.Threading;
using Microsoft.Data.Sqlite;

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.WPF;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

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
        }

        static bool _cacheDbCreated = false;

        static AsyncSemaphore _channelLimit = new AsyncSemaphore(2);

        // 获得册记录信息和书目摘要信息
        // parameters:
        //      style   风格。network 表示只从网络获取册记录；否则优先从本地获取，本地没有再从网络获取册记录。无论如何，书目摘要都是尽量从本地获取
        // .Value
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public static async Task<GetEntityDataResult> GetEntityDataAsync(string pii,
            string style)
        {
            bool network = StringUtil.IsInList("network", style);
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
            catch
            {

            }
        }

        static async Task AddOrUpdateAsync(BiblioCacheContext context,
            EntityItem item)
        {
            // 调整 PII 字段，尽量规整为 OI.PII 形态
            SetPII(item);

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
                        if (pii.Contains(".") == false)
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
                        new_borrow.SetAttribute("period", borrow_info.Period);
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

        #region GetRfidCfg

        public class GetRfidCfgResult : NormalResult
        {
            public string Xml { get; set; }
            public string LibraryName { get; set; }

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

            return new GetRfidCfgResult
            {
                Value = 1,
                Xml = strOutputInfo,
                LibraryName = libraryName,
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
    null)
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
    }
}
