using DigitalPlatform.GUI;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    public partial class BiblioDupDialog : Form
    {
        public string OriginXml { get; set; }

        public string DupBiblioRecPathList { get; set; }

        public BiblioDupDialog()
        {
            InitializeComponent();
        }

        private void BiblioDupDialog_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(FillBrowseList));
        }

        void FillBrowseList()
        {
            this.listView_browse.Items.Clear();

            List<string> recpaths = StringUtil.SplitList(this.DupBiblioRecPathList);

            LibraryChannel channel = Program.MainForm.GetChannel();

            try
            {
                // 获得书目记录
                BrowseLoader loader = new BrowseLoader();
                loader.Channel = channel;
                // loader.Stop = this.Progress;
                loader.Format = "id,xml,cols";
                loader.RecPaths = recpaths;

                int i = 0;
                foreach (DigitalPlatform.LibraryClient.localhost.Record biblio_item in loader)
                {
                    ListViewItem item = null;

                    if (biblio_item.RecordBody != null
                        && biblio_item.RecordBody.Result != null
                        && biblio_item.RecordBody.Result.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCodeValue.NoError)
                    {
                        item = Global.AppendNewLine(
                            this.listView_browse,
                            biblio_item.Path,
                            new string [] {biblio_item.RecordBody.Result.ErrorString});
                    }
                    else
                    {
                        item = Global.AppendNewLine(
                            this.listView_browse,
                            biblio_item.Path,
                            biblio_item.Cols);
                        item.Tag = biblio_item.RecordBody.Xml;
                    }

                    i++;
                }
            }
            finally
            {
                Program.MainForm.ReturnChannel(channel);
            }
        }
    }
}
