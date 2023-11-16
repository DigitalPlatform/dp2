using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.Text;

namespace DigitalPlatform.RFID.UI
{
    // 方便编辑
    [DefaultProperty("UserName")]
    public class LogicChipItem : LogicChip
    {
        #region 特殊信息

        // 保存原始的 bytes。用于读入现有的芯片内容。新创建的 LogicChipItem 对象此成员为 null
        [ReadOnly(true)]
        public byte[] OriginBytes { get; set; }
        [ReadOnly(true)]
        public string OriginLockStatus { get; set; }


        [ReadOnly(true)]
        public int BlockSize { get; set; }
        [ReadOnly(true)]
        public int MaxBlockCount { get; set; }

        string _uid = "";

        [DisplayName("UID"), Description("UID")]
        [System.ComponentModel.RefreshProperties(RefreshProperties.All)]
        [ReadOnly(true)]
        [Category("系统信息")]
        // [TypeConverter(typeof(ByteHexTypeConverter))]
        public string UID
        {
            get
            {
                return _uid;
            }
            set
            {
                if (_uid != value)
                    SetChanged(true);
                _uid = value;
                OnPropertyChanged(FieldName());
            }
        }

        // TODO: 是否需要显示出来
        uint _antenna = 0;

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

        void SetElementValue(string fieldName,
            string value,
            bool verify = true)
        {
            ElementOID oid = Element.GetOidByName(fieldName);
            Element element = FindElement(oid);
            if (element != null && element.Locked)
                throw new Exception("元素处于锁定状态，不允许修改");

            if (element != null && element.Text == value)
                return;

            // 2023/11/10
            // 空值等于删除元素
            if (string.IsNullOrEmpty(value))
            {
                RemoveElement(oid);
                SetChanged(true);
                return;
            }

            // 检查 value 是否合法
            if (verify)
            {
                string error = Element.VerifyElementText(oid, value);
                if (string.IsNullOrEmpty(error) == false)
                    throw new Exception($"值 '{value}' 不合法: {error}");
            }

            SetElement(oid, value, verify);
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
        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        // parameters:
        //      block_map   每个 char 表示一个 block 的锁定状态。'l' 表示锁定, '.' 表示没有锁定
        public static new LogicChipItem From(byte[] data,
            int block_size,
            string block_map = "")
        {
            LogicChipItem chip = new LogicChipItem();
            // Exception:
            //      可能会抛出异常 ArgumentException TagDataException
            chip.Parse(data, block_size, block_map);
            // chip.InitialAllReadonly();
            return chip;
        }

        // 构造 LogicChipItem
        // 除了基本数据外，也包括 DSFID EAS AFI
        // 注: 可以处理 HF 和 UHF 标签
        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        public static LogicChipItem FromTagInfo(TagInfo tag_info)
        {
            if (tag_info.Protocol == InventoryInfo.ISO18000P6C)
            {
                return FromUhfTagInfo(tag_info,
    out string uhfProtocol);
            }
            else
            {
                var chip = LogicChipItem.From(tag_info.Bytes,
        (int)tag_info.BlockSize,
        tag_info.LockStatus);
                chip.Protocol = tag_info.Protocol;
                chip.SetSystemValues(
                    tag_info.Bytes,
                    tag_info.LockStatus,
                    (int)tag_info.MaxBlockCount,
                    (int)tag_info.BlockSize,
                    tag_info.UID,
                    tag_info.DSFID,
                    tag_info.AFI,
                    tag_info.EAS,
                    tag_info.AntennaID);
#if NO
            chip.DSFID = tag_info.DSFID;
            chip.AFI = tag_info.AFI;
            chip.EAS = tag_info.EAS;

            // 2019/1/25
            chip.SetChanged(false);
#endif
                return chip;
            }
        }

        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        public static LogicChipItem FromUhfTagInfo(TagInfo taginfo,
            out string uhfProtocol)
        {
            var epc_bank = Element.FromHexString(taginfo.UID);

            LogicChip chip = null;
            string pii = "";
            string oi = "";
            uhfProtocol = null;
            if (UhfUtility.IsBlankTag(epc_bank, taginfo.Bytes) == true)
            {
                // 空白标签
                pii = null;
            }
            else
            {
                var isGB = UhfUtility.IsISO285604Format(epc_bank, taginfo.Bytes);
                if (isGB)
                {
                    // *** 国标 UHF
                    var parse_result = UhfUtility.ParseTag(epc_bank,
        taginfo.Bytes,
        4);
                    if (parse_result.Value == -1)
                        throw new TagDataException(parse_result.ErrorInfo);
                    chip = parse_result.LogicChip;
                    taginfo.EAS = parse_result.PC.AFI == 0x07;
                    uhfProtocol = "gb";
                    pii = GetPiiPart(parse_result.UII);
                    oi = GetOiPart(parse_result.UII, false);
                }
                else
                {
                    // *** 高校联盟 UHF
                    var parse_result = GaoxiaoUtility.ParseTag(
        epc_bank,
        taginfo.Bytes,
        "convertValueToGB");
                    if (parse_result.Value == -1)
                        throw new TagDataException(parse_result.ErrorInfo);
                    chip = parse_result.LogicChip;
                    taginfo.EAS = parse_result.EpcInfo == null ? false : !parse_result.EpcInfo.Lending;
                    uhfProtocol = "gxlm";
                    pii = GetPiiPart(parse_result.EpcInfo?.PII);
                    oi = GetOiPart(parse_result.EpcInfo?.PII, false);
                    // 2023/11/16
                    if (string.IsNullOrEmpty(oi))
                        oi = parse_result.LogicChip?.FindGaoxiaoOI();

                    /*
                    if (string.IsNullOrEmpty(oi))
                        oi = parse_result.LogicChip?.FindElement(ElementOID.OI)?.Text;
                    if (string.IsNullOrEmpty(oi))
                        oi = parse_result.LogicChip?.FindElement(ElementOID.AOI)?.Text;
                    if (string.IsNullOrEmpty(oi))
                        oi = parse_result.LogicChip?.FindElement((ElementOID)27)?.Text;
                    */
                }
            }

            var result = FromElements(chip);
            result.SetElement(ElementOID.PII, pii, false);
            if (string.IsNullOrEmpty(oi) == false)
                result.SetElement(ElementOID.OI, oi, false);

            result.SetSystemValues(
    taginfo.Bytes,
    taginfo.LockStatus,
    0, // (int)taginfo.MaxBlockCount,
    0, // (int)taginfo.BlockSize,
    taginfo.UID,
    taginfo.DSFID,
    taginfo.AFI,
    taginfo.EAS,
    taginfo.AntennaID);

            if (chip != null)
            {
                chip.Protocol = InventoryInfo.ISO18000P6C;
                if (string.IsNullOrEmpty(uhfProtocol))
                    chip.Protocol += ":" + uhfProtocol;
            }
            return result;
        }

        // 根据 chip 的元素构造一个新的 LogicChipItem 对象
        static LogicChipItem FromElements(LogicChip chip)
        {
            LogicChipItem item = new LogicChipItem();
            if (chip != null)
                item.Elements.AddRange(chip.Elements);
            return item;
        }

        static string BuildUii(string pii, string oi, string aoi)
        {
            if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                return pii;
            if (string.IsNullOrEmpty(oi) == false)
                return oi + "." + pii;
            if (string.IsNullOrEmpty(aoi) == false)
                return aoi + "." + pii;
            return pii;
        }

        // 获得 oi.pii 的 oi 部分
        public static string GetOiPart(string oi_pii, bool return_null)
        {
            // 2023/11/6
            if (oi_pii == null)
            {
                if (return_null)
                    return null;
                return "";
            }

            if (oi_pii.IndexOf(".") == -1)
            {
                if (return_null)
                    return null;
                return "";
            }
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[0];
        }

        // 获得 oi.pii 的 pii 部分
        public static string GetPiiPart(string oi_pii)
        {
            // 2023/11/6
            if (oi_pii == null)
                return null;
            if (oi_pii.IndexOf(".") == -1)
                return oi_pii;
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[1];
        }

        // 构造 ISO15693 的 TagInfo
        public static TagInfo ToTagInfo(TagInfo existing,
            LogicChipItem chip)
        {
            TagInfo new_tag_info = existing.Clone();
            new_tag_info.Bytes = chip.GetBytes(
                (int)(new_tag_info.MaxBlockCount * new_tag_info.BlockSize),
                (int)new_tag_info.BlockSize,
                LogicChip.GetBytesStyle.None,
                out string block_map);
            new_tag_info.LockStatus = block_map;

            // 2019/1/22
            new_tag_info.AFI = chip.AFI;
            new_tag_info.DSFID = chip.DSFID;
            new_tag_info.EAS = chip.EAS;

            return new_tag_info;
        }

        // 设置三个系统值。此函数不会改变 this.Changed
        public void SetSystemValues(
            byte[] bytes,
            string lock_status,
            int max_block_count,
            int block_size,
            string uid,
            byte dsfid,
            byte afi,
            bool eas,
            uint antenna_id)
        {
            this.OriginBytes = bytes;
            this.OriginLockStatus = lock_status;
            this.MaxBlockCount = max_block_count;
            this.BlockSize = block_size;
            this._uid = uid;
            this._dsfid = dsfid;
            this._afi = afi;
            this._eas = eas;
            this._antenna = antenna_id;
        }

        // 得到用16进制字符串表示的 bytes 内容
        public static string GetBytesString(byte[] bytes,
            int block_size,
            string lock_status)
        {
            if (bytes == null)
                return "";

            StringBuilder text = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                string strHex = Convert.ToString(bytes[i], 16).ToUpper();
                text.Append(strHex.PadLeft(2, '0'));
                if ((i % block_size) < block_size - 1)
                    text.Append(" ");
                if ((i % block_size) == block_size - 1)
                {
                    int line = i / 4;
                    text.Append(" " + GetLockStatus(lock_status, line));
                    text.Append("\r\n");
                }
            }

            return text.ToString();
        }

