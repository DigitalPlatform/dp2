using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Linq;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace dp2Circulation
{
    /// <summary>
    /// 读者信息编辑控件
    /// </summary>
    public partial class ReaderEditControl : ItemEditControlBase
    {
        /// <summary>
        /// 获得图书馆代码
        /// </summary>
        public event GetLibraryCodeEventHandler GetLibraryCode = null;

        /// <summary>
        /// 创建拼音的事件
        /// </summary>
        public event EventHandler CreatePinyin = null;

        /// <summary>
        /// 编辑权限的事件
        /// </summary>
        public event EventHandler EditRights = null;

        // 2022/10/27
        /// <summary>
        /// 编辑 Email 的事件
        /// </summary>
        public event EventHandler EditEmail = null;

        // 2022/10/28
        /// <summary>
        /// 编辑证号的事件
        /// </summary>
        public event EventHandler EditCardNumber = null;

#if NO
        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        /// <summary>
        /// 内容发生改变
        /// </summary>
        public event ContentChangedEventHandler ContentChanged = null;

        // Font ChangedTextFont = null;    // 表示被改动过的内容的字体

        XmlDocument RecordDom = null;

        bool m_bChanged = false;

        bool m_bInInitial = true;   // 是否正在初始化过程之中

        Color ColorChanged = Color.Yellow; // 表示内容改变过的颜色
        Color ColorDifference = Color.Blue; // 表示差异的颜色
#endif

        #region 数据成员

#if NO
        public string OldRecord = "";
        public byte[] Timestamp = null;
#endif

        /// <summary>
        /// 背景颜色
        /// </summary>
        [Category("Appearance")]
        [DescriptionAttribute("Back Color")]
        [DefaultValue(typeof(Color), "GhostWhite")]
        public override Color BackColor
        {
            get
            {
                return this.tableLayoutPanel_main.BackColor;
            }
            set
            {
                this.tableLayoutPanel_main.BackColor = value;
            }
        }

        /// <summary>
        /// 读者证条码号
        /// </summary>
        public string Barcode
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.textBox_barcode.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.textBox_barcode.Text = value;
                });
            }
        }

        /// <summary>
        /// 证号
        /// </summary>
        public string CardNumber
        {
            get
            {
                return this.textBox_cardNumber.Text;
            }
            set
            {
                this.textBox_cardNumber.Text = value;
            }
        }

        // 指纹特征字符串
        // base64编码方式
        /// <summary>
        /// 指纹特征字符串。
        /// base64编码方式
        /// </summary>
        public string FingerprintFeature
        {
            get
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                return DomUtil.GetElementText(this._dataDom.DocumentElement, "fingerprint");
            }
            set
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                DomUtil.SetElementText(this._dataDom.DocumentElement, "fingerprint", value);

                // 清除<fingerprint>元素
                if (string.IsNullOrEmpty(value) == true)
                {
                    XmlNode node = this._dataDom.DocumentElement.SelectSingleNode("fingerprint");
                    if (node != null)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }
        }

        // 指纹特征字符串的版本号
        /// <summary>
        /// 指纹特征字符串的版本号
        /// </summary>
        public string FingerprintFeatureVersion
        {
            get
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                XmlNode node = this._dataDom.DocumentElement.SelectSingleNode("fingerprint");
                if (node == null)
                    return "";
                return DomUtil.GetAttr(node, "version");
            }
            set
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                XmlNode node = this._dataDom.DocumentElement.SelectSingleNode("fingerprint");
                if (node == null)
                {
                    if (string.IsNullOrEmpty(value) == true)
                        return; // 正好,既然元素不存在, 就不用删除了
                    node = this._dataDom.CreateElement("fingerprint");
                    this._dataDom.DocumentElement.AppendChild(node);
                }

                DomUtil.SetAttr(node, "version", value);
            }
        }


        /// <summary>
        /// 人脸特征字符串。
        /// base64编码方式
        /// </summary>
        public string FaceFeature
        {
            get
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                return DomUtil.GetElementText(this._dataDom.DocumentElement, "face");
            }
            set
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                DomUtil.SetElementText(this._dataDom.DocumentElement, "face", value);

                // 清除<face>元素
                if (string.IsNullOrEmpty(value) == true)
                {
                    XmlNode node = this._dataDom.DocumentElement.SelectSingleNode("face");
                    if (node != null)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }
        }

        /// <summary>
        /// 人脸特征字符串的版本号
        /// </summary>
        public string FaceFeatureVersion
        {
            get
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                XmlNode node = this._dataDom.DocumentElement.SelectSingleNode("face");
                if (node == null)
                    return "";
                return DomUtil.GetAttr(node, "version");
            }
            set
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                XmlNode node = this._dataDom.DocumentElement.SelectSingleNode("face");
                if (node == null)
                {
                    if (string.IsNullOrEmpty(value) == true)
                        return; // 正好,既然元素不存在, 就不用删除了
                    node = this._dataDom.CreateElement("face");
                    this._dataDom.DocumentElement.AppendChild(node);
                }

                DomUtil.SetAttr(node, "version", value);
            }
        }

        /// <summary>
        /// 掌纹特征字符串。
        /// base64编码方式
        /// </summary>
        public string PalmprintFeature
        {
            get
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                return DomUtil.GetElementText(this._dataDom.DocumentElement, "palmprint");
            }
            set
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                DomUtil.SetElementText(this._dataDom.DocumentElement, "palmprint", value);

                // 清除<palmprint>元素
                if (string.IsNullOrEmpty(value) == true)
                {
                    XmlNode node = this._dataDom.DocumentElement.SelectSingleNode("palmprint");
                    if (node != null)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }
        }

        // 掌纹特征字符串的版本号
        /// <summary>
        /// 指纹特征字符串的版本号
        /// </summary>
        public string PalmprintFeatureVersion
        {
            get
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                XmlNode node = this._dataDom.DocumentElement.SelectSingleNode("palmprint");
                if (node == null)
                    return "";
                return DomUtil.GetAttr(node, "version");
            }
            set
            {
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                XmlNode node = this._dataDom.DocumentElement.SelectSingleNode("palmprint");
                if (node == null)
                {
                    if (string.IsNullOrEmpty(value) == true)
                        return; // 正好,既然元素不存在, 就不用删除了
                    node = this._dataDom.CreateElement("palmrprint");
                    this._dataDom.DocumentElement.AppendChild(node);
                }

                DomUtil.SetAttr(node, "version", value);
            }
        }

        /// <summary>
        /// 读者记录状态
        /// </summary>
        public string State
        {
            get
            {
                return this.comboBox_state.Text;
            }
            set
            {
                this.comboBox_state.Text = value;
            }
        }

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }

        /// <summary>
        /// 读者类型
        /// </summary>
        public string ReaderType
        {
            get
            {
                return this.comboBox_readerType.Text;
            }
            set
            {
                this.comboBox_readerType.Text = value;
            }
        }

        // 创建日期(RFC1123格式)
        /// <summary>
        /// 创建日期(RFC1123格式)
        /// </summary>
        public string CreateDate
        {
            get
            {
                return GetDateTimeString(this.dateControl_createDate.Value);
            }
            set
            {
                this.dateControl_createDate.Value = GetDateTime(value);
            }
        }

        // 失效日期(RFC1123格式)
        /// <summary>
        /// 失效日期(RFC1123格式)
        /// </summary>
        public string ExpireDate
        {
            get
            {
                return GetDateTimeString(this.dateControl_expireDate.Value);
            }
            set
            {
                this.dateControl_expireDate.Value = GetDateTime(value);
            }
        }

        // 2007/6/15
        // 租金失效日期(RFC1123格式)
        /// <summary>
        /// 租金失效日期(RFC1123格式)
        /// </summary>
        public string HireExpireDate
        {
            get
            {
                return GetDateTimeString(this.dateControl_hireExpireDate.Value);
            }
            set
            {
                this.dateControl_hireExpireDate.Value = GetDateTime(value);
            }
        }

        // 2007/6/15
        /// <summary>
        /// 租金周期
        /// </summary>
        public string HirePeriod
        {
            get
            {
                return this.comboBox_hirePeriod.Text;
            }
            set
            {
                this.comboBox_hirePeriod.Text = value;
            }
        }

        // 2008/11/11
        /// <summary>
        /// 押金
        /// </summary>
        public string Foregift
        {
            get
            {
                return this.textBox_foregift.Text;
            }
            set
            {
                this.textBox_foregift.Text = value;
            }
        }

        // 出生日期(RFC1123格式)
        /// <summary>
        /// 出生日期(RFC1123格式)
        /// </summary>
        public string DateOfBirth
        {
            get
            {
                return GetDateTimeString(this.dateControl_dateOfBirth.Value);
            }
            set
            {
                this.dateControl_dateOfBirth.Value = GetDateTime(value);
            }
        }

        /// <summary>
        /// 读者姓名
        /// </summary>
        public string NameString
        {
            get
            {
                return this.textBox_name.Text;
            }
            set
            {
                this.textBox_name.Text = value;
            }
        }

        /// <summary>
        /// 读者姓名拼音
        /// </summary>
        public string NamePinyin
        {
            get
            {
                return this.textBox_namePinyin.Text;
            }
            set
            {
                this.textBox_namePinyin.Text = value;
            }
        }

        /// <summary>
        /// 性别
        /// </summary>
        public string Gender
        {
            get
            {
                return this.comboBox_gender.Text;
            }
            set
            {
                this.comboBox_gender.Text = value;
            }
        }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdCardNumber
        {
            get
            {
                return this.textBox_idCardNumber.Text;
            }
            set
            {
                this.textBox_idCardNumber.Text = value;
            }
        }

        /// <summary>
        /// 部门
        /// </summary>
        public string Department
        {
            get
            {
                //return this.TryGet(() =>
                //{
                    return this.textBox_department.Text;
                //});
            }
            set
            {
                //this.TryInvoke(() =>
                //{
                    this.textBox_department.Text = value;
                //});
            }
        }

        // 2009/7/17
        /// <summary>
        /// 职别
        /// </summary>
        public string Post
        {
            get
            {
                return this.textBox_post.Text;
            }
            set
            {
                this.textBox_post.Text = value;
            }
        }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address
        {
            get
            {
                return this.textBox_address.Text;
            }
            set
            {
                this.textBox_address.Text = value;
            }
        }

        /// <summary>
        /// 电话
        /// </summary>
        public string Tel
        {
            get
            {
                return this.textBox_tel.Text;
            }
            set
            {
                this.textBox_tel.Text = value;
            }
        }

        /// <summary>
        /// Email 地址
        /// </summary>
        public string Email
        {
            get
            {
                return this.textBox_email.Text;
            }
            set
            {
                this.textBox_email.Text = value;
            }
        }

        /// <summary>
        /// 权限
        /// </summary>
        public string Rights
        {
            get
            {
                return this.textBox_rights.Text;
            }
            set
            {
                this.textBox_rights.Text = value;
            }
        }

        /// <summary>
        /// 存取定义
        /// </summary>
        public string Access
        {
            get
            {
                return this.textBox_access.Text;
            }
            set
            {
                this.textBox_access.Text = value;
            }
        }

        /// <summary>
        /// 书斋名称
        /// </summary>
        public string PersonalLibrary
        {
            get
            {
                return this.textBox_personalLibrary.Text;
            }
            set
            {
                this.textBox_personalLibrary.Text = value;
            }
        }

        /// <summary>
        /// 好友
        /// </summary>
        public string Friends
        {
            get
            {
                return this.textBox_friends.Text;
            }
            set
            {
                this.textBox_friends.Text = value;
            }
        }

        /// <summary>
        /// 读者记录路径
        /// </summary>
        public string RecPath
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.textBox_recPath.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.textBox_recPath.Text = value;
                });
            }
        }

        /// <summary>
        /// 参考ID
        /// </summary>
        public string RefID
        {
            get
            {
                return this.textBox_refID.Text;
            }
            set
            {
                this.textBox_refID.Text = value;
            }
        }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReaderEditControl()
        {
            base.InitializeComponent();

            InitializeComponent();

            base._tableLayoutPanel_main = this.tableLayoutPanel_main;

            AddEvents(true);
            /*
            Debug.Assert(false, "");
            this.dateTimePicker_birthday.Value = DateTimePicker.MinimumDateTime;    // new DateTime(0);
             * */

            // 2021/7/17
            InitialTags();
            HighlightKeyLines();
        }

        private void ReaderEditControl_SizeChanged(object sender, EventArgs e)
        {
            tableLayoutPanel_main.Size = this.Size;
        }

