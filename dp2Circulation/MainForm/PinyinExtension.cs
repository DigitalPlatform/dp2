using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

#if GCAT_SERVER
using DigitalPlatform.GcatClient;
using DigitalPlatform.GcatClient.gcat_new_ws;
#endif
using DigitalPlatform;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关拼音的功能
    /// </summary>
    public partial class MainForm
    {
        #region 次要通道池

        internal void ChannelExt_BeforeLogin(object sender,
    DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;

            if (e.FirstTry == true)
            {
                if (channel != null)
                {
                    e.UserName = channel.UserName;
                    e.Password = channel.Password;
                }

                // 2014/9/13
                e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

                e.Parameters += ",client=dp2circulation|" + Program.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            e.ErrorInfo = "不允许再次登录";
            e.Cancel = true;
        }

        internal void ChannelExt_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            // LibraryChannel channel = sender as LibraryChannel;
        }

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetExtChannel(string strServerUrl,
            string strUserName,
            GetChannelStyle style = GetChannelStyle.GUI)
        {
            LibraryChannel channel = this._channelPoolExt.GetChannel(strServerUrl, strUserName);
            if ((style & GetChannelStyle.GUI) != 0)
                channel.Idle += channelExt_Idle;
            _channelList.Add(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

        void channelExt_Idle(object sender, IdleEventArgs e)
        {
            Application.DoEvents();
        }

        public void ReturnExtChannel(LibraryChannel channel)
        {
            channel.Idle -= channel_Idle;

            this._channelPoolExt.ReturnChannel(channel);
            _channelList.Remove(channel);
        }

        #endregion

        #region 为汉字加拼音相关功能

        // 把字符串中的汉字转换为四角号码
        // parameters:
        //      bLocal  是否从本地获取四角号码
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        /// <summary>
        /// 把字符串中的汉字转换为四角号码
        /// </summary>
        /// <param name="bLocal">是否从本地获取四角号码</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="sjhms">返回四角号码字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常</returns>
        public int HanziTextToSjhm(
            bool bLocal,
            string strText,
            out List<string> sjhms,
            out string strError)
        {
            strError = "";
            sjhms = new List<string>();

            // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                if (StringUtil.IsHanzi(ch) == false)
                    continue;

                // 看看是否特殊符号
                if (StringUtil.SpecialChars.IndexOf(ch) != -1)
                    continue;

                // 汉字
                string strHanzi = "";
                strHanzi += ch;


                string strResultSjhm = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.LoadQuickSjhm(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.QuickSjhm.GetSjhm(
                        strHanzi,
                        out strResultSjhm,
                        out strError);
                }
                else
                {
                    throw new Exception("暂不支持从拼音库中获取四角号码");
                    /*
                    nRet = GetOnePinyin(strHanzi,
                         out strResultPinyin,
                         out strError);
                     * */
                }
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {	// canceled
                    return 0;
                }

                Debug.Assert(strResultSjhm != "", "");

                strResultSjhm = strResultSjhm.Trim();
                sjhms.Add(strResultSjhm);
            }

            return 1;   // 正常结束
        }

#if GCAT_SERVER
        GcatServiceClient m_gcatClient = null;
        string m_strPinyinGcatID = "";
        bool m_bSavePinyinGcatID = false;
#endif

        // 汉字字符串转换为拼音。兼容以前版本
        // 如果函数中已经MessageBox报错，则strError第一字符会为空格
        /// <summary>
        /// 汉字字符串转换为拼音，智能方式
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int SmartHanziTextToPinyin(
            IWin32Window owner,
            string strText,
            PinyinStyle style,
            out string strPinyin,
            out string strError)
        {
            return SmartHanziTextToPinyin(
                owner,
                strText,
                style,
                false,
                out strPinyin,
                out strError);
        }

        /// <summary>
        /// 汉字字符串转换为拼音，智能方式
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="bAutoSel">是否自动选择多音字</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int SmartHanziTextToPinyin(
IWin32Window owner,
string strText,
PinyinStyle style,
bool bAutoSel,
out string strPinyin,
out string strError)
        {
            return SmartHanziTextToPinyin(owner,
                strText,
                style,
               (bAutoSel ? "auto" : ""),
                out strPinyin,
                out strError);
        }

        // 汉字字符串转换为拼音。新版本
        // 如果函数中已经MessageBox报错，则strError第一字符会为空格
        /// <summary>
        /// 汉字字符串转换为拼音，智能方式
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="strDuoyinStyle">是否自动选择多音字。auto/first 的一个或者组合。如果为 auto,first 表示优先按照智能拼音选择，没有智能拼音的，选择第一个</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int SmartHanziTextToPinyin(
            IWin32Window owner,
            string strText,
            PinyinStyle style,
            string strDuoyinStyle,  // bool bAutoSel,
            out string strPinyin,
            out string strError)
        {
            strPinyin = "";
            strError = "";

            bool bAuto = StringUtil.IsInList("auto", strDuoyinStyle);
            bool bFirst = StringUtil.IsInList("first", strDuoyinStyle);

            bool bNotFoundPinyin = false;   // 是否出现过没有找到拼音、只能把汉字放入结果字符串的情况

#if !GCAT_SERVER
            string strPinyinServerUrl = this.PinyinServerUrl;
            if (string.IsNullOrEmpty(strPinyinServerUrl) == false
                && strPinyinServerUrl.Contains("gcat"))
            {
                strError = "请重新配置拼音服务器 URL。当前的配置 '" + strPinyinServerUrl + "' 已过时。可配置为 http://dp2003.com/dp2library";
                return -1;
            }
            LibraryChannel channel = this.GetExtChannel(strPinyinServerUrl, "public");
#endif

            Stop new_stop = new DigitalPlatform.Stop();
            new_stop.Register(this.stopManager, true);	// 和容器关联
#if GCAT_SERVER
            new_stop.OnStop += new StopEventHandler(new_stop_OnStop);
#else
            new_stop.OnStop += new StopEventHandler(this.DoStop);
#endif
            new_stop.Initial("正在获得 '" + strText + "' 的拼音信息 (从服务器 " + this.PinyinServerUrl + ")...");
            new_stop.BeginLoop();

#if GCAT_SERVER
            m_gcatClient = null;
#endif

            try
            {

#if GCAT_SERVER
                m_gcatClient = GcatNew.CreateChannel(this.PinyinServerUrl);
            REDO_GETPINYIN:
#endif

                //int nStatus = -1;	// 前面一个字符的类型 -1:前面没有字符 0:普通英文字母 1:空格 2:汉字
#if GCAT_SERVER

                // return:
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                long lRet = GcatNew.GetPinyin(
                    new_stop,
                    m_gcatClient,
                    m_strPinyinGcatID,
                    strText,
                    out strPinyinXml,
                    out strError);
#else
                // return:
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                long lRet = channel.GetPinyin(
                    "pinyin",
                    strText,
                    out string strPinyinXml,
                    out strError);
#endif
                if (lRet == -1)
                {
#if GCAT_SERVER
                    if (new_stop != null && new_stop.State != 0)
                        return 0;
#endif

                    DialogResult result = MessageBox.Show(owner,
    "从服务器 '" + this.PinyinServerUrl + "' 获取拼音的过程出错:\r\n" + strError + "\r\n\r\n是否要临时改为使用本机加拼音功能? \r\n\r\n(注：临时改用本机拼音的状态在程序退出时不会保留。如果要永久改用本机拼音方式，请使用主菜单的“参数配置”命令，将“服务器”属性页的“拼音服务器URL”内容清空)",
    "EntityForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        this.ForceUseLocalPinyinFunc = true;
                        strError = "将改用本机拼音，请重新操作一次。(本次操作出错: " + strError + ")";
                        return -1;
                    }
                    strError = " " + strError;
                    return -1;
                }

#if GCAT_SERVER
                if (lRet == -2)
                {
                    IdLoginDialog login_dlg = new IdLoginDialog();
                    login_dlg.Text = "获得拼音 -- "
                        + ((string.IsNullOrEmpty(this.m_strPinyinGcatID) == true) ? "请输入ID" : strError);
                    login_dlg.ID = this.m_strPinyinGcatID;
                    login_dlg.SaveID = this.m_bSavePinyinGcatID;
                    login_dlg.StartPosition = FormStartPosition.CenterScreen;
                    if (login_dlg.ShowDialog(owner) == DialogResult.Cancel)
                    {
                        return 0;
                    }

                    this.m_strPinyinGcatID = login_dlg.ID;
                    this.m_bSavePinyinGcatID = login_dlg.SaveID;
                    goto REDO_GETPINYIN;
                }
#endif

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strPinyinXml);
                }
                catch (Exception ex)
                {
                    strError = "strPinyinXml装载到XMLDOM时出错: " + ex.Message;
                    return -1;
                }

                foreach (XmlNode nodeWord in dom.DocumentElement.ChildNodes)
                {
                    if (nodeWord.NodeType == XmlNodeType.Text)
                    {
                        SelPinyinDlg.AppendText(ref strPinyin, nodeWord.InnerText);
                        //nStatus = 0;
                        continue;
                    }

                    if (nodeWord.NodeType != XmlNodeType.Element)
                        continue;

                    string strWordPinyin = DomUtil.GetAttr(nodeWord, "p");
                    if (string.IsNullOrEmpty(strWordPinyin) == false)
                        strWordPinyin = strWordPinyin.Trim();

                    // 目前只取多套读音的第一套
                    int nRet = strWordPinyin.IndexOf(";");
                    if (nRet != -1)
                        strWordPinyin = strWordPinyin.Substring(0, nRet).Trim();

                    string[] pinyin_parts = strWordPinyin.Split(new char[] { ' ' });
                    int index = 0;
                    // 让选择多音字
                    foreach (XmlNode nodeChar in nodeWord.ChildNodes)
                    {
                        if (nodeChar.NodeType == XmlNodeType.Text)
                        {
                            SelPinyinDlg.AppendText(ref strPinyin, nodeChar.InnerText);
                            //nStatus = 0;
                            continue;
                        }

                        string strHanzi = nodeChar.InnerText;
                        string strCharPinyins = DomUtil.GetAttr(nodeChar, "p");

                        if (String.IsNullOrEmpty(strCharPinyins) == true)
                        {
                            strPinyin += strHanzi;
                            //nStatus = 0;
                            index++;
                            continue;
                        }

                        if (strCharPinyins.IndexOf(";") == -1)
                        {
                            DomUtil.SetAttr(nodeChar, "sel", strCharPinyins);
                            SelPinyinDlg.AppendPinyin(ref strPinyin,
                                SelPinyinDlg.ConvertSinglePinyinByStyle(
                                    strCharPinyins,
                                    style)
                                    );
                            //nStatus = 2;
                            index++;
                            continue;
                        }

#if _TEST_PINYIN
                        // 调试！
                        string[] parts = strCharPinyins.Split(new char[] {';'});
                        {
                            DomUtil.SetAttr(nodeChar, "sel", parts[0]);
                            AppendPinyin(ref strPinyin, parts[0]);
                            nStatus = 2;
                            index++;
                            continue;
                        }
#endif


                        int nOffs = -1;
                        SelPinyinDlg.GetOffs(dom.DocumentElement,
                            nodeChar,
                            out string strSampleText,
                            out nOffs);

                        {	// 如果是多个拼音
                            SelPinyinDlg dlg = new SelPinyinDlg();
                            //float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            //float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            MainForm.SetControlFont(dlg, this.Font, false);
                            // 维持字体的原有大小比例关系
                            //dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            //dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            // 这个对话框比较特殊 MainForm.SetControlFont(dlg, this.Font, false);

                            dlg.Text = "请选择汉字 '" + strHanzi + "' 的拼音 (来自服务器 " + this.PinyinServerUrl + ")";
                            dlg.SampleText = strSampleText;
                            dlg.Offset = nOffs;
                            dlg.Pinyins = strCharPinyins;
                            if (index < pinyin_parts.Length)
                                dlg.ActivePinyin = pinyin_parts[index];
                            dlg.Hanzi = strHanzi;

#if NO
                            if (bAutoSel == true
                                && string.IsNullOrEmpty(dlg.ActivePinyin) == false)
                            {
                                dlg.ResultPinyin = dlg.ActivePinyin;
                                dlg.DialogResult = DialogResult.OK;
                            }
                            else
                            {
                                this.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                                dlg.ShowDialog(owner);

                                this.AppInfo.UnlinkFormState(dlg);
                            }
#endif
                            if (bAuto == true
    && string.IsNullOrEmpty(dlg.ActivePinyin) == false)
                            {
                                dlg.ResultPinyin = dlg.ActivePinyin;
                                dlg.DialogResult = DialogResult.OK;
                            }
                            else if (bFirst == true
                                && string.IsNullOrEmpty(dlg.Pinyins) == false)
                            {
                                dlg.ResultPinyin = SelPinyinDlg.GetFirstPinyin(dlg.Pinyins);
                                dlg.DialogResult = DialogResult.OK;
                            }
                            else
                            {
                                this.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                                dlg.ShowDialog(owner);

                                this.AppInfo.UnlinkFormState(dlg);
                            }

                            Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "推断");

                            if (dlg.DialogResult == DialogResult.Abort)
                            {
                                return 0;   // 用户希望整个中断
                            }

                            DomUtil.SetAttr(nodeChar, "sel", dlg.ResultPinyin);

                            if (dlg.DialogResult == DialogResult.Cancel)
                            {
                                SelPinyinDlg.AppendText(ref strPinyin, strHanzi);
                                //nStatus = 2;
                                bNotFoundPinyin = true;
                            }
                            else if (dlg.DialogResult == DialogResult.OK)
                            {
                                SelPinyinDlg.AppendPinyin(ref strPinyin,
                                    SelPinyinDlg.ConvertSinglePinyinByStyle(
                                    dlg.ResultPinyin,
                                    style)
                                    );
                                //nStatus = 2;
                            }
                            else
                            {
                                Debug.Assert(false, "SelPinyinDlg返回时出现意外的DialogResult值");
                            }

                            index++;
                        }
                    }
                }

#if _TEST_PINYIN
#else
                // 2014/10/22
                // 删除 word 下的 Text 节点
                XmlNodeList text_nodes = dom.DocumentElement.SelectNodes("word/text()");
                foreach (XmlNode node in text_nodes)
                {
                    Debug.Assert(node.NodeType == XmlNodeType.Text, "");
                    node.ParentNode.RemoveChild(node);
                }

                // 把没有p属性的<char>元素去掉，以便上传
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//char");
                foreach (XmlNode node in nodes)
                {
                    string strP = DomUtil.GetAttr(node, "p");
                    string strSelValue = DomUtil.GetAttr(node, "sel");  // 2013/9/13

                    if (string.IsNullOrEmpty(strP) == true
                        || string.IsNullOrEmpty(strSelValue) == true)
                    {
                        XmlNode parent = node.ParentNode;
                        parent.RemoveChild(node);

                        // 把空的<word>元素删除
                        if (parent.Name == "word"
                            && parent.ChildNodes.Count == 0
                            && parent.ParentNode != null)
                        {
                            parent.ParentNode.RemoveChild(parent);
                        }
                    }

                    // TODO: 一个拼音，没有其他选择的，是否就不上载了？
                    // 注意，前端负责新创建的拼音仍需上载；只是当初原样从服务器过来的，不用上载了
                }

                if (dom.DocumentElement.ChildNodes.Count > 0)
                {
#if GCAT_SERVER
                    // return:
                    //      -2  strID验证失败
                    //      -1  出错
                    //      0   成功
                    lRet = GcatNew.SetPinyin(
                        new_stop,
                        m_gcatClient,
                        "",
                        dom.DocumentElement.OuterXml,
                        out strError);
                    if (lRet == -1)
                    {
                        if (new_stop != null && new_stop.State != 0)
                            return 0;
                        return -1;
                    }
#else
                    // return:
                    //      -1  出错
                    //      0   成功
                    lRet = channel.SetPinyin(
                        dom.DocumentElement.OuterXml,
                        out strError);
                    if (lRet == -1)
                        return -1;
#endif

                }
#endif

                if (bNotFoundPinyin == false)
                    return 1;   // 正常结束

                return 2;   // 结果字符串中有没有找到拼音的汉字
            }
            finally
            {
#if !GCAT_SERVER
                this.ReturnExtChannel(channel);
#endif
                new_stop.EndLoop();
#if GCAT_SERVER
                new_stop.OnStop -= new StopEventHandler(new_stop_OnStop);
#else
                new_stop.OnStop -= new StopEventHandler(this.DoStop);
#endif
                new_stop.Initial("");
                new_stop.Unregister();
#if GCAT_SERVER
                if (m_gcatClient != null)
                {
                    m_gcatClient.Close();
                    m_gcatClient = null;
                }
#endif
            }
        }

