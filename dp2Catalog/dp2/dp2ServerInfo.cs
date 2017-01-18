using System;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;

namespace dp2Catalog
{
    // TODO: 把服务器信息聚集起来，成为一个数组，持续保存服务器信息
    public class dp2ServerInfoCollection : List<dp2ServerInfo>
    {
        // 获得一个服务器的信息
        // 调用前stop需要先OnStop +=
        public dp2ServerInfo GetServerInfo(
            Stop stop,
            bool bUseNewChannel,
            // LibraryChannelCollection Channels,
            string strServerName,
            string strServerUrl,
            bool bTestMode,
            out string strError)
        {
            strError = "";

            // 先看看是否已经存在
            dp2ServerInfo info = this.Find(strServerUrl);

            if (info != null)
            {
                if (info.BiblioDbProperties != null)
                    return info;
            }
            else
            {
                // 如果不存在，就试图建立
                info = new dp2ServerInfo();
                info.TestMode = bTestMode;
                info.Url = strServerUrl;
                info.Name = strServerName;

                this.Add(info);
            }

            int nRet = info.Build(strServerName,
                strServerUrl,
                stop,
                bUseNewChannel,
#if OLD_CHANNEL
                Channels,
#endif
                out strError);
            if (nRet == -1)
            {
                info.BiblioDbProperties = null;
                return null;
            }

            return info;
        }

        // 搜索特定URL的事项
        dp2ServerInfo Find(string strServerUrl)
        {
            for (int i = 0; i < this.Count; i++)
            {
                dp2ServerInfo info = this[i];
                if (info.Url == strServerUrl)
                    return info;
            }

            return null;
        }

    }


    // 服务器信息
    public class dp2ServerInfo
    {
        public string Url = ""; // URL
        public string Name = "";    // 用于显示的服务器名

        public List<BiblioDbProperty> BiblioDbProperties = null;

        public List<UtilDbProperty> UtilDbProperties = null;

        /// <summary>
        /// 当前连接的 dp2Library 版本号
        /// </summary>
        public string Version = "0.0";  // 0 表示2.1以下。2.1和以上时才具有的获取版本号功能

        public bool TestMode = false;   // 是否为评估模式

        // 获得编目库属性列表
        // 调用前stop需要先OnStop +=
        // parameters:
        //      bUseNewChannel  是否使用新的Channel对象。如果==false，表示尽量使用以前的
        public int Build(string strName,
            string strServerUrl,
            Stop stop,
            bool bUseNewChannel,
            // LibraryChannelCollection Channels,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            this.Url = strServerUrl;
            this.Name = strName;

#if OLD_CHANNEL
            LibraryChannel Channel = null;
            
            if (bUseNewChannel == false)
                Channel = Channels.GetChannel(strServerUrl);
            else
                Channel = Channels.NewChannel(strServerUrl);
#endif
            LibraryChannel channel = Program.MainForm.GetChannel(strServerUrl);

            if (stop != null)
            {
                stop.SetMessage("正在获得编目库属性 ...");
                /*
                stop.Initial("正在获得编目库属性 ...");
                stop.BeginLoop();
                 * */
            }

            try
            {
                string version = "0.0";
                // return:
                //      -1  error
                //      0   dp2Library的版本号过低。警告信息在strError中
                //      1   dp2Library版本号符合要求
                nRet = LibraryChannel.GetServerVersion(
                    channel,
                    stop,
                    out version,
                    out strError);
                if (nRet != 1)
                    return -1;
                this.Version = version;

                if (this.TestMode == true && StringUtil.CompareVersion(this.Version, "2.34") < 0)
                {
                    strError = "dp2 前端的评估模式只能在所连接的 dp2library 版本为 2.34 以上时才能使用 (当前 dp2library 版本为 " + this.Version.ToString() + ")";
                    return -1;
                }

                this.BiblioDbProperties = new List<BiblioDbProperty>();

                string strValue = "";
                long lRet = channel.GetSystemParameter(stop,
                    "biblio",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得编目库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] biblioDbNames = strValue.Split(new char[] { ',' });

                for (int i = 0; i < biblioDbNames.Length; i++)
                {
                    BiblioDbProperty property = new BiblioDbProperty();
                    property.DbName = biblioDbNames[i];
                    this.BiblioDbProperties.Add(property);
                }

                // 获得语法格式
                lRet = channel.GetSystemParameter(stop,
                    "biblio",
                    "syntaxs",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得编目库数据格式列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] syntaxs = strValue.Split(new char[] { ',' });

                if (syntaxs.Length != this.BiblioDbProperties.Count)
                {
                    strError = "针对服务器 " + channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而数据格式为 " + syntaxs.Length.ToString() + " 个，数量不一致";
                    goto ERROR1;
                }

                // 增补数据格式
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    this.BiblioDbProperties[i].Syntax = syntaxs[i];
                }


                ///

                // 获得对应的实体库名
                lRet = channel.GetSystemParameter(stop,
                    "item",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得实体库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] itemdbnames = strValue.Split(new char[] { ',' });

                if (itemdbnames.Length != this.BiblioDbProperties.Count)
                {
                    strError = "针对服务器 " + channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而实体库名为 " + itemdbnames.Length.ToString() + " 个，数量不一致";
                    goto ERROR1;
                }

                // 增补数据格式
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    this.BiblioDbProperties[i].ItemDbName = itemdbnames[i];
                }