#if NO
        public bool Initializing
        {
            get
            {
                return this.m_bInInitial;
            }
            set
            {
                this.m_bInInitial = value;
            }
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                bool bOldValue = this.m_bChanged;

                this.m_bChanged = value;
                if (this.m_bChanged == false)
                    this.ResetColor();

                // 触发事件
                if (bOldValue != value && this.ContentChanged != null)
                {
                    ContentChangedEventArgs e = new ContentChangedEventArgs();
                    e.OldChanged = bOldValue;
                    e.CurrentChanged = value;
                    ContentChanged(this, e);
                }
            }
        }
#endif

        /// <summary>
        /// 将 RFC1123 时间字符串转换为显示用的本地时间字符串
        /// </summary>
        /// <param name="strTime">RFC1123 时间字符串</param>
        /// <returns>本地时间字符串</returns>
        public static DateTime GetDateTime(string strTime)
        {
            DateTime time = new DateTime((long)0);

            if (String.IsNullOrEmpty(strTime) == true)
                return time;
            try
            {
                time = DateTimeUtil.FromRfc1123DateTimeString(strTime);
            }
            catch
            {
                return new DateTime((long)0);
            }

            return time.ToLocalTime();
        }

        // parameters:
        //      time    是本地时间
        /// <summary>
        /// 获得 RFC1123 时间字符串
        /// </summary>
        /// <param name="time">本地时间</param>
        /// <returns>RDC1123 字符串</returns>
        public static string GetDateTimeString(DateTime time)
        {
            if (time == new DateTime((long)0))
                return "";

            string strValue = "";
            try
            {
                strValue = DateTimeUtil.Rfc1123DateTimeStringEx(time);
            }
            catch
            {
                return "";
            }
            return strValue;
        }

        internal override void DomToMember(string strRecPath)
        {
            this.Barcode = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "barcode");
            this.CardNumber = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "cardNumber");

            this.State = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "state");

            this.Comment = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "comment");

            this.ReaderType = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "readerType");

            this.CreateDate = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "createDate");

            this.ExpireDate = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "expireDate");

            // 2007/6/15
            // 租金
            XmlNode nodeHire = this._dataDom.DocumentElement.SelectSingleNode("hire");
            if (nodeHire != null)
            {
                this.HireExpireDate = DomUtil.GetAttr(nodeHire, "expireDate");
                this.HirePeriod = DomUtil.GetAttr(nodeHire, "period");
            }
            else
            {
                this.HireExpireDate = "";
                this.HirePeriod = "";
            }

            // 2008/11/11
            // 押金
            this.Foregift = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "foregift");

            this.NameString = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "name");

            this.NamePinyin = DomUtil.GetElementText(this._dataDom.DocumentElement,
    "namePinyin");

            this.Gender = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "gender");

            this.DateOfBirth = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "dateOfBirth");
            if (string.IsNullOrEmpty(this.DateOfBirth) == true)
            {
                // 兼容旧习惯
                this.DateOfBirth = DomUtil.GetElementText(this._dataDom.DocumentElement,
                    "birthday");
            }

            this.IdCardNumber = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "idCardNumber");

            this.Department = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "department");

            this.Post = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "post");

            this.Address = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "address");

            this.Tel = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "tel");

            this.Email = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "email");

            this.Rights = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "rights");
            this.PersonalLibrary = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "personalLibrary");
            this.Access = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "access");
            this.Friends = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "friends");
            this.RefID = DomUtil.GetElementText(this._dataDom.DocumentElement,
                "refID");

            this.RecPath = strRecPath;

            base.DomToMember(strRecPath);
        }

        /// <summary>
        /// 清除控件内全部内容
        /// </summary>
        public override void Clear()
        {
            this.TryInvoke(() =>
            {
                this.Initializing = true; // 防止因为清除而变色

                try
                {
                    this.Barcode = "";

                    this.CardNumber = "";

                    this.State = "";

                    this.Comment = "";

                    this.ReaderType = "";

                    this.CreateDate = "";

                    this.ExpireDate = "";

                    // 2007/6/15
                    this.HirePeriod = "";
                    this.HireExpireDate = "";

                    this.Foregift = "";

                    this.NameString = "";

                    this.NamePinyin = "";

                    this.Gender = "";

                    this.DateOfBirth = "";

                    this.IdCardNumber = "";

                    this.Department = "";

                    this.Post = "";

                    this.Address = "";

                    this.Tel = "";

                    this.Email = "";

                    this.Rights = "";

                    this.PersonalLibrary = "";
                    this.Access = "";

                    this.Friends = "";
                    this.RefID = "";

                    this.ResetColor();
                }
                finally
                {
                    this.Initializing = false;
                }
            });
        }

