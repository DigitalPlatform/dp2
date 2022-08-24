using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace dp2Circulation
{
    public partial class GetWordFontsDialog : Form
    {
        public GetWordFontsDialog()
        {
            InitializeComponent();
        }

        // https://social.msdn.microsoft.com/Forums/en-US/9beda373-b616-43e7-8eca-f2cc876385db/how-can-i-get-the-language-of-the-fontfamily?forum=csharpgeneral
        static List<string> GetAllFonts()
        {
            /*
             * using System.Windows.Media;
             * using FontFamily = System.Windows.Media.FontFamily;

            List<string> results = new List<string>();

            foreach (var font in Fonts.SystemFontFamilies)
            {
                var names = font.FamilyMaps;

                var first = font.FamilyNames.FirstOrDefault();
                results.Add(first.Key + " " + first.Value);
            }
            return results;
            */

            List<string> results = new List<string>();
            InstalledFontCollection fonts = new InstalledFontCollection();
            foreach (FontFamily font in fonts.Families)
            {
                results.Add(font.Name);
            }

            return results;
        }

        private void GetWordFontsDialog_Load(object sender, EventArgs e)
        {
            var results = GetAllFonts();
            this.comboBox_ascii.Items.AddRange(results.ToArray());
            this.comboBox_hAnsi.Items.AddRange(results.ToArray());
            this.comboBox_eastAsia.Items.AddRange(results.ToArray());
            this.comboBox_cs.Items.AddRange(results.ToArray());
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string FontName
        {
            get
            {
                List<string> results = new List<string>();
                var ascii = this.comboBox_ascii.Text;
                if (string.IsNullOrEmpty(ascii) == false)
                    results.Add($"ascii:{ascii}");
                var hAnsi = this.comboBox_hAnsi.Text;
                if (string.IsNullOrEmpty(hAnsi) == false)
                    results.Add($"hAnsi:{hAnsi}");
                var eastAsia = this.comboBox_eastAsia.Text;
                if (string.IsNullOrEmpty(eastAsia) == false)
                    results.Add($"eastAsia:{eastAsia}");
                var cs = this.comboBox_cs.Text;
                if (string.IsNullOrEmpty(cs) == false)
                    results.Add($"cs:{cs}");
                return StringUtil.MakePathList(results);
            }
            set
            {
                var ascii = StringUtil.GetParameterByPrefix(value, "ascii");
                this.comboBox_ascii.Text = ascii;

                var hAnsi = StringUtil.GetParameterByPrefix(value, "hAnsi");
                this.comboBox_hAnsi.Text = hAnsi;

                var eastAsia = StringUtil.GetParameterByPrefix(value, "eastAsia");
                this.comboBox_eastAsia.Text = eastAsia;

                var cs = StringUtil.GetParameterByPrefix(value, "cs");
                this.comboBox_cs.Text = cs;
            }
        }
    }
}
