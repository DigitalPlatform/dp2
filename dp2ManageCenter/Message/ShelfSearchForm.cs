using DigitalPlatform.CirculationClient;
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

namespace dp2ManageCenter.Message
{
    public partial class ShelfSearchForm : Form
    {
        const int COLUMN_ID = 0;
        const int COLUMN_PII = 1;
        const int COLUMN_ACTION = 2;
        const int COLUMN_OPERTIME = 3;
        const int COLUMN_STATE = 4;
        const int COLUMN_ERRORCODE = 5;
        const int COLUMN_ERRORINFO = 6;

        public ShelfSearchForm()
        {
            InitializeComponent();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    new ComboBoxText(this.comboBox_query_myAccount),
                    new ComboBoxText(this.comboBox_query_shelfAccount),
                    this.textBox_query_word,
                    new ComboBoxText(this.comboBox_query_from),
                    new ComboBoxText(this.comboBox_query_matchStyle),
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    new ComboBoxText(this.comboBox_query_myAccount),
                    new ComboBoxText(this.comboBox_query_shelfAccount),
                    this.textBox_query_word,
                    new ComboBoxText(this.comboBox_query_from),
                    new ComboBoxText(this.comboBox_query_matchStyle),
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private void ShelfSearchForm_Load(object sender, EventArgs e)
        {
            FillMyAccountList();

            this.UiState = ClientInfo.Config.Get("shelfSearchForm", "ui_state", "");
        }

        private void ShelfSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClientInfo.Config?.Set("shelfSearchForm", "ui_state", this.UiState);

        }

        private void ShelfSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void comboBox_query_myAccount_DropDown(object sender, EventArgs e)
        {
            FillMyAccountList();
        }

        void FillMyAccountList()
        {
            if (this.comboBox_query_myAccount.Items.Count == 0)
            {
                var accounts = MessageAccountForm.GetAccounts();
                foreach (var account in accounts)
                {
                    this.comboBox_query_myAccount.Items.Add(account.UserName + "@" + account.ServerUrl);
                }
            }
        }
    }
}
