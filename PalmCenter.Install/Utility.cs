using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalmCenter.Install
{
    public static class Utility
    {
        // 获得一个 Service 产品的用户目录
        public static string GetServiceUserDirectory(
            string strProduct,
            string strCompany = "dp2")
        {
            /*
            string strProgramDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrEmpty(strProgramDir) == true)
                strProgramDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            Debug.Assert(string.IsNullOrEmpty(strProgramDir) == false, "");

            return Path.Combine(strProgramDir, strCompany + "\\" + strProduct);
            */
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), $"{strCompany}\\{strProduct}");
        }

        static string EncryptKey = "palmcenter_password_key";

        public static string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }

        public static string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }
    }
}
