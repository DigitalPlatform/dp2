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
            foreach(ZClientChannel channel in this._channels)
            {
                if (channel != null)
                    channel.Dispose();
            }
            this._channels.Clear();
        }

        // 从 XML 文件中装载服务器配置信息
        public NormalResult LoadServer(string xmlFileName)
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
                    null,
                    out TargetInfo targetInfo);
                if (result.Value == -1)
                    return result;
                ZClientChannel channel = new ZClientChannel();
                channel.ServerName = server.GetAttribute("name");
                channel.ZClient = new ZClient();
                channel.TargetInfo = targetInfo;
                channel.Enabled = ZServerListDialog.IsEnabled(server.GetAttribute("enabled"), true);
                _channels.Add(channel);
            }

            return new NormalResult();
        }

        public void GraceStop()
        {
            this._searching = false;
        }

        public delegate void delegate_searchCompleted(ZClientChannel channel, SearchResult result);
        public delegate void delegate_presentCompleted(ZClientChannel channel, PresentResult result);

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

        public static async Task<PresentResult> FetchRecords(ZClientChannel channel,
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
                    channel.TargetInfo.PreferredRecordSyntax);
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
        // 用户显示的服务器名字
        public string ServerName { get; set; }

        public ZClient ZClient { get; set; }

        public TargetInfo TargetInfo { get; set; }

        // 服务器是否启用了
        public bool Enabled { get; set; }

        internal long _resultCount = 0;   // 检索命中条数
        internal int _fetched = 0;   // 已经 Present 获取的条数

        public void Dispose()
        {
            if (ZClient != null)
                ZClient.Dispose();
        }
    }
}
