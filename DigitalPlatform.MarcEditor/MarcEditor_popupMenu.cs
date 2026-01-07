using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using LibraryStudio.Forms;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// 上下文菜单功能
    /// </summary>
    public partial class MarcEditor : MarcControl
    {
        public override bool RawCut()
        {
            if (this.ReadOnly)
                return false;

            if (HasSelection() == false)
                return false;
            var start = Math.Min(this.SelectionStart, this.SelectionEnd);
            var length = Math.Abs(this.SelectionEnd - this.SelectionStart);
            var text = this.Content.Substring(start, length);
            TextToClipboardFormat(text);
            RawRemoveBolckText();
            return true;
        }

        public override bool SoftlyCut()
        {
            if (this.ReadOnly)
                return false;

            if (HasSelection() == false)
                return false;

            TextToClipboardFormat(GetSelectedContent());
            SoftlyRemoveSelectionText();
            return true;
        }


        public override bool Copy()
        {
            if (HasSelection() == false)
                return false;

            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                return base.Copy();
            }

            TextToClipboardFormat(GetSelectedContent());
            return true;
        }

        public override bool SoftlyPaste(string text = null)
        {
            if (text == null)
            {
                text = MarcEditor.ClipboardToTextFormat();
                // 去掉回车换行符号
                text = text.Replace("\r\n", "\r");
                text = text.Replace("\r", "*");
                text = text.Replace("\n", "*");
                text = text.Replace("\t", "*");
            }

            return base.SoftlyPaste(text);
        }

        public override bool RawPaste(string text = null)
        {
            if (text == null)
            {
                text = MarcEditor.ClipboardToTextFormat();
                // 去掉回车换行符号
                text = text.Replace("\r\n", "\r");
                text = text.Replace("\r", "*");
                text = text.Replace("\n", "*");
                text = text.Replace("\t", "*");
            }

            return base.RawPaste(text);
        }

        void PopupCtrlAMenu()
        {
            // 有时数据加工菜单来不及更新状态，这里补充触发一次
            this.FireSelectedFieldChanged();

            // 获得当前位置缺省值
            List<string> macros = this.SetDefaultValue(true, -1);

            ContextMenu contextMenu = new ContextMenu();

            if (macros != null && macros.Count > 0)
            {
                string strText = "";
                if (macros.Count == 1)
                {
                    Debug.Assert(macros != null, "");
                    strText = "缺省值 '" + macros[0].Replace(" ", "_").Replace(Record.SUBFLD, Record.KERNEL_SUBFLD) + "'";
                }
                else
                    strText = "缺省值 " + macros.Count.ToString() + " 个";

                var menuItem = new MenuItem(strText);

                if (macros != null && macros.Count == 1)
                {
                    menuItem.Click += new System.EventHandler(this.SetCurrentDefaultValue);
                    menuItem.Tag = 0;
                }
                else if (macros != null && macros.Count > 1)
                {
                    // 子菜单
                    for (int i = 0; i < macros.Count; i++)
                    {
                        string strMenuText = macros[i];

                        MenuItem subMenuItem = new MenuItem(strMenuText);
                        subMenuItem.Click += new System.EventHandler(this.SetCurrentDefaultValue);
                        subMenuItem.Tag = i;
                        menuItem.MenuItems.Add(subMenuItem);
                    }
                }
                contextMenu.MenuItems.Add(menuItem);
            }

            // 获得当前位置可用的菜单
            GenerateDataEventArgs e = new GenerateDataEventArgs();
            e.ScriptEntry = "!getActiveMenu";
            this.OnGenerateData(e);
            if (string.IsNullOrEmpty(e.ErrorInfo) == false)
            {
                Console.Beep();
                var menuItem = new MenuItem("error: " + e.ErrorInfo);
                contextMenu.MenuItems.Add(menuItem);
            }
            else
            {
                // e.Parameter 中返回了 XML 格式的菜单项
                string xml = e.Parameter as string;
                if (string.IsNullOrEmpty(xml))
                {
                    Console.Beep();
                    var menuItem = new MenuItem($"error: e.Parameter 为空");
                    contextMenu.MenuItems.Add(menuItem);
                    goto END1;
                }
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);

                var actions = dom.DocumentElement.SelectNodes("action");

                if (actions.Count > 0
                    && macros != null && macros.Count > 0)
                {
                    var menuItem = new MenuItem("-");
                    contextMenu.MenuItems.Add(menuItem);
                }

                foreach (XmlElement action in actions)
                {
                    var scriptEntry = action.GetAttribute("scriptEntry");
                    var menuItem = new MenuItem(action.GetAttribute("name"));
                    menuItem.Tag = scriptEntry;
                    menuItem.Click += (o1, e1) =>
                    {
                        this.OnGenerateData(new GenerateDataEventArgs
                        {
                            ScriptEntry = scriptEntry,
                        });
                        this.Focus();
                    };
                    contextMenu.MenuItems.Add(menuItem);
                }
            }

            if (contextMenu.MenuItems.Count == 0)
            {
                Console.Beep();
                return;
            }

        END1:
            //if (contextMenu.MenuItems.Count > 0)
            //    contextMenu.MenuItems[0].PerformSelect();

            POINT point = new POINT();
            point.x = 0;
            point.y = 0;
            bool bRet = API.GetCaretPos(ref point);
            contextMenu.Show(this, new Point(point.x, point.y + this.CalcuTextLineHeight(null)));

            // SendKeys.Send("{DOWN}");
        }

        private List<string> GetDefaultValues()
        {
            Cursor oldcursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;   // 出现沙漏
            try
            {
                return this.SetDefaultValue(true, -1);
            }
            finally
            {
                this.Cursor = oldcursor;
            }
        }

        // 获得“缺省值” CommandItem
        // 利用 .Refresh 动态返回 CoomandItem 写法的版本
        CommandItem DefaultValueCommand()
        {
            CommandItem c = new CommandItem();

            c.Refresh = (cmd) =>
            {
                CommandItem command = new CommandItem();

                List<string> macros = null;
                macros = GetDefaultValues();
                if (macros?.Count == 1)
                {
                    command.Handler = this.SetCurrentDefaultValue;
                    command.Tag = 0;
                }
                else
                {
                    command.Handler = null;
                    command.Tag = null;
                }
                command.SubCommands = GetDefaultValueChildren();
                command.GetCaption = () =>
                {
                    string strText = "";
                    if (macros == null || macros.Count == 0)
                        strText = "缺省值(无)";
                    else if (macros.Count == 1)
                    {
                        Debug.Assert(macros != null, "");
                        strText = "缺省值 '" + macros[0].Replace(" ", "_").Replace(Record.SUBFLD, Record.KERNEL_SUBFLD) + "'";
                    }
                    else
                        strText = "缺省值 " + macros.Count.ToString() + " 个";
                    return strText;
                };
                command.CanExecute = () =>
                {
                    return macros != null && macros.Count == 1;
                };

                return command;


                IEnumerable<CommandItem> GetDefaultValueChildren()
                {
                    if (macros != null && macros.Count > 1)
                    {
                        for (int i = 0; i < macros.Count; i++)
                        {
                            string strMenuText = macros[i];
                            yield return new CommandItem
                            {
                                Caption = strMenuText,
                                Handler = this.SetCurrentDefaultValue,
                                Tag = i,
                            };
                        }
                    }
                }
            };

            return c;
        }

