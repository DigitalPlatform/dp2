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
            double ratio = 1;
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
        if (min != -1)
            progressBar1.Minimum = (Int32)min;
        if (max != -1)
        {
            if (max <= Int32.MaxValue)
            {
                ratio = 1;
                progressBar1.Maximum = (Int32)max;
            }
            else
            {
                ratio = Int32.MaxValue / max;
                progressBar1.Maximum = Int32.MaxValue;
            }
        }
        if (value != -1)
            progressBar1.Value = (int)((double)value * ratio);
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
