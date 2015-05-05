using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Resources;
using System.Globalization;
using System.Runtime.Serialization;

using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Server
{
    public class BoxesInfo
    {
        public List<Box> Boxes = null;

        // 信箱类型名
        public const string INBOX = "收件箱";
        public const string TEMP = "草稿";
        public const string OUTBOX = "已发送";
        public const string RECYCLEBIN = "废件箱";

        public BoxesInfo()
        {
            InitialStandardBoxes();
        }

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Server.res.OpacApplication",
                typeof(BoxesInfo).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }

        // 初始化标准的几个信箱
        public void InitialStandardBoxes()
        {
            this.Boxes = new List<Box>();

            Box box = null;

            // 收件箱 inbox
            box = new Box();
            box.Name = this.GetString("收件箱");
            box.Type = INBOX;
            this.Boxes.Add(box);

            // 草稿 temp
            box = new Box();
            box.Name = this.GetString("草稿");
            box.Type = TEMP;
            this.Boxes.Add(box);

            // 已发送 outbox
            box = new Box();
            box.Name = this.GetString("已发送");
            box.Type = OUTBOX;
            this.Boxes.Add(box);

            // 废件箱 recyclebin
            box = new Box();
            box.Name = this.GetString("废件箱");
            box.Type = RECYCLEBIN;
            this.Boxes.Add(box);
        }

        // 将信箱名字转换为boxtype值
        // 2009/7/6 new add
        public string GetBoxType(string strName)
        {
            for (int i = 0; i < this.Boxes.Count; i++)
            {
                Box box = this.Boxes[i];

                if (strName == box.Name)
                    return box.Type;
            }

            return null;    // not found
        }

        public static bool IsInBox(string strBoxType)
        {
            if (strBoxType == INBOX/*"收件箱"*/)
                return true;
            return false;
        }

        public static bool IsTemp(string strBoxType)
        {
            if (strBoxType == TEMP/*"草稿"*/)
                return true;
            return false;
        }

        public static bool IsOutbox(string strBoxType)
        {
            if (strBoxType == OUTBOX/*"已发送"*/)
                return true;
            return false;
        }

        public static bool IsRecycleBin(string strBoxType)
        {
            if (strBoxType == RECYCLEBIN/*"废件箱"*/)
                return true;
            return false;
        }

        const string EncryptKey = "dp2circulationpassword";

        // 加密明文
        public static string EncryptPassword(string PlainText)
        {
            return Cryptography.Encrypt(PlainText, EncryptKey);
        }

        // 解密加密过的文字
        public static string DecryptPassword(string EncryptText)
        {
            return Cryptography.Decrypt(EncryptText, EncryptKey);
        }

        public static string BuildOneAddress(string strDisplayName, string strBarcode)
        {
            string strAddress = "";
            if (String.IsNullOrEmpty(strDisplayName) == false)
            {
                if (strDisplayName.IndexOf("[") == -1)
                    strAddress = "[" + strDisplayName + "]";
                else
                    strAddress = strDisplayName;

                string strEncryptBarcode = BoxesInfo.EncryptPassword(strBarcode);

                if (String.IsNullOrEmpty(strEncryptBarcode) == false)
                    strAddress += "=encrypt_barcode:" + strEncryptBarcode;
            }
            else
                strAddress = strBarcode;

            return strAddress;
        }

    }

    public class Box
    {
        public string Name = "";
        public string Type = "";    // 类型
    }
}