        static string GetLockStatus(string map, int index)
        {
            char ch = LogicChip.GetBlockStatus(map, index);
            if (ch == 'l')
                return "locked";
            if (ch == 'w')
                return "will lock";
            return "";
        }

        public string GetDescription()
        {
            if (this.Protocol == InventoryInfo.ISO15693)
                return GetHfDescription();
            return GetUhfDescription();
        }

        // 2023/10/26
        // UHF 标签的 Description
        public string GetUhfDescription()
        {
            StringBuilder text = new StringBuilder();
            text.Append($"UID:\t{UID}\r\n");
            text.Append($"AFI:\t{Element.GetHexString(AFI)}\r\n");
            text.Append($"DSFID:\t{Element.GetHexString(DSFID)}\r\n");
            text.Append($"EAS:\t{EAS}\r\n");
            //text.Append($"MaxBlockCount:\t{MaxBlockCount}\r\n");
            //text.Append($"BlockSize:\t{BlockSize}\r\n");

            if (this.OriginBytes != null)
            {
                text.Append($"\r\nUser Bank 字节内容:\r\n{ByteArray.GetHexTimeStampString(this.OriginBytes)}\r\n");
            }
            else
                text.Append($"\r\nUser Bank 字节内容:\r\n(null)\r\n");

            // text.Append($"\r\n锁定位置:\r\n{this.OriginLockStatus}\r\n\r\n");

            /*
            if (this.OriginBytes != null)
            {
                text.Append($"初始 User Bank 元素:(共 {temp_chip.Elements.Count} 个)\r\n");
                int i = 0;
                foreach (Element element in temp_chip.Elements)
                {
                    text.Append($"{++i}) {element.ToString()}\r\n");
                }
            }
            */

            {
                text.Append($"当前 Chip 元素:(共 {this.Elements.Count} 个)\r\n");
                int i = 0;
                foreach (Element element in this.Elements)
                {
                    text.Append($"{++i}) {element.ToString()}\r\n");
                }
            }

            //
            try
            {
                bool build_user_bank = true;    // TODO
                var result = GaoxiaoUtility.BuildTag(this, build_user_bank, this.EAS);
                if (result.Value == -1)
                    throw new Exception(result.ErrorInfo);

                string epc = UhfUtility.EpcBankHex(result.EpcBank);    //  this.UID.Substring(0, 4) + Element.GetHexString(result.EpcBank);
                text.Append($"\r\n尝试按照 Chip 构造 EPC Bank 字节内容(EAS 为 {this.EAS}):\r\n{epc}\r\n");

                var bytes = result.UserBank;
                text.Append($"\r\n尝试按照 Chip 构造 User Bank 字节内容:\r\n{ByteArray.GetHexTimeStampString(bytes)}\r\n");
            }
            catch (Exception ex)
            {
                text.Append($"\r\n当前字节内容:\r\n构造 Bytes 过程出现异常: {ex.Message}\r\n");
            }

            return text.ToString();
        }