#if NO
        public XmlDocument DataDom
        {
            get
            {
                // 2012/12/28
                if (this.RecordDom == null)
                {
                    this.RecordDom = new XmlDocument();
                    this.RecordDom.LoadXml("<root />");
                } 
                this.RefreshDom();
                return this.RecordDom;
            }
        }
#endif

        /// <summary>
        /// 清除预约未取次数
        /// </summary>
        /// <returns>读者记录是否发生了修改</returns>
        public bool ClearOutofReservationCount()
        {
            if (this._dataDom == null)
                return false;

            XmlNode root = this._dataDom.DocumentElement.SelectSingleNode("outofReservations");
            if (root == null)
                return false;

            // 累计次数
            string strCount = DomUtil.GetAttr(root, "count");
            if (String.IsNullOrEmpty(strCount) == true)
                return false;
            int nCount = 0;
            try
            {
                nCount = Convert.ToInt32(strCount);
            }
            catch
            {
            }
            if (nCount == 0)
                return false;

            DomUtil.SetAttr(root, "count", "0");
            this.Changed = true;
            return true;
        }

        internal override void RefreshDom()
        {
            this.TryInvoke(() =>
            {
                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "barcode", this.Barcode);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "cardNumber", this.CardNumber);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "state", this.State);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "comment", this.Comment);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "readerType", this.ReaderType);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "createDate", this.CreateDate);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "expireDate", this.ExpireDate);

                // 2007/6/15
                XmlNode nodeHire = null;
                nodeHire = this._dataDom.DocumentElement.SelectSingleNode("hire");
                if (nodeHire == null)
                {
                    nodeHire = this._dataDom.CreateElement("hire");
                    this._dataDom.DocumentElement.AppendChild(nodeHire);
                }
                DomUtil.SetAttr(nodeHire, "expireDate", this.HireExpireDate);
                DomUtil.SetAttr(nodeHire, "period", this.HirePeriod);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "foregift", this.Foregift);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "name", this.NameString);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
        "namePinyin", this.NamePinyin);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "gender", this.Gender);

                // 2012/4/11
                // 根据记录中是否已经有<dateOfBirth>元素来决定是否使用这个元素，以免对旧的dp2Library版本写记录过程中丢失<dateOfBirth>元素
                XmlNode nodeExistBirthdate = this._dataDom.DocumentElement.SelectSingleNode("dateOfBirth");    // BUG 2012/5/3 原先少了.DocumentElement
                if (nodeExistBirthdate == null)
                    DomUtil.SetElementText(this._dataDom.DocumentElement,
                        "birthday", this.DateOfBirth);
                else
                    DomUtil.SetElementText(this._dataDom.DocumentElement,
                        "dateOfBirth", this.DateOfBirth);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "idCardNumber", this.IdCardNumber);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "department", this.Department);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "post", this.Post);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "address", this.Address);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "tel", this.Tel);

                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "email", this.Email);
                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "rights", this.Rights);
                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "personalLibrary", this.PersonalLibrary);
                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "access", this.Access);
                DomUtil.SetElementText(this._dataDom.DocumentElement,
                    "friends", this.Friends);
                DomUtil.SetElementText(this._dataDom.DocumentElement,
        "refID", this.RefID);

                base.RefreshDom();
            });
        }

        /// <summary>
        /// 创建好适合于保存的记录信息
        /// </summary>
        /// <param name="strXml">返回构造好的读者记录 XML</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int GetData(
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            if (this._dataDom == null)
            {
                this._dataDom = new XmlDocument();
                this._dataDom.LoadXml("<root />");
            }


            /*
            if (this.Barcode == "")
            {
                strError = "Barcode成员尚未定义";
                return -1;
            }*/

            this.RefreshDom();

            // 删除空元素
            if (RemoveEmptyElement(ref this._dataDom, out strError) == -1)
                return -1;

            strXml = this._dataDom.OuterXml;
            return 0;
        }

        int RemoveEmptyElement(ref XmlDocument dom,
            out string strError)
        {
            strError = "";

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*");
            foreach (XmlElement element in nodes)
            {
                string strInnerXml = element.InnerXml.Trim();
                if (string.IsNullOrEmpty(strInnerXml) == true
                    && element.Attributes.Count == 0)
                    element.ParentNode.RemoveChild(element);
            }

            return 0;
        }

