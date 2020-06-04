using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Install
{
    // http://www.cnblogs.com/wf5360308/articles/1426158.html
    // 尚未测试验证过
    public class ClickOnceCleaner
    {

        public static void ClearFiles()
        {
            string fl = string.Format("C:\"Documents and Settings\"{0}\"Local Settings\"Apps\"2.0", Environment.UserName);
            if (Directory.Exists(fl))
                Directory.Delete(fl, true);
        }

        public static void ClearReg(string appName)
        {
            RegistryKey depKey = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes").
            OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").
            OpenSubKey("CurrentVersion").OpenSubKey("Deployment").OpenSubKey("SideBySide").
            OpenSubKey("2.0");
            DeleteKey(depKey, appName);
        }

        private static void DeleteKey(RegistryKey key, string appName)
        {
            string[] subNames = key.GetSubKeyNames();
            if (subNames == null || subNames.Length == 0)
                return;
            foreach (string name in subNames)
            {
                RegistryKey childKey = key.OpenSubKey(name, RegistryKeyPermissionCheck.ReadWriteSubTree);
                string[] childKeyNames = childKey.GetSubKeyNames();
                if (childKeyNames == null || childKeyNames.Length == 0)
                    continue;
                foreach (string childKeyName in childKeyNames)
                {
                    if (childKeyName.Contains(appName))
                        childKey.DeleteSubKeyTree(childKeyName);
                    else
                        DeleteKey(childKey, appName);
                }
            }
        }
    }
}
