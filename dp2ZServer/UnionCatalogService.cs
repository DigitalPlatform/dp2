using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Marc;
using DigitalPlatform.LibraryClient;

namespace dp2ZServer
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        Namespace = "http://dp2003.com/unioncatalog/")]
    public class UnionCatalogService : IUnionCatalogService, IDisposable
    {
        UcApplication app = null;

        public void Dispose()
        {
        }

        int InitialApplication(out string strError)
        {
            strError = "";

            if (this.app != null)
                return 0;   // 已经初始化

            HostInfo info = OperationContext.Current.Host.Extensions.Find<HostInfo>();
            if (info.App != null)
            {
                this.app = info.App;
                return 0;
            }

            string strDataDir = info.DataDir;

            Debug.Assert(string.IsNullOrEmpty(strDataDir) == false, "");

            lock (info.LockObject)
            {
                info.App = new UcApplication();
                // parameter:
                //		strDataDir	data目录
                //		strError	out参数，返回出错信息
                // return:
                //		-1	出错
                //		0	成功
                // 线: 安全的
                int nRet = info.App.Initial(strDataDir,
                            out strError);
                if (nRet == -1)
                    return -1;
            }

            this.app = info.App;
            return 0;
        }

        // 上载书目记录
        // parameters:
        //      strAhtuString   身份鉴别字符串。一般是“用户名/密码”形态
        //      strAction   动作。为"new" "change" "delete" "onlydeletebiblio"之一。"delete"在删除书目记录的同时，会自动删除下属的实体记录。不过要求实体均未被借出才能删除。
        //      strRecPath  记录路径。如果是覆盖保存一条记录，路经则为类似“中文图书/100”这样的形式，斜杠左边是数据库名，右边是记录ID。如果是创建一条新记录，则路径为类似“中文图书/?”的形式。
        //      strFormat   strRecord参数中的记录内容的格式。为 marcxchange marcxml 之一
        //      strRecord   要上载的书目记录内容。目前支持的格式有marcxchange和marcxml
        //      strTimestamp    所上载的书目记录的时间戳字符串。如果是创建新记录，则时间戳字符串为空即可。
        // 注：用Z39.50检索社科院联合编目中心所获得的MARC记录，其901字段中的$p子字段内容为该记录的路经，$t子字段内容为记录的时间戳字符串
        //      strOutputRecPath    [out]返回上载保存后的记录路径。如果是创建新记录，则这里返回了该记录创建后的确定记录路径
        //      strOutputTimestamp  [out]返回记录保存后的新时间戳字符串。保存操作会令时间戳更新。如果要继续进行保存操作，需要使用新的时间戳字符串来进行调用
        //      strError    [out]返回出错信息
        // return:
        //      -2  登录不成功
        //      -1  出错
        //      0   成功
        public int UpdateRecord(
    string strAuthString,
    string strAction,
    string strRecPath,
    string strFormat,
    string strRecord,
    string strTimestamp,
    out string strOutputRecPath,
    out string strOutputTimestamp,
    out string strError)
        {
            strError = "";
            strOutputRecPath = "";
            strOutputTimestamp = "";
            int nRet = 0;

            string strXml = "";

            if (strAction != null)
                strAction = strAction.ToLower();

            if (strAction == "delete")
            {
                // 保持strXml == "";
            }
            else
            {
                if (strFormat != null)
                    strFormat = strFormat.ToLower();
                if (strFormat == "marcxchange" || strFormat == "info:lc/xmlns/marcxchange-v1")
                {
                    nRet = MarcUtil.MarcXChangeToXml(strRecord,
        out strXml,
        out strError);
                    if (nRet == -1)
                    {
                        strError = "在转换源记录格式的过程中发生错误: " + strError;
                        return -1;
                    }
                }
                else if (strFormat == "marcxml")
                    strXml = strRecord;
                else
                {
                    strError = "未知的strFormat值 '" + strFormat + "'。目前仅支持 marcxchange marcxml 之一";
                    return -1;
                }
            }

            // TODO: 是否要去除MARC中的字段901? 或者这个是前端的责任?

            nRet = InitialApplication(out strError);
            if (nRet == -1)
                return -1;

            string strUserName = "";
            string strPassword = "";
            nRet = strAuthString.IndexOf("/");
            if (nRet == -1)
            {
                strUserName = strAuthString.Trim();
            }
            else
            {
                strUserName = strAuthString.Substring(0, nRet).Trim();
                strPassword = strAuthString.Substring(nRet + 1).Trim();
            }

            string strParameters = "location=#unioncatalog";

            LibraryChannel channel = new LibraryChannel();
            try
            {
                channel.Url = app.WsUrl;
                long lRet = channel.Login(strUserName,
                    strPassword,
                    strParameters,
                    out strError);
                if (lRet == -1)
                {
                    strError = "登录过程发生错误: " + strError;
                    return -1;
                }
                if (lRet != 1)
                {
                    strError = "登录失败: " + strError;
                    return -2;
                }

                byte[] baTimestamp = ByteArray.GetTimeStampByteArray(strTimestamp);
                byte [] baOutputTimestamp = null;

                lRet = channel.SetBiblioInfo(null,
                    strAction,
                    strRecPath,
                    "xml", // strBiblioType,
                    strXml,
                    baTimestamp,
                    "", // strComment
                    out strOutputRecPath,
                    out baOutputTimestamp,
                    out strError);
                if (lRet == -1)
                    return -1;

                strOutputTimestamp = ByteArray.GetHexTimeStampString(baOutputTimestamp);
            }
            finally
            {
                channel.Close();
            }

            return 0;
        }


    }

    public class HostInfo : IExtension<ServiceHostBase>, IDisposable
    {
        public object LockObject = new object();
        ServiceHostBase owner = null;
        public UcApplication App = null;
        public string DataDir = ""; // 数据目录

        void IExtension<ServiceHostBase>.Attach(ServiceHostBase owner)
        {
            this.owner = owner;
        }
        void IExtension<ServiceHostBase>.Detach(ServiceHostBase owner)
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.App != null)
            {
                lock (this.LockObject)
                {
                    this.App.Close();
                    this.App = null;
                }
            }
        }
    }
}
