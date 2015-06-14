using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.Library
{
    /// <summary>
    /// 种次号管理器
    /// </summary>
    public class SeedManager
    {
        /// <summary>
        /// 
        /// </summary>
        SearchPanel SearchPanel = null;

        /// <summary>
        /// 种子库名
        /// </summary>
        public string SeedDbName = "";

        /// <summary>
        /// 服务器URL
        /// </summary>
        public string ServerUrl = "";

        /// <summary>
        /// 时间戳
        /// </summary>
        public byte[] Timestamp = null;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="searchpanel"></param>
        /// <param name="strServerUrl"></param>
        /// <param name="strSeedDbName"></param>
        public void Initial(
                SearchPanel searchpanel,
                string strServerUrl,
                string strSeedDbName)
        {
            this.SearchPanel = searchpanel;

            /*
            this.SearchPanel.InitialStopManager(this.button_stop,
                this.label_message);
             */

            this.ServerUrl = strServerUrl;
            this.SeedDbName = strSeedDbName;
        }

        /// <summary>
        /// 检索出种子记录路径
        /// </summary>
        /// <param name="strName">种子名</param>
        /// <param name="strPath">返回记录路径</param>
        /// <param name="strError">返回的出错信息</param>
        /// <returns>-1出错;0没有找到;1找到</returns>
        int SearchRecPath(
            string strName,
            out string strPath,
            out string strError)
        {
            strError = "";
            strPath = "";

            if (this.ServerUrl == "")
            {
                strError = "尚未指定服务器URL";
                return -1;
            }

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(this.SeedDbName + ":" + "名")       // 2007/9/14
                + "'><item><word>"
                + StringUtil.GetXmlStringSimple(strName)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            this.SearchPanel.BeginLoop("正在针对库 '" + this.SeedDbName + "' 检索 '" + strName + "'");

            // 检索一个命中结果
            // return:
            //		-1	一般错误
            //		0	not found
            //		1	found
            //		>1	命中多于一条
            int nRet = this.SearchPanel.SearchOnePath(
                this.ServerUrl,
                strQueryXml,
                out strPath,
                out strError);

            this.SearchPanel.EndLoop();

            if (nRet == -1)
            {
                strError = "检索库 " + this.SeedDbName + " 时出错: " + strError;
                return -1;
            }
            if (nRet == 0)
            {
                return 0;	// 没有找到
            }

            if (nRet > 1)
            {
                strError = "以名字 '" + strName + "' 检索库 " + this.SeedDbName + " 时命中 " + Convert.ToString(nRet) + " 条，无法取得适当的值。请修改库 '" + this.SeedDbName + "' 中相应记录，确保同一名字只有一条对应的记录。";
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// 设置种子值
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int SetSeed(
    string strName,
    string strValue,
    out string strError)
        {
            strError = "";

            string strPath = "";
            int nRet = this.SearchRecPath(
                strName,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                // 新创建记录
                strPath = this.SeedDbName + "/?";

                // 这里以后考虑加锁
            }
            else
            {
                // 覆盖记录
            }

            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);

            xw.WriteStartDocument();

            xw.WriteStartElement("r");
            xw.WriteAttributeString("n", strName);
            xw.WriteAttributeString("v", strValue);
            xw.WriteEndElement();

            xw.WriteEndDocument();
            xw.Close();

            string strXml = sw.ToString();


            byte[] baOutputTimestamp = null;

            REDO:
            // return:
            //		-2	时间戳不匹配
            //		-1	一般出错
            //		0	正常
            nRet = this.SearchPanel.SaveRecord(
                this.ServerUrl,
                strPath,
                strXml,
                this.Timestamp,
                false,
                out baOutputTimestamp,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == -2)
            {
                    this.Timestamp = baOutputTimestamp;
                    goto REDO;
            }

            this.Timestamp = baOutputTimestamp;

            return 0;
        }

        /// <summary>
        /// 增量种子值
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strDefaultValue"></param>
        /// <param name="strValue"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int IncSeed(
            string strName,
            string strDefaultValue,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";

            string strPath = "";
            int nRet = this.SearchRecPath(
                strName,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            string strXml = "";
            bool bNewRecord = false;

            if (nRet == 0)
            {
                // 新创建记录

                strPath = this.SeedDbName + "/?";

                StringWriter sw = new StringWriter();
                XmlTextWriter xw = new XmlTextWriter(sw);

                xw.WriteStartDocument();

                xw.WriteStartElement("r");
                xw.WriteAttributeString("n", strName);
                xw.WriteAttributeString("v", strDefaultValue);
                xw.WriteEndElement();

                xw.WriteEndDocument();
                xw.Close();

                strXml = sw.ToString();

                bNewRecord = true;
            }
            else
            {
                string strPartXml = "/xpath/<locate>@v</locate><action>AddInteger+</action>";   // +AddInteger为加了值并存回再返回加了的值; AddInteger为取值后再加值存回.
                strPath += strPartXml;
                strXml = "1";

                bNewRecord = false;
            }


            byte[] baOutputTimestamp = null;


            // return:
            //		-2	时间戳不匹配
            //		-1	一般出错
            //		0	正常
            nRet = this.SearchPanel.SaveRecord(
                this.ServerUrl,
                strPath,
                strXml,
                this.Timestamp,
                true,
                out baOutputTimestamp,
                out strError);
            if (nRet < 0)
            {
                return -1;
            }

            this.Timestamp = baOutputTimestamp;

            if (bNewRecord == true)
            {
                strValue = strDefaultValue;
            }
            else
            {
                strValue = strError;
            }


            return 0;
        }

        /// <summary>
        /// 获得种子值
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetSeed(
            string strName,
            out string strValue,
            out string strError)
        {
            strValue = "";
            strError = "";

            string strPath = "";
            int nRet = this.SearchRecPath(
                strName,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            XmlDocument tempdom = null;
            byte[] baTimeStamp = null;
            // 获取记录
            // return:
            //		-1	error
            //		0	not found
            //		1	found
            nRet = this.SearchPanel.GetRecord(
                this.ServerUrl,
                strPath,
                out tempdom,
                out baTimeStamp,
                out strError);
            if (nRet != 1)
                return -1;

            this.Timestamp = baTimeStamp;

            strValue = DomUtil.GetAttr(tempdom.DocumentElement, "v");
            return 1;
        }

        /*
        //
        public int SetSeed(ChannelCollection Channels,
            string strServerUrl,
            string strSeedDbName,
            string strName,
            string strValue,
            out string strError)
        {
            strError = "";

            return 0;
        }

        public int GetSeed(ChannelCollection Channels,
            string strServerUrl,
            string strSeedDbName,
            string strName,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";

            return 1;
        }

        public int IncSeed(ChannelCollection Channels,
            string strServerUrl,
            string strSeedDbName,
            string strName,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";


            return 1;
        }
         * */

    }
}