#if NO
        /// <summary>
        /// 只读状态风格
        /// </summary>
        public enum ReadOnlyStyle
        {
            /// <summary>
            /// 清除全部只读状态，恢复可编辑状态
            /// </summary>
            Clear = 0,  // 清除全部ReadOnly状态，恢复可编辑状态
            /// <summary>
            /// 全部只读
            /// </summary>
            All = 1,    // 全部禁止修改
            /// <summary>
            /// 图书馆一般工作人员，不能修改路径
            /// </summary>
            Librarian = 2,  // 图书馆工作人员，不能修改路径
            /// <summary>
            /// 读者。不能修改条码等许多字段
            /// </summary>
            Reader = 3, // 读者。不能修改条码等许多字段
        }
#endif

        /// <summary>
        /// 设置只读状态
        /// </summary>
        /// <param name="strStyle">如何设置只读状态。
        /// "all" 表示全部为只读；
        /// "librarian" 表示只有记录路径、失效时间为只读，其余为可改写;
        /// "reader" 表示只有姓名等几项为可改写，其余为只读;
        /// "clear" 表示清除全部只读状态，也即全部都是可改写状态
        /// </param>
        public override void SetReadOnly(string strStyle)
        {
            if (strStyle.StartsWith("editable:"))
            {
                // 注: editable:[all] 表示全部都可编辑
                // editable: 表示全部都不可以编辑
                string list = strStyle.Substring("editable:".Length);
                var names = StringUtil.SplitList(list);

                foreach (Control child in this.tableLayoutPanel_main.Controls)
                {
                    var name = child.Tag as string;
                    if (name == null)
                        continue;

                    // 任何情形下，refID 要设置为 readonly 状态
                    if (name == "refID" || name == "email")
                    {
                        SetReadOnly(child, true);
                        continue;
                    }

                    // [all] 情形下，recPath 和 refID 要设置为 readonly 状态
                    if (list == "[all]"
                        && (name == "recPath" || name == "refID")
                        )
                    {
                        SetReadOnly(child, true);
                        continue;
                    }

                    // 有可能是 "hire,expireDate" 这样的形态
                    if (name.Contains(","))
                        name = name.Substring(0, name.IndexOf(","));

                    if (names.IndexOf(name) == -1
                        && list != "[all]")
                        SetReadOnly(child, true);
                    else
                        SetReadOnly(child, false);
                }

                return;
            }

            else if (strStyle == "all")
            {
                this.textBox_barcode.ReadOnly = true;
                this.textBox_cardNumber.ReadOnly = true;
                this.comboBox_readerType.Enabled = false;
                this.comboBox_state.Enabled = false;
                this.textBox_comment.Enabled = false;

                this.dateControl_createDate.Enabled = false;

                this.dateControl_expireDate.Enabled = false;

                // 2007/6/15
                this.dateControl_hireExpireDate.Enabled = false;
                this.comboBox_hirePeriod.Enabled = false;

                this.textBox_foregift.ReadOnly = true;

                this.dateControl_dateOfBirth.Enabled = false;

                this.textBox_name.ReadOnly = true;
                this.textBox_namePinyin.ReadOnly = true;

                this.comboBox_gender.Enabled = false;

                this.textBox_idCardNumber.ReadOnly = true;

                this.textBox_department.ReadOnly = true;

                this.textBox_post.ReadOnly = true;

                this.textBox_address.ReadOnly = true;

                this.textBox_tel.ReadOnly = true;

                this.textBox_email.ReadOnly = true;
                this.textBox_rights.ReadOnly = true;
                this.textBox_personalLibrary.ReadOnly = true;
                this.textBox_access.ReadOnly = true;
                this.textBox_friends.ReadOnly = true;

                this.textBox_recPath.ReadOnly = true;
                this.textBox_refID.ReadOnly = true;
                return;
            }

            // 先清除
            this.textBox_barcode.ReadOnly = false;
            this.textBox_cardNumber.ReadOnly = false;
            this.comboBox_readerType.Enabled = true;
            this.comboBox_state.Enabled = true;
            this.textBox_comment.ReadOnly = false;
            this.dateControl_createDate.Enabled = true;
            this.dateControl_expireDate.Enabled = true;

            // 2007/6/15
            this.dateControl_hireExpireDate.Enabled = true;
            this.comboBox_hirePeriod.Enabled = true;

            this.textBox_foregift.ReadOnly = false;

            this.dateControl_dateOfBirth.Enabled = true;
            this.textBox_name.ReadOnly = false;
            this.textBox_namePinyin.ReadOnly = false;
            this.comboBox_gender.Enabled = true;
            this.textBox_idCardNumber.ReadOnly = false;
            this.textBox_department.ReadOnly = false;
            this.textBox_post.ReadOnly = false;
            this.textBox_address.ReadOnly = false;
            this.textBox_tel.ReadOnly = false;
            this.textBox_email.ReadOnly = false;
            this.textBox_rights.ReadOnly = false;
            this.textBox_personalLibrary.ReadOnly = false;
            this.textBox_access.ReadOnly = false;
            this.textBox_friends.ReadOnly = false;

            this.textBox_recPath.ReadOnly = false;
            this.textBox_refID.ReadOnly = false;

            if (strStyle == "librarian")
            {
                this.textBox_recPath.ReadOnly = true;
                this.textBox_refID.ReadOnly = true;
                this.textBox_email.ReadOnly = true; // 2022/10/27

                // 2007/6/15
                this.dateControl_hireExpireDate.Enabled = false;
            }
            else if (strStyle == "reader")
            {
                this.textBox_barcode.ReadOnly = true;
                this.textBox_cardNumber.ReadOnly = true;
                this.comboBox_readerType.Enabled = false;
                this.comboBox_state.Enabled = false;
                this.textBox_comment.ReadOnly = true;

                this.dateControl_createDate.Enabled = false;

                this.dateControl_expireDate.Enabled = false;
                this.textBox_foregift.ReadOnly = true;

                // 2007/6/15
                this.dateControl_hireExpireDate.Enabled = false;
                this.comboBox_hirePeriod.Enabled = false;

                this.textBox_email.ReadOnly = true; // 2022/10/27

                this.textBox_recPath.ReadOnly = true;
                this.textBox_refID.ReadOnly = true;
            }
            else if (strStyle == "clear")
            {
                // 前面已经清除
            }

            void SetReadOnly(Control child, bool value)
            {
                if (child is TextBox)
                {
                    ((TextBox)child).ReadOnly = value;
                }
                else
                    child.Enabled = !value;

                // TODO: 右侧按钮也要禁用

                // 改变 Label 颜色
                var label = GetLabel(child);
                if (label != null)
                    label.ForeColor = value ? SystemColors.GrayText : SystemColors.ControlText;
            }

            // 根据编辑 Control 找到它左侧的 Label
            Label GetLabel(Control edit)
            {
                var row = this.tableLayoutPanel_main.GetRow(edit);
                if (row == -1)
                    return null;
                return this.tableLayoutPanel_main.GetControlFromPosition(0, row) as Label;
            }
        }

        // 为每个编辑域设置 Tag
        void InitialTags()
        {
            this.textBox_barcode.Tag = "barcode";
            this.textBox_cardNumber.Tag = "cardNumber";
            this.comboBox_readerType.Tag = "readerType";
            this.comboBox_state.Tag = "state";
            this.textBox_comment.Tag = "comment";
            this.dateControl_createDate.Tag = "createDate";
            this.dateControl_expireDate.Tag = "expireDate";

            this.dateControl_hireExpireDate.Tag = "hire,expireDate";
            this.comboBox_hirePeriod.Tag = "hire,period";

            this.textBox_foregift.Tag = "foregift";
            this.button_foregiftSum.Tag = "foregift";

            this.dateControl_dateOfBirth.Tag = "dateOfBirth";
            this.textBox_name.Tag = "name";
            this.textBox_namePinyin.Tag = "namePinyin";
            this.button_createNamePinyin.Tag = "namePinyin";
            this.comboBox_gender.Tag = "gender";
            this.textBox_idCardNumber.Tag = "idCardNumber";
            this.textBox_department.Tag = "department";
            this.textBox_post.Tag = "post";
            this.textBox_address.Tag = "address";
            this.textBox_tel.Tag = "tel";
            this.textBox_email.Tag = "email";
            this.textBox_rights.Tag = "rights";
            this.button_editRights.Tag = "rights";
            this.textBox_personalLibrary.Tag = "personalLibrary";
            this.textBox_access.Tag = "access";
            this.textBox_friends.Tag = "friends";

            this.textBox_recPath.Tag = "recPath";
            this.textBox_refID.Tag = "refID";
        }

        void HighlightKeyLines()
        {
            Font font = new Font(this.Font, FontStyle.Bold);

            this.label_barcode.Font = font;
            this.textBox_barcode.Font = font;

            this.label_readerType.Font = font;
            this.comboBox_readerType.Font = font;

            /*
            this.label_name_color.Font = font;
            this.textBox_name.Font = font;
            */

            this.label_department.Font = font;
            this.textBox_department.Font = font;

            this.label_address.Font = font;
            this.textBox_address.Font = font;

            this.label_tel.Font = font;
            this.textBox_tel.Font = font;
        }

        // 比较自己和refControl的数据差异，用特殊颜色显示差异字段
        /// <summary>
        /// 比较自己和refControl的数据差异，用特殊颜色显示差异字段
        /// </summary>
        /// <param name="r">要和自己进行比较的控件对象</param>
        public override void HighlightDifferences(ItemEditControlBase r)
        {
            var refControl = r as ReaderEditControl;

            if (this.Barcode != refControl.Barcode)
                this.label_barcode_color.BackColor = this.ColorDifference;

            if (this.CardNumber != refControl.CardNumber)
                this.label_cardNumber_color.BackColor = this.ColorDifference;

            if (this.ReaderType != refControl.ReaderType)
                this.label_readerType_color.BackColor = this.ColorDifference;

            if (this.State != refControl.State)
                this.label_state_color.BackColor = this.ColorDifference;

            if (this.Comment != refControl.Comment)
                this.label_comment_color.BackColor = this.ColorDifference;


            if (this.CreateDate != refControl.CreateDate)
                this.label_createDate_color.BackColor = this.ColorDifference;

            if (this.ExpireDate != refControl.ExpireDate)
                this.label_expireDate_color.BackColor = this.ColorDifference;

            // 2007/6/15
            if (this.HireExpireDate != refControl.HireExpireDate)
                this.label_hireExpireDate_color.BackColor = this.ColorDifference;

            if (this.HirePeriod != refControl.HirePeriod)
                this.label_hirePeriod_color.BackColor = this.ColorDifference;

            if (this.Foregift != refControl.Foregift)
                this.label_foregift_color.BackColor = this.ColorDifference;

            if (this.NameString != refControl.NameString)
                this.label_name_color.BackColor = this.ColorDifference;

            if (this.NamePinyin != refControl.NamePinyin)
                this.label_namePinyin_color.BackColor = this.ColorDifference;

            if (this.Gender != refControl.Gender)
                this.label_gender_color.BackColor = this.ColorDifference;

            if (this.DateOfBirth != refControl.DateOfBirth)
                this.label_dateOfBirth_color.BackColor = this.ColorDifference;

            if (this.IdCardNumber != refControl.IdCardNumber)
                this.label_idCardNumber_color.BackColor = this.ColorDifference;

            if (this.Department != refControl.Department)
                this.label_department_color.BackColor = this.ColorDifference;

            if (this.Post != refControl.Post)
                this.label_post_color.BackColor = this.ColorDifference;


            if (this.Address != refControl.Address)
                this.label_address_color.BackColor = this.ColorDifference;

            if (this.Tel != refControl.Tel)
                this.label_tel_color.BackColor = this.ColorDifference;

            if (this.Email != refControl.Email)
                this.label_email_color.BackColor = this.ColorDifference;

            if (this.Rights != refControl.Rights)
                this.label_rights_color.BackColor = this.ColorDifference;

            if (this.PersonalLibrary != refControl.PersonalLibrary)
                this.label_personalLibrary_color.BackColor = this.ColorDifference;

            if (this.Access != refControl.Access)
                this.label_access_color.BackColor = this.ColorDifference;

            if (this.Friends != refControl.Friends)
                this.label_friends_color.BackColor = this.ColorDifference;

            if (this.RecPath != refControl.RecPath)
                this.label_recPath_color.BackColor = this.ColorDifference;

            if (this.RefID != refControl.RefID)
                this.label_refID_color.BackColor = this.ColorDifference;
        }

