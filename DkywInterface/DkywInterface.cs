using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.SqlClient;

public class DkywInterface : MarshalByRefObject
{
    public const int WM_USER = 0x0400;
    public const int WM_THREADEND = WM_USER + 200;
    public const int WM_GETPORT = WM_USER + 201;
    public const int WM_DISABLESENDKEY = WM_USER + 202;  // 进入扣款状态
    public const int WM_ENABLESENDKEY = WM_USER + 203;    // 退出扣款状态
    public const int WM_GETNEXTSEQUENCENUMBER = WM_USER + 204;    // 获得流水号


    [DllImport("user32")]
    public static extern bool
        PostMessage(IntPtr hWnd, int Msg,
        int wParam, int lParam);

    // SendMessage
    [DllImport("user32")]
    public static extern IntPtr
        SendMessage(IntPtr hWnd, uint Msg,
        UIntPtr wParam, IntPtr lParam);

    [DllImport("user32")]
    public static extern IntPtr
        SendMessage(IntPtr hWnd, int Msg,
        IntPtr wParam, IntPtr lParam);

    [DllImport("user32")]
    public static extern IntPtr
        SendMessage(IntPtr hWnd, uint Msg,
        int wParam, int lParam);

    // 
    /*
1、rf_link_com_pro():打开串口
用  法: rf_link_com_pro(incom : integer) : integer
参  数: incom     : 连接读卡器的串口,分别表示COM1到COM4(1..4)
返回值: 返回值如下
     =0 成功
 * */
    [DllImport("rf_card_pro.dll")]
    public static extern int rf_link_com_pro(int com_port);

    /*
2、rf_unlink_com_pro():关闭串口
 用  法: rf_unlink_com_pro(incom : integer) : integer
 参  数: incom     : 连接读卡器的串口,分别表示COM1到COM4(1..4)
 返回值: 返回值如下
         =0 成功
     * */
    [DllImport("rf_card_pro.dll")]
    public static extern int rf_unlink_com_pro(int com_port);

    /*
3、rf_Beep_pro():蜂鸣
 用  法: rf_Beep_pro(nums : integer) : integer
 参  数:  nums  : 蜂鸣次数
 返回值: 返回值如下
         =0 成功
     * */
    [DllImport("rf_card_pro.dll")]
    public static extern int rf_Beep_pro(int times);

    /*
4、rf_card_p():判断读卡器上是否有卡
 用  法: rf_card_p() : integer
 参  数: 无参数
 返回值: 返回值如下
         =0 成功  表示有卡
     * */
    [DllImport("rf_card_pro.dll")]
    public static extern int rf_card_p();

    /*
5、ReadCardNo_pro():读取卡编号
 用  法: ReadCardNo_pro(incom : integer;
     * user_code :pchar;
     * card_key : pchar;
     * Card_id : pchar;
     * Card_no : pchar) : integer
 参  数: incom     : 连接读卡器的串口,分别表示COM1到COM4(1..4)
     user_code : 用户代码
     card_key  : 卡密钥
         card_id   : 返回卡的唯一序列号
         card_no   : 返回卡编号
 返回值: 返回值如下
         =0 成功  
     * */
    [DllImport("rf_card_pro.dll")]
    public static extern int ReadCardNo_pro(int com_port,
        string user_code,
        string card_key,
        StringBuilder card_id,
        StringBuilder card_no);

    /*
7．ReadCardMsg(): 读卡的唯一序列号、卡号和卡内金额
     用  法: code=ReadCardMsg(incom : integer;
     * user_code :pchar;
     * card_key : pchar;
     * Card_id : pchar;
     * Card_no : pchar;
     * limit_money : pchar) : integer
     参  数: incom     : 连接读卡器的串口,分别表示COM1到COM4(1..4)
	     user_code : 用户代码
	     card_key  : 卡密钥
             card_id   : 返回卡的唯一序列号
             card_no   : 返回卡号
            limit_money: 返回卡内消费限额(以分为单位)	  
	
     返回值: 返回值如下
                >0: 成功,返回卡内金额,card_id(卡的唯一序列号),card_no(返回卡号),limit_money(返回卡内消费限额);
		-1:连接串口错误;
                -2:没有发现卡片;
                -3:无法读取卡的唯一序列号; 
                -4:装入密钥错误;
		-5:读卡错误;

     说  明: 此函数用来读卡的唯一序列号、卡号和卡内金额
     举  例: code=ReadCardMsg(1,'12345678','1122334455667788','','');
   
     * */
    [DllImport("rf_card_pro.dll")]
    public static extern int ReadCardMsg(int com_port,
        string user_code,
        string card_key,
        StringBuilder card_id,
        StringBuilder card_no,
        StringBuilder limit_money);