                // 获得对应的期库名
                lRet = channel.GetSystemParameter(stop,
                    "issue",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得实体库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] issuedbnames = strValue.Split(new char[] { ',' });

                if (issuedbnames.Length != this.BiblioDbProperties.Count)
                {
                    return 0; // TODO: 暂时不警告。等将来所有用户都更换了dp2libraryws 2007/10/19以后的版本后，这里再警告
                    /*
                    strError = "针对服务器 " + Channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而期库名为 " + issuedbnames.Length.ToString() + " 个，数量不一致";
                    goto ERROR1;
                     * */
                }

                // 增补数据格式
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    this.BiblioDbProperties[i].IssueDbName = issuedbnames[i];
                }

                // 获得实用库信息

                {
                    this.UtilDbProperties = new List<UtilDbProperty>();

                    lRet = channel.GetSystemParameter(stop,
                        "utilDb",
                        "dbnames",
                        out strValue,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 获得实用库名列表过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    string[] utilDbNames = strValue.Split(new char[] { ',' });

                    for (int i = 0; i < utilDbNames.Length; i++)
                    {
                        UtilDbProperty property = new UtilDbProperty();
                        property.DbName = utilDbNames[i];
                        this.UtilDbProperties.Add(property);
                    }

                    // 获得类型
                    lRet = channel.GetSystemParameter(stop,
                        "utilDb",
                        "types",
                        out strValue,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "针对服务器 " + channel.Url + " 获得实用库数据格式列表过程发生错误：" + strError;
                        goto ERROR1;
                    }

                    string[] types = strValue.Split(new char[] { ',' });

                    if (types.Length != this.UtilDbProperties.Count)
                    {
                        strError = "针对服务器 " + channel.Url + " 获得实用库名为 " + this.UtilDbProperties.Count.ToString() + " 个，而类型为 " + types.Length.ToString() + " 个，数量不一致";
                        goto ERROR1;
                    }

                    // 增补数据格式
                    for (int i = 0; i < this.UtilDbProperties.Count; i++)
                    {
                        this.UtilDbProperties[i].Type = types[i];
                    }

                }


                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                if (stop != null)
                {
                    /*
                    stop.EndLoop();
                    stop.Initial("");
                     * */
                }

                Program.MainForm.ReturnChannel(channel);
#if OLD_CHANNEL
                if (bUseNewChannel == true)
                {
                    Channels.RemoveChannel(Channel);
                    Channel = null;
                }
#endif
            }

            return 0;
        ERROR1:
            return -1;
        }




    }


    // 书目库属性
    public class BiblioDbProperty
    {
        public string DbName = "";  // 书目库名
        public string Syntax = "";  // 格式语法
        public string ItemDbName = "";  // 对应的实体库名

        public string IssueDbName = ""; // 对应的期库名 2007/10/19
    }

    // 实用库属性
    public class UtilDbProperty
    {
        public string DbName = "";  // 库名
        public string Type = "";  // 类型，用途
    }
}
