using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using dp2Circulation.Script;

namespace dp2Circulation
{
    /// <summary>
    /// 指定如何在 MARC 记录中包含册记录信息的对话框
    /// </summary>
    public partial class ExportMarcHoldingDialog : Form
    {
        // 存放过滤脚本 .cs 文件的子目录
        public string FilterScriptDirectory { get; set; }


        public ExportMarcHoldingDialog()
        {
            InitializeComponent();
        }

        List<Control> _freeControls = new List<Control>();

        void DisposeFreeControls()
        {
            ControlExtention.DisposeFreeControls(_freeControls);
        }

        private void ExportMarcHoldingDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            // SaveScriptFile();
        }

        private void ExportMarcHoldingDialog_Load(object sender, EventArgs e)
        {
            //if (this.comboBox_biblio_filterScript.Items.Count == 0)
            //    FillScriptNames(false);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 如果 OK 时正好在第一个属性页，要提醒使用者注意第二个属性页的参数
            // if (this.tabControl1.SelectedTab == this.tabPage_ext)
            {
                if (string.IsNullOrEmpty(this.textBox_biblio_removeFieldNameList.Text) == false
                    || string.IsNullOrEmpty(this.textBox_biblio_filterScriptFileName.Text) == false)
                {
                    bool temp = false;
                    List<string> lines = new List<string>();
                    if (string.IsNullOrEmpty(this.textBox_biblio_removeFieldNameList.Text) == false)
                        lines.Add($"删除字段 '{this.textBox_biblio_removeFieldNameList.Text}'");
                    if (string.IsNullOrEmpty(this.textBox_biblio_filterScriptFileName.Text) == false)
                        lines.Add($"执行脚本 '{this.textBox_biblio_filterScriptFileName.Text}'");

                    var result = MessageDlg.Show(this,
                        $"确实要对导出到文件中的内容 {StringUtil.MakePathList(lines, " 并且 ")} ?",
                        "导出时特殊效果提醒",
                        MessageBoxButtons.OKCancel,
                        MessageBoxDefaultButton.Button2,
                        ref temp,
                        new string[] { "是的", "取消" },
                        null);
                    if (result == DialogResult.Cancel)
                    {
                        this.tabControl1.SelectedTab = this.tabPage_biblio;
                        return;
                    }
                }
            }


            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public bool Create905
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_905.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_905.Checked = value;
                });
            }
        }

        public bool RemoveOld905
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_removeOld905.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_removeOld905.Checked = value;
                });
            }
        }

        // 如何创建 905 字段?
        public string Style905
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.comboBox_905_style.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.comboBox_905_style.Text = value;
                });
            }
        }

        public bool Create906
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_906.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_906.Checked = value;
                });
            }
        }

        // 导出时希望滤除的字段名列表。逗号间隔
        public string RemoveFieldNames
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.textBox_biblio_removeFieldNameList.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.textBox_biblio_removeFieldNameList.Text = value;
                });
            }
        }

        public string UiState
        {
            get
            {
                /*
                // 防止 combobox 初始化状态失败
                if (this.comboBox_biblio_filterScript.Items.Count == 0)
                {
                    FillScriptNames();
                }
                */

                List<object> controls = new List<object>();
                controls.Add(this.checkBox_905);
                controls.Add(this.comboBox_905_style);
                controls.Add(this.checkBox_removeOld905);
                controls.Add(this.checkBox_906);
                controls.Add(this.textBox_biblio_removeFieldNameList);
                // controls.Add(this.comboBox_biblio_filterScript);
                controls.Add(this.textBox_biblio_filterScriptFileName);
                return GuiState.GetUiState(controls);
            }
            set
            {
                /*
                // 防止 combobox 初始化状态失败
                if (this.comboBox_biblio_filterScript.Items.Count == 0)
                {
                    FillScriptNames();
                }
                */

                List<object> controls = new List<object>();
                controls.Add(this.checkBox_905);
                controls.Add(this.comboBox_905_style);
                controls.Add(this.checkBox_removeOld905);
                controls.Add(this.checkBox_906);
                controls.Add(this.textBox_biblio_removeFieldNameList);
                // controls.Add(this.comboBox_biblio_filterScript);
                controls.Add(this.textBox_biblio_filterScriptFileName);
                GuiState.SetUiState(controls, value);
            }
        }

        private void checkBox_905_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_905.Checked)
            {
                this.comboBox_905_style.Visible = true;
                this.checkBox_removeOld905.Visible = true;
            }
            else
            {
                this.comboBox_905_style.Visible = false;
                this.checkBox_removeOld905.Visible = false;
            }
        }

        // public const string DO_NOT_USE_SCRIPT = "<不使用脚本>";

        public string ScriptFileName
        {
            get
            {
                string value = this.TryGet(() =>
                {
                    return this.textBox_biblio_filterScriptFileName.Text;
                });
                if (value == ScriptDialog.DO_NOT_USE_SCRIPT)
                    return "";

                return value;
            }
        }

        public bool TryGetScriptCode(out string code)
        {
            try
            {
                string temp = this.TryGet(() =>
                {
                    return this.ScriptCode;
                });
                code = temp;
                return true;

            }
            catch (FileNotFoundException)
            {
                code = "";
                return false;
            }
        }

        // 可以考虑用代码的 MD5 hash 字符串来当作它的名字，把 Assembly 存储到 Cache 中
        public string ScriptCode
        {
            get
            {
                /*
                return this.TryGet(() =>
                {
                    return this.textBox_biblio_filterScriptCode.Text;
                });
                */

                if (string.IsNullOrEmpty(ScriptFileName))
                    return "";

                var scriptFileName = Path.Combine(FilterScriptDirectory, ScriptFileName);

                if (File.Exists(scriptFileName))
                    return File.ReadAllText(scriptFileName);
                throw new FileNotFoundException($"脚本文件 '{scriptFileName}' 不存在 ...");
            }
        }

