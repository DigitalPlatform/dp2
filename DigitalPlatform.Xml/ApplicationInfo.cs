using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Deployment.Application;

namespace DigitalPlatform.Xml
{
    /// <summary>
    /// 用 XML 文件保存程序的各种配置信息
    /// </summary>
    public class ApplicationInfo
    {
        public XmlDocument dom = new XmlDocument();
        public string PureFileName = "";
        public string CurrentDirectory = "";
        public string FileName = "";

        Hashtable titleTable = null;

        // bool m_bFirstMdiOpened = false;

        public event EventHandler LoadMdiLayout = null;
        public event EventHandler SaveMdiLayout = null;

        // 2017/12/20
        public event AppInfoChangedEventHandler AppInfoChanged = null;

        public ApplicationInfo()
        {
        }

        /*
        public bool FirstMdiOpened
        {
            get
            {
                return m_bFirstMdiOpened;
            }
            set
            {
                m_bFirstMdiOpened = value;
            }
        }*/

        // 构造函数
        // 本函数将XML文件中的内容装入内存。
        // parameters:
        //		strPureFileName	要打开的XML文件名，注意这是一个纯文件名，不包含路径部分。本函数自动从模块的当前目录中装载此文件。
        public ApplicationInfo(string strPureFileName)
        {
            PrepareFileName(strPureFileName);

            string strErrorInfo;
            int nRet = Load(out strErrorInfo);
            if (nRet < 0)
            {
                CreateBlank();
            }
        }

        // 将内存中的内容保存回XML文件
        public void Save()
        {
            if (FileName != "")
            {
                Save(out string strErrorInfo);
            }
        }

        // parameters:
        //      strFileName 文件名字符串。如果是纯文件名，则自动按照 ClickOnce 安装和绿色安装获得数据目录；如果是全路径，则直接使用这个路径
        public void PrepareFileName(string strFileName)
        {
            string strPureName = Path.GetFileName(strFileName);
            if (strPureName.ToUpper() != strFileName.ToUpper())
            {
                this.PureFileName = strPureName;
                this.FileName = strFileName;
            }
            else
            {
                this.PureFileName = strFileName;

                if (ApplicationDeployment.IsNetworkDeployed == true)
                {
                    this.CurrentDirectory = Application.LocalUserAppDataPath;
                }
                else
                {
                    this.CurrentDirectory = Environment.CurrentDirectory;
                }

                this.FileName = Path.Combine(this.CurrentDirectory, PureFileName);
            }
        }

        // 从文件中装载信息
        public int Load(out string strErrorInfo)
        {
            this.dom.PreserveWhitespace = true;

            strErrorInfo = "";

            if (FileName == "")
            {
                strErrorInfo = "FileName为空...";
                return -1;
            }

            try
            {
                dom.Load(FileName);
            }
            catch (FileNotFoundException ex)
            {
                strErrorInfo = "文件没有找到: " + ex.Message;
                return -2;
            }
            catch (XmlException ex)
            {
                strErrorInfo = "装载文件 " + FileName + "时出错:" + ex.Message;
                return -1;
            }

            return 0;
        }

        public int CreateBlank()
        {
            dom.LoadXml("<?xml version='1.0' encoding='utf-8' ?><root/>");
            return 0;
        }

        public int Save(out string strErrorInfo)
        {
            strErrorInfo = "";

            if (string.IsNullOrEmpty(FileName) == true)
            {
                strErrorInfo = "FileName为空...";
                return -1;
            }

            // 2018/6/25
            // 先把以前的文件改名作为备份保存
            string strTempFileName = FileName + ".save";
            FileInfo fi = new FileInfo(FileName);
            if (File.Exists(FileName) && fi.Length != 0)
                File.Copy(FileName, strTempFileName, true);

            dom.Save(FileName);
            return 0;
        }

        // Hashtable _cacheTable = new Hashtable();    // path --> string value

