using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2SIPServer
{
    public partial class MainForm : Form
    {
        private Thread acceptThread = null;

        Thread clientThread = null;

        private TcpListener Listener = null;

        Session _session = null;

        public MainForm()
        {
            InitializeComponent();
        }

        public int Port
        {
            get
            {
                return Properties.Settings.Default.Port;
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            DoListen();

            this.toolStripStatusLabel_port.Text = "监听端口：" + this.Port;

            this.button_start.Enabled = false;
        }

        public void WriteHtml(string strHtml)
        {
            strHtml = String.Format("{0}  {1}<br />", DateTime.Now.ToString("yyyyMMdd HH:mm:ss"), strHtml);
            WriteHtml(this.webBrowser1,
                strHtml);
        }


        // 不支持异步调用
        public static void WriteHtml(WebBrowser webBrowser,
            string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            // doc = doc.OpenNew(true);
            doc.Write("<pre>");
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DoStop();

            if (_session != null)
            {
                _session.CloseSocket();
            }
        }

        private void DoListen()
        {
            try
            {
                Listener = new TcpListener(IPAddress.Any, Port);
                Listener.Start();
                this.toolStripStatusLabel1.Text = "正在监听...";
                WriteHtml("启动成功");

                acceptThread = new Thread(new ThreadStart(DoAccept));
                acceptThread.Start();
            }
            catch (Exception ex)
            {
                this.toolStripStatusLabel1.Text = "侦听失败!";
                WriteHtml("启动失败" + ex.Message);
            }
        }

        private void DoStop()
        {
            try
            {
                if (Listener != null)
                    Listener.Stop();

                if (acceptThread != null)
                    acceptThread.Abort();

                if (clientThread != null)
                    clientThread.Abort();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.toolStripStatusLabel1.Text = "监听已停止";
            }
        }


        private void DoAccept()
        {
            try
            {
                while (true)
                {
                    TcpClient client = Listener.AcceptTcpClient();
                    _session = new Session(client);

                    clientThread = new Thread(new ThreadStart(_session.Processing));

                    // Start proccessing
                    clientThread.Start();
                }
            }
            catch (SocketException ex)
            {
                string strError = ex.Message;
            }
        }

        private void toolStripMenuItem_settings_Click(object sender, EventArgs e)
        {
            SettingsForm dlg = new SettingsForm();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                DoStop();
                DoListen();
                this.toolStripStatusLabel_port.Text = "监听端口：" + this.Port;
            }
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            DoListen();
            this.button_start.Enabled = false;
            this.button_stop.Enabled = true;
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            DoStop();
            this.button_start.Enabled = true;
            this.button_stop.Enabled = false;
        }
    }
}