#if REMOVED
        // 获得“缺省值” CommandItem
        CommandItem DefaultValueCommand()
        {
            CommandItem command = new CommandItem();

            List<string> macros = null;
            command.Refresh = () =>
            {
                macros = GetDefaultValues();
                if (macros?.Count == 1)
                {
                    command.Handler = this.SetCurrentDefaultValue;
                    command.Tag = 0;
                }
                else
                {
                    command.Handler = null;
                    command.Tag = null;
                }
                return null;
            };
            command.SubCommands = GetDefaultValueChildren();
            command.GetCaption = () =>
            {
                string strText = "";
                if (macros == null || macros.Count == 0)
                    strText = "缺省值(无)";
                else if (macros.Count == 1)
                {
                    Debug.Assert(macros != null, "");
                    strText = "缺省值 '" + macros[0].Replace(" ", "_").Replace(Record.SUBFLD, Record.KERNEL_SUBFLD) + "'";
                }
                else
                    strText = "缺省值 " + macros.Count.ToString() + " 个";
                return strText;
            };

            command.CanExecute = () =>
            {
                return macros != null && macros.Count == 1;
            };

            return command;

            IEnumerable<CommandItem> GetDefaultValueChildren()
            {
                if (macros != null && macros.Count > 1)
                {
                    for (int i = 0; i < macros.Count; i++)
                    {
                        string strMenuText = macros[i];
                        yield return new CommandItem
                        {
                            Caption = strMenuText,
                            Handler = this.SetCurrentDefaultValue,
                            Tag = i,
                        };
                    }
                }
            }

        }
