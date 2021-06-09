using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using MySqlConnector;

namespace TestMySQL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button_testConnectionString_Click(object sender, EventArgs e)
        {
            try
            {
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
                {
                    Server = ".",
                    ConnectionProtocol = MySqlConnectionProtocol.Pipe,
                    PipeName = "MYSQL",
                    UserID = "root",
                    Password = "test",
                    SslMode = MySqlSslMode.None,
                    AllowPublicKeyRetrieval = true,
                };

                var s = builder.ToString();

                using (MySqlConnection connection = new MySqlConnection(s))
                {
                    connection.Open();
                }

            }
            catch (Exception ex)
            {
                // MessageBox.Show(this, s);
                string error = ExceptionUtil.GetDebugText(ex);
                MessageBox.Show(this, error);
            }
        }
    }
}