#if NO
        private void textBox_barcode_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_barcode_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_cardNumber_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_cardNumber_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void comboBox_readerType_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_readerType_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void comboBox_state_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_state_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_comment_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_comment_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void dateControl_createDate_DateTextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_createDate_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void dateControl_expireDate_DateTextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_expireDate_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        // 2007/6/15
        private void comboBox_hirePeriod_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_hirePeriod_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void dateControl_hireExpireDate_DateTextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_hireExpireDate_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }

        }

        private void textBox_foregift_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_foregift_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_name_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_name_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_namePinyin_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_namePinyin_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void comboBox_gender_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_gender_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void dateControl_dateOfBirth_DateTextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_dateOfBirth_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_idCardNumber_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_idCardNumber_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_department_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_department_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_post_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_post_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_address_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_address_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_tel_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_tel_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_email_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_email_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_rights_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_rights_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_personalLibrary_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_personalLibrary_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_access_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_access_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }

        private void textBox_friends_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == false)
            {
                this.label_friends_color.BackColor = this.ColorChanged;
                this.Changed = true;
            }
        }
#endif

        private void textBox_recPath_TextChanged(object sender, EventArgs e)
        {
            // 迫使后面自动重新获得列表值
            this.comboBox_readerType.Items.Clear();
            this.comboBox_state.Items.Clear();
            this.comboBox_hirePeriod.Items.Clear();
        }

        private void ReaderEditControl_Load(object sender, EventArgs e)
        {
            // this.ChangedTextFont = new Font(this.textBox_barcode.Font, FontStyle.Bold);
        }

        // 2009/7/19
        int m_nInDropDown = 0;

        private void comboBox_readerType_DropDown(object sender, EventArgs e)
        {
            ComboBox combobox = (ComboBox)sender;
            if (combobox.Items.Count > 0
    /*|| this.GetValueTable == null*/)
                return;

            // 防止重入
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                e1.DbName = Global.GetDbName(this.RecPath);

                if (combobox == this.comboBox_readerType)
                    e1.TableName = "readerType";
                else if (combobox == this.comboBox_state)
                    e1.TableName = "readerState";
                else if (combobox == this.comboBox_hirePeriod)
                    e1.TableName = "hirePeriod";
                else
                {
                    Debug.Assert(false, "不支持的sender");
                    return;
                }

                // this.GetValueTable(this, e1);
                this.OnGetValueTable(this, e1);

                if (e1.values != null)
                {
                    List<string> results = null;

                    string strRecPath = this.textBox_recPath.Text;
                    string strDbName = Global.GetDbName(strRecPath);
                    if (string.IsNullOrEmpty(strDbName) == false
                        && this.GetLibraryCode != null)
                    {
                        GetLibraryCodeEventArgs e2 = new GetLibraryCodeEventArgs();
                        e2.DbName = strDbName;
                        this.GetLibraryCode(this, e2);
                        string strLibraryCode = e2.LibraryCode;
                        // 过滤出符合管代码的那些值字符串
                        results = Global.FilterValuesWithLibraryCode(strLibraryCode,
                            StringUtil.FromStringArray(e1.values));
                    }
                    else
                    {
                        results = StringUtil.FromStringArray(e1.values);
                    }

                    foreach (string s in results)
                    {
                        combobox.Items.Add(s);
                    }
                }
                else
                {
                    combobox.Items.Add("<not found>");
                }

            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        private void comboBox_state_DropDown(object sender, EventArgs e)
        {
            comboBox_readerType_DropDown(sender, e);

            // 给出缺省值 2014/9/7
            if (this.comboBox_state.Items.Count == 0)
            {
                List<string> values = StringUtil.SplitList("注销,停借,挂失");
                foreach (string s in values)
                {
                    this.comboBox_state.Items.Add(s);
                }
            }
        }

        private void tableLayoutPanel_main_BackColorChanged(object sender, EventArgs e)
        {
            ResetColor();
        }

        // 2007/6/15
        private void comboBox_hirePeriod_DropDown(object sender, EventArgs e)
        {
            comboBox_readerType_DropDown(sender, e);
        }

        private void button_foregiftSum_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_foregift.Text == "")
            {
                strError = "金额为空";
                goto ERROR1;
            }

            List<string> results = null;
            // 将形如"-123.4+10.55-20.3"的价格字符串归并汇总
            int nRet = PriceUtil.SumPrices(this.textBox_foregift.Text,
                out results,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strText = "";
            for (int i = 0; i < results.Count; i++)
            {
                strText += results[i] + "\r\n";
            }

            MessageBox.Show(this, "汇总后的金额为: \r\n" + strText);

            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

#if NO
        private void comboBox_readerType_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_readerType.Invalidate();
        }

        private void comboBox_state_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_state.Invalidate();
        }

        private void comboBox_hirePeriod_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_hirePeriod.Invalidate();
        }

        private void comboBox_gender_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_gender.Invalidate();
        }