#if REMOVED

        void FillScriptNames(bool auto_select_first = false)
        {
            this.comboBox_biblio_filterScript.Items.Clear();
            if (string.IsNullOrEmpty(FilterScriptDirectory) == true)
                return;

            this.comboBox_biblio_filterScript.Items.Add(DO_NOT_USE_SCRIPT);

            DirectoryInfo di = new DirectoryInfo(FilterScriptDirectory);
            foreach (var fi in di.GetFiles("*.cs"))
            {
                this.comboBox_biblio_filterScript.Items.Add(fi.Name);
            }

            if (auto_select_first)
            {
                if (this.comboBox_biblio_filterScript.Items.Count == 0)
                {
                    this.textBox_biblio_filterScriptCode.Text = "";
                    _scriptFileName = null;
                    _codeChanged = false;
                }
                else
                {
                    if (this.comboBox_biblio_filterScript.SelectedIndex != 0)
                    {
                        _scriptFileName = null;
                        _codeChanged = false;
                        this.comboBox_biblio_filterScript.SelectedIndex = 0;
                    }
                }
            }
        }

        void LoadScriptFile()
        {
            var name = this.comboBox_biblio_filterScript.Text;
            if (string.IsNullOrEmpty(name) || name == DO_NOT_USE_SCRIPT)
            {
                this.textBox_biblio_filterScriptCode.Text = "";
                _scriptFileName = null;
                _codeChanged = false;
                return;
            }
            try
            {
                string fileName = Path.Combine(this.FilterScriptDirectory, this.comboBox_biblio_filterScript.Text);
                using (StreamReader sr = new StreamReader(fileName))
                {
                    this.textBox_biblio_filterScriptCode.Text = sr.ReadToEnd();
                }

                _scriptFileName = fileName;
                _codeChanged = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"LoadScriptFile() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        void SaveScriptFile()
        {
            if (_codeChanged == false)
                return;
            if (string.IsNullOrEmpty(_scriptFileName))
                return;

            try
            {
                string oldCode = null;
                if (File.Exists(_scriptFileName))
                    File.ReadAllText(_scriptFileName);
                string newCode = this.textBox_biblio_filterScriptCode.Text;
                if (oldCode != newCode)
                {
                    File.WriteAllText(_scriptFileName,
                        newCode,
                        Encoding.UTF8);

                    // 触发 AssemblyCache 刷新
                    var cacheKey = ExportMarcHoldingDialog.BuildCacheKey(Path.GetFileName(_scriptFileName));
                    Program.MainForm.AssemblyCache.Clear(cacheKey);
                }
                _codeChanged = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"SaveScriptFile() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        string _scriptFileName = null;  // 当前正在观察和编辑的文件名
        bool _codeChanged = false;

        private void comboBox_biblio_filterScript_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 先保存修改
            SaveScriptFile();

            // 然后装载新文件内容
            LoadScriptFile();
        }

        private void button_biblio_createScriptFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            string name = InputDlg.GetInput(this,
                "创建新的脚本文件",
                "脚本文件名",
                "",
                this.Font);
            if (name == null)
                return;
            if (string.IsNullOrEmpty(name))
            {
                strError = "文件名不允许为空";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(name))
            {
                strError = "尚未指定脚本文件名字";
                goto ERROR1;
            }

            if (name == DO_NOT_USE_SCRIPT)
            {
                strError = "文件名不合法";
                goto ERROR1;
            }

            if (name.Contains("/") || name.Contains("\\"))
            {
                strError = "脚本文件名字中不允许包含斜杠";
                goto ERROR1;
            }

            var ext = Path.GetExtension(name);
            if (string.IsNullOrEmpty(ext) == false
                && ext != ".cs")
            {
                strError = $"文件名的扩展名部分只允许 .cs";
                goto ERROR1;
            }

            if (name.Contains(".") == false)
                name += ".cs";

            // 名字查重
            if (this.comboBox_biblio_filterScript.Items.IndexOf(name) != -1)
            {
                strError = $"名字 '{name}' 在列表中已经存在了，无法重复创建";
                goto ERROR1;
            }

            var fileName = Path.Combine(this.FilterScriptDirectory, name);
            File.WriteAllText(fileName,
                @"// TODO: 以下为样本代码，请在此基础上修改为所需功能

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Text;
using System.Data;
using System.Linq;
using System.Globalization;

using dp2Circulation;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Core;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;

public class MyVerifyHost : VerifyHost
{
    // 进行过滤
    public override VerifyResult Verify(string action, string marc)
    {
        string old_marc = marc;
        var record = new MarcRecord(marc, ""4**"");

        // 删除 997 和 998 字段
        record.select(""field[@name='997' or @name='998']"").detach();

        return new VerifyResult
        {
            Value = 0,
            ChangedMarc = record.Text,
            // ChangedMarc = old_marc != record.Text ? record.Text : null,
            ErrorInfo = """",
        };
    }
}
",
                Encoding.UTF8);

            /*
            {
                this.comboBox_biblio_filterScript.Text = "";
                this.textBox_biblio_filterScriptCode.Text = "";
                _codeChanged = false;
                _scriptFileName = null;
            }
            */

            // 重新填充名字列表
            FillScriptNames();
            this.comboBox_biblio_filterScript.Text = name;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#endif

        // 准备默认的 MARC 输出过滤脚本目录
        public static string PrepareScriptDirectory()
        {
            var path = Path.Combine(Program.MainForm.UserDir, "export_marc_scripts");
            PathUtil.CreateDirIfNeed(path);
            return path;
        }

#if REMOVED
        private void textBox_biblio_filterScriptCode_TextChanged(object sender, EventArgs e)
        {
            _codeChanged = true;
        }
#endif

        public static string BuildCacheKey(string pure_fileName)
        {
            return "export:" + pure_fileName;
        }

#if REMOVED

        public delegate void delagate_initializeHost(VerifyHost host);

        // 导出 MARC 前过滤和处理 MARC 字段
        // return:
        //      -1  出错
        //      其它  使用 hostObj.VerifyResult.Value 返回值(-1 除外)
        //              此处做一个基本约定:
        //              -2:出错，并且希望把此前包含已经导出的所有记录的文件删除掉(注意这样做追加保存情形会有瑕疵)
        public static int FilterRecord(
            string cacheKey,
            string strCode,
            string strRef,
            MarcRecord record,
            delagate_initializeHost func_init,
            out VerifyHost hostObj,
            out string strError)
        {
            hostObj = null;
            int nRet = ExportMarcHoldingDialog.GetAssembly(
                cacheKey,
                strCode,
                strRef,
                out Assembly assembly,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;

            nRet = EntityForm.NewVerifyHostObject(assembly,
out hostObj,
out strError);
            if (nRet == -1)
                return -1;

            // 为Host派生类设置参数
            hostObj.DetailForm = null;
            hostObj.Assembly = assembly;

            func_init?.Invoke(hostObj);

            try
            {
                hostObj.VerifyResult = hostObj.Verify("", record.Text);
            }
            catch(Exception ex)
            {
                strError = $"在执行脚本的过程中出现异常: {ExceptionUtil.GetDebugText(ex)}";
                return -1;
            }

            if (hostObj.VerifyResult != null
                && string.IsNullOrEmpty(hostObj.VerifyResult.ChangedMarc) == false)
                record.Text = hostObj.VerifyResult.ChangedMarc;
            strError = hostObj.VerifyResult.ErrorInfo;
            // TODO: strError 中包含 hostObj.VerifyResult.Errors 里面的信息
            return hostObj.VerifyResult.Value;
        }


        // 根据源代码，获得 Assembly
        // return:
        //      -1  出错
        //      0   没有找到配置文件
        //      1   成功获得 Assembly
        static int GetAssembly(
            string cacheKey,
            string strCode,
            string strRef,
            out Assembly assembly,
            out string strError)
        {
            strError = "";
            assembly = null;
            //ext = "";

            string key = cacheKey;
            // string key = "hash:" + StringUtil.GetMd5(strCode + strRef);
            assembly = Program.MainForm.AssemblyCache.FindObject(key);
            if (assembly != null)
                return 1;

            int nRet = EntityForm.CompileVerifyCs(
strCode,
strRef,
out assembly,
out strError);
            if (nRet == -1)
            {
                strError = $"脚本文件 {key} 编译时出错: \r\n{strError}";
                return -1;
            }

            Program.MainForm.AssemblyCache.SetObject(key, assembly);
            return 1;
        }

        private void button_biblio_deleteScriptFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_scriptFileName) == false
                && File.Exists(_scriptFileName) == true)
            {
                File.Delete(_scriptFileName);
                this.comboBox_biblio_filterScript.Text = "";
                this.textBox_biblio_filterScriptCode.Text = "";
                FillScriptNames(true);
            }
            else
            {
                MessageBox.Show(this, "没有文件可供删除");
            }
        }
#endif


        public void AttachPanel(Panel panel, string page_title)
        {
            if (this.tabControl1.TabPages.IndexOf(this.tabPage_ext) == -1)
            {
                this.tabControl1.TabPages.Add(this.tabPage_ext);
                ControlExtention.RemoveFreeControl(_freeControls, this.tabPage_ext);
            }

            this.tabPage_ext.Text = page_title;
            this.tabPage_ext.Padding = new Padding(4, 4, 4, 4);
            this.tabPage_ext.Controls.Add(panel);
            panel.Dock = DockStyle.Fill;
        }

        public void HideExtPage()
        {
            if (this.tabControl1.TabPages.IndexOf(this.tabPage_ext) != -1)
            {
                this.tabControl1.TabPages.Remove(this.tabPage_ext);
                ControlExtention.AddFreeControl(_freeControls, this.tabPage_ext);
            }
        }

        private void button_biblio_findFilterScriptFileName_Click(object sender, EventArgs e)
        {
            using (ScriptDialog dlg = new ScriptDialog())
            {
                dlg.Font = this.Font;
                dlg.FilterScriptDirectory = this.FilterScriptDirectory;
                dlg.ScriptFileName = this.textBox_biblio_filterScriptFileName.Text;
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.LinkFormState(dlg, "holding_script_dialog");
                if (dlg.DialogResult == DialogResult.Cancel)
                    return;
                this.textBox_biblio_filterScriptFileName.Text = dlg.ScriptFileName;
            }
        }

#if REMOVED
        public static void FilterBiblioRecord(MarcRecord record,
            string removeFieldNames)
        {
            if (string.IsNullOrEmpty(removeFieldNames))
                return;

            var names = StringUtil.SplitList(removeFieldNames);
            foreach (string name in names)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException($"字段名列表 '{removeFieldNames}' 不合法。不允许出现空元素");

                if (name.Length == 3 || name.Length == 5)
                {
                }
                else
                    throw new ArgumentException($"字段名列表 '{removeFieldNames}' 不合法。'{name}' 字符数不正确，应为 3 字符或 5 字符");

                if (name.Length == 3)
                    record.select($"field[@name='{name}']").detach();
                else
                {
                    var field_name = name.Substring(0, 3);
                    var subfield_name = name.Substring(4);

                    List<MarcNode> parents = new List<MarcNode>();
                    var subfields = record.select($"field[@name='{field_name}']/subfield[@name='{subfield_name}']");
                    foreach (MarcSubfield subfield in subfields)
                    {
                        if (parents.Contains(subfield.Parent) == false)
                            parents.Add(subfield.Parent);
                        subfield.detach();
                    }

                    // 检查这些父节点是否 Content 为空。为空则删除这个父节点
                    foreach(var parent in parents)
                    {
                        if (string.IsNullOrEmpty(parent.Content))
                            parent.detach();
                    }
                }
            }
        }
#endif
    }
}
