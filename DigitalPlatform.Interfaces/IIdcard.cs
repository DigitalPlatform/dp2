using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DigitalPlatform.Interfaces
{
    /// <summary>
    /// 身份证读卡器接口
    /// </summary>
    public interface IIdcard 
    {
        int Open(out string strError);

        int Close();

        bool CardReady();

        bool SendKeyEnabled
        {
            get;
            set;
        }

        // prameters:
        //      strStyle 如何获取数据。all/xml/photo 的一个或者多个的组合
        // return:
        //      -1  出错
        //      0   成功
        //      1   重复读入未拿走的卡号
        int ReadCard(string strStyle,
            out string strResultXml,
            out byte[] photo,
            out string strError);

        // return:
        //      -1  出错
        //      0   成功
        //      1   重复读入未拿走的卡号
        int ReadCardID(string strStyle,
    out string strID,
    out string strError);
    }

#if NO
    public interface IServerFactory
    {
        IIdcard GetInterface();
    }
#endif

#if NO
    public class IdcardServerBase : MarshalByRefObject, IIdcard
    {
        public virtual int Open(out string strError)
        {
            strError = "";
            return 0;
        }

        public virtual int Close()
        {
            return 0;
        }

        public virtual int ReadCard(string strStyle,
            out string strResultXml,
            out Image photo,
            out string strError)
        {
            strResultXml = "";
            photo = null;
            strError = "";
            return 0;
        }
    }
#endif

}
