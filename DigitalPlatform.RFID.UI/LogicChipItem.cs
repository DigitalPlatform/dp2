using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID.UI
{
    // 方便编辑
    [DefaultProperty("UserName")]
    public class LogicChipItem : LogicChip
    {
        #region 特殊信息

        byte _afi = 0;

        [DisplayName("AFI"), Description("AFI")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        [Category("系统信息")]
        [TypeConverter(typeof(ByteHexTypeConverter))]
        public byte AFI
        {
            get
            {
                return _afi;
            }
            set
            {
                if (_afi != value)
                    SetChanged(true);
                _afi = value;
                OnPropertyChanged(FieldName());
            }
        }

        // 默认的图书 AFI 值。归架状态
        public static byte DefaultBookAFI = 0x07;

        byte _dsfid = 0;
        [DisplayName("DSFID"), Description("DSFID")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        [Category("系统信息")]
        [TypeConverter(typeof(ByteHexTypeConverter))]
        public byte DSFID
        {
            get
            {
                return _dsfid;
            }
            set
            {
                if (_dsfid != value)
                    SetChanged(true);

                _dsfid = value;
                OnPropertyChanged(FieldName());
            }
        }

        // 默认的 GB 35660 DSFID 值
        public static byte DefaultDSFID = 0x06;

        bool _eas = false;
        [DisplayName("EAS"), Description("EAS")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        [Category("系统信息")]
        public bool EAS
        {
            get
            {
                return _eas;
            }
            set
            {
                if (_eas != value)
                    SetChanged(true);

                _eas = value;
                OnPropertyChanged(FieldName());
            }
        }

        // 默认的图书 EAS 值。归架状态
        public static bool DefaultBookEAS = true;

        #endregion

        public LogicChipItem() : base()
        {
        }

        // 获得当前方法的名称
        static string FieldName()
        {
            // https://weblogs.asp.net/palermo4/how-to-obtain-method-name-programmatically-for-tracing
            string text = new System.Diagnostics.StackFrame(1).GetMethod().Name;
            if (text.StartsWith("get_") || text.StartsWith("set_"))
                text = text.Substring(4);   //  去掉 get_ 或者 set_ 部分
            return text;
        }

        string GetElementValue(string fieldName)
        {
            ElementOID oid = Element.GetOidByName(fieldName);
            Element element = FindElement(oid);
            if (element == null)
                return "";
            return element.Text;
        }

        void SetElementValue(string fieldName, string value)
        {
            ElementOID oid = Element.GetOidByName(fieldName);
            Element element = FindElement(oid);
            if (element != null && element.Locked)
                throw new Exception("元素处于锁定状态，不允许修改");

            if (element != null && element.Text == value)
                return;

            // 检查 value 是否合法
            string error = Element.VerifyElementText(element.OID, value);
            if (string.IsNullOrEmpty(error) == false)
                throw new Exception($"值 '{value}' 不合法: {error}");

            SetElement(oid, value);
            SetChanged(true);
        }

        #region 元素

        [DisplayName("01 馆藏单件主标识符"), Description("用于唯一标识一个册。常用册条码号来充当")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string PrimaryItemIdentifier
        {

            get { return GetElementValue(FieldName()); }

            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("馆藏单件主标识符 不应为空");

#if NO
                // 检查用户名合法性
                // return:
                //      -1  校验过程出错
                //      0   校验发现不正确
                //      1   校验正确
                if (VerifyPII(value,
                    out string strError) != 1)
                    throw new ArgumentException("馆藏单件主标识符 '" + value + "' 不合法：" + strError);
#endif

                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("03 所属机构"), Description("该馆藏单件所属机构 ISIL 代码")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string OwnerInstitution
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("04 卷(册)信息"), Description("馆藏卷(册)总数和分卷(册)编号")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string SetInformation
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("05 应用类别"), Description("针对馆藏单件或者卷(册)附加的限制性信息")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string TypeOfUsage
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("06 排架位置"), Description("馆藏单件位置代码")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string ShelfLocation
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("07 ONIX媒体格式"), Description("ONIX媒体描述符")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string OnixMediaFormat
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("08 MARC媒体格式"), Description("MARC媒体分类描述符")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string MarcMediaFormat
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("09 供应商标识符"), Description("馆藏供应商标识代码")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string SupplierIdentifier
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("10 订购号"), Description("图书馆与供应商馆藏交易编号")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string OrderNumber
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("11 馆际互借借入机构(ISIL)"), Description("馆际互借借入机构 ISIL 代码")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string IllBorrowingInstitution
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("12 馆际互借作业编号"), Description("标识 1 次馆际互借作业的编号")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string IllBorrowingTransactionNumber
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("13 GS1产品标识符"), Description("GS1 的 GTIN-13 代码")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string Gs1ProductIndentifier
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("14 备选馆藏单件唯一标识符"), Description("新标签架构下可能的编码")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string AlternativeUniqueItemIdentifier
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("15 本地数据A"), Description("本地定义的任何功能项")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string LocalDataA
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("16 本地数据B"), Description("本地定义的任何功能项")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string LocalDataB
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("17 题名"), Description("馆藏单件正题名/题名")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string Title
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("18 本地产品标识符"), Description("非基于 GTIN-13 的产品标识符")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string ProductIdentifierLocal
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("19 媒体格式(其他)"), Description("非 ONIX 或 MARC 的媒体描述符")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string MediaFormat
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("20 供应链阶段"), Description("馆藏当前所在的供应链阶段")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string SupplyChainStage
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("21 供应商发票编号"), Description("图书馆与供应商进行馆藏交易的发票编号")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string SupplierInvoiceNumber
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("22 备选馆藏单件标识符"), Description("馆藏单件可选标识符")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string AlternativeItemIdentifier
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("23 备选所属机构"), Description("所属图书馆/机构的非 ISIL 代码")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string AlternativeOwnerInstitution
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("24 所属机构分馆"), Description("图书馆机构定义的内部代码")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string SubsidiaryOfAnOwnerInstitution
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("25 备选馆际互借借入机构"), Description("馆际互借借入机构的 非ISIL 代码")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string AlternativeIllBorrowingInstitution
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        [DisplayName("26 本地数据C"), Description("本地定义的任何功能项")]
        [Category("元素")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(false)]
        public string LocalDataC
        {
            get { return GetElementValue(FieldName()); }

            set
            {
                SetElementValue(FieldName(), value);
                OnPropertyChanged(FieldName());
            }
        }

        #endregion

#if NO
        void SetReadOnly(string fieldName, bool isReadonly)
        {
            // PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())[fieldName];
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)[fieldName];

            ReadOnlyAttribute attrib = (ReadOnlyAttribute)descriptor.Attributes[typeof(ReadOnlyAttribute)];
            FieldInfo isReadOnly = attrib.GetType().GetField("isReadOnly",
                BindingFlags.NonPublic | BindingFlags.Instance);
            isReadOnly.SetValue(attrib, isReadonly);
        }

        public void InitialAllReadonly()
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this.GetType()))
            {
                // 找元素名
                if (Element.TryGetOidByName(descriptor.Name, out ElementOID oid) == false)
                    continue;

                Element element = FindElement(oid);
                if (element != null)
                {
                    ReadOnlyAttribute attrib = (ReadOnlyAttribute)descriptor.Attributes[typeof(ReadOnlyAttribute)];
                    FieldInfo isReadOnly = attrib.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
                    isReadOnly.SetValue(attrib, element.Locked);
                }
                else
                {
                    ReadOnlyAttribute attrib = (ReadOnlyAttribute)descriptor.Attributes[typeof(ReadOnlyAttribute)];
                    FieldInfo isReadOnly = attrib.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (isReadOnly != null)
                        isReadOnly.SetValue(attrib, false);
                }
            }
        }
#endif

#if NO
        string _ownerInstitution = "";

        public string OwnerInstitution
        {
            get { return _ownerInstitution; }

            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("所属机构(ISIL) 不应为空");

#if NO
                // 检查用户名合法性
                // return:
                //      -1  校验过程出错
                //      0   校验发现不正确
                //      1   校验正确
                if (VerifyOwnerInstitution(value,
                    out string strError) != 1)
                    throw new ArgumentException("_ownerInstitution '" + value + "' 不合法：" + strError);
#endif

                _ownerInstitution = value;
                OnPropertyChanged("OwnerInstitution");
            }
        }

        string _setInformation = "";

        public string SetInformation
        {
            get { return _setInformation; }

            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("卷(册)信息 不应为空");

                _setInformation = value;
                OnPropertyChanged("SetInformation");
            }
        }

#endif

#if NO
        // return:
        //      -1  校验过程出错
        //      0   校验发现不正确
        //      1   校验正确
        public static int VerifyPII(string text,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(text))
            {
                strError = "馆藏单件标识符 不应为空";
                return 0;
            }

            try
            {
                // Compact.CheckIsil(text);
            }
            catch (Exception ex)
            {
                strError = $"馆藏单件标识符 '{text}' 不合法: {ex.Message}";
                return 0;
            }

            return 1;
        }
#endif

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

#endregion

        // 根据物理数据构造 (拆包)
        // parameters:
        //      block_map   每个 char 表示一个 block 的锁定状态。'l' 表示锁定, '.' 表示没有锁定
        public static new LogicChipItem From(byte[] data,
            int block_size,
            string block_map = "")
        {
            LogicChipItem chip = new LogicChipItem();
            chip.Parse(data, block_size, block_map);
            // chip.InitialAllReadonly();
            return chip;
        }
        
        // 构造 LogicChipItem
        // 除了基本数据外，也包括 DSFID EAS AFI
        public static LogicChipItem FromTagInfo(TagInfo tag_info)
        {
            var chip = LogicChipItem.From(tag_info.Bytes,
    (int)tag_info.BlockSize,
    tag_info.LockStatus);

            chip.DSFID = tag_info.DSFID;
            chip.AFI = tag_info.AFI;
            chip.EAS = tag_info.EAS;

            return chip;
        }
    }

}
