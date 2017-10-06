using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.LibraryServer
{
    public partial class SelectRestoreModeDialog : Form
    {
        public string Action { get; set; }  // 空/full/blank

        public SelectRestoreModeDialog()
        {
            InitializeComponent();
        }

        private void button_fullRestore_Click(object sender, EventArgs e)
        {
            this.Action = "full";
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_blank_Click(object sender, EventArgs e)
        {
            this.Action = "blank";
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
