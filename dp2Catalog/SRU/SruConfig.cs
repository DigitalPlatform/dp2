using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

/*
 * <root>
 *     <server name="target" baseUrl="https://bnu.alma.exlibrisgroup.com/view/sru/86BNU_INST"
 *     version="1.2"
 *     explainUrl="http://bibsys-network.alma.exlibrisgroup.com/view/sru/47BIBSYS_NETWORK?version=1.2&operation=explain" />
 * </root>
 * */

namespace dp2Catalog
{
    public class SruConfig
    {
        XmlDocument _dom = new XmlDocument();

        string _cacheDirectory = "";

        public static SruConfig From(string fileName)
        {
            SruConfig result = new SruConfig(fileName);
            return result;
        }

        public SruConfig(string fileName)
        {
            if (File.Exists(fileName) == false)
                this._dom.LoadXml("<root />");
            else
                this._dom.Load(fileName);

            _cacheDirectory = Path.GetDirectoryName(fileName);
        }

        public List<SruTarget> ListTargets(string name)
        {
            List<SruTarget> results = new List<SruTarget>();
            XmlNodeList servers = _dom.DocumentElement.SelectNodes("server");
            foreach (XmlElement server in servers)
            {
                if (name == "*" || server.GetAttribute("name") == name)
                    results.Add(new SruTarget(server));
            }
            return results;
        }

        public async Task<string> BuildSearchUrl(string server_name,
            string word,
            string use_name)
        {
            var targets = this.ListTargets(server_name);
            if (targets.Count == 0)
            {
                throw new Exception( $"配置中没有找到名为 '{server_name}' 的服务器对象");
            }

            var target = targets[0];
            string use_value = await GetUseValue(target, use_name).ConfigureAwait(false);

            // &startRecord=1&maximumRecords=5
            if (string.IsNullOrEmpty(target.Version))
                return $"{target.BaseUrl}?operation=searchRetrieve&recordSchema=marcxml&query={use_value}={word}";

            return $"{target.BaseUrl}?version={target.Version}&operation=searchRetrieve&recordSchema=marcxml&query={use_value}={word}";
        }

        // 根据 use 的名称，获得可用于 API 的 use 值
        public async Task<string> GetUseValue(SruTarget target, string use_name)
        {
            var lines = await ListUses(target).ConfigureAwait(false);
            foreach(var line in lines)
            {
                if (line.Name == use_name)
                    return line.Value;
                if (line.Value == use_name)
                    return line.Value;
            }

            return null;
        }

        // 列出一个检索目标的所有可用检索途径
        public async Task<List<NameValueLine>> ListUses(SruTarget target)
        {
            List<NameValueLine> results = new List<NameValueLine>();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("srw", "http://www.loc.gov/zing/srw/");
            nsmgr.AddNamespace("explain20", "http://explain.z3950.org/dtd/2.0/");
            nsmgr.AddNamespace("explain21", "http://explain.z3950.org/dtd/2.1/");

            /*
             *
    <explain xmlns="http://explain.z3950.org/dtd/2.0/" xmlns:ns="http://explain.z3950.org/dtd/2.1/">
        <serverInfo>
          <ns:host>bibsys-network.alma.exlibrisgroup.com</ns:host>
          <ns:port>80</ns:port>
          <ns:database>47BIBSYS_NETWORK</ns:database>
        </serverInfo>
        <indexInfo>
          <set name="alma" identifier="marcxml"/>
          <index>
            <ns:title>Accompanying Material</ns:title>
            <map>
              <name set="alma">accompanying_material</name>
            </map>
            <configInfo>
              <supports type="relation">all</supports>
              <supports type="relation">=</supports>
              <supports type="emptyTerm"/>
            </configInfo>
          </index>
          <index>
            <ns:title>Additional physical form available note</ns:title>
            <map>
              <name set="alma">additional_physical_form_available_note</name>
            </map>
            <configInfo>
              <supports type="relation">all</supports>
              <supports type="relation">=</supports>
              <supports type="relation">==</supports>
            </configInfo>
          </index>
             * */

            XmlDocument dom = await GetExplainDomAsync(target.Name,
    "explain",
    target.GetExplainUrl()).ConfigureAwait(false);
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//explain20:indexInfo/explain20:index", nsmgr);
            foreach (XmlElement index in nodes)
            {
                string title = index.SelectSingleNode("explain21:title", nsmgr)?.InnerText;
                XmlElement map = index.SelectSingleNode("explain20:map/explain20:name", nsmgr) as XmlElement;
                string set = map.GetAttribute("set");
                NameValueLine line = new NameValueLine(title, $"{set}.{map.InnerText}");
                results.Add(line);
            }

            return results;
        }

        async Task<XmlDocument> GetExplainDomAsync(string name,
            string type,
            string expainUrl)
        {
            // 先看看缓存文件是否存在
            string cacheFileName = MakeCacheFileName(name, type);
            if (File.Exists(cacheFileName))
            {
                XmlDocument result = new XmlDocument();
                result.Load(cacheFileName);
                return result;
            }
            else
            {
                var download_result = await WebClientEx.DownloadStringAsync(expainUrl).ConfigureAwait(false);
                File.WriteAllText(cacheFileName, download_result.String, download_result.Encoding);
                XmlDocument result = new XmlDocument();
                result.LoadXml(download_result.String);
                return result;
            }
        }

        string MakeCacheFileName(string name, string type)
        {
            return Path.Combine(this._cacheDirectory, name + "_" + type);
        }
    }



    public class SruTarget
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string Version { get; set; }
        // public string ExplainUrl { get; set; }

        public SruTarget(XmlElement server)
        {
            this.Name = server.GetAttribute("name");
            this.BaseUrl = server.GetAttribute("baseUrl");

            // 去掉末尾的问号
            if (this.BaseUrl[this.BaseUrl.Length - 1] == '?')
                this.BaseUrl = this.BaseUrl.Substring(0, this.BaseUrl.Length - 1);

            this.Version = server.GetAttribute("version");
            // this.ExplainUrl = server.GetAttribute("explainUrl");
        }

        public string GetExplainUrl()
        {
            string version = this.Version;
            if (string.IsNullOrEmpty(version))
                return $"{this.BaseUrl}?operation=explain";
            return $"{this.BaseUrl}?version={this.Version}&operation=explain";
        }
    }
}
