using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID.UI
{
    [DefaultProperty("UserName")]
    public class ChipItem
    {
        Hashtable _lockTable = new Hashtable(); // 被锁定元素的名字集合。元素名字 --> true

        public void SetLocked(string fieldName, bool locked)
        {
            _lockTable[fieldName] = locked;
        }

        string _primaryItemIdentifier = "";

        [DisplayName("馆藏单件主标识符"), Description("用于唯一标识一个册。常用册条码号来充当")]
        [Category(" 馆藏单件主标识符")]
        public string PrimaryItemIdentifier
        {

            get { return _primaryItemIdentifier; }

            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("馆藏单件主标识符 不应为空");

                // 检查用户名合法性
                // return:
                //      -1  校验过程出错
                //      0   校验发现不正确
                //      1   校验正确
                if (VerifyPII(value,
                    out string strError) != 1)
                    throw new ArgumentException("馆藏单件主标识符 '" + value + "' 不合法：" + strError);

                _primaryItemIdentifier = value;
                OnPropertyChanged("PrimaryItemIdentifier");
            }
        }

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
                Compact.CheckIsil(text);
            }
            catch (Exception ex)
            {
                strError = $"馆藏单件标识符 '{text}' 不合法: {ex.Message}";
                return 0;
            }

            return 1;
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
