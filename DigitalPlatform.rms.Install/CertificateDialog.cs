using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;

namespace DigitalPlatform.rms
{
    public partial class CertificateDialog : Form
    {
        public string SN = "";

        public CertificateDialog()
        {
            InitializeComponent();
        }

        private void button_viewCurrentCert_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.SN) == true)
            {
                MessageBox.Show(this, "当前尚未选择任何证书");
            }
            else
            {
#if NO
                X509Certificate2 cert = FindCertificate(
      StoreLocation.LocalMachine,
      StoreName.Root,
      X509FindType.FindBySerialNumber,
      this.SN);
                if (cert != null)
                    goto DO_VIEW;

                cert = FindCertificate(
      StoreLocation.CurrentUser,
      StoreName.Root,
      X509FindType.FindBySerialNumber,
      this.SN);
                if (cert == null)
                {
                    MessageBox.Show(this, "序列号为 '"+this.SN+"' 的证书在 StoreLocation.LocalMachine | StoreLocation.CurrentUser / StoreName.Root 中不存在。请重新设置");
                    return;
                }
            DO_VIEW:
                X509Certificate2UI.DisplayCertificate(cert);
#endif
                X509Certificate2 cert = FindCertificate(
StoreLocation.LocalMachine,
StoreName.Root,
X509FindType.FindBySerialNumber,
this.SN);
                if (cert == null)
                {
                    MessageBox.Show(this, "序列号为 '" + this.SN + "' 的证书在 StoreLocation.LocalMachine / StoreName.Root 中不存在。请重新设置");
                    return;
                }
                X509Certificate2UI.DisplayCertificate(cert);
            }
        }

        private void button_selectCert_Click(object sender, EventArgs e)
        {
            X509Certificate2 cert = PickCertificate();
            if (cert == null)
            {
                return;
            }
            this.SN = cert.SerialNumber;
            EnableControls();
        }

        private void button_clearSelection_Click(object sender, EventArgs e)
        {
            this.SN = "";
            EnableControls();
        }

        void EnableControls()
        {
            if (String.IsNullOrEmpty(this.SN) == true)
            {
                this.button_clearSelection.Enabled = false;
                this.button_selectCert.Enabled = true;
                this.button_viewCurrentCert.Enabled = false;
            }
            else
            {
                this.button_clearSelection.Enabled = true;
                this.button_selectCert.Enabled = true;
                this.button_viewCurrentCert.Enabled = true;
            }
        }

        private static X509Certificate2 PickCertificate()
        {
            X509Certificate2Collection collection = new X509Certificate2Collection();
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            collection.AddRange(store.Certificates);
#if NO
            store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            collection.AddRange(store.Certificates);
#endif
            try
            {

                // pick a certificate from the store

                X509Certificate2Collection selected =
                    X509Certificate2UI.SelectFromCollection(
                         collection,
                         "受信任的根证书",
                         "请选择一个证书：",
                         X509SelectionFlag.SingleSelection);

                if (selected.Count == 0)
                    return null;

                return selected[0];

                /*
                X509Certificate2 cert =
                // show certificate details dialog
                X509Certificate2UI.DisplayCertificate(cert);
                 * */
            }
            finally
            {
                store.Close();
            }
        }

        static X509Certificate2 FindCertificate(
    StoreLocation location, StoreName name,
    X509FindType findType, string findValue)
        {
            X509Store store = new X509Store(name, location);
            try
            {
                // create and open store for read-only access
                store.Open(OpenFlags.ReadOnly);

                // search store
                X509Certificate2Collection col = store.Certificates.Find(
                  findType, findValue, true);

                if (col.Count == 0)
                    return null;

                // return first certificate found
                return col[0];
            }
            // always close the store
            finally { store.Close(); }
        }

        private void CertificateDialog_Load(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
