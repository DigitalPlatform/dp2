using GreenInstall;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace greenSetup
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button_test_Click(object sender, EventArgs e)
        {
            /*
            var result = Class1.Test();
            MessageBox.Show(this, result);
            */
            _ = install();
        }

        async Task install()
        {
            var result = await GreenInstaller.InstallFromWeb("http://dp2003.com/dp2ssl/v1_dev",
"c:\\dp2ssl",
null,
// "dp2ssl.exe",
false,
(double min, double max, double value, string text) =>
{
    this.Invoke(new Action(() =>
    {
        if (text != null)
            label_message.Text = text;
    }));
});
            if (result.Value == -1)
            {
                MessageBox.Show(this, result.ErrorInfo);
                return;
            }
            // 迁移用户文件夹
            string sourceDirectory = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
"dp2ssl");
            if (Directory.Exists(sourceDirectory))
            {
                string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "dp2\\dp2ssl");
                var move_result = GreenInstaller.MoveUserDirectory(sourceDirectory,
                    targetDirectory,
                    "maskSource");
            }
            return;
        }
    }
}