#if GCAT_SERVER
        void new_stop_OnStop(object sender, StopEventArgs e)
        {
            if (this.m_gcatClient != null)
            {
                this.m_gcatClient.Abort();
            }
        }
#endif

        // 2015/7/20
        // 包装后的版本
        public int HanziTextToPinyin(
    IWin32Window owner,
    bool bLocal,
    string strText,
    PinyinStyle style,
    bool bAutoSel,
    out string strPinyin,
    out string strError)
        {
            return HanziTextToPinyin(
    owner,
    bLocal,
    strText,
    style,
    (bAutoSel ? "auto" : ""),
    out strPinyin,
    out strError);
        }

        // 把字符串中的汉字和拼音分离
        // parameters:
        //      bLocal  是否从本机获取拼音
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        /// <summary>
        /// 汉字字符串转换为拼音，普通方式
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="bLocal">是否从本地获取拼音信息</param>
        /// <param name="strText">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="strDuoyinStyle">处理多音字的风格</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int HanziTextToPinyin(
            IWin32Window owner,
            bool bLocal,
            string strText,
            PinyinStyle style,
            string strDuoyinStyle,  // 2015/7/20
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";

            bool bAuto = StringUtil.IsInList("auto", strDuoyinStyle);
            bool bFirst = StringUtil.IsInList("first", strDuoyinStyle);

            // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";
            bool bNotFoundPinyin = false;   // 是否出现过没有找到拼音、只能把汉字放入结果字符串的情况
            string strHanzi;
            int nStatus = -1;	// 前面一个字符的类型 -1:前面没有字符 0:普通英文字母 1:空格 2:汉字

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                strHanzi = "";

                if (ch >= 0 && ch <= 128)
                {
                    if (nStatus == 2)
                        strPinyin += " ";

                    strPinyin += ch;

                    if (ch == ' ')
                        nStatus = 1;
                    else
                        nStatus = 0;

                    continue;
                }
                else
                {	// 汉字
                    strHanzi += ch;
                }

                // 汉字前面出现了英文或者汉字，中间间隔空格
                if (nStatus == 2 || nStatus == 0)
                    strPinyin += " ";

                // 看看是否特殊符号
                if (StringUtil.SpecialChars.IndexOf(strHanzi) != -1)
                {
                    strPinyin += strHanzi;	// 放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }

                // 获得拼音
                string strResultPinyin = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.LoadQuickPinyin(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.QuickPinyin.GetPinyin(
                        strHanzi,
                        out strResultPinyin,
                        out strError);
                }
                else
                {
                    throw new Exception("暂不支持从拼音库中获取拼音");
                    /*
                    nRet = GetOnePinyin(strHanzi,
                         out strResultPinyin,
                         out strError);
                     * */
                }
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    // canceled
                    strPinyin += strHanzi;	// 只好将汉字放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }

                Debug.Assert(strResultPinyin != "", "");

                strResultPinyin = strResultPinyin.Trim();
                if (strResultPinyin.IndexOf(";", 0) != -1)
                {	// 如果是多个拼音
                    SelPinyinDlg dlg = new SelPinyinDlg();
                    //float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    //float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    MainForm.SetControlFont(dlg, this.Font, false);
                    // 维持字体的原有大小比例关系
                    //dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                    //dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                    // 这个对话框比较特殊 MainForm.SetControlFont(dlg, this.Font, false);

                    dlg.Text = "请选择汉字 '" + strHanzi + "' 的拼音 (来自本机)";
                    dlg.SampleText = strText;
                    dlg.Offset = i;
                    dlg.Pinyins = strResultPinyin;
                    dlg.Hanzi = strHanzi;

                    if (bFirst == true
&& string.IsNullOrEmpty(dlg.Pinyins) == false)
                    {
                        dlg.ResultPinyin = SelPinyinDlg.GetFirstPinyin(dlg.Pinyins);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        this.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                        dlg.ShowDialog(owner);

                        this.AppInfo.UnlinkFormState(dlg);
                    }

                    Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "推断");

                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        strPinyin += strHanzi;
                        bNotFoundPinyin = true;
                    }
                    else if (dlg.DialogResult == DialogResult.OK)
                    {
                        strPinyin += SelPinyinDlg.ConvertSinglePinyinByStyle(
                            dlg.ResultPinyin,
                            style);
                    }
                    else if (dlg.DialogResult == DialogResult.Abort)
                    {
                        return 0;   // 用户希望整个中断
                    }
                    else
                    {
                        Debug.Assert(false, "SelPinyinDlg返回时出现意外的DialogResult值");
                    }
                }
                else
                {
                    // 单个拼音

                    strPinyin += SelPinyinDlg.ConvertSinglePinyinByStyle(
                        strResultPinyin,
                        style);
                }
                nStatus = 2;
            }

            if (bNotFoundPinyin == false)
                return 1;   // 正常结束

            return 2;   // 结果字符串中有没有找到拼音的汉字
        }

        // parameters:
        //      strIndicator    字段指示符。如果用null调用，则表示不对指示符进行筛选
        // return:
        //      0   没有找到匹配的配置事项
        //      >=1 找到。返回找到的配置事项个数
        /// <summary>
        /// 获得和一个字段相关的拼音配置事项集合
        /// </summary>
        /// <param name="cfg_dom">存储了配置信息的 XmlDocument 对象</param>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strIndicator">字段指示符</param>
        /// <param name="cfg_items">返回匹配的配置事项集合</param>
        /// <returns>0: 没有找到匹配的配置事项; >=1: 找到。值为配置事项个数</returns>
        public static int GetPinyinCfgLine(XmlDocument cfg_dom,
            string strFieldName,
            string strIndicator,
            out List<PinyinCfgItem> cfg_items)
        {
            cfg_items = new List<PinyinCfgItem>();

            XmlNodeList nodes = cfg_dom.DocumentElement.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                PinyinCfgItem item = new PinyinCfgItem(node);

                if (item.FieldName != strFieldName)
                    continue;

                if (string.IsNullOrEmpty(item.IndicatorMatchCase) == false
                    && string.IsNullOrEmpty(strIndicator) == false)
                {
                    if (MarcUtil.MatchIndicator(item.IndicatorMatchCase, strIndicator) == false)
                        continue;
                }

                cfg_items.Add(item);
            }

            return cfg_items.Count;
        }

        // 2015/7/20 新函数
        // 汉字字符串转换为拼音
        // 这个函数会按照当前配置，自动决定使用下层的加拼音函数
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        /// <summary>
        /// 汉字字符串转换为拼音
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strHanzi">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="strDuoyinStyle">处理多音字的风格</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int GetPinyin(
            IWin32Window owner,
            string strHanzi,
            PinyinStyle style,
            string strDuoyinStyle,  // bool bAutoSel,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
               || this.ForceUseLocalPinyinFunc == true)
            {
                return this.HanziTextToPinyin(
                    owner,
                    true,	// 本地，快速
                    strHanzi,
                    style,
                    strDuoyinStyle,
                    out strPinyin,
                    out strError);
            }
            else
            {
                // 汉字字符串转换为拼音
                // 如果函数中已经MessageBox报错，则strError第一字符会为空格
                // return:
                //      -1  出错
                //      0   用户希望中断
                //      1   正常
                return this.SmartHanziTextToPinyin(
                    owner,
                    strHanzi,
                    style,
                    strDuoyinStyle,
                    out strPinyin,
                    out strError);
            }
        }

        // 包装后的 汉字到拼音 函数
        // parameters:
        // return:
        //      -1  出错
        //      0   用户中断选择
        //      1   成功
        /// <summary>
        /// 汉字字符串转换为拼音
        /// </summary>
        /// <param name="owner">用于函数中 MessageBox 和对话框 的宿主窗口</param>
        /// <param name="strHanzi">输入字符串</param>
        /// <param name="style">转换为拼音的风格</param>
        /// <param name="bAutoSel">是否自动选择多音字</param>
        /// <param name="strPinyin">返回拼音字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 用户希望中断; 1: 正常; 2: 结果字符串中有没有找到拼音的汉字</returns>
        public int GetPinyin(
            IWin32Window owner,
            string strHanzi,
            PinyinStyle style,  // PinyinStyle.None,
            bool bAutoSel,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";
            int nRet = 0;

            // 把字符串中的汉字和拼音分离
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
               || this.ForceUseLocalPinyinFunc == true)
            {
                nRet = this.HanziTextToPinyin(
                    owner,
                    true,	// 本地，快速
                    strHanzi,
                    style,
                    "auto",
                    out strPinyin,
                    out strError);
            }
            else
            {
                // 汉字字符串转换为拼音
                // 如果函数中已经MessageBox报错，则strError第一字符会为空格
                // return:
                //      -1  出错
                //      0   用户希望中断
                //      1   正常
                nRet = this.SmartHanziTextToPinyin(
                    owner,
                    strHanzi,
                    style,
                    "auto", // bAutoSel,
                    out strPinyin,
                    out strError);
            }
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "用户中断。拼音子字段内容可能不完整。";
                return 0;
            }

            return 1;
        }
