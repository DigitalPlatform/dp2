
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DocumentFormat.OpenXml.Drawing;

namespace dp2Circulation
{
    public partial class OutputDocxCatalogDialog : Form
    {
        public OutputDocxCatalogDialog()
        {
            InitializeComponent();
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

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.numericUpDown_biblioNoStart);
                controls.Add(this.numericUpDown_pageNumberStart);
                controls.Add(this.textBox_noFontName);
                controls.Add(this.textBox_noFontSize);
                controls.Add(this.textBox_barcodeFontName);
                controls.Add(this.textBox_barcodeFontSize);
                controls.Add(this.textBox_contentFontName);
                controls.Add(this.textBox_contentFontSize);
                controls.Add(this.textBox_accessNoFontName);
                controls.Add(this.textBox_accessNoFontSize);

                controls.Add(new ControlWrapper(this.checkBox_boldTitleArea, true));

                controls.Add(new ControlWrapper(this.checkBox_title_area, true));
                controls.Add(new ControlWrapper(this.checkBox_edition_area, true));
                controls.Add(new ControlWrapper(this.checkBox_material_specific_area, true));
                controls.Add(new ControlWrapper(this.checkBox_publication_area, true));
                controls.Add(new ControlWrapper(this.checkBox_material_description_area, true));
                controls.Add(new ControlWrapper(this.checkBox_series_area, true));
                controls.Add(new ControlWrapper(this.checkBox_notes_area, true));
                controls.Add(new ControlWrapper(this.checkBox_resource_identifier_area, true));
                controls.Add(new ControlWrapper(this.checkBox_summary_field, false));

                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();

                controls.Add(this.numericUpDown_biblioNoStart);
                controls.Add(this.numericUpDown_pageNumberStart);
                controls.Add(this.textBox_noFontName);
                controls.Add(this.textBox_noFontSize);
                controls.Add(this.textBox_barcodeFontName);
                controls.Add(this.textBox_barcodeFontSize);
                controls.Add(this.textBox_contentFontName);
                controls.Add(this.textBox_contentFontSize);
                controls.Add(this.textBox_accessNoFontName);
                controls.Add(this.textBox_accessNoFontSize);

                controls.Add(new ControlWrapper(this.checkBox_boldTitleArea, true));

                controls.Add(new ControlWrapper(this.checkBox_title_area, true));
                controls.Add(new ControlWrapper(this.checkBox_edition_area, true));
                controls.Add(new ControlWrapper(this.checkBox_material_specific_area, true));
                controls.Add(new ControlWrapper(this.checkBox_publication_area, true));
                controls.Add(new ControlWrapper(this.checkBox_material_description_area, true));
                controls.Add(new ControlWrapper(this.checkBox_series_area, true));
                controls.Add(new ControlWrapper(this.checkBox_notes_area, true));
                controls.Add(new ControlWrapper(this.checkBox_resource_identifier_area, true));
                controls.Add(new ControlWrapper(this.checkBox_summary_field, false));

