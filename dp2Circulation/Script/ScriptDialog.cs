using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;

namespace dp2Circulation.Script
{
    public partial class ScriptDialog : Form
    {
        // 存放过滤脚本 .cs 文件的子目录
        public string FilterScriptDirectory { get; set; }

        public ScriptDialog()
        {
            InitializeComponent();
        }

        public Panel MainPanel
        {
            get
            {
                return this.panel_main;
            }
        }

        private void ScriptDialog_Load(object sender, EventArgs e)
        {
            if (this.comboBox_biblio_filterScript.Items.Count == 0)
                FillScriptNames(false);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.comboBox_biblio_filterScript.Text))
            {
                MessageBox.Show(this, "尚未指定脚本文件名");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string UiState
        {
            get
            {
                // 防止 combobox 初始化状态失败
                if (this.comboBox_biblio_filterScript.Items.Count == 0)
                {
                    FillScriptNames();
                }

                List<object> controls = new List<object>();
                controls.Add(this.comboBox_biblio_filterScript);
                return GuiState.GetUiState(controls);
            }
            set
            {
                // 防止 combobox 初始化状态失败
                if (this.comboBox_biblio_filterScript.Items.Count == 0)
                {
                    FillScriptNames();
                }

                List<object> controls = new List<object>();
                controls.Add(this.comboBox_biblio_filterScript);
                GuiState.SetUiState(controls, value);
            }
        }


        public const string DO_NOT_USE_SCRIPT = "<不使用脚本>";

        public string ScriptFileName
        {
            get
            {
                string value = this.TryGet(() =>
                {
                    return this.comboBox_biblio_filterScript.Text;
                });
                if (value == DO_NOT_USE_SCRIPT)
                    return "";

                return value;
            }
            set
            {
                this.TryInvoke(() =>
                {
                    // 确保 dropdown list 先有内容
                    if (this.comboBox_biblio_filterScript.Items.Count == 0)
                        FillScriptNames(false);

                    this.comboBox_biblio_filterScript.Text = value;
                });
            }
        }

        // 可以考虑用代码的 MD5 hash 字符串来当作它的名字，把 Assembly 存储到 Cache 中
        public string ScriptCode
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.textBox_biblio_filterScriptCode.Text;
                });
            }
        }

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

        // 准备默认的 MARC 输出过滤脚本目录
        public static string PrepareScriptDirectory()
        {
            var path = Path.Combine(Program.MainForm.UserDir, "export_marc_scripts");
            PathUtil.CreateDirIfNeed(path);
            return path;
        }

        private void textBox_biblio_filterScriptCode_TextChanged(object sender, EventArgs e)
        {
            _codeChanged = true;
        }

        public delegate void delagate_initializeHost(VerifyHost host);

        // 导出 MARC 前过滤和处理 MARC 字段
        // parameters:
        //      func_init   hostObj 初始化回调函数。注意，每次调用本函数都会触发一次这个回调函数
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
            ref VerifyHost hostObj,
            out string strError)
        {
            strError = "";

            if (hostObj == null)
            {
                int nRet = GetAssembly(
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
            }

            func_init?.Invoke(hostObj);

            try
            {
                hostObj.VerifyResult = hostObj.Verify("", record.Text);
            }
            catch (Exception ex)
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
                    foreach (var parent in parents)
                    {
                        if (string.IsNullOrEmpty(parent.Content))
                            parent.detach();
                    }
                }
            }
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

        private void ScriptDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveScriptFile();
        }
    }
}