        public string GetHfDescription()
        {
            StringBuilder text = new StringBuilder();
            text.Append($"UID:\t{UID}\r\n");
            text.Append($"AFI:\t{Element.GetHexString(AFI)}\r\n");
            text.Append($"DSFID:\t{Element.GetHexString(DSFID)}\r\n");
            text.Append($"EAS:\t{EAS}\r\n");
            text.Append($"MaxBlockCount:\t{MaxBlockCount}\r\n");
            text.Append($"BlockSize:\t{BlockSize}\r\n");

            if (this.OriginBytes != null)
            {
                text.Append($"\r\n初始字节内容:\r\n{GetBytesString(this.OriginBytes, this.BlockSize, this.OriginLockStatus)}\r\n");
            }

            // 2020/6/22
            text.Append($"\r\n锁定位置:\r\n{this.OriginLockStatus}\r\n\r\n");

            {
                LogicChip chip = LogicChip.From(this.OriginBytes, this.BlockSize, this.OriginLockStatus);
                text.Append($"初始元素:(共 {chip.Elements.Count} 个)\r\n");
                int i = 0;
                foreach (Element element in chip.Elements)
                {
                    text.Append($"{++i}) {element.ToString()}\r\n");
                }
            }

            try
            {
                // 注意 GetBytes() 调用后，元素排列顺序会发生变化
                byte[] bytes = this.GetBytes(
                    this.MaxBlockCount * this.BlockSize,
                    this.BlockSize,
                    GetBytesStyle.None,
                    out string block_map);
                text.Append($"\r\n当前字节内容:\r\n{GetBytesString(bytes, this.BlockSize, block_map)}\r\n");
            }
            catch (Exception ex)
            {
                text.Append($"\r\n当前字节内容:\r\n构造 Bytes 过程出现异常: {ex.Message}\r\n");
            }

            {
                text.Append($"当前元素:(共 {this.Elements.Count} 个)\r\n");
                int i = 0;
                foreach (Element element in this.Elements)
                {
                    text.Append($"{++i}) {element.ToString()}\r\n");
                }
            }

            return text.ToString();
        }

        // 是否为空白内容？
        public override bool IsBlank()
        {
            if (base.IsBlank())
                return true;
            if (this.AFI == 0 && this.DSFID == 0 && this.EAS == false)
                return true;
            return false;
        }
    }

}
