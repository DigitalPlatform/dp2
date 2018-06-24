using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform
{
    public static class ProgramUtil
    {
        // return:
        //      true    函数返回后需要立即退出程序
        //      false   返回返回后继续后面处理
        public static bool TryUpgrade(string module_name,   // "内务"
            string new_version, // "V3"
            string shortcut_path,    // "DigitalPlatform/dp2 V3/dp2内务 V3"
            string app_url  // "http://dp2003.com/dp2circulation/v3/dp2circulation.application"
            )
        {
            // 2018/6/24
            // 观察 V3 版本是否已经安装。如果没有安装，并且当前操作系统条件具备，则提示升级到 V3
            if (ApplicationDeployment.IsNetworkDeployed == true
                // https://stackoverflow.com/questions/2819934/detect-windows-version-in-net
                && (Environment.OSVersion.Version.Major > 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1)))
            {
                string strShortcutFilePath = GetShortcutFilePath(shortcut_path);
                if (File.Exists(strShortcutFilePath) == false)
                {
                    // 提示可以安装 V3
                    DialogResult result = MessageBox.Show(
        module_name + "当前有 " + new_version + " 版本可以安装。强烈推荐升级到 " + new_version + " 版本。\r\n\r\n是否现在立即安装 " + module_name + " " + new_version + " 版本? ",
        module_name,
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        strShortcutFilePath = app_url;
                        try
                        {
                            Process.Start(strShortcutFilePath);
                            return true;
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    // 启动 V3
                    try
                    {
                        MessageBox.Show("启动已经安装在本机的" + module_name + " " + new_version + " 版本");
                        Process.Start(strShortcutFilePath);
                        return true;
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                // MessageBox.Show("Environment.OSVersion.Version.Major: " + Environment.OSVersion.Version.Major);
            }

            return false;
        }

        // get clickonce shortcut filename
        // parameters:
        //      strApplicationName  "DigitalPlatform/dp2 V2/dp2内务 V2"
        public static string GetShortcutFilePath(string strApplicationName)
        {
            // string publisherName = "Publisher Name";
            // string applicationName = "Application Name";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), strApplicationName) + ".appref-ms";
        }

    }
}