    /*
8、rf_WriteCard():从卡中减钱函数
     用  法: code=rf_WriteCard(incom : integer;
     user_code :pchar;
     card_key : pchar;
     user_pwd : pchar;
     inmoney : integer;
     card_money : pchar) : integer
     参  数: incom     : 连接读卡器的串口,分别表示COM1到COM4(1..4)
             user_code : 用户代码
	     card_key  : 卡密钥
             user_pwd  : 用户密码
             inmoney   : 要从卡中扣去的金额(以分为单位)	
             card_money: 返回卡内余额
     返回值: 返回值如下
                >0 : 成功,返回写卡后的累计用卡次数;
		-1:连接串口错误;
                -2:没有发现卡片;
                -3:无法读取卡的唯一序列号; 
                -4:装入密钥错误;
		-5:读卡错误;
		-6:卡已过有有效期;
		-7:密码错误
		-8:输入的金额太大;
		-9:写卡失败;
     说  明: 此函数用来从用户的卡中扣钱。
　　 举  例: code=rf_WriteCard(1,'12345678','1122334455667788','123456',200);
     * */
    [DllImport("rf_card_pro.dll")]
    public static extern int rf_WriteCard(int com_port,
        string user_code,
        string card_key,
        string user_password,
        int sub_money,
        StringBuilder card_money);

    static string GetErrorString(int nErrorCode)
    {
        switch (nErrorCode)
        {
            case -1:
                return "连接串口错误";
            case -2:
                return "没有发现卡片";
            case -3:
                return "无法读取卡的唯一序列号";
            case -4:
                return "装入密钥错误";
            case -5:
                return "读卡错误";
            case -6:
                return "卡已过有有效期";
            case -7:
                return "密码错误";
            case -8:
                return "输入的金额太大";
            case -9:
                return "写卡失败";
        }

        return "未知的错误 " + nErrorCode.ToString();
    }

    public DkywInterface()
    {
        // Console.WriteLine("Constructor called");
    }

    // 禁止SendKey状态
    public void DisableSendKey()
    {
        Form form = Application.OpenForms[0];
        IntPtr r = SendMessage(form.Handle,
            WM_DISABLESENDKEY,
            0,
            0);
    }

    // 恢复SendKey状态
    public void EnableSendKey()
    {
        Form form = Application.OpenForms[0];
        IntPtr r = SendMessage(form.Handle,
            WM_ENABLESENDKEY,
            0,
            0);
    }
    /*
    // 获得驻留程序配置的参数
    // return:
    //      -1  error
    //      0   not found label control
    //      1   succeed
    static int GetParameters(out int nPort,
        out string strUserCode,
        out string strCardKey,
        out string strError)
    {
        nPort = 0;
        strUserCode = "";
        strCardKey = "";
        strError = "";

        Form form = Application.OpenForms[0];
        for(int i=0;i<form.Controls.Count;i++)
        {
            Control control = form.Controls[i];

            if (control is Label)
            {
                string strText = ((Label)control).Text;
                if (strText.Length > 0 && strText[0] == '@')
                {
                    strText = strText.Substring(1);

                    string [] parts = strText.Split(new char [] {','});
                    if (parts.Length != 3)
                    {
                        strError = "参数格式错误。应当为3部分";
                        return -1;
                    }

                    try {
                    nPort = Convert.ToInt32(parts[0]);
                    }
                    catch {
                        strError = "端口号数字格式错误 '" + parts[0] + "'";
                        return -1;
                    }

                    strUserCode = parts[1];
                    strCardKey = parts[2];
                    return 1;
                }
            }

        }

        strError = "not found label control";
        return 0;
    }
    */

        // 获得驻留程序配置的参数
    // 包装版本
    // return:
    //      -1  error
    //      0   not found label control
    //      1   succeed
    static int GetParameters(out int nPort,
        out string strUserCode,
        out string strCardKey,
        out string strError)
    {
        string strSqlConnectionString = "";
        string strSqlDbName = "";
        string strAccount = "";
        string strSubject = "";

        return GetParameters(out nPort,
            out strUserCode,
            out strCardKey,
            out strSqlConnectionString,
            out strSqlDbName,
            out strAccount,
            out strSubject,
            out strError);
    }

