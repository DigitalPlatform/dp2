﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.Data.Sqlite;

using static dp2SSL.Models.PatronReplication;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;

namespace dp2SSL.Models
{
    public static class EntityReplication
    {
        public delegate void Delegate_writeLog(string text);

        static int _inDownloadingPatron = 0;

#if NO
        // 第一阶段：获得全部册记录，进入本地数据库
        // result.Value
        //      -1  出错
        //      >=0 实际获得的读者记录条数
        public static async Task<ReplicationPlan> DownloadAllEntityRecordAsync(
            Delegate_writeLog writeLog,
            CancellationToken token)
        {
            _inDownloadingPatron++;

            if (_inDownloadingPatron > 1)
            {
                _inDownloadingPatron--;
                return new ReplicationPlan
                {
                    Value = -1,
                    ErrorCode = "running",
                    ErrorInfo = "前一次的“下载全部册记录到本地缓存”过程还在进行中，本次触发被放弃"
                };
            }

            writeLog?.Invoke($"开始下载全部册记录到本地缓存");
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为读者记录检索需要一定时间
            try
            {

                ReplicationPlan plan = PatronReplication.GetReplicationPlan(channel);

                writeLog?.Invoke($"GetReplicationPlan() return {plan.ToString()}");

                if (plan.Value == -1)
                    return plan;

                int nRedoCount = 0;
            REDO:
                if (token.IsCancellationRequested)
                    return new ReplicationPlan
                    {
                        Value = -1,
                        ErrorInfo = "用户中断"
                    };
                // 检索全部实体库记录
                long lRet = channel.SearchItem(null,  // stop,
"<all>",
"",
-1,
"__id",
"left",
"zh",
null,   // strResultSetName
"",     // strSearchStyle
"", // strOutputStyle
out string strError);
                if (lRet == -1)
                {
                    writeLog?.Invoke($"SearchItem() 出错, strError={strError}, channel.ErrorCode={channel.ErrorCode}");

                    // 一次重试机会
                    if (lRet == -1
                        && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                        && nRedoCount < 2)
                    {
                        nRedoCount++;
                        goto REDO;
                    }

                    return new ReplicationPlan
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                long hitcount = lRet;

                writeLog?.Invoke($"共检索命中册记录 {hitcount} 条");

                // 把超时时间改短一点
                channel.Timeout = TimeSpan.FromSeconds(20);

                DateTime search_time = DateTime.Now;

                Hashtable pii_table = new Hashtable();
                int skip_count = 0;
                int error_count = 0;

                string strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/uid";

                // 获取和存储记录
                ResultSetLoader loader = new ResultSetLoader(channel,
    null,
    null,
    strStyle,   // $"id,xml,timestamp",
    "zh");
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    context.Database.EnsureCreated();

                    // 删除 Entities 里面的已有记录
                    context.Entities.RemoveRange(context.Entities.ToList());
                    await context.SaveChangesAsync(token);

                    // loader.Prompt += this.Loader_Prompt;
                    if (hitcount > 0)
                    {
                        int i = 0;
                        foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                        {
                            if (token.IsCancellationRequested)
                                return new ReplicationPlan
                                {
                                    Value = -1,
                                    ErrorInfo = "用户中断"
                                };

                            if (record.Cols != null)
                            {
                                string barcode = "";
                                if (record.Cols.Length > 0)
                                    barcode = record.Cols[0];
                                string location = "";
                                if (record.Cols.Length > 1)
                                    location = record.Cols[1];

                                // 2021/1/31
                                // 推算出 OI
                                /*
                                string oi = "";
                                {
                                    location = StringUtil.GetPureLocation(location);
                                    var ret = ShelfData.GetOwnerInstitution(location, out string isil, out string alternative);
                                    if (ret == true)
                                    {
                                        if (string.IsNullOrEmpty(isil) == false)
                                            oi = isil;
                                        else if (string.IsNullOrEmpty(alternative) == false)
                                            oi = alternative;
                                    }
                                }
                                */
                                location = StringUtil.GetPureLocation(location);
                                string oi = "";
                                if (oi_table.ContainsKey(location))
                                    oi = (string)oi_table[location];
                                else
                                {
                                    oi = GetInstitution(location);
                                    oi_table[location] = oi;
                                }

                                string uid = "";
                                if (record.Cols.Length > 2)
                                    uid = record.Cols[2];
                                if (string.IsNullOrEmpty(barcode) == false
                                    && string.IsNullOrEmpty(uid) == false)
                                    uid_table[uid] = oi + "." + barcode;
                            }


                            PatronItem item = new PatronItem();

                            // result.Value:
                            //      -1  出错
                            //      0   需要跳过这条读者记录
                            //      1   成功
                            var result = Set(item, record, search_time);
                            if (result.Value == -1 || result.Value == 0)
                            {
                                // TODO: 是否汇总报错信息？

                                if (result.Value == -1)
                                {
                                    writeLog?.Invoke($"Set() ({item.RecPath}) 出错: {result.ErrorInfo}");
                                    error_count++;
                                }
                                if (result.Value == 0)
                                    skip_count++;
                                continue;
                            }

                            // 
                            if (pii_table.ContainsKey(result.PII))
                            {
                                string recpath = (string)pii_table[result.PII];
                                writeLog?.Invoke($"发现读者记录 {item.RecPath} 的 PII '{result.PII}' 和 {recpath} 的 PII 重复了。跳过它");
                                continue;
                            }

                            pii_table[result.PII] = item.RecPath;

                            // TODO: PII 应该是包含 OI 的严格形态
                            context.Patrons.Add(item);

                            if ((i % 10) == 0)
                                await context.SaveChangesAsync(token);

                            i++;
                        }

                        await context.SaveChangesAsync(token);
                    }
                }

                writeLog?.Invoke($"plan.StartDate='{plan.StartDate}'。skip_count={skip_count}, error_count={error_count}。返回");

                return new ReplicationPlan
                {
                    Value = (int)hitcount,
                    StartDate = plan.StartDate
                };
            }
            catch (Exception ex)
            {
                // 2020/9/26
                writeLog?.Invoke($"DownloadAllPatronRecord() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new ReplicationPlan
                {
                    Value = -1,
                    ErrorInfo = $"DownloadAllPatronRecord() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);

                writeLog?.Invoke($"结束下载全部读者记录到本地缓存");

                _inDownloadingPatron--;
            }
        }
#endif

        public static async Task<NormalResult> DownloadAllEntityRecordAsync(
    List<string> item_dbnames,
    List<string> unprocessed_dbnames,
    Delegate_writeLog writeLog,
    CancellationToken token)
        {
            writeLog?.Invoke($"开始下载全部册记录到本地缓存。item_dbnames={StringUtil.MakePathList(item_dbnames)}");

            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为册记录检索需要一定时间
            try
            {
                bool first_round = false;
                if (item_dbnames == null)
                {
                    first_round = true;
                    long lRet = channel.GetSystemParameter(
    null,
    "item",
    "dbnames",
    out string strValue,
    out string strError);
                    if (lRet == -1)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    item_dbnames = StringUtil.SplitList(strValue);
                    StringUtil.RemoveBlank(ref item_dbnames);
                }

                unprocessed_dbnames.AddRange(item_dbnames);

                // location --> oi
                Hashtable oi_table = new Hashtable();


                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    context.Database.EnsureCreated();
                    if (first_round)
                    {
                        // 删除 Entities 里面的已有记录
                        context.Entities.RemoveRange(context.Entities.ToList());
                        await context.SaveChangesAsync(token);
                        // 删除 BiblioSummaries 里面的已有记录
                        context.BiblioSummaries.RemoveRange(context.BiblioSummaries.ToList());
                        await context.SaveChangesAsync(token);
                    }
                }

                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    foreach (string name in item_dbnames)
                    {
                        // func_showProgress?.Invoke($"正在从 {dbName} 获取信息 ...");

                        // name 形态为 数据库名:开始偏移
                        string dbName = name;
                        long start = 0;
                        {
                            var parts = StringUtil.ParseTwoPart(name, ":");
                            dbName = parts[0];
                            string offset = parts[1];
                            if (string.IsNullOrEmpty(offset) == false)
                            {
                                if (long.TryParse(offset, out start) == false)
                                    return new NormalResult
                                    {
                                        Value = -1,
                                        ErrorInfo = $"条目 '{name}' 格式不正确。应为 数据库名:偏移量 形态"
                                    };
                            }
                        }

                        int nRedoCount = 0;
                    REDO:
                        if (token.IsCancellationRequested)
                            return new NormalResult
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
        "download111",   // strResultSetName
        "", // strSearchStyle
        "", // strOutputStyle
        out string strError);
                        if (lRet == -1)
                        {
                            writeLog?.Invoke($"SearchItem() 出错, strError={strError}, channel.ErrorCode={channel.ErrorCode}");

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
                                Value = -1,
                                ErrorInfo = strError,
                                ErrorCode = channel.ErrorCode.ToString()
                            };
                        }

                        long hitcount = lRet;

                        writeLog?.Invoke($"{dbName} 共检索命中册记录 {hitcount} 条");

                        // 把超时时间改短一点
                        channel.Timeout = TimeSpan.FromSeconds(20);

                        DateTime search_time = DateTime.Now;

                        int skip_count = 0;
                        int error_count = 0;

                        if (hitcount > 0)
                        {
                            // string strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/uid";

                            // 获取和存储记录
                            ResultSetLoader loader = new ResultSetLoader(channel,
                null,
                "download111",
                $"id,xml,timestamp",
                "zh");
                            loader.Start = start;

                            // loader.Prompt += this.Loader_Prompt;
                            long i = start;
                            foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    int index = IndexOf(unprocessed_dbnames, dbName);
                                    if (index != -1)
                                    {
                                        unprocessed_dbnames.RemoveAt(index);
                                        unprocessed_dbnames.Insert(index, dbName + ":" + i);
                                    }

                                    return new NormalResult
                                    {
                                        Value = -1,
                                        ErrorInfo = "用户中断"
                                    };
                                }

                                string item_xml = record.RecordBody.Xml;
                                var timestamp = record.RecordBody.Timestamp;
                                string item_recpath = record.Path;

                                XmlDocument itemdom = new XmlDocument();
                                try
                                {
                                    itemdom.LoadXml(item_xml);
                                }
                                catch (Exception ex)
                                {
                                    writeLog?.Invoke($"册记录 {item_recpath} 的 XML 装入 DOM 时出错: {ex.Message}");
                                    continue;
                                }

                                string barcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");
                                if (string.IsNullOrEmpty(barcode))
                                    continue;

                                string location = DomUtil.GetElementText(itemdom.DocumentElement, "location");

                                location = StringUtil.GetPureLocation(location);
                                string oi = "";
                                if (oi_table.ContainsKey(location))
                                    oi = (string)oi_table[location];
                                else
                                {
                                    oi = InventoryData.GetInstitution(location);
                                    oi_table[location] = oi;
                                }

                                var item = new EntityItem
                                {
                                    // PII = pii,
                                    Xml = item_xml,
                                    RecPath = item_recpath,
                                    Timestamp = timestamp,
                                };

                                // 调整 PII 字段，尽量规整为 OI.PII 形态
                                SetPII(item, oi);

                                // 保存册记录到本地数据库
                                await AddOrUpdateAsync(context,
                                    item);

                                // 保存书目摘要到本地数据库
                                await GetBiblioSummaryAsync(
    channel,
    context,
    item.PII,
    item_recpath);

                                i++;

                                /*
                                if ((i % 100) == 0)
                                {
                                    // func_showProgress?.Invoke($"正在从 {dbName} 获取信息 ({i.ToString()}) {record.Path} ...");
                                }
                                */
                            }
                        }

