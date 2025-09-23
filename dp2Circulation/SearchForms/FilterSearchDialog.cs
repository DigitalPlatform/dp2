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

namespace dp2Circulation.SearchForms
{
    public partial class FilterSearchDialog : Form
    {
        public FilterSearchDialog()
        {
            InitializeComponent();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkedComboBox_biblioDbNames);
                //controls.Add(this.textBox_from);
                //controls.Add(this.comboBox_matchStyle);
                controls.Add(this.comboBox_location);
                controls.Add(this.checkBox_detect);
                controls.Add(this.checkBox_dontUseBatch);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkedComboBox_biblioDbNames);
                //controls.Add(this.textBox_from);
                //controls.Add(this.comboBox_matchStyle);
                controls.Add(this.comboBox_location);
                controls.Add(this.checkBox_detect);
                controls.Add(this.checkBox_dontUseBatch);
                GuiState.SetUiState(controls, value);
            }
        }

        public string QueryWord
        {
            get
            {
                return this.textBox_queryWord.Text;
            }
            set
            {
                this.textBox_queryWord.Text = value;
            }
        }

        public string BiblioDbNames
        {
            get
            {
                return this.checkedComboBox_biblioDbNames.Text;
            }
            set
            {
                this.checkedComboBox_biblioDbNames.Text = value;
            }
        }

        public string From
        {
            get
            {
                return this.textBox_from.Text;
            }
            set
            {
                this.textBox_from.Text = value;
            }
        }

        public string Combinations
        {
            get
            {
                return this.textBox_combinations.Text;
            }
            set
            {
                this.textBox_combinations.Text = value;
            }
        }

        public string MatchStyle
        {
            get
            {
                return this.comboBox_matchStyle.Text;
            }
            set
            {
                this.comboBox_matchStyle.Text = value;
            }
        }

        public string LocationFilter
        {
            get
            {
                return this.comboBox_location.Text;
            }
            set
            {
                this.comboBox_location.Text = value;
            }
        }

        public bool UseDetect
        {
            get
            {
                return this.checkBox_detect.Checked;
            }
            set
            {
                this.checkBox_detect.Checked = value;
            }
        }

        public bool DontUseBatch
        {
            get
            {
                return this.checkBox_dontUseBatch.Checked;
            }
            set
            {
                this.checkBox_dontUseBatch.Checked = value;
            }
        }

        private async void FilterSearchDialog_Load(object sender, EventArgs e)
        {
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.162") < 0)
                this.checkBox_dontUseBatch.Visible = false;

            this.numericUpDown_maxBiblioResultCount.Value = BiblioSearchForm.MultilineMaxSearchResultCount;
            /*
            if (this._dbType == "biblio")
                Program.MainForm.FillBiblioFromList(this.comboBox_from);
            else if (this._dbType == "authority")
                BiblioSearchForm.FillAuthorityFromList(this.comboBox_from);
            */
            await BiblioSearchForm.FillLocationFilterListAsync(this.comboBox_location);
        }

        private void FilterSearchDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            BiblioSearchForm.MultilineMaxSearchResultCount = (int)this.numericUpDown_maxBiblioResultCount.Value;
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

        string _dbType = "biblio";

        public string DbType
        {
            get
            {
                return this._dbType;
            }
            set
            {
                this.checkedComboBox_biblioDbNames.Items.Clear();
                this._dbType = value;
            }
        }

        private void checkedComboBox_biblioDbNames_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_biblioDbNames.Items.Count > 0)
                return;

            if (this._dbType == "biblio")
            {
                if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.6") >= 0)
                    this.checkedComboBox_biblioDbNames.Items.Add("<全部书目>");
                else
                    this.checkedComboBox_biblioDbNames.Items.Add("<全部>");

                if (Program.MainForm.BiblioDbProperties != null)
                {
                    foreach (BiblioDbProperty property in Program.MainForm.BiblioDbProperties)
                    {
                        this.checkedComboBox_biblioDbNames.Items.Add(property.DbName);
                    }
                }
            }

            if (this._dbType == "authority")
            {
                this.checkedComboBox_biblioDbNames.Items.Add("<全部规范>");
                if (Program.MainForm.AuthorityDbProperties != null)
                {
                    foreach (BiblioDbProperty property in Program.MainForm.AuthorityDbProperties)
                    {
                        this.checkedComboBox_biblioDbNames.Items.Add(property.DbName);
                    }
                }
            }

        }
    }
}