        // 获得一个布尔值
        // parameters:
        //		strPath	参数路径
        //		strName	参数名
        //		bDefault	缺省值
        // return:
        //		所获得的布尔值
        public bool GetBoolean(string strPath,
            string strName,
            bool bDefault)
        {
#if NO
            strPath = GetSectionPath(strPath);

            XmlNode node = dom.SelectSingleNode(strPath);
            string strText = null;

            if (node == null)
                return bDefault;

            strText = DomUtil.GetAttrOrDefault(node, strName, null);
            if (strText == null)
                return bDefault;

            if (String.Compare(strText, "true", true) == 0)
                return true;

            if (String.Compare(strText, "false", true) == 0)
                return false;

            return false;
#endif
            string value = GetString(strPath,
strName,
null);
            if (value == null)
                return bDefault;

            if (String.Compare(value, "true", true) == 0)
                return true;

            if (String.Compare(value, "false", true) == 0)
                return false;

            return false;
        }


        // 写入一个布尔值
        // parameters:
        //		strPath	参数路径
        //		strName	参数名
        //		bValue	要写入的布尔值
        public void SetBoolean(string strPath,
            string strName,
            bool bValue)
        {
#if NO
            strPath = GetSectionPath(strPath);

            string[] aPath = strPath.Split(new char[] { '/' });
            XmlNode node = DomUtil.CreateNode(dom, aPath);

            if (node == null)
            {
                throw (new Exception("SetInt() set error ..."));
            }

            DomUtil.SetAttr(node,
                strName,
                (bValue == true ? "true" : "false"));
#endif
            SetString(strPath,
strName,
(bValue == true ? "true" : "false"));
        }

        //
        static string GetSectionPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                throw new ArgumentException("strPath 参数值不应为空");
            if (char.IsDigit(strPath[0]) == true)
                return "/root/n_" + strPath;
            return "/root/" + strPath;
        }

        // 获得一个整数值
        // parameters:
        //		strPath	参数路径
        //		strName	参数名
        //		nDefault	缺省值
        // return:
        //		所获得的整数值
        public int GetInt(string strPath,
            string strName,
            int nDefault)
        {
#if NO
			strPath = GetSectionPath(strPath);

            XmlNode node = null;
            try
            {
                node = dom.SelectSingleNode(strPath);
            }
            catch(Exception ex)
            {
                throw new Exception("strPath 名称 '"+strPath+"' 不合法。应符合 XML 元素命名规则", ex);
            }
			string strText = null;

			if (node == null)
				return nDefault;

			strText = DomUtil.GetAttrOrDefault(node, strName, null);
			if (strText == null)
				return nDefault;

			return Convert.ToInt32(strText);
#endif
            string value = GetString(strPath,
            strName,
            null);
            if (value == null)
                return nDefault;
            return Convert.ToInt32(value);
        }

        // 写入一个整数值
        // parameters:
        //		strPath	参数路径
        //		strName	参数名
        //		nValue	要写入的整数值
        public void SetInt(string strPath,
            string strName,
            int nValue)
        {
#if NO
            strPath = GetSectionPath(strPath);

            string[] aPath = strPath.Split(new char[] { '/' });
            XmlNode node = DomUtil.CreateNode(dom, aPath);

            if (node == null)
            {
                throw (new Exception("SetInt() set error ..."));
            }

            DomUtil.SetAttr(node,
                strName,
                Convert.ToString(nValue));
#endif
            SetString(strPath,
strName,
Convert.ToString(nValue));
        }

        // 获得一个字符串
        // parameters:
        //		strPath	参数路径
        //		strName	参数名
        //		strDefalt	缺省值
        // return:
        //		要获得的字符串
        public string GetString(string strPath,
            string strName,
            string strDefault)
        {
            strPath = GetSectionPath(strPath);

            XmlNode node = null;

            try
            {
                node = dom.SelectSingleNode(strPath);
            }
            catch (Exception ex)
            {
                throw new Exception("strPath 名称 '" + strPath + "' 不合法。应符合 XML 元素命名规则", ex);
            }

            if (node == null)
                return strDefault;

            return DomUtil.GetAttrOrDefault(node, strName, strDefault);
        }

