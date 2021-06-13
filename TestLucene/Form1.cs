using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestLucene
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var basePath = Environment.GetFolderPath(
    Environment.SpecialFolder.CommonApplicationData);
            var directory = Path.Combine(basePath, "_testlucene");
            DataModel.Initialize(directory);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            DataModel.End();
        }

        // 添加 Document
        private void MenuItem_test_addDocument_Click(object sender, EventArgs e)
        {
            DocumentDialog dlg = new DocumentDialog();
            dlg.ShowDialog(this);
            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            var doc = new Document
            {
                new StringField("id", dlg.ID, Field.Store.YES),
    // StringField indexes but doesn't tokenize
                new StringField("title",
        dlg.Title,
        Field.Store.YES),
                new StringField("title",
                dlg.Title2,
                Field.Store.YES),
    new TextField("author",
        dlg.Author,
        Field.Store.YES),
        new TextField("author",
        dlg.Author2,
        Field.Store.YES)
};
            DataModel.AddDocument(doc);
        }

        // 删除 Document
        private void MenuItem_test_deleteDocument_Click(object sender, EventArgs e)
        {
            DocumentDialog dlg = new DocumentDialog();
            dlg.ShowDialog(this);
            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            DataModel.DeleteDocument(dlg.ID);
        }

        private void MenuItem_test_search_Click(object sender, EventArgs e)
        {
            DocumentDialog dlg = new DocumentDialog();
            dlg.ShowDialog(this);
            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            StringBuilder text = new StringBuilder();
            var docs = DataModel.Search(dlg.Title,
                (hit, doc) => {
                    string line = $"doc={doc.ToString()}, Score={hit.Score}";
                    text.AppendLine(line);
                }
                );

            MessageBox.Show(this, text.ToString());
        }
    }
}
