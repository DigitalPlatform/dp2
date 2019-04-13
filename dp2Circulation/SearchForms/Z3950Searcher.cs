using DigitalPlatform;
using DigitalPlatform.Z3950;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Circulation
{
    /// <summary>
    /// 用于 Z39.50 检索的工具类
    /// 它管理了若干个 Z39.50 通道
    /// </summary>
    public class Z3950Searcher
    {
        // 通道集合
        List<ZClient> _clients = new List<ZClient>();

        // Z39.50 服务器定义 XML 文件的文件名
        public string XmlFileName { get; set; }

        // 从 XML 文件中装载服务器配置信息
        NormalResult LoadServer()
        {

        }

        // 首次检索
        public NormalResult Search(string strQueryWord,
            int nMax,
            string strFromStyle,
            string strMatchStyle)
        {
            foreach(ZClient client in _clients)
            {

            }

            return new NormalResult();
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
}