        // 设置一个字符串
        // parameters:
        //		strPath	参数路径
        //		strName	参数名
        //		strValue	要设置的字符串，如果为null，表示删除这个事项
        public void SetString(string strPathParam,
            string strName,
            string strValue)
        {
            string strPath = GetSectionPath(strPathParam);

            string[] aPath = strPath.Split(new char[] { '/' });
            XmlNode node = DomUtil.CreateNode(dom, aPath);

            if (node == null)
            {
                throw (new Exception("SetString() error ..."));
            }

            DomUtil.SetAttr(node,
                strName,
                strValue);

            var handler = this.AppInfoChanged;
            if (handler != null)
            {
                AppInfoChangedEventArgs e = new AppInfoChangedEventArgs
                {
                    Path = strPathParam,
                    Name = strName,
                    Value = strValue
                };
                handler(this, e);
            }
        }

        ////
        // 获得一个浮点数
        // parameters:
        //		strPath	参数路径
        //		strName	参数名
        //		fDefault	缺省值
        // return:
        //		要获得的字符串
        public float GetFloat(string strPath,
            string strName,
            float fDefault)
        {
#if NO
            strPath = GetSectionPath(strPath);

            XmlNode node = dom.SelectSingleNode(strPath);

            if (node == null)
                return fDefault;

            string strDefault = fDefault.ToString();

            string strValue = DomUtil.GetAttrOrDefault(node, 
                strName,
                strDefault);

            try
            {
                return (float)Convert.ToDouble(strValue);
            }
            catch
            {
                return fDefault;
            }
#endif
            string value = GetString(strPath,
strName,
null);
            if (value == null)
                return fDefault;

            try
            {
                return (float)Convert.ToDouble(value);
            }
            catch
            {
                return fDefault;
            }
        }


        // 设置一个浮点数
        // parameters:
        //		strPath	参数路径
        //		strName	参数名
        //		fValue	要设置的字符串
        public void SetFloat(string strPath,
            string strName,
            float fValue)
        {
#if NO
            strPath = GetSectionPath(strPath);

            string[] aPath = strPath.Split(new char[] { '/' });
            XmlNode node = DomUtil.CreateNode(dom, aPath);

            if (node == null)
            {
                throw (new Exception("SetString() error ..."));
            }

            DomUtil.SetAttr(node,
                strName,
                fValue.ToString());
#endif
            SetString(strPath,
            strName,
            fValue.ToString());
        }

        // 包装后的版本
        public void LoadFormStates(Form form,
            string strCfgTitle)
        {
            LoadFormStates(form, strCfgTitle, FormWindowState.Normal);
        }

        // 从ApplicationInfo中读取信息，设置form尺寸位置状态
        // parameters:
        //		form	Form对象
        //		strCfgTitle	配置信息路径。本函数将用此值作为GetString()或GetInt()的strPath参数使用
        public void LoadFormStates(Form form,
            string strCfgTitle,
            FormWindowState default_state)
        {
            // 为了优化视觉效果
            bool bVisible = form.Visible;

            if (bVisible == true)
                form.Visible = false;

            form.Width = this.GetInt(
                strCfgTitle, "width", form.Width);
            form.Height = this.GetInt(
                strCfgTitle, "height", form.Height);

            form.Location = new Point(
                this.GetInt(strCfgTitle, "x", form.Location.X),
                this.GetInt(strCfgTitle, "y", form.Location.Y));

            string strState = this.GetString(
                strCfgTitle,
                "window_state",
                "");
            if (String.IsNullOrEmpty(strState) == true)
            {
                form.WindowState = default_state;
            }
            else
            {
                form.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState),
                    strState);
            }

            if (bVisible == true)
                form.Visible = true;

            /// form.Update();  // 2007/4/8
        }

        // 装载MDI子窗口的最大化特性。需要在至少一个MDI子窗口打开后调用
        public void LoadFormMdiChildStates(Form form,
            string strCfgTitle)
        {
            if (form.ActiveMdiChild == null)
                return;

            string strState = this.GetString(
                strCfgTitle,
                "mdi_child_window_state",
                "");
            if (String.IsNullOrEmpty(strState) == false)
            {
                form.ActiveMdiChild.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState),
                    strState);
            }
        }

        public void LoadMdiChildFormStates(Form form,
            string strCfgTitle)
        {
            LoadMdiChildFormStates(form,
                strCfgTitle,
                SizeStyle.All);
        }

        // 包装后的版本
        public void LoadMdiChildFormStates(Form form,
            string strCfgTitle,
            SizeStyle style)
        {
            // 默认值可以按照屏幕分辨率估算
            int nDefaultWidth = (int)(((double)Screen.PrimaryScreen.WorkingArea.Width) * 0.5);
            int nDefaultHeight = (int)(((double)Screen.PrimaryScreen.WorkingArea.Height) * 0.7);

            LoadMdiChildFormStates(form,
                strCfgTitle,
                style,
                nDefaultWidth,  // 600,
                nDefaultHeight  // 400
                );
        }

        // http://blogs.msdn.com/b/rprabhu/archive/2005/11/28/497792.aspx
        // 2015/10/12 优化
        // 从ApplicationInfo中读取信息，设置MDI Child form尺寸位置状态
        // 和一般Form的区别是,不修改x,y信息
        // parameters:
        //		form	Form对象
        //		strCfgTitle	配置信息路径。本函数将用此值作为GetString()或GetInt()的strPath参数使用
        //      strStyle    size/layout 之一或者组合
        public void LoadMdiChildFormStates(Form form,
            string strCfgTitle,
            SizeStyle style,
            int nDefaultWidth,
            int nDefaultHeight)
        {
            if ((style & SizeStyle.Size) != 0)
            {
                form.Size = new Size(this.GetInt(
                    strCfgTitle, "width", nDefaultWidth),
                    this.GetInt(
                    strCfgTitle, "height", nDefaultHeight));
            }

            if ((style & SizeStyle.Layout) != 0)
            {
                if (this.LoadMdiLayout != null)
                    this.LoadMdiLayout(form, null);
            }
        }

