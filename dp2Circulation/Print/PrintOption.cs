using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;

namespace dp2Circulation
{

    // 一个列
    internal class Column
    {
        public string Caption = "";
        public string Name = "";
        public int WidthChars = -1; // 2014/11/30
        public int MaxChars = -1;
        public string Evalue = "";  // 脚本代码
    }

    // 2008/11/23 new add
    /// <summary>
    /// 模板页参数
    /// </summary>
    internal class TemplatePageParam
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Caption = "";
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath = "";
    }


    // 打印参数
    // TODO: 需要增加预定义缺省栏目的功能
    internal class PrintOption
    {
        public string DataDir = ""; // 数据目录。当有模板文件定义的时候，必须设置此成员

        public string PageHeader = "";  // "%pageno%/%pagecount%";  // 页眉文字
        public string PageHeaderDefault = "";

        public string PageFooter = "";  // 页脚文字
        public string PageFooterDefault = "";

        public string TableTitle = "";  // "册移交清单";  // 表格标题
        public string TableTitleDefault = "";

        public int LinesPerPage = 10;   // 每页多少行内容
        public int LinesPerPageDefault = 10;

        public List<Column> Columns = new List<Column>();   // 栏目列表。定义了需要打印出哪些栏目。

        public List<TemplatePageParam> TemplatePages = new List<TemplatePageParam>();   // 定制的页面

        // 从Application对象中装载数据
        public virtual void LoadData(ApplicationInfo ai,
            string strPath)
        {
            this.PageHeader = ai.GetString(strPath,
                "PageHeader", this.PageHeaderDefault);

                // "%date% 册移交清单 - %barcodefilename% - (共 %pagecount% 页)");
            this.PageFooter = ai.GetString(strPath,
                "PageFooter", this.PageFooterDefault);
            // "%pageno%/%pagecount%");

            this.TableTitle = ai.GetString(strPath,
                "TableTitle", this.TableTitleDefault);
            
            // "%date% 册移交清单");

            this.LinesPerPage = ai.GetInt(strPath,
                "LinesPerPage", this.LinesPerPageDefault);
            
            // 20);

            int nCount = ai.GetInt(strPath, "ColumnsCount", 0);
            if (nCount != 0) // 只有当外部存储中有配置信息时，才清除构造函数创建的缺省信息
            {
                Columns.Clear();
                for (int i = 0; i < nCount; i++)
                {
                    string strColumnName = ai.GetString(strPath,
                        "ColumnName_" + i.ToString(),
                        "");
                    if (String.IsNullOrEmpty(strColumnName) == true)
                        break;

                    string strColumnCaption = ai.GetString(strPath,
                        "ColumnCaption_" + i.ToString(),
                        "");

                    int nMaxChars = ai.GetInt(strPath,
                        "ColumnMaxChars_" + i.ToString(),
                        -1);
                    int nWidthChars = ai.GetInt(strPath,
    "ColumnWidthChars_" + i.ToString(),
    -1);

                    string strEvalue = ai.GetString(strPath,
    "ColumnEvalue_" + i.ToString(),
    "");


                    Column column = new Column();
                    column.Name = strColumnName;
                    column.Caption = strColumnCaption;
                    column.WidthChars = nWidthChars;
                    column.MaxChars = nMaxChars;
                    column.Evalue = strEvalue;

                    this.Columns.Add(column);
                }
            }

            nCount = ai.GetInt(strPath, "TemplatePagesCount", 0);
            if (nCount != 0) // 只有当外部存储中有配置信息时，才清除构造函数创建的缺省信息
            {
                this.TemplatePages.Clear();
                for (int i = 0; i < nCount; i++)
                {
                    TemplatePageParam param = new TemplatePageParam();
                    param.Caption = ai.GetString(strPath,
                        "TemplateCaption_" + i.ToString(),
                        "");
                    param.FilePath = ai.GetString(strPath,
                        "TemplateFilePath_" + i.ToString(),
                        "");

                    Debug.Assert(String.IsNullOrEmpty(this.DataDir) == false, "");

                    param.FilePath = UnMacroPath(param.FilePath);

                    Debug.Assert(param.FilePath.IndexOf("%") == -1, "去除宏以后的路径字符串里面不能有%符号");

                    this.TemplatePages.Add(param);
                }
            }
        }

        public virtual void SaveData(ApplicationInfo ai,
            string strPath)
        {
            ai.SetString(strPath, "PageHeader",
                this.PageHeader);
            ai.SetString(strPath, "PageFooter",
                this.PageFooter);
            ai.SetString(strPath, "TableTitle",
                this.TableTitle);

            ai.SetInt(strPath, "LinesPerPage",
                this.LinesPerPage);
            /*
            ai.SetInt(strPath, "MaxSummaryChars",
                this.MaxSummaryChars);
             * */

            ai.SetInt(strPath, "ColumnsCount",
                this.Columns.Count);

            for (int i = 0; i < this.Columns.Count; i++)
            {
                ai.SetString(strPath,
                    "ColumnName_" + i.ToString(),
                    this.Columns[i].Name);

                ai.SetString(strPath,
                    "ColumnCaption_" + i.ToString(),
                    this.Columns[i].Caption);

                ai.SetInt(strPath,
                    "ColumnMaxChars_" + i.ToString(),
                    this.Columns[i].MaxChars);

                ai.SetInt(strPath,
    "ColumnWidthChars_" + i.ToString(),
    this.Columns[i].WidthChars);

                ai.SetString(strPath,
    "ColumnEvalue_" + i.ToString(),
    this.Columns[i].Evalue);

            }

            ai.SetInt(strPath, "TemplatePagesCount",
    this.TemplatePages.Count);

            for (int i = 0; i < this.TemplatePages.Count; i++)
            {
                ai.SetString(strPath,
                    "TemplateCaption_" + i.ToString(),
                    this.TemplatePages[i].Caption);

                Debug.Assert(String.IsNullOrEmpty(this.DataDir) == false, "");

                // 变换为带有宏的通用型态
                string strFilePath = this.TemplatePages[i].FilePath;
                strFilePath = MacroPath(strFilePath);

                ai.SetString(strPath,
                    "TemplateFilePath_" + i.ToString(),
                    strFilePath);
            }

        }

        string MacroPath(string strPath)
        {
            if (String.IsNullOrEmpty(this.DataDir) == true)
                return strPath;

            // 测试strPath1是否为strPath2的下级目录或文件
            if (PathUtil.IsChildOrEqual(strPath, this.DataDir) == true)
            {
                string strPart = strPath.Substring(this.DataDir.Length);
                return "%datadir%" + strPart;
            }

            return strPath;
        }

        string UnMacroPath(string strPath)
        {
            if (String.IsNullOrEmpty(this.DataDir) == true)
                return strPath;

            return strPath.Replace("%datadir%", this.DataDir);
        }

        // 获得模板页文件
        // parameters:
        //      strCaption  模板名称。大小写不敏感
        public string GetTemplatePageFilePath(string strCaption)
        {
            if (this.TemplatePages == null)
                return null;

            for (int i = 0; i < this.TemplatePages.Count; i++)
            {
                TemplatePageParam param = this.TemplatePages[i];
                if (param.Caption.ToLower() == strCaption.ToLower())
                {
                    Debug.Assert(param.FilePath.IndexOf("%") == -1, "去除宏以后的路径字符串里面不能有%符号");

                    return param.FilePath;
                }
            }

            return null;    // not found
        }

        // 是否至少包含一个脚本定义？
        public bool HasEvalue()
        {
            foreach (Column column in this.Columns)
            {
                if (string.IsNullOrEmpty(column.Evalue) == false)
                    return true;
            }

            return false;
        }
    }
}
