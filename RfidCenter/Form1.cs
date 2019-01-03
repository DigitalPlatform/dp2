using RfidDrivers.First;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RfidCenter
{
    public partial class Form1 : Form
    {
        Driver1 _driver = new Driver1();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Run(()=> {
                InitializeDriver();
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _driver.ReleaseDriver();
        }

        void InitializeDriver()
        {
            try
            {
                _driver.InitializeDriver();
            }
            catch(Exception ex)
            {
                ShowMessageBox(ex.Message);
            }

        }

        public void ShowMessageBox(string strText)
        {
            if (this.IsHandleCreated)
                this.Invoke((Action)(() =>
                {
                    try
                    {
                        MessageBox.Show(this, strText);
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }));
        }


        private void MenuItem_openReader_Click(object sender, EventArgs e)
        {
            _driver.OpenReader();
        }

        private void MenuItem_closeReader_Click(object sender, EventArgs e)
        {
            _driver.CloseReader();
        }

        private void MenuItem_inventory_Click(object sender, EventArgs e)
        {
            _driver.Inventory();
        }
    }
}
