using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// W3CDTF时间控件
    /// </summary>
    public partial class W3cDtfControl : UserControl
    {
        public W3cDtfControl()
        {
            InitializeComponent();
        }

        [Category("Appearance")]
        [DescriptionAttribute("Border style of the control")]
        [DefaultValue(typeof(System.Windows.Forms.BorderStyle), "None")]
        public new BorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;
            }
        }

        public string ValueString
        {
            get
            {
                this.maskedTextBox_date.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
                this.maskedTextBox_timeZone.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;

                string strResult = "";
                string strError = "";

                string strTimeZone = "";
                if (String.IsNullOrEmpty(this.maskedTextBox_timeZone.Text) == false)
                    strTimeZone = this.label_eastWest.Text
                    + this.maskedTextBox_timeZone.Text;

                int nRet = BuildW3cDtfString(this.maskedTextBox_date.Text,
                    strTimeZone,
                    out strResult,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                return strResult;
            }
            set
            {
                this.maskedTextBox_date.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
                this.maskedTextBox_timeZone.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;

                string strDateTimeString = "";
                string strTimeZoneString = "";
                string strError = "";

                int nRet = ParseW3cDtfString(value,
                    out strDateTimeString,
                    out strTimeZoneString,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                this.maskedTextBox_date.Text = strDateTimeString;

                if (strTimeZoneString != "")
                {
                    this.label_eastWest.Text = strTimeZoneString.Substring(0, 1);
                    this.maskedTextBox_timeZone.Text = strTimeZoneString.Substring(1);
                }
                else
                {
                    this.label_eastWest.Text = "+";
                    this.maskedTextBox_timeZone.Text = "";
                }

            }
        }

        static bool IsAllBlank(string strText)
        {
            bool bFound = false;    // 是否发现了非空格字符？
            for (int i = 0; i < strText.Length; i++)
            {
                if (strText[i] != ' ')
                {
                    bFound = true;
                    break;
                }
            }
            if (bFound == true)
                return false;

            return true;
        }

        // 将W3CDTF字符串解析为 密集形态的 时间 和 时区 字符串
        /*
W3CDTF是基于ISO8601格式，即以下都是合法的：
   Year:
      YYYY (eg 1997)
   Year and month:
      YYYY-MM (eg 1997-07)
   Complete date:
      YYYY-MM-DD (eg 1997-07-16)
   Complete date plus hours and minutes:
      YYYY-MM-DDThh:mmTZD (eg 1997-07-16T19:20+01:00)
   Complete date plus hours, minutes and seconds:
      YYYY-MM-DDThh:mm:ssTZD (eg 1997-07-16T19:20:30+01:00)
   Complete date plus hours, minutes, seconds and a decimal fraction of a
second
      YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45+01:00)
         * */
        int ParseW3cDtfString(string strW3cDtfString,
            out string strDateTimeString,
            out string strTimeZoneString,
            out string strError)
        {
            strError = "";
            strDateTimeString = "";
            strTimeZoneString = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strW3cDtfString) == true)
            {
                return 0;   // 返回空值
            }

            if (strW3cDtfString.Length < 4)
            {
                strError = "长度不足4字符";
                return -1;
            }

            string strYear = "";
            if (strW3cDtfString.Length >= 4)
            {
                strYear = strW3cDtfString.Substring(0, 4);

                nRet = CheckNumberRange(strYear,
                    "0000",
                    "9999",
                    "年",
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (strW3cDtfString.Length == 4)
            {
                // Year:
                //      YYYY (eg 1997)
                strDateTimeString = strYear;
                return 0;
            }

            if (strW3cDtfString.Length > 4 && strW3cDtfString.Length < 7)
            {
                strError = "月份部分格式错误";
                return -1;
            }

            string strMonth = "";
            if (strW3cDtfString.Length >= 7)
            {
                if (strW3cDtfString[4] != '-')
                {
                    strError = "第5字符应当为'-'";
                    return -1;
                }

                strMonth = strW3cDtfString.Substring(5, 2);

                nRet = CheckNumberRange(strMonth,
                    "01",
                    "12",
                    "月",
                    out strError);
                if (nRet == -1)
                    return -1;

            }

            if (strW3cDtfString.Length == 7)
            {
                //   Year and month:
                //      YYYY-MM (eg 1997-07)
                strDateTimeString = strYear + strMonth;
                return 0;
            }

            if (strW3cDtfString.Length > 7 && strW3cDtfString.Length < 10)
            {
                strError = "日值部分格式错误";
                return -1;
            }


            string strDay = "";
            if (strW3cDtfString.Length >= 10)
            {
                if (strW3cDtfString[7] != '-')
                {
                    strError = "第8字符应当为'-'";
                    return -1;
                }

                strDay = strW3cDtfString.Substring(8, 2);


                nRet = CheckNumberRange(strDay,
                    "01",
                    "31",
                    "日",
                    out strError);
                if (nRet == -1)
                    return -1;

                // TODO: 还需要精确检查当时那个月的天数值范围
            }

            if (strW3cDtfString.Length == 10)
            {
                //   Complete date:
                //      YYYY-MM-DD (eg 1997-07-16)
                strDateTimeString = strYear + strMonth + strDay;
                return 0;
            }

            string strTimeSegment = ""; // 时间段
            string strTimeZoneSegment = ""; // 时区段

            nRet = strW3cDtfString.IndexOf("T");
            if (nRet != -1)
            {
                strTimeSegment = strW3cDtfString.Substring(nRet + 1);
                nRet = strTimeSegment.IndexOfAny(new char[] { '+', '-' });
                if (nRet != -1)
                {
                    strTimeZoneSegment = strTimeSegment.Substring(nRet);
                    strTimeSegment = strTimeSegment.Substring(0, nRet);   // 去掉后面多余的TimeZone部分
                }
            }

            // 细节解剖时间段
            if (strTimeSegment != "")
            {
                string strHour = "";
                string strMinute = "";
                string strSecond = "";
                string strSecondDecimal = "";
                // T19:20:30.45

                nRet = ParseTimeSegment(strTimeSegment,
                    out strHour,
                    out strMinute,
                    out strSecond,
                    out strSecondDecimal,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 装配为紧凑形态
                strTimeSegment = strHour;
                strTimeSegment += strMinute;
                if (String.IsNullOrEmpty(strSecond) == true)
                    strTimeSegment += "  ";
                else
                    strTimeSegment += strSecond;

                if (String.IsNullOrEmpty(strSecondDecimal) == true)
                    strTimeSegment += "  ";
                else
                    strTimeSegment += strSecondDecimal;

            }

            // 细节解剖时区段

            if (strTimeZoneSegment != "")
            {
                string strEastWest = "";
                string strTzdHour = "";
                string strTzdMinute = "";

                nRet = ParseTimeZoneSegment(strTimeZoneSegment,
                    out strEastWest,
                    out strTzdHour,
                    out strTzdMinute,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 装配为紧凑形态
                strTimeZoneSegment = strEastWest;
                strTimeZoneSegment += strTzdHour;
                strTimeZoneSegment += strTzdMinute;
            }

            if (strTimeSegment != ""
                && strTimeZoneSegment != "")
            {
                //      Complete date plus hours and minutes:
                //      YYYY-MM-DDThh:mmTZD (eg 1997-07-16T19:20+01:00)
                strDateTimeString = strYear + strMonth + strDay + strTimeSegment;
                strTimeZoneString = strTimeZoneSegment;
                return 0;
            }

            if (strTimeSegment != ""
                && strTimeZoneSegment == "")
            {
                strError = "具备时间段(T引导的部分)就必须具备时区段(+或-引导的部分)";
                return -1;
            }



            return 0;
        }

        // return:
        //      0   没有错误
        //      -1  有错误
        static int CheckNumberRange(string strText,
            string strMin,
            string strMax,
            string strName,
            out string strError)
        {
            strError = "";

            if (strText.IndexOf(" ") != -1)
            {
                strError = strName + "值 '" + strText + "' 中不应包含空格";
                return -1;
            }

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];
                if (ch < '0' || ch > '9')
                {
                    strError = strName + "值 '" +strText+ "' 不是纯数字";
                    return -1;
                }
            }

            if (String.Compare(strText, strMin) < 0)
            {
                strError = strName + "值不应小于 '" + strMin + "'";
                return -1;
            }

            if (String.Compare(strText, strMax) > 0)
            {
                strError = strName + "值不应大于 '" + strMax + "'";
                return -1;
            }

            return 0;
        }

        // 细节解剖时间段
        // 19:20:30.45
        static int ParseTimeSegment(string strSegment,
            out string strHour,
            out string strMinute,
            out string strSecond,
            out string strSecondDecimal,
            out string strError)
        {
            strHour = "";
            strMinute = "";
            strSecond = "";
            strSecondDecimal = "";
            strError = "";
            int nRet = 0;

            if (strSegment.Length != 5
                && strSegment.Length != 8
                && strSegment.Length != 11)
            {
                strError = "时间字符串 '" + strSegment + "' 格式不正确，长度应为5 8 11字符";
                return -1;
            }


            if (strSegment.Length >= 5)
            {
                strHour = strSegment.Substring(0, 2);
                strMinute = strSegment.Substring(3, 2);

                // 检查数值范围
                nRet = CheckNumberRange(strHour,
                    "00",
                    "23",
                    "小时",
                    out strError);
                if (nRet == -1)
                    return -1;

                nRet = CheckNumberRange(strMinute,
                    "00",
                    "59",
                    "分",
                    out strError);
                if (nRet == -1)
                    return -1;

            }

            if (strSegment.Length >= 8)
            {
                strSecond = strSegment.Substring(6, 2);

                nRet = CheckNumberRange(strSecond,
                    "00",
                    "59",
                    "秒",
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (strSegment.Length >= 11)
            {
                strSecondDecimal = strSegment.Substring(9, 2);

                nRet = CheckNumberRange(strSecondDecimal,
                    "00",
                    "99",
                    "百分秒",
                    out strError);
                if (nRet == -1)
                    return -1;

            }


            return 0;
        }

        // 细节解剖时区段
        // +01:00
        int ParseTimeZoneSegment(string strSegment,
            out string strEastWest,
            out string strHour,
            out string strMinute,
            out string strError)
        {
            strHour = "";
            strMinute = "";
            strEastWest = "";
            strError = "";
            int nRet = 0;

            if (strSegment.Length != 6)
            {
                strError = "时区字符串 '" + strSegment + "' 格式不正确，长度应为6字符";
                return -1;
            }

            strEastWest = strSegment.Substring(0, 1);
            if (strEastWest != "+"
                && strEastWest != "-")
            {
                strError = "时区字符串 '" + strSegment + "' 第一字符'"+strEastWest+"'格式不正确，应为+ -之一";
                return -1;
            }

            strSegment = strSegment.Substring(1);

            if (strSegment.Length >= 5)
            {
                strHour = strSegment.Substring(0, 2);
                strMinute = strSegment.Substring(3, 2);

                // TODO: 检查数值范围
                
                nRet = CheckNumberRange(strHour,
                    "00",
                    strEastWest == "-" ? "12" : "13",
                    "时区 小时",
                    out strError);
                if (nRet == -1)
                    return -1;

                nRet = CheckNumberRange(strMinute,
                    "00",
                    "59",
                    "时区 分",
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // 将01:00变换0100
        static string GetPureHourMinte(string strText)
        {
            return strText.Substring(0, 2)
            + strText.Substring(3, 2);
        }

        // 将密集形态的时间和时区值 变换为W3CDTF形态
        int BuildW3cDtfString(string strDateTimeString,
            string strTimeZoneString,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strDateTimeString) == true
                && String.IsNullOrEmpty(strTimeZoneString) == true)
            {
                return 0;   // 返回空值
            }
            
            // 取得年段
            string strYearSegment = "";
            if (strDateTimeString.Length >= 4)
            {
                strYearSegment = strDateTimeString.Substring(0, 4);
            }

            // 处理年
            string strYear = "";
            if (strYearSegment != "")
            {
                if (strYearSegment.Length < 4)
                {
                    strError = "年份应当为4位数字";
                    goto ERROR1;
                }

                strYear = strYearSegment;

                nRet = CheckNumberRange(strYear,
                    "0000",
                    "9999",
                    "年",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            if (strYear == "")
            {
                strError = "至少也要输入4位数字的年份值";
                goto ERROR1;
            }

            // 取得月段
            string strMonthSegment = "";
            if (strDateTimeString.Length >= 6)
            {
                strMonthSegment = strDateTimeString.Substring(4, 2);
            }

            if (strDateTimeString.Length > 4 && strDateTimeString.Length < 6)
            {
                strError = "月份值应为2位数字";
                goto ERROR1;
            }

            // 处理月
            string strMonth = "";
            if (strMonthSegment != "")
            {
                if (strMonthSegment.Length < 2)
                {
                    strError = "月份应当为2位数字";
                    goto ERROR1;
                }

                strMonth = strMonthSegment;
                nRet = CheckNumberRange(strMonth,
                    "01",
                    "12",
                    "月份",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }


            // 取得日段
            string strDaySegment = "";
            if (strDateTimeString.Length >= 8)
            {
                strDaySegment = strDateTimeString.Substring(6, 2);
            }

            if (strDateTimeString.Length > 6 && strDateTimeString.Length < 8)
            {
                strError = "日值应为2位数字";
                goto ERROR1;
            }

            // 处理日
            string strDay = "";
            if (strDaySegment != "")
            {
                // 8
                if (strDaySegment.Length < 2)
                {
                    strError = "日值应当为2位数字";
                    goto ERROR1;
                }

                strDay = strDaySegment;
                nRet = CheckNumberRange(strDay,
                    "01",
                    "31",
                    "日",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;


            }

            if (strYear == "")
            {
                if (strMonth != "" || strDay != "")
                {
                    strError = "请输入年份值";
                    goto ERROR1;
                }
            }

            if (strMonth == "")
            {
                if (strDay != "")
                {
                    strError = "请输入月份值";
                    goto ERROR1;
                }
            }


            // 取得时分段
            string strHourMinuteSegment = "";
            if (strDateTimeString.Length >= 12)
            {
                strHourMinuteSegment = strDateTimeString.Substring(8, 4);
            }

            if (strDateTimeString.Length > 8 && strDateTimeString.Length < 12)
            {
                strError = "时、分值应为4位数字";
                goto ERROR1;
            }

            // 处理时、分
            string strHour = "";
            string strMinute = "";
            if (strHourMinuteSegment != ""
                && IsAllBlank(strHourMinuteSegment) == false)
            {
                if (strHourMinuteSegment.Length < 4)
                {
                    strError = "时、分值应当为4位数字";
                    goto ERROR1;
                }

                strHour = strHourMinuteSegment.Substring(0, 2);
                nRet = CheckNumberRange(strHour,
                    "00",
                    "23",
                    "小时",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                strMinute = strHourMinuteSegment.Substring(2, 2);
                nRet = CheckNumberRange(strMinute,
                    "00",
                    "59",
                    "分",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                /*
                int hour = Convert.ToInt32(strHour);
                if (hour < 0 || hour > 23)
                {
                    strError = "时值应当在00-23之间";
                    goto ERROR1;
                }

                int minute = Convert.ToInt32(strMinute);
                if (minute < 0 || minute > 59)
                {
                    strError = "分值应当在00-59之间";
                    goto ERROR1;
                }
                 * */
            }

            // 取得秒、百分秒段
            string strSecondSegment = "";
            if (strDateTimeString.Length >= 16)
            {
                strSecondSegment = strDateTimeString.Substring(12, 4);
            }
            else if (strDateTimeString.Length >= 14)
            {
                strSecondSegment = strDateTimeString.Substring(12, 2);
            }

            if (strDateTimeString.Length > 12 && strDateTimeString.Length < 14)
            {
                strError = "秒应为2位数字";
                goto ERROR1;
            }

            // 处理秒、百分秒
            string strSecond = "";
            string strSecondDecimal = "";
            if (strSecondSegment != ""
                && IsAllBlank(strSecondSegment) == false)
            {
                if (strSecondSegment.Length < 2)
                {
                    strError = "秒值应当为2位数字";
                    goto ERROR1;
                }

                if (strSecondSegment.Length == 2)
                {
                    strSecond = strSecondSegment;
                    strSecondDecimal = "";
                }
                else
                {
                    if (strSecondSegment.Length != 4)
                    {
                        strError = "秒值应当为4位数字(包含百分秒时)";
                        goto ERROR1;
                    }

                    strSecond = strSecondSegment.Substring(0, 2);
                    strSecondDecimal = strSecondSegment.Substring(2, 2);

                    if (strSecondDecimal == "  ")
                        strSecondDecimal = "";
                    else
                    {
                        nRet = CheckNumberRange(strSecondDecimal,
                            "00",
                            "99",
                            "百分秒",
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                }

                /*
                if (strSecond.IndexOf(" ") != -1)
                {
                    strError = "秒值中不应当包含空格字符";
                    goto ERROR1;
                }

                int second = Convert.ToInt32(strSecond);
                if (second < 0 || second > 59)
                {
                    strError = "秒值应当在00-59之间";
                    goto ERROR1;
                }
                */
                nRet = CheckNumberRange(strSecond,
                    "00",
                    "59",
                    "秒",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }

            // 取得时区段
            string strTimeZoneSegment = "";
            if (String.IsNullOrEmpty(strTimeZoneString) == false)
            {
                if (strTimeZoneString.Length < 5)
                {
                    strError = "时区应为5位字符(一个+/-符号位和4位数字)";
                    goto ERROR1;
                }
                strTimeZoneSegment = strTimeZoneString;
            }

            // 处理时区段
            string strTzdHour = "";
            string strTzdMinute = "";
            string strTzdDirection = "";
            if (strTimeZoneSegment != ""
                && IsAllBlank(strTimeZoneSegment) == false)
            {
                if (strTimeZoneSegment.Length < 5)
                {
                    strError = "时区 符号、时、分值应当为5位字符";
                    goto ERROR1;
                }

                strTzdDirection = strTimeZoneSegment.Substring(0, 1);
                if (strTzdDirection != "+" && strTzdDirection != "-")
                {
                    strError = "时区 符号值 应当为 +/-之一";
                    goto ERROR1;
                }

                /*
                strTzdHour = strTimeZoneSegment.Substring(1, 2);
                if (strTzdHour.IndexOf(" ") != -1)
                {
                    strError = "时区 时值中不应当包含空格字符";
                    goto ERROR1;
                }

                strTzdMinute = strTimeZoneSegment.Substring(3, 2);
                if (strTzdMinute.IndexOf(" ") != -1)
                {
                    strError = "时区 分值中不应当包含空格字符";
                    goto ERROR1;
                }

                int hour = Convert.ToInt32(strTzdHour);
                if (hour < 0 || hour > 23)
                {
                    strError = "时区 时值应当在00-23之间";
                    goto ERROR1;
                }

                int minute = Convert.ToInt32(strTzdMinute);
                if (minute < 0 || minute > 59)
                {
                    strError = "时区 分值应当在00-59之间";
                    goto ERROR1;
                }
                 * */

                strTzdHour = strTimeZoneSegment.Substring(1, 2);

                nRet = CheckNumberRange(strTzdHour,
                    "00",
                    strTzdDirection == "-" ? "12" : "13",
                    "时区 小时",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                strTzdMinute = strTimeZoneSegment.Substring(3, 2);

                nRet = CheckNumberRange(strTzdMinute,
                    "00",
                    "59",
                    "时区 分",
                     out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 月份或者日期缺省
            if (strMonth == "" && strDay == "")
            {
                if (strTimeZoneSegment != "")
                {
                    strError = "在没有输入时、分值的情况下，不允许输入时区值";
                    goto ERROR1;
                }

                strResult = strYear;
                return 0;
            }

            if (strDay == "")
            {
                if (strTimeZoneSegment != "")
                {
                    strError = "在没有输入时、分值的情况下，不允许输入时区值";
                    goto ERROR1;
                }

                strResult = strYear + "-" + strMonth;
                return 0;
            }

            Debug.Assert(strYear != ""
                && strMonth != ""
                && strDay != "", "");

            // 年月日都齐全的情况下，检查日数字的有效性
            try
            {
                DateTime date = new DateTime(Convert.ToInt32(strYear),
                    Convert.ToInt32(strMonth),
                    Convert.ToInt32(strDay));
            }
            catch // (Exception ex)
            {
                strError = strYear + "年" + strMonth + "月不存在" + strDay + "日";
                goto ERROR1;
            }
 

            if (strHourMinuteSegment == "")
            {
                if (strSecondSegment != "")
                {
                    strError = "输入了秒值，就必须也输入时、分值";
                    goto ERROR1;
                }

                if (strTimeZoneSegment != "")
                {
                    strError = "在没有输入时、分值的情况下，不允许输入时区值";
                    goto ERROR1;
                }

                strResult = strYear + "-" + strMonth + "-" + strDay;
                return 0;
            }

            if (strHourMinuteSegment != ""
                && strSecondSegment != "")
            {
                strResult = strYear + "-" + strMonth + "-" + strDay + "T" + strHour + ":" + strMinute + ":" + strSecond;

                if (strSecondDecimal != "")
                {
                    // Complete date plus hours, minutes, seconds and a decimal fraction of a second
                    // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45+01:00)
                    strResult += "." + strSecondDecimal;
                }
                else
                {
                    // Complete date plus hours, minutes and seconds:
                    // YYYY-MM-DDThh:mm:ssTZD (eg 1997-07-16T19:20:30+01:00)
                }

                if (strTimeZoneSegment != "")
                    strResult += strTzdDirection + strTzdHour + ":" + strTzdMinute;
                else
                    strResult += "+00:00";

                return 0;
            }


            if (strHourMinuteSegment != "")
            {
                strResult = strYear + "-" + strMonth + "-" + strDay + "T" + strHour + ":" + strMinute;

                if (strTimeZoneSegment != "")
                    strResult += strTzdDirection + strTzdHour + ":" + strTzdMinute;
                else
                    strResult += "+00:00";

                // Complete date plus hours and minutes:
                // YYYY-MM-DDThh:mmTZD (eg 1997-07-16T19:20+01:00)
                return 0;
            }

            strError = "格式错误！";
            goto ERROR1;
            // return 0;

        ERROR1:
            return -1;
        }

        // 出现菜单
        private void label_eastWest_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;



            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            //
            menuItem = new MenuItem("+\t东部时区");
            menuItem.Click += new System.EventHandler(this.menu_east_Click);
            if (this.label_eastWest.Text == "+")
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("-\t西部时区");
            menuItem.Click += new System.EventHandler(this.menu_west_Click);
            if (this.label_eastWest.Text == "-")
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.label_eastWest, new Point(e.X, e.Y));
        }

        // toggle
        private void label_eastWest_DoubleClick(object sender, EventArgs e)
        {
            if (this.label_eastWest.Text == "+")
                this.label_eastWest.Text = "-";
            else
                this.label_eastWest.Text = "+";
        }

        void menu_east_Click(object sender, EventArgs e)
        {
            this.label_eastWest.Text = "+";
        }

        void menu_west_Click(object sender, EventArgs e)
        {
            this.label_eastWest.Text = "-";
        }

        /*
http://read.newbooks.com.cn/info/180524.html
(GMT) 卡萨布兰卡，蒙罗维亚，雷克雅未克
(GMT) 格林威治标准时间: 都柏林, 爱丁堡, 伦敦, 里斯本
(GMT+01:00) 中非西部
(GMT+01:00) 布鲁塞尔，哥本哈根，马德里，巴黎
(GMT+01:00) 萨拉热窝，斯科普里，华沙，萨格勒布
(GMT+01:00) 贝尔格莱德，布拉迪斯拉发，布达佩斯，卢布尔雅那，布拉格
(GMT+01:00) 阿姆斯特丹，柏林，伯尔尼，罗马，斯德哥尔摩，维也纳
(GMT+02:00) 哈拉雷，比勒陀利亚
(GMT+02:00) 安曼
(GMT+02:00) 开罗
(GMT+02:00) 明斯克
(GMT+02:00) 温得和克
(GMT+02:00) 耶路撒冷
(GMT+02:00) 贝鲁特
(GMT+02:00) 赫尔辛基，基辅，里加，索非亚，塔林，维尔纽斯
(GMT+02:00) 雅典，布加勒斯特，伊斯坦布尔
(GMT+03:00) 内罗毕
(GMT+03:00) 巴格达
(GMT+03:00) 科威特，利雅得
(GMT+03:00) 第比利斯
(GMT+03:00) 莫斯科，圣彼得堡, 伏尔加格勒
(GMT+03:30) 德黑兰
(GMT+04:00) 埃里温
(GMT+04:00) 巴库
(GMT+04:00) 阿布扎比，马斯喀特
(GMT+04:00) 高加索标准时间
(GMT+04:30) 喀布尔
(GMT+05:00) 伊斯兰堡，卡拉奇，塔什干
(GMT+05:00) 叶卡捷琳堡
(GMT+05:30) 斯里哈亚华登尼普拉
(GMT+05:30) 马德拉斯，加尔各答，孟买，新德里
(GMT+05:45) 加德满都
(GMT+06:00) 阿拉木图，新西伯利亚
(GMT+06:00) 阿斯塔纳，达卡
(GMT+06:30) 仰光
(GMT+07:00) 克拉斯诺亚尔斯克
(GMT+07:00) 曼谷，河内，雅加达
(GMT+08:00) 伊尔库茨克，乌兰巴图
(GMT+08:00) 北京，重庆，香港特别行政区，乌鲁木齐
(GMT+08:00) 台北
(GMT+08:00) 吉隆坡，新加坡
(GMT+08:00) 珀斯
(GMT+09:00) 大坂，札幌，东京
(GMT+09:00) 汉城
(GMT+09:00) 雅库茨克
(GMT+09:30) 达尔文
(GMT+09:30) 阿德莱德
(GMT+10:00) 关岛，莫尔兹比港
(GMT+10:00) 堪培拉，墨尔本，悉尼
(GMT+10:00) 布里斯班
(GMT+10:00) 符拉迪沃斯托克
(GMT+10:00) 霍巴特
(GMT+11:00) 马加丹，索罗门群岛，新喀里多尼亚
(GMT+12:00) 奥克兰，惠灵顿
(GMT+12:00) 斐济，堪察加半岛，马绍尔群岛
(GMT+13:00) 努库阿洛法
(GMT-01:00) 亚速尔群岛
(GMT-01:00) 佛得角群岛
(GMT-02:00) 中大西洋
(GMT-03:00) 巴西利亚
(GMT-03:00) 布宜诺斯艾利斯，乔治敦
(GMT-03:00) 格陵兰
(GMT-03:00) 蒙得维的亚
(GMT-03:30) 纽芬兰
(GMT-04:00) 圣地亚哥
(GMT-04:00) 大西洋时间(加拿大)
(GMT-04:00) 拉巴斯
(GMT-04:00) 马瑙斯
(GMT-04:30) 加拉加斯
(GMT-05:00) 东部时间(美国和加拿大)
(GMT-05:00) 印地安那州(东部)
(GMT-05:00) 波哥大，利马，里奥布朗库
(GMT-06:00) 中美洲
(GMT-06:00) 中部时间(美国和加拿大)
(GMT-06:00) 瓜达拉哈拉，墨西哥城，蒙特雷(新)
(GMT-06:00) 瓜达拉哈拉，墨西哥城，蒙特雷(旧)
(GMT-06:00) 萨斯喀彻温
(GMT-07:00) 亚利桑那
(GMT-07:00) 奇瓦瓦，拉巴斯，马扎特兰(新)
(GMT-07:00) 奇瓦瓦，拉巴斯，马萨特兰(旧)
(GMT-07:00) 山地时间(美国和加拿大)
(GMT-08:00) 太平洋时间(美国和加拿大)
(GMT-08:00) 蒂华纳，下加利福尼亚州
(GMT-09:00) 阿拉斯加
(GMT-10:00) 夏威夷
(GMT-11:00) 中途岛，萨摩亚群岛
(GMT-12:00) 日界线西
 
         * */

    }
}