#endif

        private void comboBox_readerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.FilterValue(this, (Control)sender);
#if NO
            Delegate_filterValue d = new Delegate_filterValue(FileterValue);
            this.BeginInvoke(d, new object[] { sender });
#endif
        }

        private void comboBox_state_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.FilterValue(this, (Control)sender);
#if NO
            Delegate_filterValue d = new Delegate_filterValue(FileterValue);
            this.BeginInvoke(d, new object[] { sender });
#endif
        }

        private void comboBox_hirePeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.FilterValue(this, (Control)sender);
#if NO
            Delegate_filterValue d = new Delegate_filterValue(FileterValue);
            this.BeginInvoke(d, new object[] { sender });
#endif
        }

        private void button_createNamePinyin_Click(object sender, EventArgs e)
        {
            if (this.CreatePinyin != null)
            {
                this.CreatePinyin(this, new EventArgs());
            }
        }

        // 值列表缓存被清理了。要清除相关 list.Items
        internal void OnValueTableCacheCleared()
        {
            this.comboBox_readerType.Items.Clear();
            this.comboBox_state.Items.Clear();
            this.comboBox_hirePeriod.Items.Clear();
        }

        // 编辑权限
        private void button_editRights_Click(object sender, EventArgs e)
        {
            if (this.EditRights != null)
            {
                this.EditRights(this, new EventArgs());
            }
        }


        public void SetEditable(string visibleFields, string writeableFields)
        {
            var visible_names = StringUtil.SplitList(visibleFields);
            var writeable_names = StringUtil.SplitList(writeableFields);

            List<string> names = null;

            if (IsAll(writeable_names))
                names = visible_names;
            else
            {
                if (IsAll(visible_names))
                    names = writeable_names;
                else
                    names = new List<string>(visible_names.Intersect(writeable_names));
            }

            /*
            if (names.Count == 1
    && names[0] == "[all]")
                names = null;

            if (names == null)
            {
                SetReadOnly("editable:[all]");
                // SetReadOnly("librarian");
            }
            else */

            this.TryInvoke(() =>
            {
                SetReadOnly("editable:" + StringUtil.MakePathList(new List<string>(names)));
            });
        }

        static bool IsAll(List<string> names)
        {
            if (names == null)
                return false;
            return (names.Count == 1
    && names[0] == "[all]");
        }

        private void button_editEmail_Click(object sender, EventArgs e)
        {
            this.EditEmail?.Invoke(this, new EventArgs());
        }

        private void button_editCardNumber_Click(object sender, EventArgs e)
        {
            this.EditCardNumber?.Invoke(this, new EventArgs());
        }
    }
    // 
    /// <summary>
    /// 获得图书馆代码事件
    /// </summary>
    /// <param name="sender">触发者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetLibraryCodeEventHandler(object sender,
    GetLibraryCodeEventArgs e);

    /// <summary>
    /// 获得图书馆代码事件的参数
    /// </summary>
    public class GetLibraryCodeEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 数据库名
        /// </summary>
        public string DbName = "";  // [in] 数据库名

        /// <summary>
        /// [out] 图书馆代码
        /// </summary>
        public string LibraryCode = ""; // [out] 图书馆代码
    }
}
