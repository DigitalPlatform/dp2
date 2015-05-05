using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Net.Mail;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Range;

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和XML检索式加工相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 检查检索式内有没有超越规定读者检索的数据库
        // return:
        //      -1  error
        //      0   没有超越要求
        //      1   超越了要求
        public int CheckReaderOnlyXmlQuery(string strSourceQueryXml,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

#if NO
            if (this.vdbs == null)
            {
                this.ActivateManagerThreadForLoad();
                strError = "app.vdbs == null。故障原因请检查dp2Library日志";
                return -1;
            }

            Debug.Assert(this.vdbs != null, "");
#endif

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strSourceQueryXml);
            }
            catch (Exception ex)
            {
                strError = "XML检索式装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            // 遍历所有<target>元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//target");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strList = DomUtil.GetAttr(node, "list");

                if (String.IsNullOrEmpty(strList) == true)
                    continue;

                DbCollection dbs = new DbCollection();

                dbs.Build(strList);

                for (int j = 0; j < dbs.Count; j++)
                {
                    Db db = dbs[j];
                    string strBiblioDbName = "";
                    string strDbType = this.GetDbType(db.DbName,
                        out strBiblioDbName);
                    if (String.IsNullOrEmpty(strDbType) == true)
                    {
                        strError = "数据库 '"+db.DbName+"' 超出了读者可检索的数据库范围";
                        return 1;
                    }
                }

            }

            return 0;
        }

        // 检查检索式内有没有超越当前用户管辖的读者库范围的读者库
        // return:
        //      -1  error
        //      0   没有超越要求
        //      1   超越了要求
        public int CheckReaderDbXmlQuery(string strSourceQueryXml,
            string strLibraryCodeList,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

#if NO
            if (this.vdbs == null)
            {
                this.ActivateManagerThreadForLoad();
                strError = "app.vdbs == null。故障原因请检查dp2Library日志";
                return -1;
            }

            Debug.Assert(this.vdbs != null, "");
#endif

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strSourceQueryXml);
            }
            catch (Exception ex)
            {
                strError = "XML检索式装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            // 遍历所有<target>元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//target");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strList = DomUtil.GetAttr(node, "list");

                if (String.IsNullOrEmpty(strList) == true)
                    continue;

                DbCollection dbs = new DbCollection();

                dbs.Build(strList);

                for (int j = 0; j < dbs.Count; j++)
                {
                    Db db = dbs[j];

                    // 需要限制检索读者库为当前管辖的范围
                    {
                        string strLibraryCode = "";
                        bool bReaderDbInCirculation = true;
                        if (this.IsReaderDbName(db.DbName,
                            out bReaderDbInCirculation,
                            out strLibraryCode) == true)
                        {
                            // 检查当前操作者是否管辖这个读者库
                            // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                            if (this.IsCurrentChangeableReaderPath(db.DbName + "/?",
                                strLibraryCodeList) == false)
                            {
                                strError = "读者库 '" + db.DbName + "' 不在当前用户管辖范围内";
                                return 1;
                            }
                        }
                    }
                }
            }

            return 0;
        }

        // 将包含虚拟库要求的XML检索式变换为内核能够理解的实在库XML检索式
        // return:
        //      -1  error
        //      0   没有发生变化
        //      1   发生了变化
        public int KernelizeXmlQuery(string strSourceQueryXml,
            out string strTargetQueryXml,
            out string strError)
        {
            strTargetQueryXml = "";
            strError = "";
            int nRet = 0;

            Debug.Assert(this.vdbs != null, "");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strSourceQueryXml);
            }
            catch (Exception ex)
            {
                strError = "XML检索式装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            bool bChanged = false;

            // 遍历所有<target>元素

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//target");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strList = DomUtil.GetAttr(node, "list");

                if (String.IsNullOrEmpty(strList) == true)
                    continue;

                string strOutputList = "";
                        // 变换list参数值，将其中的虚拟库（连带途径）变换为物理库和途径
        // parameters:
        // return:
        //      -1  error
        //      0   没有发生变化
        //      1   发生了变化
                nRet = ConvertList(strList,
                    out strOutputList,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    continue;

                bChanged = true;
                DomUtil.SetAttr(node, "list", strOutputList);
            }

            if (bChanged == false)
            {
                strTargetQueryXml = strSourceQueryXml;
                return 0;
            }

            strTargetQueryXml = dom.OuterXml;
            return 1;
        }

        // 变换list参数值，将其中的虚拟库（连带途径）变换为物理库和途径
        // parameters:
        // return:
        //      -1  error
        //      0   没有发生变化
        //      1   发生了变化
        int ConvertList(string strSourceList,
            out string strTargetList,
            out string strError)
        {
            strTargetList = "";
            strError = "";

            DbCollection dbs = new DbCollection();

            dbs.Build(strSourceList);

            bool bChanged = false;

            DbCollection target_dbs = new DbCollection();

            for (int i = 0; i < dbs.Count; i++)
            {
                Db db = dbs[i];

                Debug.Assert(this.vdbs != null, "");

                VirtualDatabase vdb = this.vdbs[db.DbName];

                if (vdb == null)  // 不是虚拟库
                {
                    target_dbs.Add(db);
                    continue; 
                }

                if (vdb.IsVirtual == false)  // 不是虚拟库
                {
                    target_dbs.Add(db);
                    continue; 
                }

                bChanged = true;

                // 一个Db对象可能演化为多个Db对象
                List<Db> multi_dbs = new List<Db>();

                // 获得下属的所有真实数据库名
                List<string> real_dbnames = vdb.GetRealDbNames();
                for (int j = 0; j < real_dbnames.Count; j++)
                {
                    Db target_db = new Db();
                    target_db.DbName = real_dbnames[j];

                    List<string> real_froms = new List<string>();
                    for(int k=0;k<db.Froms.Count;k++)
                    {
                        // 虚拟的路径名
                        string strVirtualFromName = db.Froms[k];

                        // 实在的路径名
                        string strRealFroms = vdb.GetRealFromName(
                            this.vdbs.db_dir_results,
                            target_db.DbName,
                            strVirtualFromName);

                        if (String.IsNullOrEmpty(strRealFroms) == true)
                            continue;

                        string [] froms = strRealFroms.Split(new char [] {','} );

                        for(int l = 0;l<froms.Length; l++)
                        {
                            real_froms.Add(froms[l]);
                        }
                    }

                    if (real_froms.Count == 0)
                        continue;

                    target_db.Froms = real_froms;
                    multi_dbs.Add(target_db);
                }

                if (multi_dbs.Count == 0)
                    continue;

                target_dbs.AddRange(multi_dbs);

            }

            if (bChanged == false)
            {
                strTargetList = strSourceList;
                return 0;
            }

            strTargetList = target_dbs.GetString();
            return 1;
        }
    }

    class DbCollection : List<Db>
    {
        public int Build(string strList)
        {
            string[] segments = strList.Split(new char[] {';'});
            for (int i = 0; i < segments.Length; i++)
            {
                string strSegment = segments[i].Trim();

                if (String.IsNullOrEmpty(strSegment) == true)
                    continue;

                // 解析出数据库名
                string strDbName = "";
                int nRet = strSegment.IndexOf(':');
                if (nRet == -1)
                {
                    strDbName = strSegment;
                    strSegment = "";
                }
                else
                {
                    strDbName = strSegment.Substring(0, nRet).Trim();
                    strSegment = strSegment.Substring(nRet + 1).Trim();
                }

                Db db = new Db(strDbName, strSegment);
                this.Add(db);
            }

            return 0;
        }

        public string GetString()
        {
            string strResult = "";
            for(int i=0;i<this.Count;i++)
            {
                Db db = this[i];

                if (i != 0)
                    strResult += ";";
                strResult += db.GetString();
            }

            return strResult;
        }
    }

    class Db
    {
        public string DbName = "";
        public List<string> Froms = new List<string>();

        public Db()
        {
        }

        public Db(string strDbName,
            string strFromList)
        {
            this.DbName = strDbName;

            if (String.IsNullOrEmpty(strFromList) == true)
                return;

            string[] froms = strFromList.Split(new char[] { ',' });
            for (int i = 0; i < froms.Length; i++)
            {
                string strFrom = froms[i].Trim();

                if (String.IsNullOrEmpty(strFrom) == true)
                    continue;

                this.Froms.Add(strFrom);
            }
        }

        public string GetString()
        {
            if (this.Froms.Count == 0)
                return this.DbName;

            string strFroms = "";
            for (int i = 0; i < this.Froms.Count; i++)
            {
                if (i != 0)
                    strFroms += ",";
                strFroms += this.Froms[i];
            }

            return this.DbName + ":" + strFroms;
        }
    }
}