    // 获得驻留程序配置的参数
    // return:
    //      -1  error
    //      0   not found label control
    //      1   succeed
    static int GetParameters(out int nPort,
        out string strUserCode,
        out string strCardKey,
        out string strConnectionString,
        out string strSqlDbName,
        out string strAccount,
        out string strSubject,
        out string strError)
    {
        nPort = 0;
        strUserCode = "";
        strCardKey = "";
        strConnectionString = "";
        strSqlDbName = "";
        strAccount = "";
        strSubject = "";
        strError = "";

        Form form = Application.OpenForms[0];
        for (int i = 0; i < form.Controls.Count; i++)
        {
            Control control = form.Controls[i];

            if (control is Label)
            {
                string strText = ((Label)control).Text;
                if (strText.Length > 0 && strText[0] == '@')
                {
                    strText = strText.Substring(1);

                    string[] parts = strText.Split(new char[] { '|' });
                    if (parts.Length != 7)
                    {
                        strError = "参数格式错误。应当为7部分";
                        return -1;
                    }

                    try
                    {
                        nPort = Convert.ToInt32(parts[0]);
                    }
                    catch
                    {
                        strError = "端口号数字格式错误 '" + parts[0] + "'";
                        return -1;
                    }

                    strUserCode = parts[1];
                    strCardKey = parts[2];
                    strConnectionString = parts[3];
                    strSqlDbName = parts[4];
                    strAccount = parts[5];
                    strSubject = parts[6];
                    return 1;
                }
            }

        }

        strError = "not found label control";
        return 0;
    }

    // parameters:
    //      strRest    返回金额。以元为单位
    //      nErrorCode 原始错误码
    //          -1:连接串口错误;
    //          -2:没有发现卡片;
    //          -3:无法读取卡的唯一序列号; 
    //          -4:装入密钥错误;
    //          -5:读卡错误;
    // return:
    //      -1  出错
    //      0   没有卡
    //      1   成功获得信息
    public int GetCardInfo(out string strCardNumber,
        out string strRest,
        out string strLimitMoney,
        out int nErrorCode,
        out string strError)
    {
        strError = "";
        strCardNumber = "";
        strRest = "";
        strLimitMoney = "";
        nErrorCode = 0;

        int nPort = 0;
        string strUserCode = "";
        string strCardKey = "";

        // 获得参数
        // return:
        //      -1  error
        //      0   not found label control
        //      1   succeed
        int nRet = GetParameters(out nPort,
            out strUserCode,
            out strCardKey,
            out strError);
        if (nRet != 1)
            return -1;

        StringBuilder card_id = new StringBuilder(255);
        StringBuilder card_no = new StringBuilder(255);
        StringBuilder limit_money = new StringBuilder(255);


        nRet = ReadCardMsg(nPort,
            strUserCode,
            strCardKey,
            card_id,
            card_no,
            limit_money);

        if (nRet > 0)
        {
            strCardNumber = card_no.ToString();
            strLimitMoney = limit_money.ToString();

            // 转换为元
            Decimal v = Convert.ToDecimal(strLimitMoney);
            v = v / 100;

            strLimitMoney = v.ToString();

            // 转换为元
            v = Convert.ToDecimal(nRet);
            v = v / 100;
            strRest = v.ToString();

            return 1;
        }

        if (nRet == -2)
        {
            strError = "阅读器上没有IC卡";
            return 0;
        }

        strError = GetErrorString(nRet) + "。错误码:" + nRet.ToString();

        nErrorCode = nRet;
        return -1;
    }

    // 获得流水号。每获得一次，自动增量一次
    int GetSequenceNumber()
    {
        Form form = Application.OpenForms[0];
        IntPtr r = SendMessage(form.Handle,
            WM_GETNEXTSEQUENCENUMBER,
            0,
            0);

        return r.ToInt32();
    }