#endif

        public override IEnumerable<CommandItem> GetCommandItems()
        {
            return new List<CommandItem>()
            {
                new CommandItem()
                { 
                    // 只提供快捷键
                    KeyData= Keys.Control | Keys.A,
                    Handler=(s,e) => this.PopupCtrlAMenu(),
                },
                DefaultValueCommand(),  // 可用 005 字段测试
                new CommandItem()
                {
                    Caption="撤销(&U)",
                    KeyData=Keys.Control | Keys.Z,
                    Handler=(s,e) => this.Undo(),
                    CanExecute=()=> this.CanUndo(),
                },
                new CommandItem()
                {
                    Caption="重做(&R)",
                    KeyData=Keys.Control | Keys.Y,
                    Handler=(s,e) => this.Redo(),
                    CanExecute=()=> this.CanRedo(),
                },
                new CommandItem()
                {
                    Caption="-",
                },
                new CommandItem()
                {
                    Caption="剪切(&T)",
                    KeyData=Keys.Control | Keys.X,
                    Handler=(s,e) =>this.SoftlyCut(),
                    CanExecute=()=> this.CanCut(),
                },
                new CommandItem()
                {
                    Caption="复制(&C)",
                    KeyData=Keys.Control | Keys.C,
                    Handler=(s,e) => this.Copy(),
                    CanExecute=()=> this.HasSelection(),
                },
                new CommandItem()
                {
                    Caption="粘贴(&V)",
                    KeyData=Keys.Control | Keys.V,
                    Handler=(s,e) => this.SoftlyPaste(),
                    CanExecute=()=> this.CanPaste(),
                },
                new CommandItem() { Caption="-" },

                new CommandItem()
                {
                    Caption="原始剪切",
                    // 不设置快捷键（若需要可添加）
                    Handler=(s,e) => this.RawCut(),
                    CanExecute=()=> this.HasSelection(),
                },
                new CommandItem()
                {
                    Caption="原始粘贴",
                    Handler=(s,e) => this.RawPaste(),
                    CanExecute=()=> this.CanPaste(),
                },

                new CommandItem() { Caption="-" },

                new CommandItem()
                {
                    Caption="全选(&A)",
                    KeyData=Keys.None,
                    Handler=(s,e) => {
                        this.SelectAll();
                        this.FireSelectedFieldChanged();
                    },
                    CanExecute=()=> true,
                },

                new CommandItem() { Caption="-" },

                new CommandItem()
                {
                    Caption="选择子字段",
                    KeyData=Keys.Control | Keys.B,
                    Handler=(s,e) => {
                        this.SelectCaretSubfield();
                    },
                    CanExecute=()=> true,
                },

                new CommandItem()
                {
                    Caption="删除当前子字段",
                    KeyData=Keys.Shift | Keys.Delete,
                    Handler=(s,e) => {
                        var ret = this.DeleteCaretSubfield() != null;
                        TriggerEvenArgs.SetHandled(e, ret);
                        },
                    CanExecute=()=> true,
                },

                new CommandItem()
                {
                    Caption="到下一个字段",
                    KeyData=Keys.Enter,
                    Handler=(s,e) => {
                        this.ToNextField();
                    },
                    CanExecute=()=> true,  
                },
                new CommandItem()
                {
                    Caption="折行",
                    KeyData=Keys.Shift | Keys.Enter,
                    Handler=(s,e) => this.BreakText(),
                    CanExecute=()=> true,
                },
                new CommandItem()
                {
                    Caption="插入新字段",
                    KeyData=Keys.Control | Keys.Enter,
                    Handler=(s,e) => InsertField(this.CaretFieldIndex, 0, 1),
                    CanExecute=()=> true,
                },


                new CommandItem() { Caption="-" },

                // 定长模板
                new CommandItem()
                {
                    // 这两行不能少。否则击键会捕捉不到，因为击键探测时为了提高速度是不会触发 .Refresh 的
                    KeyData = Keys.Control | Keys.M,
                    Handler = (s,e) => this.GetValueFromTemplate(),

                    Refresh = (cmd) =>
                    {
                        var enabled = this.HasTemplateOrValueListDef(
                "template",
                out string name) == 1;

                        cmd.GetCaption = ()=>"定长模板 " + (enabled ? name : "");
                        // cmd.KeyData = Keys.Control | Keys.M;
                        cmd.Handler = (s,e) => this.GetValueFromTemplate();
                        cmd.CanExecute= ()=> enabled;
                        return cmd;
                    },
                },

                // 值列表
                new CommandItem()
                {
                    Refresh= (cmd)=>{
                        var enabled = this.HasTemplateOrValueListDef(
                        "valuelist",
                        out string name) == 1;

                        cmd.GetCaption= ()=> "值列表 " + name + " ...";
                        cmd.Handler = (s,e) => this.GetValueFromValueList();
                        cmd.CanExecute= ()=> enabled;

                        return cmd;
                    },
                },

                new CommandItem()
                {
                    Caption="插入子字段符号",
                    KeyData=Keys.Control | Keys.I,
                    Handler=(s,e) => this.InsertSubfieldChar(),
                    CanExecute=()=> true,
                },
                new CommandItem()
                {
                    Caption="校验 MARC",
                    KeyData=Keys.Control | Keys.U,
                    Handler = (s,e)=>{
                        var ea = new GenerateDataEventArgs();
                        this.OnVerifyData(ea);
                    },
                    CanExecute=()=> true,
                },
                new CommandItem()
                {
                    Caption="加拼音",
                    KeyData=Keys.Control | Keys.S,
                    Handler = (s,e)=>{
                        var e1 = new GenerateDataEventArgs(){
                        ScriptEntry = "AddPinyin",
                        FocusedControl = this,
                        };
                        this.OnGenerateData(e1);
                    },
                    CanExecute=()=> true,
                },
                new CommandItem()
                {
                    Caption="删除拼音",
                    KeyData=Keys.Control | Keys.D,
                    Handler = (s,e)=>{
                        var e1 = new GenerateDataEventArgs(){
                        ScriptEntry = "RemovePinyin",
                        FocusedControl = this,
                        };
                        this.OnGenerateData(e1);
                    },
                    CanExecute=()=> true,
                },
                new CommandItem()
                {
                    Caption="插入新字段(询问字段名) ...",
                    Handler= this.InsertField,
                },
                new CommandItem()
                {
                    KeyData = Keys.Insert,
                    Handler= (s,e)=>{
                        // 头标区，或者其它字段的字段名和指示符区域，可以触发本功能
                        var region = this.CaretFieldRegion;
                        if (this.CaretFieldIndex == 0
                            || region == FieldRegion.Name
                            || region == FieldRegion.Indicator)
                        {
                            this.InsertField(s, e);
                        }
                        else
                        {
                            TriggerEvenArgs.SetHandled(e, false);
                        }
                    },
                },
                new CommandItem()
                {
                    Caption="插入新字段以",
                    SubCommands = new List<CommandItem>()
                    {
                        new CommandItem()
                        {
                            Caption="前插",
                            Handler= this.InsertBeforeFieldNoDlg,
                            CanExecute=()=> this.FocusedField?.Name != "###",
                        },
                        new CommandItem()
                        {
                            Caption="后插",
                            Handler= this.InsertAfterFieldWithoutDlg,
                        },
                        new CommandItem()
                        {
                            Caption="末尾",
                            Handler= this.AppendFieldNoDlg,
                        },
                    },
                },

                new CommandItem()
                {
                    Caption="删除字段",
                    KeyData=Keys.None,
                    Handler=(s,e) => this.DeleteFieldWithDlg(),
                    CanExecute=()=> true,
                },

                new CommandItem() { Caption="-" },

                CopyWholeMarc(),

                PasteWHoleMarc(),

                new CommandItem() { Caption="-" },

                OrganizeCommand(),



                // 突出显示空格
                new CommandItem()
                {
                    Refresh = (o)=>{
                        o.Checked = this.HighlightBlankChar != ' ';
                        return null;
                    },
                    Caption="突出显示空格",
                    Handler= (s, e)=>{
                        if (this.HighlightBlankChar == ' ')
                            this.HighlightBlankChar = '·';
                        else
                            this.HighlightBlankChar =' ';
                    },
                },

                /*
                // testing SplitField()
                new CommandItem()
                {
                    Caption="SplitField()",
                    KeyData=Keys.None,
                    Handler=(s,e) => this.SplitField(-1, "prev"),
                    CanExecute=()=> true,
                },
                */

                new CommandItem()
                {
                    Caption="视觉风格 ...",
                    Handler= (s, e) => this.SettingVisualStyle(),
                },
                new CommandItem()
                {
                    Caption="字体 ...",
                    Handler= (s, e) => this.SettingFont(),
                },
                new CommandItem()
                {
                    Caption="属性 ...",
                    Handler= this.Property_menu,
                },
            };
        }

        CommandItem OrganizeCommand()
        {
            return new CommandItem()
            {
                Caption = "整理",
                SubCommands = new List<CommandItem>()
                {
                new CommandItem()
                {
                    Caption="简转繁\tCtrl+K,L",
                    KeyData=Keys.Control | Keys.K,
                    KeyData2=Keys.Control | Keys.L,
                    Handler= this.menuItem_s2t,
                    CanExecute=()=>this.HasSelection(),
                },
                new CommandItem()
                {
                    Caption="繁转简\tCtrl+K,J",
                    KeyData=Keys.Control | Keys.K,
                    KeyData2=Keys.Control | Keys.J,
                    Handler= this.menuItem_t2s,
                    CanExecute=()=>this.HasSelection(),
                },
                new CommandItem()
                {
                    Caption="字段重新排序(&S)",
                    KeyData=Keys.Control | Keys.Q,
                    Handler=(s,e) => this.SortFields(),
                    CanExecute=()=> true,                },

                new CommandItem() { Caption="-" },

                new CommandItem()
                {
                    Caption="删除全部空字段、子字段(&D)",
                    Handler= this.menuItem_removeEmptyFieldsSubfields,
                },
                new CommandItem()
                {
                    Caption="删除全部空子字段(&D)",
                    Handler= this.menuItem_removeEmptySubfields,
                },
                new CommandItem()
                {
                    Caption="删除全部空字段(&D)",
                    Handler= this.menuItem_removeEmptyFields,
                },
                new CommandItem() { Caption="-" },

                new CommandItem()
                {
                    Caption="平行模式(&P)",
                    Handler= this.menuItem_toParallel,
                },

                new CommandItem()
                {
                    Caption="880 模式(&P)",
                    Handler= this.menuItem_to880,
                },

                },
            };
        }

        CommandItem PasteWHoleMarc()
        {
            return new CommandItem()
            {
                Caption = "粘贴完整记录来自",
                SubCommands = new List<CommandItem>()
                    {
                        new CommandItem()
                        {
                            Caption="机内格式",
                            Handler= this.menuItem_PasteFromJinei,
                            CanExecute= ()=> this.CanPaste(),
                        },

                        new CommandItem()
                        {
                            Caption="工作单格式",
                            Handler= this.menuItem_PasteFromWorksheet,
                            CanExecute= ()=> this.CanPaste(),
                        },
                        new CommandItem()
                        {
                            Caption="dp2OPAC 页面",
                            Handler= this.menuItem_PasteFromDp2OPAC,
                            CanExecute=()=> this.CanPaste() && this.ReadOnly == false,
                        },

                        new CommandItem() { Caption="-" },

                        new CommandItem()
                        {
                            Caption="来自 NLC 页面",
                            Handler= this.menuItem_PasteFromNlcMarc,
                            CanExecute=()=> this.CanPaste() && this.ReadOnly == false,
                        },
                        new CommandItem()
                        {
                            Caption="来自 tcmarc",
                            Handler= this.menuItem_PasteFromTcMarc,
                            CanExecute=()=> this.CanPaste() && this.ReadOnly == false,
                        },
                        new CommandItem()
                        {
                            Caption="来自 MARCXML",
                            Handler= this.menuItem_PasteFromMarcXml,
                            CanExecute=()=> this.CanPaste() && this.ReadOnly == false,
                        },
                    },
            };
        }

        CommandItem CopyWholeMarc()
        {
            return new CommandItem()
            {
                Caption = "复制完整记录为",
                SubCommands = new List<CommandItem>()
                {
                    new CommandItem()
                    {
                        Caption="机内格式",
                        Handler= this.CopyJineiToClipboard,
                        CanExecute=()=>true,
                    },
                    new CommandItem()
                    {
                        Caption="工作单格式",
                        Handler= this.CopyWorksheetToClipboard,
                        CanExecute=()=>true,
                    },
                },
            };
        }


        #region 具体的菜单功能命令函数


        internal void SetCurrentDefaultValue(object sender, EventArgs e)
        {
            int index = (int)GetItemMenuTag(sender);

            SetDefaultValue(false, index);
        }


        #endregion

