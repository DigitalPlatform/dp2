using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using static DigitalPlatform.Z3950.ZClient;
using DigitalPlatform;
using DigitalPlatform.Script;
using DigitalPlatform.Z3950;
using DigitalPlatform.Z3950.UI;
using System.Threading;
using DigitalPlatform.Core;
using Microsoft.VisualStudio.Threading;
using DigitalPlatform.Text;
using System.Diagnostics;

namespace dp2Circulation
{
    /// <summary>
    /// 用于 Z39.50 检索的工具类
    /// 它管理了若干个 Z39.50 通道
    /// </summary>
    public class Z3950Searcher : IDisposable
    {
        bool _searching = false;

        public bool InSearching
        {
            get
            {
                return _searching;
            }
            set
            {
                _searching = value;
            }
        }

        // 通道集合
        List<ZClientChannel> _channels = new List<ZClientChannel>();

        // Z39.50 服务器定义 XML 文件的文件名
        public string XmlFileName { get; set; }

        public void Dispose()
        {
            this.Clear();
        }

        public void Clear()
        {
            foreach (ZClientChannel channel in this._channels)
            {
                if (channel != null)
                    channel.Dispose();
            }
            this._channels.Clear();
        }

        // 从 XML 文件中装载服务器配置信息
        public NormalResult LoadServer(string xmlFileName,
            Marc8Encoding marc8Encoding)
        {
            this.Clear();

            this.XmlFileName = xmlFileName;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(xmlFileName);
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }

            XmlNodeList servers = dom.DocumentElement.SelectNodes("server");
            foreach (XmlElement server in servers)
            {
                var result = ZServerUtil.GetTarget(server,
                    marc8Encoding,
                    out TargetInfo targetInfo);
                if (result.Value == -1)
                    return result;
                ZClientChannel channel = new ZClientChannel
                {
                    ServerName = server.GetAttribute("name"),
                    ZClient = new ZClient(),
                    TargetInfo = targetInfo,
                    Enabled = ZServerListDialog.IsEnabled(server.GetAttribute("enabled"), true)
                };
                _channels.Add(channel);
            }

            return new NormalResult();
        }

        public void GraceStop()
        {
            this._searching = false;
        }

        public void Stop()
        {
            foreach (ZClientChannel channel in _channels)
            {
                channel.ZClient.CloseConnection();
            }
        }

        public void ClearChannelsFetched()
        {
            foreach (ZClientChannel channel in _channels)
            {
                channel._total_fetched += channel._fetched;
                channel._fetched = 0;
            }
        }