#if NO
		// 从ApplicationInfo中读取信息，设置MDI Child form尺寸位置状态
		// 和一般Form的区别是,不修改x,y信息
		// parameters:
		//		form	Form对象
		//		strCfgTitle	配置信息路径。本函数将用此值作为GetString()或GetInt()的strPath参数使用
		public void LoadMdiChildFormStates(Form form,
			string strCfgTitle,
            int nDefaultWidth,
            int nDefaultHeight)
		{
            // 2009/11/9
            FormWindowState savestate = form.WindowState;
            bool bStateChanged = false;
            if (form.WindowState != FormWindowState.Normal)
            {
                form.WindowState = FormWindowState.Normal;
                bStateChanged = true;
            }
			form.Width = this.GetInt(
                strCfgTitle, "width", nDefaultWidth);
			form.Height = this.GetInt(
                strCfgTitle, "height", nDefaultHeight);

            if (this.LoadMdiSize != null)
                this.LoadMdiSize(form, null);

            // 2009/11/9
            if (bStateChanged == true)
                form.WindowState = savestate;
		}
#endif

        public void SaveMdiChildFormStates(Form form,
            string strCfgTitle)
        {
            SaveMdiChildFormStates(form,
                strCfgTitle,
                SizeStyle.All);
        }
        // 保存Mdi Child form尺寸位置状态到ApplicationInfo中
        // parameters:
        //		form	Form对象
        //		strCfgTitle	配置信息路径。本函数将用此值作为SetString()或SetInt()的strPath参数使用
        public void SaveMdiChildFormStates(Form form,
            string strCfgTitle,
            SizeStyle style)
        {
            if ((style & SizeStyle.Size) != 0)
            {

                FormWindowState savestate = form.WindowState;

                Size size = form.Size;
                Point location = form.Location;

                if (form.WindowState != FormWindowState.Normal)
                {
                    size = form.RestoreBounds.Size;
                    location = form.RestoreBounds.Location;
                }

                this.SetInt(strCfgTitle, "width", size.Width);
                this.SetInt(strCfgTitle, "height", size.Height);

                this.SetInt(strCfgTitle, "x", location.X);
                this.SetInt(strCfgTitle, "y", location.Y);
            }

            if ((style & SizeStyle.Layout) != 0)
            {
                if (this.SaveMdiLayout != null)
                    this.SaveMdiLayout(form, null);
            }
        }

        // 保存form尺寸位置状态到ApplicationInfo中
        // parameters:
        //		form	Form对象
        //		strCfgTitle	配置信息路径。本函数将用此值作为SetString()或SetInt()的strPath参数使用
        public void SaveFormStates(Form form,
            string strCfgTitle)
        {
            // 保存窗口状态
            this.SetString(
                strCfgTitle, "window_state",
                Enum.GetName(typeof(FormWindowState),
                form.WindowState));

            Size size = form.Size;
            Point location = form.Location;

            if (form.WindowState != FormWindowState.Normal)
            {
                size = form.RestoreBounds.Size;
                location = form.RestoreBounds.Location;
            }

#if NO
            // 尺寸
            form.WindowState = FormWindowState.Normal;	// 是否先隐藏窗口?
#endif

            this.SetInt(
                strCfgTitle, "width", size.Width);  // form.Width
            this.SetInt(
                strCfgTitle, "height", size.Height);    // form.Height

            this.SetInt(strCfgTitle, "x", location.X); // form.Location.X
            this.SetInt(strCfgTitle, "y", location.Y); // form.Location.Y

            // 保存MDI窗口状态 -- 是否最大化？
            if (form.ActiveMdiChild != null)
            {
                if (form.ActiveMdiChild.WindowState == FormWindowState.Minimized)
                    this.SetString(
                        strCfgTitle,
                        "mdi_child_window_state",
                        Enum.GetName(typeof(FormWindowState),
                        FormWindowState.Normal));
                else
                    this.SetString(
                        strCfgTitle,
                        "mdi_child_window_state",
                        Enum.GetName(typeof(FormWindowState),
                        form.ActiveMdiChild.WindowState));
            }
            else
            {
                this.SetString(
                    strCfgTitle,
                    "mdi_child_window_state",
                    "");
            }
        }

        // 将本对象和Form建立联系，当Form Load和Closed阶段，会自动触发本类
        // 的相关事件函数，恢复和保存Form尺寸位置等状态。
        // parameters:
        //		form	Form对象
        //		strCfgTitle	配置信息路径。本函数将用此值作为相关GetString()或GetInt()的strPath参数使用
        public void LinkFormState(Form form,
            string strCfgTitle)
        {
            if (titleTable == null)
                titleTable = new Hashtable();

            // titleTable.Add(form, strCfgTitle);
            titleTable[form] = strCfgTitle; // 重复加入不会抛出异常

            form.Load += new System.EventHandler(this.FormLoad);
            form.Closed += new System.EventHandler(this.FormClosed);
        }

        // 原来外部主动调用一次本函数的做法没有必要了。正确的做法是，调用 LinkFormState() 即可，对话框关闭时会自动保存好尺寸
        public void UnlinkFormState(Form form)
        {
            if (titleTable == null)
                return;

            titleTable.Remove(form);
            // If the Hashtable does not contain an element with the specified key,
            // the Hashtable remains unchanged. No exception is thrown.

            // 2015/6/5
            form.Load -= new System.EventHandler(this.FormLoad);
            form.Closed -= new System.EventHandler(this.FormClosed);
        }

        private void FormLoad(object sender, System.EventArgs e)
        {
            Debug.Assert(sender != null, "sender不能为null");
            Debug.Assert(sender is Form, "sender应为Form对象");

            Debug.Assert(titleTable != null, "titleTable应当已经被LinkFromState()初始化");

            string strCfgTitle = (string)titleTable[sender];
            Debug.Assert(strCfgTitle != null, "strCfgTitle不能为null");

            this.LoadFormStates((Form)sender, strCfgTitle);
        }

        private void FormClosed(object sender, System.EventArgs e)
        {
            Debug.Assert(sender != null, "sender不能为null");
            Debug.Assert(sender is Form, "sender应为Form对象");

            Debug.Assert(titleTable != null, "titleTable应当已经被LinkFromState()初始化");

            string strCfgTitle = (string)titleTable[sender];
            if (string.IsNullOrEmpty(strCfgTitle) == true)
                return;

            Debug.Assert(strCfgTitle != null, "strCfgTitle不能为null");

            this.SaveFormStates((Form)sender, strCfgTitle);

            this.Save();
            this.UnlinkFormState((Form)sender);
        }
    }

    [Flags]
    public enum SizeStyle
    {
        Size = 0x01,
        Layout = 0x02,
        All = Size | Layout,
    }

    // AppInfo 值发生改变的事件
    public delegate void AppInfoChangedEventHandler(object sender,
AppInfoChangedEventArgs e);

    public class AppInfoChangedEventArgs : EventArgs
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