    // 扣款
    // parameters:
    //      strCardNumber   要求的卡号。如果为空，则表示不要求卡号，直接从当前卡上扣款
    //      strSubMoney 要扣的款额。例如："0.01"
    //      strUsedCardNumber   实际扣款的卡号
    //      strRest    扣款后的余额
    //      nErrorCode 原始错误码
    //          -1:连接串口错误;
    //          -2:没有发现卡片;
    //          -3:无法读取卡的唯一序列号; 
    //          -4:装入密钥错误;
    //          -5:读卡错误;
    //          -6:卡已过有有效期;
    //          -7:密码错误
    //          -8:输入的金额太大;
    //          -9:写卡失败;
    // return:
    //      -1  出错
    //      0   没有卡
    //      1   成功扣款和获得信息
    //      2   虽然扣款成功，但是上传流水失败
    public int SubCardMoney(string strCardNumber,
        string strSubMoney,
        string strPassword,
        out string strUsedCardNumber,
        out string strRest,
        out int nErrorCode,
        out string strError)
    {
        strError = "";
        strCardNumber = "";
        strRest = "";
        strUsedCardNumber = "";
        nErrorCode = 0;

        int nPort = 0;
        string strUserCode = "";
        string strCardKey = "";
        string strSqlConnectionString = "";
        string strSqlDbName = "";
        string strAccount = "";
        string strSubject = "";

        // 获得参数
        // return:
        //      -1  error
        //      0   not found label control
        //      1   succeed
        int nRet = GetParameters(out nPort,
            out strUserCode,
            out strCardKey,
            out strSqlConnectionString,
            out strSqlDbName,
            out strAccount,
            out strSubject,
            out strError);
        if (nRet != 1)
            return -1;

        int nProcAcc = 0;

        try
        {
            nProcAcc = Convert.ToInt32(strAccount);
        }
        catch
        {
            strError = "帐户 '" + strAccount + "' 应当为纯数字值";
            return -1;
        }

        StringBuilder card_id = new StringBuilder(255);
        StringBuilder card_no = new StringBuilder(255);
        StringBuilder limit_money = new StringBuilder(255);

        // string strThisCardNumber = "";

        nRet = ReadCardMsg(nPort,
            strUserCode,
            strCardKey,
            card_id,
            card_no,
            limit_money);
        if (nRet > 0)
        {
            strUsedCardNumber = card_no.ToString();

            // 转换为元
            Decimal v = Convert.ToDecimal(nRet);
            v = v / 100;
            strRest = v.ToString();
        }
        else
        {
            if (nRet == -2)
            {
                strError = "阅读器上没有IC卡";
                return 0;
            }

            strError = GetErrorString(nRet) + "。ReadCard错误码:" + nRet.ToString();

            nErrorCode = nRet;
            return -1;
        }

        // 判断卡号
        if (String.IsNullOrEmpty(strCardNumber) == false)
        {
            if (strCardNumber != strUsedCardNumber)
            {
                strError = "读卡器上放的卡号 '" + strUsedCardNumber + "' 不是所请求的卡号 '" + strCardNumber + "'";
                return -1;
            }
        }

        // 判断余额是否足够

        /* 测试 制造扣款错误
        strError = "test error";
        nErrorCode = -6;
        return -1;
         * */

        /* 模拟测试密码
        if (strPassword != "9")
        {
            strError = "test error";
            nErrorCode = -7;
            return -1;
        }*/


        // 换算为分单位
        decimal sub_v = Convert.ToDecimal(strSubMoney);
        int sub_money = Convert.ToInt32(sub_v * 100);

        StringBuilder card_money = new StringBuilder(255);

        // 扣款
        nRet = rf_WriteCard(nPort,
            strUserCode,
            strCardKey,
            strPassword,
            sub_money,
            card_money);

        if (nRet > 0)
        {
            int nUseCount = nRet;   // 用卡次数

            // 扣款前的余额
            strRest = card_money.ToString();

            int nBalance = 0;

            try
            {
                nBalance = Convert.ToInt32(strRest);
            }
            catch
            {
                strError = "扣款前余额 '" + strRest + "' 格式不正确，应当为纯数字";
                return -1;
            }

            // 转换为元
            Decimal v = Convert.ToDecimal(strRest);
            v = v / 100;

            // 新的余额
            v -= sub_v;

            strRest = v.ToString();


            // 上传流水
            if (String.IsNullOrEmpty(strSqlConnectionString) == false)
            {
                int nSequenceNumber = GetSequenceNumber();

                string strDateTime = "";

                strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                // parameters:
                //      nAmount 扣款额。以分为单位
                //      nCount  累计用卡次数(注意用API返回值+1的值)
                //      nBalance    扣款前的余额，以分为单位
                //      nSequeceNumber  流水号
                //      strDateTime 交易日期 14位字符
                //      nProcAcc    帐号
                //      strSubject  科目代码
                nRet = UploadInfo(
                    card_no.ToString().TrimStart(new char[] { '0' }),
                    card_id.ToString().TrimStart(new char[] { '0' }),
                    sub_money,
                    nUseCount + 1,
                    nBalance,
                    nSequenceNumber,
                    strSqlConnectionString,
                    strSqlDbName,
                    strDateTime,
                    nProcAcc,
                    strSubject,
                    out strError);
                if (nRet == -1)
                {
                    strError = "虽然上传流水失败，但是从IC卡扣款已经成功。错误原因: " + strError;
                    return 2;
                }
            }

            return 1;
        }

        if (nRet == -2)
        {
            strError = "阅读器上没有IC卡";
            return 0;
        }

        strError = GetErrorString(nRet) + "。WriteCard错误码:" + nRet.ToString();

        nErrorCode = nRet;
        return -1;
    }

