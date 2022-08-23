using DigitalPlatform.CommonControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    }
}
