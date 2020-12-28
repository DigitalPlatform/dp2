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
        // 获得 dpkernel 或 dp2library 的程序存储目录
        // 在 64 位操作系统下，获得 Program files (x86)
        // 在 32 位操作系统下，获得 Program Files
        // 目前 dp2kernel 和 dp2library 在 64 位操作系统下还都是 32 位的模块
        public static string GetProductDirectory(
            string strProduct,
            string strCompany = "digitalplatform")
        {
            string strProgramDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrEmpty(strProgramDir) == true)
                strProgramDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            Debug.Assert(string.IsNullOrEmpty(strProgramDir) == false, "");

            return Path.Combine(strProgramDir, strCompany + "\\" + strProduct);
        }

        static string EncryptKey = "palmcenter_password_key";

        internal static string DecryptPasssword(string strEncryptedText)
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

        internal static string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }
    }
}
