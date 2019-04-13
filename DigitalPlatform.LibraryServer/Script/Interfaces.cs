using System;
using System.Reflection;
using System.Xml;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;

// === 为脚本提供的各种接口 ===

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 各种 XML 记录的基础类
    /// </summary>
    public class XmlRecord
    {
        public string Path { get; set; }

        public string DbName
        {
            get
            {
                if (string.IsNullOrEmpty(Path))
                    return "";
                return StringUtil.GetDbName(Path);
            }
        }

        internal XmlDocument _dom = null;

        // public string RefID { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dom">XmlDocument 对象。复制之后使用，以保证安全</param>
        public XmlRecord(XmlDocument dom, string recpath)
        {
            _dom = new XmlDocument();
            _dom.LoadXml(dom.OuterXml);

            Path = recpath;
        }

        public string GetField(string name)
        {
            return DomUtil.GetElementText(_dom.DocumentElement, name);
        }

        // 获得当前方法的名称
        public static string FieldName()
        {
            // https://weblogs.asp.net/palermo4/how-to-obtain-method-name-programmatically-for-tracing
            string text = new System.Diagnostics.StackFrame(1).GetMethod().Name;
            if (text.StartsWith("get_"))
                text = text.Substring(4);   //  去掉 get_ 部分
            return new string(char.ToLower(text[0]), 1) // 第一个字母变小写
                + text.Substring(1);
        }
    }

    /// <summary>
    /// 读者 XML 记录
    /// </summary>
    public class PatronRecord : XmlRecord
    {
        public PatronRecord(XmlDocument dom, string recpath) : base(dom, recpath)
        {
        }

        public string refID
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string libraryCode
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string barcode
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string state
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string readerType
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string patronType
        {
            get
            {
                return this.GetField("readerType");
            }
        }

        public string createDate
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string expireDate
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }


        public string name
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string namePinyin
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string gender
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

#if NO
        public string birthday
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }
#endif

        public string dateOfBirth
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string idCardNumber
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string department
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string post
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string address
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string tel
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        // TODO: 需要提供细节分解的属性
        public string email
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string comment
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string hire
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string cardNumber
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string preference
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

#if NO
        public string outofReservations
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }
#endif

        public string nation
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string fingerprint
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string rights
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string personalLibrary
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string friends
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string access
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }
    }

    /// <summary>
    /// 册 XML 记录
    /// </summary>
    public class ItemRecord : XmlRecord
    {
        public ItemRecord(XmlDocument dom, string recpath) : base(dom, recpath)
        {
            //this.BookType = DomUtil.GetElementText(dom.DocumentElement, "bookType");
            //this.Barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
        }

        public string refID
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string parent
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string barcode
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string state
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string publishTime
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string location
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        // 加工后纯净的 location 内容，不包含 '#reservation' 部分
        public string pureLocation
        {
            get
            {
                string text = location;
                return StringUtil.GetPureLocation(text);
            }
        }

        public string seller
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string source
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string price
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string bookType
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string registerNo
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string comment
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string mergeComment
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string batchNo
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string volume
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string accessNo
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string intact
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string bindingCost
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string oldRefID
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }

        public string shelfNo
        {
            get
            {
                return this.GetField(XmlRecord.FieldName());
            }
        }
    }

    // 帐户信息
    public class AccountRecord
    {
        private Account _account = null;

        // 获得当前方法的名称
        static string FieldName()
        {
            // https://weblogs.asp.net/palermo4/how-to-obtain-method-name-programmatically-for-tracing
            string text = new System.Diagnostics.StackFrame(1).GetMethod().Name;
            if (text.StartsWith("get_"))
                text = text.Substring(4);   //  去掉 get_ 部分
            return text;
        }

        static string CallMethod(Object obj, string name)
        {
            MethodInfo method = obj.GetType().GetMethod(name);
            return (string)method.Invoke(obj, new object[0]);
        }

        static string GetProperty(Object obj, string name)
        {
            obj.GetType().GetField(name);   // ?
            PropertyInfo prop = obj.GetType().GetProperty(name);
            return (string)prop.GetValue(obj);
        }

        static string GetField(Object obj, string name)
        {
            FieldInfo info = obj.GetType().GetField(name);
            return (string)info.GetValue(obj);
        }

        // 工作台号
        /// <summary>
        /// 工作台号。前端用 dp2library Login() API 时，strParameters 参数中 location=??? 子参数的值
        /// </summary>
        public string Location
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        // 登录名 带有前缀的各种渠道的登录名字
        public string LoginName
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 获得账户类型。值为 空/worker/reader 之一。空或worker 表示工作人员类型；reader 表示读者类型
        /// </summary>
        public string Type
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 用户权限。这是一个逗号分隔的字符串
        /// </summary>
        public string Rights
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 账户所从属的馆代码。
        /// 这是一个逗号分隔的字符串。一个账户有可能具备多于一个馆代码。
        /// 馆代码空表示这是一个全局账户
        /// </summary>
        public string AccountLibraryCode
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 存取权限代码
        /// </summary>
        public string Access
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 用户唯一标识。对于工作人员类型，这是账户名；对于读者类型，这就是证条码号
        /// </summary>
        public string UserID
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 账户的绑定信息
        /// </summary>
        public string Binding
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 证条码号。只有读者类型的帐户用到此字段
        /// </summary>
        public string Barcode
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 姓名。只有读者类型的帐户用到此字段
        /// </summary>
        public string Name
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 显示名。只有读者类型的帐户用到此字段
        /// </summary>
        public string DisplayName
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 书斋名。只有读者类型的帐户用到此字段
        /// </summary>
        public string PersonalLibrary
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        XmlDocument _patronDom = null;

        /// <summary>
        /// 如果是读者帐户，这里是读者记录的 XmlDocument 对象
        /// </summary>
        public XmlDocument ReaderDom
        {
            get
            {
                return _patronDom;
            }
        }

        /// <summary>
        /// 读者记录路径
        /// </summary>
        public string PatronDomPath
        {
            get
            {
                return GetProperty(_account, "get_ReaderDomPath");
            }
        }

        /// <summary>
        /// 最原始的权限定义
        /// </summary>
        public string RightsOrigin
        {
            get
            {
                return GetProperty(_account, FieldName());
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="account"></param>
        public AccountRecord(Account account)
        {
            this._account = account;

            if (account.PatronDom != null)
            {
                // 复制
                this._patronDom = new XmlDocument();
                this._patronDom.LoadXml(account.PatronDom.OuterXml);
            }
        }
    }
}