    // parameters:
    //      nAmount 扣款额。以分为单位
    //      nCount  累计用卡次数(注意用API返回值+1的值)
    //      nBalance    扣款前的余额，以分为单位
    //      nSequeceNumber  流水号
    //      strDateTime 交易日期 14位字符
    //      nProcAcc    帐号
    //      strSubject  科目代码
    int UploadInfo(
        string strCardNo,
        string strCardID,
        int nAmount,
        int nCount,
        int nBalance,
        int nSequanceNumber,
        string strConnectionString,
        string strSqlDbName,
        // string strSystemNo,
        // string strComputerCode,
        string strDateTime,
        int nProcAcc,
        string strSubject,
        out string strError)
    {
        strError = "";

        try
        {
            SqlConnection connection = new SqlConnection(strConnectionString);
            connection.Open();

            SqlCommand command = new SqlCommand("", connection);

            string strCommand = "";
            strCommand = "use " + strSqlDbName;

            strCommand += " INSERT INTO Jour_list "
    + " (CARDNO,CARDID,TRCD,OLDTRCD,TRANAMT,SYSTEMNO,CompCode,PosCode,PassWord,JOURNO,JNTOTAL,BALANCE,JDATETIME,PROCACC,SUBJECT,JFlag) "
    + " VALUES(@CARDNO,@CARDID,@TRCD,@OLDTRCD,@TRANAMT,@SYSTEMNO,@CompCode,@PosCode,@PassWord,@JOURNO,@JNTOTAL,@BALANCE,@JDATETIME,@PROCACC,@SUBJECT,@JFlag)";

            // 卡编号
            command.Parameters.Add("@CARDNO",
                SqlDbType.VarChar).Value = strCardNo;

            // 卡序列号
            command.Parameters.Add("@CARDID",
                SqlDbType.VarChar).Value = strCardID;

            // 交易代码
            command.Parameters.Add("@TRCD",
                SqlDbType.Char).Value = "1210";

            // 原交易代码
            command.Parameters.Add("@OLDTRCD",
                SqlDbType.Char).Value = "";

            // 扣款发生额
            command.Parameters.Add("@TRANAMT",
                SqlDbType.Int).Value = nAmount;

            // 系统代码
            command.Parameters.Add("@SYSTEMNO",
                SqlDbType.Char).Value = "0003";

            // 本地计算机编码
            command.Parameters.Add("@CompCode",
                SqlDbType.Char).Value = "0000";

            // POS代码
            command.Parameters.Add("@PosCode",
                SqlDbType.Char).Value = "1616";

            // 密码
            command.Parameters.Add("@PassWord",
                SqlDbType.Char).Value = "";

            // 流水号
            command.Parameters.Add("@JOURNO",
                SqlDbType.Int).Value = nSequanceNumber;

            // 累计用卡次数
            command.Parameters.Add("@JNTOTAL",
                SqlDbType.Int).Value = nCount;

            // 交易前余额
            command.Parameters.Add("@BALANCE",
                SqlDbType.Int).Value = nBalance;

            // 交易日期
            command.Parameters.Add("@JDATETIME",
                SqlDbType.Char).Value = strDateTime;

            // 帐号
            command.Parameters.Add("@PROCACC",
                SqlDbType.Int).Value = nProcAcc;

            // 科目代码
            command.Parameters.Add("@SUBJECT",
                SqlDbType.Char).Value = strSubject; // "0243010000"

            command.Parameters.Add("@JFlag",
                SqlDbType.Char).Value = "0";

            command.CommandText = strCommand;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                strError = "SQL命令执行出错，原因：" + ex.Message;
                return -1;
            }
            strCommand = "";
            command.Parameters.Clear();
        }
        catch (Exception ex)
        {
            strError = "Exception: " + ex.Message;
            return -1;
        }
        finally
        {
        }
        return 0;
    }

    /*
    public string Greeting(string name)
    {
        Form form = Application.OpenForms[0];
        IntPtr r = SendMessage(form.Handle,
            WM_GETPORT,
            0,
            0);

        return "Hello," + name + "  port:" + r.ToInt32().ToString();

        // return ChildTree(0, ForegroundWindow.Instance.Handle);
    }*/

}