#if REF
        // 在已有的菜单事项上追加事项
        // parameters:
        //      bFull   是否包含一些重复事项
        internal void AppendMenu(ContextMenu contextMenu,
            bool bFull)
        {
            MenuItem menuItem;
            MenuItem subMenuItem;

            // 插入字段(询问字段名)
            menuItem = new MenuItem("插入新字段(询问字段名)");// + strName);
            menuItem.Click += new System.EventHandler(this.InsertField);
            contextMenu.MenuItems.Add(menuItem);


            // 插入字段
            menuItem = new MenuItem("插入新字段");
            if (this.ReadOnly == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ** 子菜单

            // 前插
            subMenuItem = new MenuItem("前插");
            subMenuItem.Click += new System.EventHandler(this.InsertBeforeFieldNoDlg);
            menuItem.MenuItems.Add(subMenuItem);
            if (this.SelectedFieldIndices.Count == 1)
            {
                // 头标区不能修改字段名
                if (this.FocusedField.Name == "###")
                    subMenuItem.Enabled = false;
                else
                    subMenuItem.Enabled = true;
            }
            else
            {
                subMenuItem.Enabled = false;
            }

            //后插
            subMenuItem = new MenuItem("后插");
            subMenuItem.Click += new System.EventHandler(this.InsertAfterFieldWithoutDlg);
            menuItem.MenuItems.Add(subMenuItem);
            if (this.SelectedFieldIndices.Count == 1)
            {
                subMenuItem.Enabled = true;
            }
            else
            {
                subMenuItem.Enabled = false;
            }

            //末尾
            subMenuItem = new MenuItem("末尾");
            subMenuItem.Click += new System.EventHandler(this.AppendFieldNoDlg);
            menuItem.MenuItems.Add(subMenuItem);


            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 撤消
            menuItem = new MenuItem("撤消(&U)\tCtrl+Z");
            menuItem.Click += new System.EventHandler(this.menuItem_Undo);
            contextMenu.MenuItems.Add(menuItem);
            menuItem.Enabled = this.CanUndo();

            // 重做
            menuItem = new MenuItem("重做(&R)\tCtrl+Y");
            menuItem.Click += new System.EventHandler(this.menuItem_Redo);
            menuItem.Enabled = this.CanRedo();
            contextMenu.MenuItems.Add(menuItem);

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 剪切
            menuItem = new MenuItem("剪切字段(Ctrl+X)");// + strName);
            menuItem.Click += new System.EventHandler(this.menuItem_Cut);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectedFieldIndices.Count > 0
                && this.ReadOnly == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            //复制
            menuItem = new MenuItem("复制字段(Ctrl+C)");// + strName);
            menuItem.Click += new System.EventHandler(this.menuItem_Copy);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectedFieldIndices.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            IDataObject ido = GetClipboardDataObject(); // Clipboard.GetDataObject();

            // 插入字段
            menuItem = new MenuItem("从特定格式粘贴 ...");
            if (this.ReadOnly == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            {
                // 从dp2OPAC粘贴整个记录
                subMenuItem = new MenuItem("从 dp2OPAC 粘贴整个记录");// + strName);
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromDp2OPAC);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)    // 原来是==1
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 从 NLC 粘贴整个记录
                subMenuItem = new MenuItem("从 NLC 粘贴整个记录");// + strName);
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromNlcMarc);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 从tcmarc粘贴整个记录
                subMenuItem = new MenuItem("从 tcmarc 粘贴整个记录");// + strName);
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromTcMarc);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)    // 原来是==1
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 从 XML 粘贴整个记录
                subMenuItem = new MenuItem("从 MARCXML 粘贴整个记录");// + strName);
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromMarcXml);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)    // 原来是==1
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 2021/12/16
                // 从 工作单 粘贴整个记录
                subMenuItem = new MenuItem("从 工作单 粘贴整个记录");
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromWorksheet);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)    // 原来是==1
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 2024/5/20
                // 从 机内格式 粘贴整个记录
                subMenuItem = new MenuItem("从 机内格式 粘贴整个记录");
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromJinei);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;
            }

            //粘贴覆盖
            menuItem = new MenuItem("粘贴覆盖字段");// + strName);
            menuItem.Click += new System.EventHandler(this.menuItem_PasteOverwrite);
            contextMenu.MenuItems.Add(menuItem);
            if (ido.GetDataPresent(DataFormats.Text)
                && this.SelectedFieldIndices.Count >= 1
                && this.ReadOnly == false)    // 原来是==1
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("整理");
            contextMenu.MenuItems.Add(menuItem);

            /*
            {
                subMenuItem = new MenuItem("简转繁(Ctrl+K,L)");
                subMenuItem.Click += new System.EventHandler(this.menuItem_s2t);
                menuItem.MenuItems.Add(subMenuItem);
                if ((this.curEdit.Visible && string.IsNullOrEmpty(this.curEdit.SelectedText) == false)
                    || this.SelectedFieldIndices.Count > 0)
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                subMenuItem = new MenuItem("繁转简(Ctrl+K,J)");
                subMenuItem.Click += new System.EventHandler(this.menuItem_t2s);
                menuItem.MenuItems.Add(subMenuItem);
                if ((this.curEdit.Visible && string.IsNullOrEmpty(this.curEdit.SelectedText) == false)
                    || this.SelectedFieldIndices.Count > 0)
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);
            }
            */

            // 
            subMenuItem = new MenuItem("字段重新排序(&S)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_sortFields);
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("-");
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("删除全部空字段、子字段(&D)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_removeEmptyFieldsSubfields);
            menuItem.MenuItems.Add(subMenuItem);


            subMenuItem = new MenuItem("-");
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("删除全部空子字段(&D)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_removeEmptySubfields);
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("删除全部空字段(&D)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_removeEmptyFields);
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("-");
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("平行模式(&P)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_toParallel);
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("880 模式(&P)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_to880);
            menuItem.MenuItems.Add(subMenuItem);

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 删除
            menuItem = new MenuItem("删除字段");
            menuItem.Click += new System.EventHandler(this.DeleteFieldWithDlg);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectedFieldIndices.Count > 0
                && this.ReadOnly == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;


            if (bFull == true)
            {
                //--------------
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);

                // 定长模板
                // TODO: 当MarcEditor为ReadOnly状态时，定长也应该是ReadOnly状态。或者至少要在修改后确定时警告
                menuItem = new MenuItem("定长模板");
                menuItem.Click += new System.EventHandler(this.GetValueFromTemplate);
                contextMenu.MenuItems.Add(menuItem);
                if (this.SelectedFieldIndices.Count == 1)
                    menuItem.Enabled = true;
                else
                    menuItem.Enabled = false;

            }

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 全选
            menuItem = new MenuItem("全选字段(&A)");
            menuItem.Click += new System.EventHandler(this.Menu_SelectAll);
            contextMenu.MenuItems.Add(menuItem);
            if (this.record.Fields.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 复制机内格式到剪贴板
            menuItem = new MenuItem("复制整个记录(机内格式)");
            menuItem.Click += new System.EventHandler(this.CopyJineiToClipboard);
            contextMenu.MenuItems.Add(menuItem);
            if (this.record.Fields.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 复制工作单到剪贴板
            menuItem = new MenuItem("复制整个记录(工作单格式)");
            menuItem.Click += new System.EventHandler(this.CopyWorksheetToClipboard);
            contextMenu.MenuItems.Add(menuItem);
            if (this.record.Fields.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 2024/5/20
            // 从 机内格式 粘贴整个记录
            menuItem = new MenuItem("粘贴整个记录(机内格式)");
            menuItem.Click += new System.EventHandler(this.menuItem_PasteFromJinei);
            contextMenu.MenuItems.Add(menuItem);
            if (ido.GetDataPresent(DataFormats.Text)
                && this.ReadOnly == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 从 工作单 粘贴整个记录
            menuItem = new MenuItem("粘贴整个记录(工作单格式)");
            menuItem.Click += new System.EventHandler(this.menuItem_PasteFromWorksheet);
            contextMenu.MenuItems.Add(menuItem);
            if (ido.GetDataPresent(DataFormats.Text)
                && this.ReadOnly == false)    // 原来是==1
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            /*
                        //--------------
                        menuItem = new MenuItem ("-");
                        contextMenu.MenuItems.Add(menuItem);

                        // Marc
                        menuItem = new MenuItem("查看Marc");
                        menuItem.Click += new System.EventHandler(this.ShowMarc);
                        contextMenu.MenuItems.Add(menuItem);
                        if (this.record.Count > 0)
                            menuItem.Enabled = true;
                        else
                            menuItem.Enabled = false;

            */

            /*
            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 从右向左的阅读顺序
            menuItem = new MenuItem("从右向左的阅读顺序(R)");
            menuItem.Click += new System.EventHandler(this.Menu_r2l);
            if (this.RightToLeft == RightToLeft.Yes)
                menuItem.Checked = true;
            contextMenu.MenuItems.Add(menuItem);
            */

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 属性
            menuItem = new MenuItem("属性");
            menuItem.Click += new System.EventHandler(this.Property_menu);
            contextMenu.MenuItems.Add(menuItem);
        }
#endif

#if REF

        public void PopupMenuEx(Point p)
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem;

            // 缺省值
            List<string> macros = null;
            Cursor oldcursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;   // 出现沙漏
            try
            {
                macros = this.SetDefaultValue(true, -1);
            }
            finally
            {
                this.Cursor = oldcursor;
            }

            string strText = "";
            if (macros == null || macros.Count == 0)
                strText = "缺省值(无)";
            else if (macros.Count == 1)
            {
                Debug.Assert(macros != null, "");
                strText = "缺省值 '" + macros[0].Replace(" ", "_").Replace(Record.SUBFLD, Record.KERNEL_SUBFLD) + "'";
            }
            else
                strText = "缺省值 " + macros.Count.ToString() + " 个";

            menuItem = new MenuItem(strText);
            // menuItem.Click += new System.EventHandler(this.MarcEditor.SetCurFirstDefaultValue);

            if (macros != null && macros.Count == 1)
            {
                menuItem.Click += new System.EventHandler(this.SetCurrentDefaultValue);
                menuItem.Tag = 0;
            }
            else if (macros != null && macros.Count > 1)
            {
                // 子菜单
                for (int i = 0; i < macros.Count; i++)
                {
                    string strMenuText = macros[i];

                    MenuItem subMenuItem = new MenuItem(strMenuText);
                    subMenuItem.Click += this.SetCurrentDefaultValue;
                    subMenuItem.Tag = i;
                    menuItem.MenuItems.Add(subMenuItem);
                }
            }
            contextMenu.MenuItems.Add(menuItem);
            if (macros == null || macros.Count == 0)
                menuItem.Enabled = false;

            var dom = this.GetDomRecord();

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 撤消
            menuItem = new MenuItem("撤消(&U)\tCtrl+Z");
            menuItem.Click += new System.EventHandler((s, e) => this.Undo());
            contextMenu.MenuItems.Add(menuItem);
            menuItem.Enabled = this.CanUndo();

            // 重做
            menuItem = new MenuItem("重做(&R)\tCtrl+Y");
            menuItem.Click += new System.EventHandler((s, e) => this.Redo());
            menuItem.Enabled = this.CanRedo();
            contextMenu.MenuItems.Add(menuItem);

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 剪切
            menuItem = new MenuItem("剪切(&T)\tCtrl+X");
            menuItem.Click += new System.EventHandler((s, e) => SoftlyCut());
            contextMenu.MenuItems.Add(menuItem);
            menuItem.Enabled = this.CanCut();

            // 复制
            menuItem = new MenuItem("复制(&C)\tCtrl+C");
            menuItem.Click += new System.EventHandler((s, e) => Copy());
            contextMenu.MenuItems.Add(menuItem);
            menuItem.Enabled = this.HasBlock();

            // 粘贴
            menuItem = new MenuItem("粘贴(&P)\tCtrl+V");
            menuItem.Click += new System.EventHandler((s, e) => SoftlyPaste());
            contextMenu.MenuItems.Add(menuItem);
            menuItem.Enabled = this.CanPaste();

            /*
            // 删除
            menuItem = new MenuItem("删除(&D)\tDel");
            menuItem.Click += new System.EventHandler(this.Menu_Delete);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectionLength > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            */

            // 删除当前子字段
            menuItem = new MenuItem("删除当前子字段\tShift+Del");
            menuItem.Click += (o, e1) =>
            {
                DeleteCaretSubfield();
            };
            contextMenu.MenuItems.Add(menuItem);

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 全选
            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler((s, e) => SelectAll());
            contextMenu.MenuItems.Add(menuItem);

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 定长模板
            string strCurName = "";
            bool bEnable = this.HasTemplateOrValueListDef(
                "template",
                out strCurName) == 1;

            menuItem = new MenuItem("定长模板 " + strCurName + "\tCtrl+M");
            menuItem.Click += new System.EventHandler(this.GetValueFromTemplate);
            contextMenu.MenuItems.Add(menuItem);

            // 值列表
            bEnable = this.HasTemplateOrValueListDef(
                "valuelist",
                out strCurName) == 1;

            menuItem = new MenuItem("值列表 " + strCurName);
            menuItem.Click += new System.EventHandler((s, e) => this.GetValueFromValueList());
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectedFieldIndices.Count == 1
                && bEnable == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

#if NO
            // 测试
            menuItem = new MenuItem("IMEMODE");
            menuItem.Click += new System.EventHandler(this.ShowImeMode);
            contextMenu.MenuItems.Add(menuItem);
#endif

            /*
            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 删除字段
            menuItem = new MenuItem("删除字段");
            menuItem.Click += new System.EventHandler(this.MarcEditor.DeleteFieldWithDlg);
            contextMenu.MenuItems.Add(menuItem);
            if (this.MarcEditor.m_nFocusCol == 1 || this.MarcEditor.m_nFocusCol == 2)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
             */

            // 追加其他菜单项
            {
                //--------------
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);

                this.AppendMenu(contextMenu, false);
            }

            contextMenu.Show(this, p);
        }
#endif


#if REMOVED
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PopupMenu(e.Location);
                return;
            }
            base.OnMouseDown(e);
        }

        // 右键弹出上下文菜单
        private void PopupMenu(Point p)
        {
            if (/*this.curEdit.Visible == false*/true)
            {
                // 小 edit 不可见时，包括选择了一个或者多个字段的各种情况
                ContextMenu contextMenu = new ContextMenu();
                this.AppendMenu(contextMenu, true);
                contextMenu.Show(this, p);
            }
            else
            {
                // 小 edit 可见时

                // TODO 重新编写
                // this.curEdit.PopupMenu(this, p);

                /*
                this.curEdit.PopupMenu(this.curEdit,
                    this.curEdit.PointToClient(this.PointToScreen(p) ) );
                 */
            }
        }
#endif
    }
}