#if NO
        // 包装后的 汉字到拼音 函数
        // parameters:
        // return:
        //      -1  出错
        //      0   用户中断选择
        //      1   成功
        public int HanziTextToPinyin(string strHanzi,
            bool bAutoSel,
            PinyinStyle style,  // PinyinStyle.None,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";
            int nRet = 0;

            // 把字符串中的汉字和拼音分离
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
               || this.ForceUseLocalPinyinFunc == true)
            {
                nRet = this.HanziTextToPinyin(
                    this,
                    true,	// 本地，快速
                    strHanzi,
                    style,
                    out strPinyin,
                    out strError);
            }
            else
            {
                // 汉字字符串转换为拼音
                // 如果函数中已经MessageBox报错，则strError第一字符会为空格
                // return:
                //      -1  出错
                //      0   用户希望中断
                //      1   正常
                nRet = this.SmartHanziTextToPinyin(
                    this,
                    strHanzi,
                    style,
                    bAutoSel,
                    out strPinyin,
                    out strError);
            }
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "用户中断。拼音子字段内容可能不完整。";
                return 0;
            }

            return 1;
        }
#endif

        // parameters:
        //      strPrefix   要加入拼音子字段内容前部的前缀字符串。例如 {cr:NLC} 或 {cr:CALIS}
        // return:
        //      -1  出错。包括中断的情况
        //      0   正常
        /// <summary>
        /// 为 MarcRecord 对象内的记录加拼音
        /// </summary>
        /// <param name="record">MARC 记录对象</param>
        /// <param name="strCfgXml">拼音配置 XML</param>
        /// <param name="style">风格</param>
        /// <param name="strPrefix">前缀字符串。缺省为空</param>
        /// <param name="bAutoSel">是否自动选择多音字</param>
        /// <returns>-1: 出错。包括中断的情况; 0: 正常</returns>
        public int AddPinyin(
            MarcRecord record,
            string strCfgXml,
            PinyinStyle style = PinyinStyle.None,
            string strPrefix = "",
            bool bAutoSel = false)
        {
            string strError = "";
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.LoadXml(strCfgXml);
            }
            catch (Exception ex)
            {
                strError = "strCfgXml装载到XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            string strRuleParam = "";
            if (string.IsNullOrEmpty(strPrefix) == false)
            {
                string strCmd = StringUtil.GetLeadingCommand(strPrefix);
                if (string.IsNullOrEmpty(strCmd) == false
&& StringUtil.HasHead(strCmd, "cr:") == true)
                {
                    strRuleParam = strCmd.Substring(3);
                }
            }

            MarcNodeList fields = record.select("field");

            foreach (MarcField field in fields)
            {

                List<PinyinCfgItem> cfg_items = null;
                int nRet = GetPinyinCfgLine(
                    cfg_dom,
                    field.Name,
                    field.Indicator,
                    out cfg_items);
                if (nRet <= 0)
                    continue;

                string strHanzi = "";

                string strFieldPrefix = "";

                // 2012/11/5
                // 观察字段内容前面的 {} 部分
                {
                    string strCmd = StringUtil.GetLeadingCommand(field.Content);
                    if (string.IsNullOrEmpty(strRuleParam) == false
                        && string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        string strCurRule = strCmd.Substring(3);
                        if (strCurRule != strRuleParam)
                            continue;
                    }
                    else if (string.IsNullOrEmpty(strCmd) == false)
                    {
                        strFieldPrefix = "{" + strCmd + "}";
                    }
                }

                // 2012/11/5
                // 观察 $* 子字段
                {
                    MarcNodeList subfields = field.select("subfield[@name='*']");
                    //

                    if (subfields.count > 0)
                    {
                        string strCurStyle = subfields[0].Content;
                        if (string.IsNullOrEmpty(strRuleParam) == false
                            && strCurStyle != strRuleParam)
                            continue;
                        else if (string.IsNullOrEmpty(strCurStyle) == false)
                        {
                            strFieldPrefix = "{cr:" + strCurStyle + "}";
                        }
                    }
                }

                foreach (PinyinCfgItem item in cfg_items)
                {
                    for (int k = 0; k < item.From.Length; k++)
                    {
                        if (item.From.Length != item.To.Length)
                        {
                            strError = "配置事项 fieldname='" + item.FieldName + "' from='" + item.From + "' to='" + item.To + "' 其中from和to参数值的字符数不等";
                            goto ERROR1;
                        }

                        string from = new string(item.From[k], 1);
                        string to = new string(item.To[k], 1);

                        // 删除已经存在的目标子字段
                        field.select("subfield[@name='" + to + "']").detach();

                        MarcNodeList subfields = field.select("subfield[@name='" + from + "']");

                        foreach (MarcSubfield subfield in subfields)
                        {
                            strHanzi = subfield.Content;

                            if (DetailHost.ContainHanzi(strHanzi) == false)
                                continue;

                            string strSubfieldPrefix = "";  // 当前子字段内容本来具有的前缀

                            // 检查内容前部可能出现的 {} 符号
                            string strCmd = StringUtil.GetLeadingCommand(strHanzi);
                            if (string.IsNullOrEmpty(strRuleParam) == false
                                && string.IsNullOrEmpty(strCmd) == false
                                && StringUtil.HasHead(strCmd, "cr:") == true)
                            {
                                string strCurRule = strCmd.Substring(3);
                                if (strCurRule != strRuleParam)
                                    continue;   // 当前子字段属于和strPrefix表示的不同的编目规则，需要跳过，不给加拼音
                                strHanzi = strHanzi.Substring(strPrefix.Length); // 去掉 {} 部分
                            }
                            else if (string.IsNullOrEmpty(strCmd) == false)
                            {
                                strHanzi = strHanzi.Substring(strCmd.Length + 2); // 去掉 {} 部分
                                strSubfieldPrefix = "{" + strCmd + "}";
                            }

                            string strPinyin = "";

#if NO
                            // 把字符串中的汉字和拼音分离
                            // return:
                            //      -1  出错
                            //      0   用户希望中断
                            //      1   正常
                            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
                               || this.ForceUseLocalPinyinFunc == true)
                            {
                                nRet = this.HanziTextToPinyin(
                                    this,
                                    true,	// 本地，快速
                                    strHanzi,
                                    style,
                                    out strPinyin,
                                    out strError);
                            }
                            else
                            {
                                // 汉字字符串转换为拼音
                                // 如果函数中已经MessageBox报错，则strError第一字符会为空格
                                // return:
                                //      -1  出错
                                //      0   用户希望中断
                                //      1   正常
                                nRet = this.SmartHanziTextToPinyin(
                                    this,
                                    strHanzi,
                                    style,
                                    bAutoSel,
                                    out strPinyin,
                                    out strError);
                            }
#endif
                            nRet = this.GetPinyin(
                                this,
                                strHanzi,
                                style,
                                bAutoSel,
                                out strPinyin,
                                out strError);
                            if (nRet == -1)
                            {
                                goto ERROR1;
                            }
                            if (nRet == 0)
                            {
                                strError = "用户中断。拼音子字段内容可能不完整。";
                                goto ERROR1;
                            }

                            string strContent = strPinyin;

                            if (string.IsNullOrEmpty(strPrefix) == false)
                                strContent = strPrefix + strPinyin;
                            else if (string.IsNullOrEmpty(strSubfieldPrefix) == false)
                                strContent = strSubfieldPrefix + strPinyin;

                            subfield.after(MarcQuery.SUBFLD + to + strPinyin);
                        }
                    }
                }
            }

            return 0;
        ERROR1:
            if (string.IsNullOrEmpty(strError) == false)
            {
                if (strError[0] != ' ')
                    MessageBox.Show(this, strError);
            }
            return -1;
        }

        /// <summary>
        /// 为 MarcRecord 对象内的记录删除拼音
        /// </summary>
        /// <param name="record">MARC 记录对象</param>
        /// <param name="strCfgXml">拼音配置 XML</param>
        /// <param name="strPrefix">前缀字符串。缺省为空</param>
        public void RemovePinyin(
            MarcRecord record,
            string strCfgXml,
            string strPrefix = "")
        {
            string strError = "";
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.LoadXml(strCfgXml);
            }
            catch (Exception ex)
            {
                strError = "strCfgXml装载到XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            string strRuleParam = "";
            if (string.IsNullOrEmpty(strPrefix) == false)
            {
                string strCmd = StringUtil.GetLeadingCommand(strPrefix);
                if (string.IsNullOrEmpty(strCmd) == false
&& StringUtil.HasHead(strCmd, "cr:") == true)
                {
                    strRuleParam = strCmd.Substring(3);
                }
            }

            MarcNodeList fields = record.select("field");

            foreach (MarcField field in fields)
            {

                List<PinyinCfgItem> cfg_items = null;
                int nRet = GetPinyinCfgLine(
                    cfg_dom,
                    field.Name,
                    field.Indicator,    // TODO: 可以不考虑指示符的情况，扩大删除的搜寻范围
                    out cfg_items);
                if (nRet <= 0)
                    continue;

                string strField = field.Text;

                // 观察字段内容前面的 {} 部分
                if (string.IsNullOrEmpty(strRuleParam) == false)
                {
                    string strCmd = StringUtil.GetLeadingCommand(field.Content);
                    if (string.IsNullOrEmpty(strRuleParam) == false
                        && string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        string strCurRule = strCmd.Substring(3);
                        if (strCurRule != strRuleParam)
                            continue;
                    }
                }

                // 2012/11/6
                // 观察 $* 子字段
                if (string.IsNullOrEmpty(strRuleParam) == false)
                {
                    MarcNodeList subfields = field.select("subfield[@name='*']");
                    if (subfields.count > 0)
                    {
                        string strCurStyle = subfields[0].Content;
                        if (string.IsNullOrEmpty(strRuleParam) == false
                            && strCurStyle != strRuleParam)
                            continue;
                    }
                }

                foreach (PinyinCfgItem item in cfg_items)
                {
                    for (int k = 0; k < item.To.Length; k++)
                    {
                        string to = new string(item.To[k], 1);
                        if (string.IsNullOrEmpty(strPrefix) == true)
                        {
                            // 删除已经存在的目标子字段
                            field.select("subfield[@name='" + to + "']").detach();
                        }
                        else
                        {
                            MarcNodeList subfields = field.select("subfield[@name='" + to + "']");

                            // 只删除具有特定前缀的内容的子字段
                            foreach (MarcSubfield subfield in subfields)
                            {
                                string strContent = subfield.Content;
                                if (subfield.Content.Length == 0)
                                    subfields.detach(); // 空内容的子字段要删除
                                else
                                {
                                    if (StringUtil.HasHead(subfield.Content, strPrefix) == true)
                                        subfields.detach();
                                }
                            }
                        }
                    }
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #endregion

    }
}