        public delegate void delegate_searchCompleted(ZClientChannel channel, SearchResult result);
        public delegate void delegate_presentCompleted(ZClientChannel channel, PresentResult result);

#if NO
        // 首次检索
        public async Task<NormalResult> Search(
            UseCollection useList,
            IsbnSplitter isbnSplitter,
            string strQueryWord,
            int nMax,
            string strFromStyle,
            string strMatchStyle,
            delegate_searchCompleted searchCompleted,
            delegate_presentCompleted presentCompleted)
        {
            _searching = true;
            try
            {
                foreach (ZClientChannel channel in _channels)
                {
                    if (_searching == false)
                        break;

                    if (channel.Enabled == false)
                        continue;

                    var _targetInfo = channel.TargetInfo;
                    IsbnConvertInfo isbnconvertinfo = new IsbnConvertInfo
                    {
                        IsbnSplitter = isbnSplitter,
                        ConvertStyle =
        (_targetInfo.IsbnAddHyphen == true ? "addhyphen," : "")
        + (_targetInfo.IsbnRemoveHyphen == true ? "removehyphen," : "")
        + (_targetInfo.IsbnForce10 == true ? "force10," : "")
        + (_targetInfo.IsbnForce13 == true ? "force13," : "")
        + (_targetInfo.IsbnWild == true ? "wild," : "")
        // TODO:
        + (_targetInfo.IssnForce8 == true ? "issnforce8," : "")
                    };

                    string strQueryString = "";
                    {
                        // 创建只包含一个检索词的简单 XML 检索式
                        // 注：这种 XML 检索式不是 Z39.50 函数库必需的。只是用它来方便构造 API 检索式的过程
                        string strQueryXml = BuildQueryXml(strQueryWord, strFromStyle);
                        // 将 XML 检索式变化为 API 检索式
                        var result = ZClient.ConvertQueryString(
                            useList,
                            strQueryXml,
                            isbnconvertinfo,
                            out strQueryString);
                        if (result.Value == -1)
                        {
                            searchCompleted?.Invoke(channel, new SearchResult(result));
                            var final_result = new NormalResult { Value = result.Value, ErrorInfo = result.ErrorInfo };
                            if (result.ErrorCode == "notFound")
                                final_result.ErrorCode = "useNotFound";
                            return final_result;
                        }
                    }

                    REDO_SEARCH:
                    {
                        // return Value:
                        //      -1  出错
                        //      0   成功
                        //      1   调用前已经是初始化过的状态，本次没有进行初始化
                        // InitialResult result = _zclient.TryInitialize(_targetInfo).GetAwaiter().GetResult();
                        // InitialResult result = _zclient.TryInitialize(_targetInfo).Result;
                        InitialResult result = await channel.ZClient.TryInitialize(_targetInfo);
                        if (result.Value == -1)
                        {
                            searchCompleted?.Invoke(channel, new SearchResult(result));
                            // TODO: 是否继续向后检索其他 Z39.50 服务器?
                            return new NormalResult { Value = -1, ErrorInfo = "Initialize error: " + result.ErrorInfo };
                        }
                    }

                    // result.Value:
                    //		-1	error
                    //		0	fail
                    //		1	succeed
                    // result.ResultCount:
                    //      命中结果集内记录条数 (当 result.Value 为 1 时)
                    SearchResult search_result = await channel.ZClient.Search(
            strQueryString,
            _targetInfo.DefaultQueryTermEncoding,
            _targetInfo.DbNames,
            _targetInfo.PreferredRecordSyntax,
            "default");
                    if (search_result.Value == -1 || search_result.Value == 0)
                    {
                        if (search_result.ErrorCode == "ConnectionAborted")
                        {
                            // 自动重试检索
                            goto REDO_SEARCH;
                        }
                    }

                    searchCompleted?.Invoke(channel, search_result);
                    channel._resultCount = search_result.ResultCount;

                    if (search_result.Value == -1
                        || search_result.Value == 0
                        || search_result.ResultCount == 0)
                        continue;

                    var present_result = await FetchRecords(channel, 10);

                    presentCompleted?.Invoke(channel, present_result);

                    if (present_result.Value != -1)
                        channel._fetched += present_result.Records.Count;
                }

                return new NormalResult();
            }
            finally
            {
                _searching = false;
            }
        }
#endif
        // 独立 Task 版本
        // 首次检索
        public async Task<NormalResult> SearchAsync(
            UseCollection useList,
            IsbnSplitter isbnSplitter,
            string strQueryWord,
            int nMax,
            string strFromStyle,
            string strMatchStyle,
            delegate_searchCompleted searchCompleted,
            delegate_presentCompleted presentCompleted)
        {
            _searching = true;
            try
            {
                List<Task<NormalResult>> tasks = new List<Task<NormalResult>>();
                List<ZClientChannel> channels = new List<ZClientChannel>();
                foreach (ZClientChannel channel in _channels)
                {
                    if (_searching == false)
                        break;

                    if (channel.Enabled == false)
                        continue;

                    var task = Task.Factory.StartNew(
                        async () =>
                        {
                            return await SearchOne(channel,
                                   useList,
                                   isbnSplitter,
                                   strQueryWord,
                                   nMax,
                                   strFromStyle,
                                   strMatchStyle,
                                   searchCompleted,
                                   presentCompleted);
                        },
                        new CancellationToken(),
    TaskCreationOptions.PreferFairness,
    TaskScheduler.Default);

                    // tasks 和 channels 下标一一对应
                    tasks.Add(task.Unwrap());
                    channels.Add(channel);
                    Debug.Assert(tasks.Count == channels.Count);
                }

                /*
                await Task.Run(() =>
                {
                    Task.WaitAll(tasks.ToArray(), TimeSpan.FromMinutes(1));
                }).ConfigureAwait(false);
                */
                await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(TimeSpan.FromMinutes(1)));

                // 2020/11/4 观察返回值
                List<string> errors = new List<string>();
                foreach (var task in tasks)
                {
                    if (task.IsCompleted == false)
                    {
                        // 中断那些超时后还没有结束的 channel
                        int index = tasks.IndexOf(task);
                        var channel = channels[index];
                        channel.ZClient.CloseConnection();
                        continue;   // 不计入报错？
                    }
                    var result = await task;
                    if (result.Value == -1)
                        errors.Add(result.ErrorInfo);
                }
                if (errors.Count == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, "; ")
                    };
            }
            finally
            {
                _searching = false;
            }
        }

        // 获取结果集时的每批数量
        public int PresentBatchSize = 10;

        // 针对一个特定 Z30.50 服务器发起检索
        async Task<NormalResult> SearchOne(
            ZClientChannel channel,
            UseCollection useList,
            IsbnSplitter isbnSplitter,
            string strQueryWord,
            int nMax,
            string strFromStyle,
            string strMatchStyle,
            delegate_searchCompleted searchCompleted,
            delegate_presentCompleted presentCompleted)
        {
            var ok = channel.Enter();
            try
            {
                if (ok == false)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "通道已被占用",
                        ErrorCode = "channelInUse"
                    };
                }

                var _targetInfo = channel.TargetInfo;
                IsbnConvertInfo isbnconvertinfo = new IsbnConvertInfo
                {
                    IsbnSplitter = isbnSplitter,
                    ConvertStyle =
        (_targetInfo.IsbnAddHyphen == true ? "addhyphen," : "")
        + (_targetInfo.IsbnRemoveHyphen == true ? "removehyphen," : "")
        + (_targetInfo.IsbnForce10 == true ? "force10," : "")
        + (_targetInfo.IsbnForce13 == true ? "force13," : "")
        + (_targetInfo.IsbnWild == true ? "wild," : "")
        // TODO:
        + (_targetInfo.IssnForce8 == true ? "issnforce8," : "")
                };

                string strQueryString = "";
                {
                    // 创建只包含一个检索词的简单 XML 检索式
                    // 注：这种 XML 检索式不是 Z39.50 函数库必需的。只是用它来方便构造 API 检索式的过程
                    string strQueryXml = BuildQueryXml(strQueryWord, strFromStyle);
                    // 将 XML 检索式变化为 API 检索式
                    var result = ZClient.ConvertQueryString(
                        useList,
                        strQueryXml,
                        isbnconvertinfo,
                        out strQueryString);
                    if (result.Value == -1)
                    {
                        searchCompleted?.Invoke(channel, new SearchResult(strQueryString, result));
                        var final_result = new NormalResult { Value = result.Value, ErrorInfo = result.ErrorInfo };
                        if (result.ErrorCode == "notFound")
                            final_result.ErrorCode = "useNotFound";
                        return final_result;
                    }
                }

            REDO_SEARCH:
                {
                    // return Value:
                    //      -1  出错
                    //      0   成功
                    //      1   调用前已经是初始化过的状态，本次没有进行初始化
                    // InitialResult result = _zclient.TryInitialize(_targetInfo).GetAwaiter().GetResult();
                    // InitialResult result = _zclient.TryInitialize(_targetInfo).Result;
                    InitialResult result = await channel.ZClient.TryInitialize(_targetInfo);
                    if (result.Value == -1)
                    {
                        searchCompleted?.Invoke(channel, new SearchResult(strQueryString, result));
                        // TODO: 是否继续向后检索其他 Z39.50 服务器?
                        return new NormalResult { Value = -1, ErrorInfo = "Initialize error: " + result.ErrorInfo };
                    }
                }

                // result.Value:
                //		-1	error
                //		0	fail
                //		1	succeed
                // result.ResultCount:
                //      命中结果集内记录条数 (当 result.Value 为 1 时)
                SearchResult search_result = await channel.ZClient.Search(
        strQueryString,
        _targetInfo.DefaultQueryTermEncoding,
        _targetInfo.DbNames,
        ZServerPropertyForm.GetLeftValue(_targetInfo.PreferredRecordSyntax),
        "default");
                if (search_result.Value == -1 || search_result.Value == 0)
                {
                    if (search_result.ErrorCode == "ConnectionAborted")
                    {
                        // 自动重试检索
                        goto REDO_SEARCH;
                    }
                }

                searchCompleted?.Invoke(channel, search_result);
                channel._query = search_result.Query;
                channel._resultCount = search_result.ResultCount;

                if (search_result.Value == -1
                    || search_result.Value == 0
                    || search_result.ResultCount == 0)
                    return new NormalResult();  // continue

                var present_result = await _fetchRecords(channel, PresentBatchSize/*10*/);

                presentCompleted?.Invoke(channel, present_result);

                if (present_result.Value != -1)
                    channel._fetched += present_result.Records.Count;

                return new NormalResult();
            }
            finally
            {
                channel.Exit();
            }
        }

        public static async Task<PresentResult> FetchRecords(ZClientChannel channel,
    long count)
        {
            var ok = channel.Enter();
            try
            {
                if (ok == false)
                    return new PresentResult
                    {
                        Value = -1,
                        ErrorInfo = "通道已被占用",
                        ErrorCode = "channelInUse"
                    };

                return await _fetchRecords(channel, count).ConfigureAwait(false);
            }
            finally
            {
                channel.Exit();
            }
        }

        static async Task<PresentResult> _fetchRecords(ZClientChannel channel,
            long count)
        {
            if (channel._resultCount - channel._fetched > 0)
            {
                return await channel.ZClient.Present(
                    "default",
                    channel._fetched,
                    Math.Min((int)channel._resultCount - channel._fetched, (int)count),
                    10,
                    "F",
                    ZServerPropertyForm.GetLeftValue(channel.TargetInfo.PreferredRecordSyntax)).ConfigureAwait(false);
            }

            return new PresentResult { Value = -1, ErrorInfo = "已经获取完成" };
        }


        public string BuildQueryXml(string queryWord, string strFrom)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            {
                XmlElement node = dom.CreateElement("line");
                dom.DocumentElement.AppendChild(node);

                string strLogic = "OR";

                node.SetAttribute("logic", strLogic);
                node.SetAttribute("word", queryWord);
                node.SetAttribute("from", strFrom);
            }

            return dom.OuterXml;
        }

