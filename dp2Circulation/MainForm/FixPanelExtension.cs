using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关固定面板的功能
    /// </summary>
    public partial class MainForm
    {
        internal int _fixedPanelAnimation = 0; // 0 表示尚未初始化; -1: false; 1: true

        public bool FixedPanelAnimationEnabled
        {
            get
            {
                if (_fixedPanelAnimation != 0)
                    return _fixedPanelAnimation == 1 ? true : false;

                bool bRet = this.AppInfo.GetBoolean(
                "MainForm",
                "fixed_panel_animation",
                false);

                if (bRet)
                    _fixedPanelAnimation = 1;
                else
                    _fixedPanelAnimation = -1;
                return bRet;
            }
            set
            {
                _fixedPanelAnimation = 0;
                this.AppInfo.SetBoolean(
"MainForm",
"fixed_panel_animation",
value);
            }
        }

        // 属性页标题文字动画
        public void FixedPanelAnimation(TabPage page)
        {
            if (this.FixedPanelAnimationEnabled == false)
                return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<TabPage>(FixedPanelAnimation), page);
                return;
            }

            string strChars = "▁▂▃▅▆▇▉";

            string strText = page.Text;
            int index = strChars.IndexOf(strText[0]);
            if (index == -1)
            {
                page.Text = strChars[0] + " " + page.Text;
                return;
            }
            char new_char;
            if (index < strChars.Length - 1)
                new_char = strChars[index + 1];
            else
                new_char = strChars[0];
            page.Text = new string(new_char, 1) + page.Text.Substring(1);
        }

        public TabPage PageOperHistory
        {
            get
            {
                return this.tabPage_history;
            }
        }

        /// <summary>
        /// 激活固定面板区域的“属性”属性页
        /// </summary>
        public void ActivatePropertyPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_property;
        }

        /// <summary>
        /// 固定面板区域的属性控件
        /// </summary>
        public Control CurrentPropertyControl
        {
#if NO
            get
            {
                if (this.tabPage_property.Controls.Count == 0)
                    return null;
                return this.tabPage_property.Controls[0];
            }
            set
            {
                // 清除原有控件
                while (this.tabPage_property.Controls.Count > 0)
                    this.tabPage_property.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_property.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_property;
                }
            }
#endif
            get
            {
                return this.GetFixPageControl("property");
            }
            set
            {
                this.SetFixPageControl("property", value);
            }
        }

        /// <summary>
        /// 激活固定面板区域的“验收”属性页
        /// </summary>
        public void ActivateAcceptPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_accept;
        }

        /// <summary>
        /// 固定面板区域的验收控件
        /// </summary>
        public Control CurrentAcceptControl
        {
#if NO
            get
            {
                if (this.tabPage_accept.Controls.Count == 0)
                    return null;
                return this.tabPage_accept.Controls[0];
            }
            set
            {
                // 清除原有控件
                while (this.tabPage_accept.Controls.Count > 0)
                    this.tabPage_accept.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_accept.Controls.Add(value);
                }
            }
#endif
            get
            {
                return this.GetFixPageControl("accept");
            }
            set
            {
                this.SetFixPageControl("accept", value);
            }
        }

        /// <summary>
        /// 激活固定面板区域的“校验结果”属性页
        /// </summary>
        public void ActivateVerifyResultPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_verifyResult;
        }

        /// <summary>
        /// 固定面板区域的校验结果控件
        /// </summary>
        public Control CurrentVerifyResultControl
        {
#if NO
            get
            {
                if (this.tabPage_verifyResult.Controls.Count == 0)
                    return null;
                return this.tabPage_verifyResult.Controls[0];
            }
            set
            {
                // 清除原有控件
                while (this.tabPage_verifyResult.Controls.Count > 0)
                    this.tabPage_verifyResult.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_verifyResult.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_verifyResult;
                }

            }
#endif
            get
            {
                return this.GetFixPageControl("verifyResult");
            }
            set
            {
                this.SetFixPageControl("verifyResult", value);
            }
        }

        /// <summary>
        /// 激活固定面板区域的“创建数据”属性页
        /// </summary>
        public void ActivateGenerateDataPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_generateData;
        }

        /// <summary>
        /// 固定面板区域的创建数据控件
        /// </summary>
        public Control CurrentGenerateDataControl
        {
#if NO
            get
            {
                if (this.tabPage_generateData.Controls.Count == 0)
                    return null;
                return this.tabPage_generateData.Controls[0];
            }
            set
            {
                // 清除原有控件
                while (this.tabPage_generateData.Controls.Count > 0)
                    this.tabPage_generateData.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_generateData.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_generateData;

                    // 避免出现半截窗口图像的闪动
                    if (this.tabControl_panelFixed.Visible
                        && this.tabControl_panelFixed.SelectedTab == this.tabPage_generateData)
                        this.tabPage_generateData.Update();
                }
            }
#endif
            get
            {
                return this.GetFixPageControl("generateData");
            }
            set
            {
                this.SetFixPageControl("generateData", value);
            }
        }

        public Control GetFixPageControl(string strName)
        {
            TabPage page = GetFixPage(strName);
            if (page == null)
                return null;
            if (page.Controls.Count == 0)
                return null;
            return page.Controls[0];
        }

        public void SetFixPageControl(string strName, Control value)
        {
            TabPage page = GetFixPage(strName);
            if (page == null)
                throw new ArgumentException("名字为 '" + strName + "' 的 fixpage 不存在");

            // 清除原有控件
#if NO
            while (page.Controls.Count > 0)
            {
                page.Controls.RemoveAt(0); 可能造成资源泄露！
            }
#endif
            page.ClearControls();   // 2015/11/7

            if (value != null)
            {
                page.Controls.Add(value);
                // this.tabControl_panelFixed.SelectedTab = this.tabPage_generateData;

                // 避免出现半截窗口图像的闪动
                if (this.tabControl_panelFixed.Visible
                    && this.tabControl_panelFixed.SelectedTab == page)
                    page.Update();
            }
        }

        public void ActivateFixPage(string strName)
        {
            TabPage page = GetFixPage(strName);
            if (page == null)
                throw new ArgumentException("名字为 '" + strName + "' 的 fixpage 不存在");
            this.tabControl_panelFixed.SelectedTab = page;
        }

        public TabPage GetFixPage(string strName)
        {
            switch (strName)
            {
                case "share":
                    return this.tabPage_share;
                case "generateData":
                    return this.tabPage_generateData;
                case "verifyResult":
                    return this.tabPage_verifyResult;
                case "accept":
                    return this.tabPage_accept;
                case "camera":
                    return this.tabPage_camera;
                case "history":
                    return this.tabPage_history;
                case "property":
                    return this.tabPage_property;
                default:
                    return null;
            }
        }



        /// <summary>
        /// 固定面板区域是否可见
        /// </summary>
        public bool PanelFixedVisible
        {
            get
            {
                return this.panel_fixed.Visible;
            }
            set
            {
                this.panel_fixed.Visible = value;
                this.splitter_fixed.Visible = value;

                this.MenuItem_displayFixPanel.Checked = value;
            }
        }

    }
}
