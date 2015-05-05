using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryServer;

namespace dp2Circulation
{
    internal partial class ArrangementLocationDialog : Form
    {
        public string LibraryCodeList = ""; // 当前用户管辖的馆代码

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        public List<string> ExcludingLocationNames = new List<string>();   // 要排除的、已经被使用了的种次号库名

        public ArrangementLocationDialog()
        {
            InitializeComponent();
        }

        private void ArrangementLocationDialog_Load(object sender, EventArgs e)
        {

        }

        private void ArrangementLocationDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ArrangementLocationDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        static bool MatchLocationNames(List<string> names, string name)
        {
            bool bNamePattern = false;  // name 里面是否包含通配符?
            if (name.IndexOf("*") != -1)
                bNamePattern = true;

            foreach (string current in names)
            {
                bool bCurrentPattern = false;  // current 里面是否包含通配符?
                if (current.IndexOf("*") != -1)
                    bCurrentPattern = true;

                if (bNamePattern == true)
                {
                    if (LibraryServerUtil.MatchLocationName(current, name) == true)
                        return true;

                    if (bCurrentPattern == true)
                    {
                        if (LibraryServerUtil.MatchLocationName(name, current) == true)
                            return true;
                    }
                }
                else
                {
                    if (bCurrentPattern == false)
                    {
                        if (current == name)
                            return true;
                    }
                    else
                    {
                        Debug.Assert(bCurrentPattern == true, "");
                        if (LibraryServerUtil.MatchLocationName(name, current) == true)
                            return true;
                    }
                }
            }

            return false;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.comboBox_location.Text == "")
            {
                strError = "尚未指定馆藏地点";
                goto ERROR1;
            }

            // 检查对话框中输入的馆藏地点，是不是被别处用过的？
            if (String.IsNullOrEmpty(this.comboBox_location.Text) == false
                && this.ExcludingLocationNames != null)
            {
                string strLocation = this.LocationString;   // 这是正规化了的值，将"<空>"等转换成了实际的值

#if NO
                if (this.ExcludingLocationNames.IndexOf(strLocation) != -1)
                {
                    strError = "您所指定的馆藏地点 '" + this.comboBox_location.Text + "' 已经被使用过了";
                    goto ERROR1;
                }
#endif
                if (MatchLocationNames(this.ExcludingLocationNames, strLocation) == true)
                {
                    strError = "您所指定的馆藏地点 '" + this.comboBox_location.Text + "' 已经被使用过了";
                    goto ERROR1;
                }
            }


            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string LocationString
        {
            get
            {
                /*
                if (this.comboBox_location.Text == "<空>"
                    || this.comboBox_location.Text == "<blank>")
                    return "";

                return this.comboBox_location.Text;
                 * */
                string strText = this.comboBox_location.Text;
                return strText.Replace("<空>", "").Replace("<blank>", "");
            }
            set
            {
                /*
                if (String.IsNullOrEmpty(value) == true)
                    this.comboBox_location.Text = "<空>";
                else
                    this.comboBox_location.Text = value;
                 * */

                this.comboBox_location.Text = GetDisplayString(value);
            }
        }

        public static string GetDisplayString(string value)
        {
            if (String.IsNullOrEmpty(value) == true)
            {
                return "<空>";
            }

            string strLibraryCode = "";
            string strPureName = "";

            LocationCollection.ParseLocationName(value,
                out strLibraryCode,
                out strPureName);
            if (String.IsNullOrEmpty(strPureName) == true)
                return strLibraryCode + "/<空>";
            else
                return value;
        }

        int m_nInDropDown = 0;

        private void comboBox_location_DropDown(object sender, EventArgs e)
        {
            // 防止重入 2009/2/23 new add
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                ComboBox combobox = (ComboBox)sender;
                int nCount = combobox.Items.Count;

                if (combobox.Items.Count == 0
                    && this.GetValueTable != null)
                {
                    // combobox.Items.Add("<空>");

                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = "";

                    if (combobox == this.comboBox_location)
                        e1.TableName = "location";
                    else
                    {
                        Debug.Assert(false, "不支持的sender");
                        return;
                    }

                    this.GetValueTable(this, e1);

                    if (e1.values != null)
                    {
                        List<string> values = new List<string>();
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            // 排除部分已经用过的值
                            if (this.ExcludingLocationNames != null)
                            {
                                if (this.ExcludingLocationNames.IndexOf(e1.values[i]) != -1)
                                    continue;
                            }

                            values.Add(e1.values[i]);
                        }

                        List<string> results = null;

                        if (String.IsNullOrEmpty(this.LibraryCodeList) == false)
                        {
                            // 过滤出符合馆代码的那些值字符串
                            results = Global.FilterLocationsWithLibraryCodeList(this.LibraryCodeList,
                                values);
                        }
                        else
                        {
                            results = values;
                        }

                        foreach (string s in results)
                        {
                            combobox.Items.Add(GetDisplayString(s));
                        }
                    }

                    else
                    {
                        combobox.Items.Add("<not found>");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }
    }
}