#if REF
        public long SearchBiblio(
    DigitalPlatform.Stop stop,
    string strBiblioDbNames,
    string strQueryWord,
    int nPerMax,
    string strFromStyle,
    string strMatchStyle,
    string strLang,
    string strResultSetName,
    string strSearchStyle,
    string strOutputStyle,
    string strLocationFilter,
    out string strQueryXml,
    out string strError)
#endif
    }

    public class ZClientChannel : IDisposable
    {
        int _in = 0;

        // 用户显示的服务器名字
        public string ServerName { get; set; }

        public ZClient ZClient { get; set; }

        public TargetInfo TargetInfo { get; set; }

        // 服务器是否启用了
        public bool Enabled { get; set; }

        internal string _query = "";    // 检索式
        internal long _resultCount = 0;   // 检索命中条数
        internal int _fetched = 0;   // 已经 Present 获取的条数
        internal int _total_fetched = 0;    // 多行检索总共 Present 获取的条数。每次当清除 _fetched 以前先把 _fetched 里面的值加到 _total_fetched 上面

        public void Dispose()
        {
            if (ZClient != null)
                ZClient.Dispose();
        }

        public bool Enter()
        {
            int ret = Interlocked.Increment(ref this._in);
            if (ret == 1)
                return true;
            return false;
        }

        public void Exit()
        {
            Interlocked.Decrement(ref this._in);
        }
    }
}