                        writeLog?.Invoke($"dbName='{dbName}'。skip_count={skip_count}, error_count={error_count}");

                        {
                            int index = IndexOf(unprocessed_dbnames, dbName);
                            if (index != -1)
                                unprocessed_dbnames.RemoveAt(index);
                        }
                    }
                }

                writeLog?.Invoke($"下载全部册记录到本地缓存，全部数据库成功完成");

                return new NormalResult
                {
                    Value = 0,
                };
            }
            catch (Exception ex)
            {
                writeLog?.Invoke($"DownloadAllEntityRecordAsync() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadAllEntityRecordAsync() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);

                writeLog?.Invoke($"结束下载全部册记录到本地缓存。unprocessed_dbnames={StringUtil.MakePathList(unprocessed_dbnames)}");
            }

            int IndexOf(List<string> names, string db_name)
            {
                int j = 0;
                foreach (var one in names)
                {
                    if (db_name == one || one.StartsWith(db_name + ":"))
                        return j;
                    j++;
                }

                return -1;
            }
        }

        // 从 dp2library 服务器获取书目摘要
        public static async Task<NormalResult> GetBiblioSummaryAsync(
            LibraryChannel channel,
            BiblioCacheContext context,
            string pii,
            string strConfirmItemRecPath)
        {
            int nRedoCount = 0;
        REDO_GETBIBLIOSUMMARY:
            long lRet = channel.GetBiblioSummary(
null,
pii,
strConfirmItemRecPath,
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

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError,
                    ErrorCode = channel.ErrorCode.ToString()
                };
            }
            else
            {
                strSummary = strSummary?.Replace(". -- ", "\r\n");   // .Replace("/", "\r\n");

                // 存入数据库备用
                if (lRet == 1 && string.IsNullOrEmpty(strSummary) == false)
                {
                    try
                    {
                        await LibraryChannelUtil.AddOrUpdateAsync(context, new BiblioSummaryItem
                        {
                            PII = pii,
                            BiblioSummary = strSummary
                        });
                    }
                    catch (Exception ex)
                    {
                        string error = $"GetEntityDataAsync() 中保存 summary 时(PII 为 '{pii}')出现异常:{ExceptionUtil.GetDebugText(ex)}";
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = error
                        };
                    }
                }

                return new NormalResult();
            }
        }

        public static async Task AddOrUpdateAsync(BiblioCacheContext context,
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

        static void SetPII(EntityItem item, string oi)
        {
            try
            {
                XmlDocument itemdom = new XmlDocument();
                itemdom.LoadXml(item.Xml);

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


        /* 日志记录格式
<root>
  <operation>setEntity</operation> 操作类型
  <action>new</action> 具体动作。有new change delete 3种。2019/7/30 增加 transfer，transfer 行为和 change 相似
  <style>...</style> 风格。有force nocheckdup noeventlog 3种
  <record recPath='中文图书实体/3'><root><parent>2</parent><barcode>0000003</barcode><state>状态2</state><location>阅览室</location><price></price><bookType>教学参考</bookType><registerNo></registerNo><comment>test</comment><mergeComment></mergeComment><batchNo>111</batchNo><borrower></borrower><borrowDate></borrowDate><borrowPeriod></borrowPeriod></root></record> 记录体
  <oldRecord recPath='中文图书实体/3'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 08:41:46 GMT</operTime> 操作时间
</root>

注：1) 当<action>为delete时，没有<record>元素。为new时，没有<oldRecord>元素。
	2) <record>中的内容, 涉及到流通的<borrower><borrowDate><borrowPeriod>等, 在日志恢复阶段, 都应当无效, 这几个内容应当从当前位置库中记录获取, 和<record>中其他内容合并后, 再写入数据库
	3) 一次SetEntities()API调用, 可能创建多条日志记录。
         
         * */
        public static async Task<NormalResult> TraceSetEntity(
XmlDocument domLog,
PatronReplication.ProcessInfo info)
        {
            try
            {
                string strAction = DomUtil.GetElementText(domLog.DocumentElement, "action");

                string strOperTime = DomUtil.GetElementText(domLog.DocumentElement, "operTime");
                DateTime operTime = DateTimeUtil.FromRfc1123DateTimeString(strOperTime).ToLocalTime();

                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "transfer")
                {
                    string strNewRecPath = "";
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"日志记录中缺<record>元素。日志记录内容如下：{domLog.OuterXml}"
                        };
                    }
                    else
                    {
                        strNewRecPath = DomUtil.GetAttr(node, "recPath");
                    }

                    /*
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "delete")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"日志记录中缺<oldRecord>元素。日志记录内容如下：{domLog.OuterXml}"
                            };
                        }

                        if (node != null)
                        {
                            strOldRecPath = DomUtil.GetAttr(node, "recPath");
                            if (string.IsNullOrEmpty(strOldRecPath) == true)
                            {
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = "日志记录中<oldRecord>元素内缺recPath属性值"
                                };
                            }
                        }
                    }
                    */

                    return await UpdateLocalEntityRecordAsync(
            strNewRecPath,
            strRecord);
                }
                else if (strAction == "delete")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "日志记录中缺<oldRecord>元素"
                        };
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");


                    return DeleteLocalEntityRecord(
strRecPath,
strOldRecord);
                }
                else
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "无法识别的<action>内容 '" + strAction + "'"
                    };
                }
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"TraceSetEntity() 出现异常: {ex.Message}"
                };
            }
        }

        static async Task<NormalResult> UpdateLocalEntityRecordAsync(
            string strRecPath,
            string strRecord)
        {
            XmlDocument itemdom = new XmlDocument();
            try
            {
                itemdom.LoadXml(strRecord);
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"装载 XML 记录进入 DOM 时出错: {ex.Message}"
                };
            }

            string barcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(barcode))
                return new NormalResult();

            string location = DomUtil.GetElementText(itemdom.DocumentElement, "location");

            location = StringUtil.GetPureLocation(location);
            string oi = InventoryData.GetInstitution(location);

            var item = new EntityItem
            {
                // PII = pii,
                Xml = strRecord,
                RecPath = strRecPath,
                Timestamp = null,
            };

            // 调整 PII 字段，尽量规整为 OI.PII 形态
            SetPII(item, oi);

            using (BiblioCacheContext context = new BiblioCacheContext())
            {
                // 保存册记录到本地数据库
                await AddOrUpdateAsync(context,
                item);

                LibraryChannel channel = App.CurrentApp.GetChannel();
                var old_timeout = channel.Timeout;
                channel.Timeout = TimeSpan.FromSeconds(5);
                try
                {
                    // 保存书目摘要到本地数据库
                    await GetBiblioSummaryAsync(
channel,
context,
item.PII,
strRecPath);
                }
                finally
                {
                    channel.Timeout = old_timeout;
                    App.CurrentApp.ReturnChannel(channel);
                }
            }

            return new NormalResult();
        }

        static NormalResult DeleteLocalEntityRecord(
    string strRecPath,
    string strRecord)
        {
            XmlDocument itemdom = new XmlDocument();
            try
            {
                itemdom.LoadXml(strRecord);
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"装载 XML 记录进入 DOM 时出错: {ex.Message}"
                };
            }

            string barcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(barcode))
                return new NormalResult();

            string location = DomUtil.GetElementText(itemdom.DocumentElement, "location");

            location = StringUtil.GetPureLocation(location);
            string oi = InventoryData.GetInstitution(location);

            var item = new EntityItem
            {
                // PII = pii,
                Xml = strRecord,
                RecPath = strRecPath,
                Timestamp = null,
            };

            // 调整 PII 字段，尽量规整为 OI.PII 形态
            SetPII(item, oi);

            using (BiblioCacheContext context = new BiblioCacheContext())
            {
                string oi_pii = item.PII;
                var entity = context.Entities
    .Where(o => o.PII == oi_pii)
    .FirstOrDefault();
                if (entity != null)
                {
                    context.Entities.Remove(entity);
                    context.SaveChanges();
                }

                var summary = context.BiblioSummaries
                    .Where(o => o.PII == oi_pii)
                    .FirstOrDefault();
                if (summary != null)
                {
                    context.BiblioSummaries.Remove(summary);
                    context.SaveChanges();
                }
            }

            return new NormalResult();
        }

    }
}