                GuiState.SetUiState(controls, value);
            }
        }

        public int BiblioNoStart
        {
            get
            {
                return (int)this.numericUpDown_biblioNoStart.Value;
            }
            set
            {
                this.numericUpDown_biblioNoStart.Value = value;
            }
        }

        public int PageNumberStart
        {
            get
            {
                return (int)this.numericUpDown_pageNumberStart.Value;
            }
            set
            {
                this.numericUpDown_pageNumberStart.Value = value;
            }
        }

        public string NoFontName
        {
            get
            {
                return this.textBox_noFontName.Text;
            }
            set
            {
                this.textBox_noFontName.Text = value;
            }
        }

        public string NoFontSize
        {
            get
            {
                return this.textBox_noFontSize.Text;
            }
            set
            {
                this.textBox_noFontSize.Text = value;
            }
        }

        //

        public string BarcodeFontName
        {
            get
            {
                return this.textBox_barcodeFontName.Text;
            }
            set
            {
                this.textBox_barcodeFontName.Text = value;
            }
        }

        public string BarcodeFontSize
        {
            get
            {
                return this.textBox_barcodeFontSize.Text;
            }
            set
            {
                this.textBox_barcodeFontSize.Text = value;
            }
        }

        //

        public string ContentFontName
        {
            get
            {
                return this.textBox_contentFontName.Text;
            }
            set
            {
                this.textBox_contentFontName.Text = value;
            }
        }

        public string ContentFontSize
        {
            get
            {
                return this.textBox_contentFontSize.Text;
            }
            set
            {
                this.textBox_contentFontSize.Text = value;
            }
        }

        //

        public string AccessNoFontName
        {
            get
            {
                return this.textBox_accessNoFontName.Text;
            }
            set
            {
                this.textBox_accessNoFontName.Text = value;
            }
        }

        public string AccessNoFontSize
        {
            get
            {
                return this.textBox_accessNoFontSize.Text;
            }
            set
            {
                this.textBox_accessNoFontSize.Text = value;
            }
        }

        public bool BoldTitleArea
        {
            get
            {
                return this.checkBox_boldTitleArea.Checked;
            }
            set
            {
                this.checkBox_boldTitleArea.Checked = value;
            }
        }

        // 注意，竖线间隔
        public string AreaList
        {
            get
            {
                return GetAreaList();
            }
            set
            {
                ClearAllAreaCheckboxs();

                var ons = StringUtil.SplitList(value, '|');
                foreach(var name in ons)
                {
                    CheckOnByName(name, true);
                }
            }
        }

        string GetAreaList()
        {
            List<string> results = new List<string>();
            foreach(Control control in this.tabPage_content.Controls)
            {
                if (!(control is CheckBox))
                    continue;
                CheckBox checkBox = (CheckBox)control;
                if (checkBox.Checked == false)
                    continue;
                var caption = checkBox.Text;
                if (caption.EndsWith("_area") == false && caption.EndsWith("_field") == false)
                    continue;
                var name = StringUtil.ParseTwoPart(caption, " ")[1];
                if (name.EndsWith("_field"))
                    name = name.Substring(0, name.Length - "_field".Length);
                results.Add(name);
            }

            return String.Join("|", results.ToArray());
        }

        void ClearAllAreaCheckboxs()
        {
            foreach (Control control in this.tabPage_content.Controls)
            {
                if (!(control is CheckBox))
                    continue;
                CheckBox checkBox = (CheckBox)control;
                var caption = checkBox.Text;
                if (caption.EndsWith("_area") || caption.EndsWith("_field"))
                    checkBox.Checked = false;
            }
        }

        void CheckOnByName(string name, bool on)
        {
            foreach (Control control in this.tabPage_content.Controls)
            {
                if (!(control is CheckBox))
                    continue;
                CheckBox checkBox = (CheckBox)control;
                var caption = checkBox.Text;
                if (caption.EndsWith(" " + name))
                {
                    checkBox.Checked = on;
                    return;
                }
            }

            throw new Exception($"没有找到名字和 '{name}' 有关的 checkbox");
        }

        private void button_getNoFont_Click(object sender, EventArgs e)
        {
            using (GetWordFontsDialog dlg = new GetWordFontsDialog())
            {
                dlg.FontName = this.textBox_noFontName.Text;
                if (dlg.ShowDialog(this) == DialogResult.Cancel)
                    return;
                this.textBox_noFontName.Text = dlg.FontName;
            }
        }

        private void button_getBarcodeFont_Click(object sender, EventArgs e)
        {
            using (GetWordFontsDialog dlg = new GetWordFontsDialog())
            {
                dlg.FontName = this.textBox_barcodeFontName.Text;
                if (dlg.ShowDialog(this) == DialogResult.Cancel)
                    return;
                this.textBox_barcodeFontName.Text = dlg.FontName;
            }
        }

        private void button_getContentFont_Click(object sender, EventArgs e)
        {
            using (GetWordFontsDialog dlg = new GetWordFontsDialog())
            {
                dlg.FontName = this.textBox_contentFontName.Text;
                if (dlg.ShowDialog(this) == DialogResult.Cancel)
                    return;
                this.textBox_contentFontName.Text = dlg.FontName;
            }
        }

        private void button_getAccessNoFont_Click(object sender, EventArgs e)
        {
            using (GetWordFontsDialog dlg = new GetWordFontsDialog())
            {
                dlg.FontName = this.textBox_accessNoFontName.Text;
                if (dlg.ShowDialog(this) == DialogResult.Cancel)
                    return;
                this.textBox_accessNoFontName.Text = dlg.FontName;
            }
        }
    }
}
