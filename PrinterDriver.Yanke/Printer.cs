using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PrinterDriver.Yanke
{
    class Printer
    {
        public static int POS_SUCCESS = 1001; // 函数执行成功

        public static int POS_FAIL = 1002; // 函数执行失败

        public static int POS_ERROR_INVALID_HANDLE = 1101;// 端口或文件的句柄无效

        public static int POS_ERROR_INVALID_PARAMETER = 1102; // 参数无效

        public static int POS_ERROR_NOT_BITMAP = 1103;// 不是位图格式的文件

        public static int POS_ERROR_NOT_MONO_BITMAP = 1104; // 位图不是单色的

        public static int POS_ERROR_BEYONG_AREA = 1105; // 位图超出打印机可以
                                                        // 处理的大小

        public static int POS_ERROR_INVALID_PATH = 1106; // 没有找到指定的文件
                                                         // 路径或名称

        public static int POS_COM_DTR_DSR = 0x00; // 流控制为DTR/DST

        public static int POS_COM_RTS_CTS = 0x01; // 流控制为RTS/CTS 

        public static int POS_COM_XON_XOFF = 0x02; // 流控制为XON/OFF 

        public static int POS_COM_NO_HANDSHAKE = 0x03; // 无握手 

        public static int POS_OPEN_PARALLEL_PORT = 0x12; // 打开并口通讯端口 

        public static int POS_OPEN_USB_PORT = 0x13;// 打开USB通讯端口 

        public static int POS_OPEN_PRINTNAME = 0X14; //打开打印机驱动程序

        public static int POS_OPEN_NETPORT = 0X15; // 打开以太网打印机

        public static int POS_FONT_TYPE_STANDARD = 0x00; // 标准 ASCII

        public static int POS_FONT_TYPE_COMPRESSED = 0x01; // 压缩 ASCII 

        public static int POS_FONT_TYPE_UDC = 0x02; // 用户自定义字符

        public static int POS_FONT_TYPE_CHINESE = 0x03; // 标准 “宋体”

        public static int POS_FONT_STYLE_NORMAL = 0x00; // 正常

        public static int POS_FONT_STYLE_BOLD = 0x08; // 加粗

        public static int POS_FONT_STYLE_THIN_UNDERLINE = 0x80; // 1点粗的下划线

        public static int POS_FONT_STYLE_THICK_UNDERLINE = 0x100; // 2点粗的下划线

        public static int POS_FONT_STYLE_UPSIDEDOWN = 0x200; // 倒置（只在行首有效）

        public static int POS_FONT_STYLE_REVERSE = 0x400; // 反显（黑底白字）

        public static int POS_FONT_STYLE_SMOOTH = 0x800; // 平滑处理（用于放大时）

        public static int POS_FONT_STYLE_CLOCKWISE_90 = 0x1000; // 每个字符顺时针旋转 90 度

        public static int POS_PRINT_MODE_STANDARD = 0x00; // 标准模式（行模式）

        public static int POS_PRINT_MODE_PAGE = 0x01; // 页模式

        public static int POS_PRINT_MODE_BLACK_MARK_LABEL = 0x02; // 黑标记标签模式(部分打印机本身硬件支持)


        public static int POS_PRINT_MODE_WHITE_MARK_LABEL = 0x03; //白标记标签模式 (部分打印机本身硬件支持)
        public static int POS_PRINT_MODE_VIRTUAL_PAGE = 0x04; //虚拟页模式（动态库软件仿真）


        public static int POS_BARCODE_TYPE_UPC_A = 0x41; // UPC-A

        public static int POS_BARCODE_TYPE_UPC_E = 0x42; // UPC-C

        public static int POS_BARCODE_TYPE_JAN13 = 0x43; // JAN13(EAN13)

        public static int POS_BARCODE_TYPE_JAN8 = 0x44; // JAN8(EAN8)

        public static int POS_BARCODE_TYPE_CODE39 = 0x45; // CODE39

        public static int POS_BARCODE_TYPE_ITF = 0x46; // INTERLEAVED 2 OF 5

        public static int POS_BARCODE_TYPE_CODEBAR = 0x47; // CODEBAR

        public static int POS_BARCODE_TYPE_CODE93 = 0x48; // 25

        public static int POS_BARCODE_TYPE_CODE128 = 0x49; // CODE 128



        public static int POS_HRI_POSITION_NONE = 0x00; // 不打印

        public static int POS_HRI_POSITION_ABOVE = 0x01; // 只在条码上方打印

        public static int POS_HRI_POSITION_BELOW = 0x02;// 只在条码下方打印

        public static int POS_HRI_POSITION_BOTH = 0x03;// 条码上、下方都打印

        public static int POS_BITMAP_PRINT_NORMAL = 0x00; // 正常

        public static int POS_BITMAP_PRINT_DOUBLE_WIDTH = 0x01; // 倍宽

        public static int POS_BITMAP_PRINT_DOUBLE_HEIGHT = 0x02; // 倍高

        public static int POS_BITMAP_PRINT_QUADRUPLE = 0x03; // 倍宽且倍高

        public static int POS_CUT_MODE_FULL = 0x00;// 全切

        public static int POS_CUT_MODE_PARTIAL = 0x01;// 半切


        public static int POS_CUT_MODE_ALL = 0x02; //不区别半/全切刀类弄，直接切纸


        public static int POS_AREA_LEFT_TO_RIGHT = 0x0; // 左上角

        public static int POS_AREA_BOTTOM_TO_TOP = 0x1; // 左下角

        public static int POS_AREA_RIGHT_TO_LEFT = 0x2; // 右下角

        public static int POS_AREA_TOP_TO_BOTTOM = 0x3; // 右上角



        public static int POS_BITMAP_MODE_8SINGLE_DENSITY = 0x00; // 8点单密度 
        public static int POS_BITMAP_MODE_8DOUBLE_DENSITY = 0x01;// 8点双密度 
        public static int POS_BITMAP_MODE_24SINGLE_DENSITY = 0x20;// 24点单密度 
        public static int POS_BITMAP_MODE_24DOUBLE_DENSITY = 0x21;// 24点双密度 



        public static int PRINTER_TYPE = 1;   ///  1  清单打印机关干部 0   发票打印机






        public static int S_COMMUNICATION_OK = 0;   ///正常
        public static int S_COMMUNICATION_FAILED = -1010;    ///通讯故障
        public static int S_DEVICE_FAULT = 1011;    ///设备故障
        public static int S_PAPER_OUT = 1012;    ////缺纸
        public static int S_PAPER_JAM = 1013;     ///卡纸
        public static int S_PAPER_NEAR_END = 1014;   ///纸将尽
        public static int S_COVER_OPEN = 1015;   ///机盖（抬杆）打开  
        public static int S_DEVICE_BUSY = 1016;    ////设备忙


        ///状态返回值位定义
        public static int E_COMMUNICATION_OK = 0;   ///正常
        public static int E_COMMUNICATION_FAILED = 0X01;    ///通讯故障
        public static int E_DEVICE_FAULT = 0x02;    ///设备故障
        public static int E_PAPER_OUT = 0x04;    ////缺纸
        public static int E_PAPER_JAM = 0x08;     ///卡纸
        public static int E_PAPER_NEAR_END = 0x10;   ///纸将尽
        public static int E_COVER_OPEN = 0x20;   ///机盖（抬杆）打开  
        public static int E_DEVICE_BUSY = 0x40;    ////设备忙
        public static int E_OVER_TEMPERATURE = 0x80;    ////设备忙
        public static int E_CUTTER_ERROR = 0x0100;    ////切刀错误
        public static int E_READ_ERROR = 0x0200;    ////无数据读回
        public static int E_POWER_OFF = 0x0400;    ////机器断电



        //函数功能：通过系列号连接打印机设备,
        //参数: szSerial -- 设备系列号 
        //数据类型：字符串指针，长度最长12个,不区分大小写
        //返回值：0 -- 成功   -1  --  失败
        //数据类型：整型


        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        //int OpenUSBDeviceBySerial(char *szSerial);
        public static extern int OpenUSBDeviceBySerial(String szSerial);

        //函数功能：枚举正与主机相连的打印机设备系列号
        //参数: szSerial -- 设备系列号数组，主机程序需预先分配足够的内存空间 至少200 字节
        //数据类型：字符串指针
        //返回值：>0 返回枚举设备数   -1  --  失败
        //数据类型：整型

        //int EnumUSBDeviceSerials(char *szSerials[10]);
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]

        public static extern int EnumUSBDeviceSerials(StringBuilder sb);



        //HANDLE OpenUsbPrinterByName(LPCTSTR PrinterName);
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr OpenUsbPrtByName(String PrinterName);



        //函数功能：连接打印机设备
        //返回值：0 -- 成功   -1  --  失败
        /*
        iport:
        ///串口
        #define COM1	1
        #define COM2	2
        #define COM3	3
        #define COM4	4
        #define COM5	5
        #define COM6	6
        #define COM7	7
        #define COM8	8
        #define COM9	9
        #define COM10	10
        ///并口
        #define LPT1	11
        #define LPT2	12

        ///USB 口
        #define USB		13

        baud:
        波特率

        iflow:
        0  无流控
        1  DTR/DSR
        2  RTS/CTS
        3  XON/XOFF


        */


        //public static extern  int YkOpenDevice(int iport, int baud,int iflow=0);

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkOpenDevice(int iport, int baud, int iflow);

        //函数功能：断开打印设备
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        //public static extern  int YkCloseDevice();

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]

        public static extern int YkCloseDevice();

        //函数功能：获取设备的操作句柄
        //参数：无
        //返回值：>0 -- 句柄值   -1  --  失败
        //public static extern  int YkGetDeviceHandle();
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkGetDeviceHandle();


        //函数功能：设备是否已经到连接计算机(仅支持USB口打印机)
        //参数：无
        //返回值：>0 -- 连接   -1  --  没有连接
        //public static extern  int YkIsConnected();
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkIsConnected();


        //函数功能：初始化打印机 〈详见命令：ESC @〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkInitPrinter();

        //函数功能：把将要打印的字符串送入打印机缓冲区(注: 达到满行时会自动打印出来)
        //参数：pstr -- 将要送打印机的字符串数据缓冲，len -- 字符串数据长度
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrintStr(String pstr);

        //函数功能：打印并回车,但不走纸〈详见命令：CR〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnter();

        //函数功能：打印并换行，走纸到下一行首〈详见命令：LF〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkFeedPaper();

        //函数功能：页模式下，取消打印数据〈详见命令：CAN〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkCancelPrintInPM();

        //函数功能：实时响应主机请求,  〈详见命令：DLE ENQ n〉
        //参数：n=1：从错误状态恢复，从错误状态出现的行重新开始打印。
        //参数：n=2：清除接收缓冲区，打印缓冲区内容。
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkResponse(int n);

        //函数功能：Tab水平定位,从下一个水平定位点位置开始打印。〈详见命令：HT〉
        //参数：无 
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkTabMove();

        //函数功能：页模式下，打印数据〈详见命令：ESC FF〉
        //参数：无 
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrintInPM();

        //函数功能：设置西文字符右间距，以半点为设定单位〈详见命令：ESC SP n〉
        //参数：n = 0~255 ,default = 0
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetCharRightSpace(int n);

        //函数功能：设置字符打印方式〈详见命令：ESC ! n〉
        //参数：n = 0~255 ,default = 0
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetFontStyle(int n);

        //函数功能：设置绝对打印位置，〈详见命令:ESC $ nL nH〉
        //参数：nL -- 位置数值的低字节，nH -- 位置数值的高字节 ,0 ≤ (nL + nH × 256) ≤ 65535 (0 ≤ nL ≤ 255 , 0 ≤ nH ≤ 255)
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetAbsPrnPos(int nL, int nH);

        //函数功能：使能/禁用用户自定义字符户〈详见命令:ESC % n〉
        //参数：n = 0~255 ,default = 0
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnableUserDefineChar(int n);

        //函数功能：设置用户自定义字符， 〈详见命令：ESC & y c1 c2 [ x1 d1...d(y×x1)]...[ xk d1...d(y×xk)]〉
        //参数：c1, c2分别为起始码和终止码，最多为95个。
        //参数：字符宽度，以点为单位，视用什么字体大小而定
        //参数：code --自定义字符的内码值，例：使用Font B字体,把空格定义为黑块 内码为c1=0x20，c2=0x20，code[0]=0xff，code[1]=0xff...code[26]=0xff
        // YkSetUserDefineChar(0x20,0x20,9,code);
        ///以下数据详见说明书
        //y==3
        //0 ≤ x ≤ 12 [Font A (12 × 24)]
        //0 ≤ x ≤ 9 [Font B (9 × 17)]
        //k = c2 – c1 +1
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetUserDefineChar(int c1, int c2, int x, byte[] code);

        //函数功能：使能或禁用字符加下画线功能   〈详见命令：ESC - n〉
        //参数：n=1 使能，n=0 禁用，默认值n＝0。  
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnableUnderLine(int n);

        //函数功能：设置字符行距为默认值3.75毫米 〈详见命令：ESC 2〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetDefaultLineSpace();

        //设置字符行距 〈详见命令：ESC 3 n〉
        //参数：n=0~255,默认 n=30
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetLineSpace(int n);

        //函数功能：取消用户自定义字符, 〈详见命令：ESC ? n〉
        //参数：n=32~126
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkCancelUserDefChar(int n);

        //函数功能：设置水平制表位置 〈详见命令：ESC D〉
        //参数：tabstr --tab 位置值组成的字符串序列，如
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetTabPos(String tabstr);

        //函数功能：使能或禁用加重打印模式 〈详见命令： ESC E n〉
        //参数：n =0 禁用 n=1 使能
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetEmphasized(int n);

        //函数功能：使能或禁用重叠打印模式 〈详见命令： ESC G〉
        //参数：n =0 禁用 n=1 使能
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkOverlap(int n);

        //函数功能：打印后走纸 n 点行(注：可以精确走纸 1点行=0.125mm) 〈详见命令： ESC J n〉
        //参数：n =0~255
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrnAndFeedPaper(int n);

        //函数功能：进入页模式工作 〈详见命令：ESC L〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnablePM();

        //函数功能：选择字符字型  〈详见命令：ESC M〉
        //参数：n=0  选择字型 A (12×24); n=1 选择字型 B (9×17)
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSelectFont(int n);

        //函数功能：设置11个国家的不同ASCII字符集 〈详见命令：ESC R n〉
        //参数： n 国家代号
        /*
            n	 International character set

            0	 U.S.A.
            1	 France
            2	 Germany
            3	 U.K.
            4	 Denmark I
            5	 Sweden
            6	 Italy
            7	 Spain I
            8	 Japan
            9	 Norway
            10	 Denmark II
            11	 Spain II
            12	 Latin America
            13	 Korea
            14	 Slovenia / Croatia
            15	 China
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetInterCharSet(int n);

        //函数功能：从页模式切换到标准模式 〈详见命令：ESC S〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnterSM();

        //函数功能：页模式下，设置打印方向,  〈详见命令：ESC T n〉
        //参数：n=0~3 ; 
        /*
            n	Print direction		Starting position
            0	Left to right		Upper left
            1	Bottom to top		Lower left
            2	Right to left		Lower right
            3	Top to bottom		Upper right
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetDirectionInPM(int n);

        //函数功能：使能或禁用顺时针90度旋转字符打印,  〈详见命令：ESC V n〉
        //参数：n=0禁用顺时针90度旋转打印 n=1 使能顺时针90度旋转打印
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkClockwiseD90(int n);

        //函数功能：页模式下，设置设置打印区域，打印页长度范围(76~185mm),打印宽度(最大72mm) 〈详见命令：ESC W xL xH yL yH dxL dxH dyL dyH〉
        //参数：left 打印区域左上角x坐标 top 打印区域左上角y坐标 width  打印区域宽度 height 打印区域高度
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetPrnAreaInPM(int left, int top, int width, int height);

        //函数功能：设置相对打印位置     〈详见命令：ESC \ nL nH〉
        //参数：nL ,nH ;  实际位置= (nL + nH x 256)x0.125 毫米
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetRelPrnPos(int nL, int nH);

        //函数功能：设置打印时的对齐方式   〈详见命令：ESC  a n〉
        //参数：n=0 左对齐    n=1 居中  n=2	右对齐
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetAlign(int n);

        //函数功能：设置测纸传感器输出缺纸信号   〈详见命令：ESC c 3 n〉
        //参数：n=0~255 , 各位定义，详见命令说明书表格说明
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetPaperSensor(int n);

        //函数功能：设置纸尽时停止打印    〈详见命令：ESC c 4 n〉
        //参数：n=0~255 , 各位定义，详见命令说明书表格说明
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetSensorToStopPrint(int n);

        //函数功能：使能或禁用打印机面板上的开关  〈详见命令：ESC c 5 n〉
        //参数：n=0 禁用 n=1 使能
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnablePanelButton(int n);

        //函数功能：打印后走纸n字符行   〈详见命令：ESC d n〉
        //参数：n 字符行数
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrnAndFeedLine(int n);

        //函数功能：设置字符代码表      〈详见命令：ESC t n〉
        //参数：n 代码表代号 取值范围：0 ≤ n ≤ 5 , 16 ≤ n ≤ 19 , n = 255
        /*
        n	Character code table
        0	Page 0 [PC437 (USA: Standard Europe)]
        1	Page 1 [Katakana]
        2	Page 2 [PC850 (Multilingual)]
        3	Page 3 [PC860 (Portuguese)]
        4	Page 4 [PC863 (Canadian-French)
        5	Page 5 [PC865 (Nordic)]
        16	Page 16 [WPC1252]
        17	Page 17 [PC866 (Cyrillic #2)]
        18	Page 18 [PC852 (Latin 2)]
        19	Page 19 [PC858 (Euro)]
        255 Page 255 [User-defined page]
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetCharCodeTable(int n);

        //函数功能：走黑标纸到打印起始位置    〈详见命令：GS FF〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkFeedToStartPos();

        //函数功能：设置字符倍数             〈详见命令：GS ! n〉
        //参数：hsize 水平放大倍数 vsize 垂直放在倍数 ,取值范围0~7 
        /*
        hsize ,vsize 值与倍数对应关系
        0	1倍（原大小）
        1	2倍
        2	3倍
        3	4倍
        4	5倍
        5	6倍
        6	7倍
        7	8倍
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetCharSize(int hsize, int vsize);

        //函数功能：页模式下，设置打印区域内绝对垂直打印起始位置    〈详见命令：GS $ nL nH〉
        //参数：nL nH 位置值  位置 = (nL + nH x256) x 0.125 毫米
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetAbsVertPosInPM(int nL, int nH);

        //函数功能：进行测试打印         〈详见命令：GS ( A pL pH n m〉
        //参数：n,m
        /*
            n 指定测试时的纸张来源

            0	Basic sheet (roll paper)
            1	Roll paper
            2	Roll paper

            m 指定测试样式

            1	Hexadecimal dump print
            2	Printer status print
            3	Rolling pattern print

        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkDoTestPrint(int n, int m);



        //函数功能：进入或退出用户设置模式    〈详见命令:GS ( E pL pH 〉
        //参数：m=1 进入  m=2 退出
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkUserDefCmd(int m);



        //函数功能：设置MemorySwitch开关                    〈详见命令:GS ( E pL pH 〉 
        //参数：n=0~7对应MemorySwitch开关1～8。
        //参数：数组memory[]对应memory1~8每个开关值。             
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetMemorySwitch(int n, int[] memory);  ///???

        //函数功能：读取MemorySwitch开关值      〈详见命令:GS ( E pL pH〉
        //参数：n=0~7对应MemorySwitch开关1～8。
        //参数：数组memory[]用于存储读回来对应memory1~8每个开关值。             
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkGetMemorySwitch(int n, byte[] memory); ///???

        //函数功能：设置黑标的切撕纸位置和起始打印位置        〈详见命令:GS ( F pL pH a m〉
        //参数a =1 设置起始打印位置的设定值 a=2 设置开始切纸位置的设定值
        //参数m = 0 指定为前向进纸的方向 m = 1 指定为逆向进纸的方向
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetBlackMarkParam(int a, int m, int nL, int nH);

        //函数功能：(注：按工作状态功能有所不同)〈详见命令：FF〉
        //	页模式：打印后返回到标准模式
        //	黑标模式：打印后走黑标到打印起始位置。 
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrnAndBackToStd();

        //函数功能：定制打印机控制值。         〈详见命令:GS ( M pL pH a n m〉
        //参数：
        /*
            保存或者载入命令所定义的数据。

            n	功能

            1	将命令GS ( F 所设置的数据保存到用户NV 存储器。
            2	从用户NV 存储器载入命令GS ( F 所设置的数据。
            3	指定在初始设定时禁止或允许自动数据载入程序。

            m 指定数据如下：

            m = 0	与该规格参考手册所叙述的GS ( F 命令的初始设定值相同。
            m = 1	将被保存的存储区。
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkDefineControl(int n, int m);

        //函数功能：设置条码HRI字符的打印位置             〈详见命令:GS H n〉
        //参数：n=0 不打印,n=1 条形码上方,n=2条形码下方,n=3条形码的上方和下方
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetHRIPos(int n);

        //函数功能：读取打印机ID         〈详见命令:GS I n〉
        //参数：n=1~3   不同型号机型，具体内容不同，请查说明书
        /*
        n	打印机ID
        1	打印机型号ID 
        2	类型ID 见说明书类型表
        3	固件版本ID 
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkGetPrinterID(int n);

        //函数功能：设置左边距             〈详见命令:GS L nL nH〉
        //参数：nL nH  左边距=(nL + nH x 256) x 0.125 毫米
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetLeftMargin(int nL, int nH);


        //函数功能：打印位置设置为打印行起点         〈详见命令:GS T n〉
        //参数：n=0 删除打印缓冲区中的所有数据后设置打印位置为打印行起始点 n=1 将打印缓冲区中的所有数据打印后设置打印位置为打印行起始点
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkToLineHome(int n);

        //函数功能：执行切纸动作，包括进纸    〈详见命令：GS V m〉
        //参数：m=66 n:打印机进纸到(切纸位置+ [n × 0.125 毫米{0.0049英寸}])并切纸，一般n=0
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkCutPaper(int m, int n);

        //函数功能：设置打印区域宽度。           〈详见命令：GS W nL nH〉
        //参数：nL --打印区域宽度低字节 nH -- 打印区域宽度高字节
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetPrnAreaWidth(int nL, int nH);

        //函数功能：页模式下，设置相对于当前位置的垂直打印起点位置  〈详见命令：GS \ nL nH〉
        //参数：nL --位置低字节 nH -- 位置高字节
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetRelVertPosInPM(int nL, int nH);

        //函数功能：使能或禁用自动状态回复功能(ASB)        〈详见命令：GS a n〉
        //参数：0 -- 禁用  1 -- 使能
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnableASB(int n);

        //函数功能：使能或禁用平滑模式       〈详见命令：GS b n〉
        //参数：0 -- 禁用  1 -- 使能
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnableSmoothPrn(int n);

        // 函数功能：设置条码的HRI字符字型          〈详见命令：GS f n〉
        //参数：0 -- 字体A (12 × 24)  1 -- 字体B (9 × 17)
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetHRICharStyle(int n);

        //函数功能：设置条码高度             〈详见命令：GS h n〉
        //参数：n=1~255  垂直方向的点数
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetBarCodeHeight(int n);

        //函数功能：打印条码                  〈详见命令：GS k m n d1 ... dn〉
        //参数：m=65~75
        /*
        m	条形码系统		字符个数			备注
        65	UPC-A			11 ≤ n ≤ 12		48 ≤ d ≤ 57
        66	UPC-E			11 ≤ n ≤ 12		48 ≤ d ≤ 57
        67	JAN13 (EAN13)	12 ≤ n ≤ 13		48 ≤ d ≤ 57
        68	JAN8 (EAN8)		7 ≤ n ≤ 8			48 ≤ d ≤ 57
        69	CODE39			1 ≤ n ≤ 255		48 ≤ d ≤ 57, 65 ≤ d ≤ 90, 32, 36,37, 43, 45, 46, 47
        70	ITF				1 ≤ n ≤ 255 (n 为偶数) 48 ≤ d ≤ 57
        71	CODABAR			1 ≤ n ≤ 255		48 ≤ d ≤ 57, 65 ≤ d ≤ 68, 36, 43,45, 46, 47, 58
        72	CODE93			1 ≤ n ≤ 255		0 ≤ d ≤ 127
        73	CODE128			1 ≤ n ≤ 255		0 ≤ d ≤ 127
        74	标准EAN13		12 ≤ n ≤ 13		48 ≤ d ≤ 57
        75	标准EAN8		7 ≤ n ≤ 8			48 ≤ d ≤ 57
        */
        //参数：n 上表中字符个数
        //参数：barcode 要转化为条码的数据
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrintBarCode(int m, int n, String barcode);

        //函数功能：获取打印机状态,   〈详见命令：DLE EOT  n〉
        //参数：n = 1~5 
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkGetStatus(byte n);

        //函数功能：读取打印机状态                   〈详见命令：GS r n〉
        //参数：n=1 传送打印纸传感器状态
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkGetPrinterStatus(byte n);

        //函数功能：设置条码宽度              〈详见命令：GS w n〉
        //参数：参数n=2~6
        /*
                                                    二元条形码
          n		多元条形码单位宽度(毫米)	窄条宽度(毫米)		宽条宽度(毫米)
          2		0.250						0.250				0.625
          3		0.375						0.375				1.000
          4		0.560						0.500				1.250
          5		0.625						0.625				1.625
          6		0.750						0.750				2.000
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetBarCodeWidth(int n);

        //函数功能：设置汉字字符打印模式组合。〈详见命令：FS ! n〉
        //参数：n=0~255 详见命令说明书中表格
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetChineseFontStyle(int n);

        //函数功能：进入汉字打印方式。〈详见命令：FS &〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnableChineseMode();


        //函数功能：退出汉字打印方式。〈详见命令：FS .〉
        //参数：无
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkDisableChineseMode();

        //函数功能：使能或禁用汉字下划线模式。〈详见命令：FS - n〉
        //参数：n=0 禁用 n=1 使能
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnableChineseUnderLine(int n);


        //函数功能：定义用户自定义汉字 〈详见命令：FS 2 c1 c2 d1...dk〉
        //参数：c1、c2汉字用户自定义汉字区内的区位码，详见说明书表格，如汉字区GB18030,c1 = FE A1 ≤ c2 ≤ FE  ,k=72,数组d元素取值范围0~255
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetUserDefChinese(int c1, int c2, int[] code);

        //函数功能：设置用户自定义字符代码系统    〈详见命令：FS C n〉
        //参数：自定义字符代码系统代号 n= 0~1
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetUserDefChineseArea(int n);

        //函数功能：设置汉字字符左右间距   〈详见命令：FS S n1 n2〉
        //参数：左间距n1,右间距n2,0 ≤ n1 ≤ 255,0 ≤ n2 ≤ 255
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetChineseLeftRightSpace(int n1, int n2);

        //函数功能：使能或禁用汉字四倍大小打印。〈详见命令：FS W n〉
        //参数：n=0 禁用 n=1 使能
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetChinese4TimesSize(int n);

        //函数功能：使能或禁用颠倒打印模式〈详见命令: ESC { n〉
        //参数：n=0 禁用 n=1 使能
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnableUpsidedown(int n);

        //函数功能：打印预定义位图      〈详见命令: FS p n m〉
        //参数：m 打印密度
        /*  m	效果	垂直密度(dpi)	水平密度(dpi)
            0	普通	203				203
            1	倍宽	203				101
            2	倍高	101				203
            3	4倍		101				101
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrintDefineBitmap(int m, int n);

        //函数功能：下载预定义位图      〈详见命令: FS q n〉
        //参数：szBmpFile，要下载的位图的全路径。
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkDefineBitmap(String szBmpFile);

        //函数功能：即时打印BMP图象。    〈详见命令:ESC * m nL nH d1...dk 〉
        //参数：szBmpFile，要下载的位图的全路径。
        /*
            m		垂直密度	水平密度
            0		60			90
            1		60			180
            32		180			90
            33		180			180
            一般 m=33
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrintBitmap(String szBmpFile, int m);

        //函数功能：下装图形后即时打印。       〈详见命令：先执行GS * ，后执行 GS / m〉
        //参数：szBmpFile，要下载的位图的全路径。
        /*
            m	效果	垂直密度(dpi)	水平密度(dpi)
            0	普通	180			180
            1	倍宽	180			90
            2	倍高	90			180
            3	四倍	90			90
            一般 m=0
        */
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkDownloadBitmapAndPrint(String szBmpFile, int m);


        ///函数功能：设置钱箱驱动方式    〈详见命令： ESC p m t1 t2〉
        //参数：使用那个管脚输出脉冲  m=0  2脚  m=1  5脚    脉冲宽度为 t1*2ms t2*2ms ，其中要求 t1 < t2 ，一般 t1=150 t2=250
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkSetCashBoxDriveMode(int m, int t1, int t2);


        //函数功能：设置单状态回调函数(注：适用于能返回状态的打印机型号，如串口打印机)
        //参数：pCallBack -- 回调函数指针
        //返回值：0 -- 成功   -1  --  失败
        //          [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        //public static extern  int YkSetCallBack(CallBack pCallBack);

        //函数功能：使能打印状态回调函数，方便在回调函数根据打印机状态处理业务流程
        //参数：enable = 1 ，使能  =0 禁用
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkEnableCallBack(int enable);


        //函数功能：直接与设备数据交互通信(注: 不熟悉设备功能，不建议直接使用)
        //参数：pdata -- 将要送打印机的数据缓冲，len -- 数据长度
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkDirectIO(byte[] pdata, int len);


        //函数功能：写入打印机序列号（仅使用于杭州宽达公司）
        //参数：pdata -- 将要写入的打印机序列号的的数据缓冲,字符串以0结尾
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkWriteSerialNo(String pdata);

        //函数功能：读取打印机序列号（仅使用于杭州宽达公司）
        //参数：pdata -- 将要接收打印机序列号的的数据缓冲,字符串以0结尾
        //返回值：0 -- 成功   -1  --  失败
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkReadSerialNo(String pdata);

        //// iXpos  水平位置，单位：点
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrintRasterBmp(String szBmpFile, int iXpos);


        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkGetState();


        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrintBitmapMatrix(String szBmpFile);


        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkGetASBStatus();
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkPrintDownloadBitmap(int m);
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int YkDownloadBitmap(String szBmpFile);


        ////通用函数

        /*
        函数描述：设置字体
                 变量名称			类型		变量含义		备注
        入口参数：
                iInDoubleHieght		int			倍高
                iInDoubleWide		int			倍宽
                    iInUnderLine		int			下划线


        返回值：					int						0:成功
                                                            1:失败					
        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPrintFont(int iInDoubleHeight, int iInDoubleWide, int iInUnderLine);


        /*
        函数描述：设置左边距
                 变量名称			类型		变量含义		备注
        入口参数：
                iInDistance			int			左边距单位		 单位：0.1mm
        返回值：					int							0:成功
                                                                1:失败					
        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetLeftDistance(int iInDistance);

        /*
        函数描述：设置行高
                 变量名称			类型		变量含义		备注
        入口参数：
                iInDistance			int			行间距		 单位：0.1mm 等于0时默认高3.75mm
        返回值：					int							0:成功
                                                                1:失败					
        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetRowDistance(int iInDistance);


        /*
        函数描述：打印位图LOGO
                 变量名称			类型		变量含义		备注
        入口参数：
                pcInBmpFile			char*		Bmp文件名		支持相对路径和绝对路径，图片采用黑白单色bmp格式(一点占一位),分辩率300*100左右,大小不超过5k
        返回值：					int							0:成功
                                                                1:失败					
        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int PrintBmp(String pcInBmpFile);


        /*
        函数描述：打印字符
                 变量名称			类型		变量含义		备注
        入口参数：
                pcInstring			char*		需打印字符		不自动换行
        返回值：					int							0:成功
                                                                1:失败					
        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int PrintString(String pcInstring);


        /*
        函数描述：打印字符
                 变量名称			类型		变量含义				备注
        入口参数：
                pcInstring			char*		需打印的一行字符		自动换行
        返回值：					int							0:成功
                                                                1:失败					
        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int PrintLine(String pcInstring);


        /*
        函数描述：切纸
                 变量名称			类型		变量含义				备注
        入口参数：
                iInCutMode			int		切纸方式		0:全切  1:半切
        返回值：					int							0:成功
                                                                1:失败					
        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int CutPaper(int iInCutMode);


        /*
        函数描述：设置黑标切纸位置
                 变量名称			类型		变量含义				备注
        入口参数：
                iInn				int			黑标切纸位置		单位:0.1mm n=0:切纸到黑标位置 n>0切纸位置在黑标下边 n<0 切纸位置在黑标上边
        返回值：					int								0:成功
                                                                    1:失败					
        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetBlackMark(int iInn);


        /*
        函数描述：走纸
                 变量名称			类型		变量含义				备注
        入口参数：
                fInDistance			float		mm		单位:mm 
        返回值：					int								0:成功
                                                                    1:失败					
        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int FeedPaper(float fInDistance);


        //int WINAPI PrintRasterBmp(unsigned char *szBmpFileData,int len);


        /*
        函数描述：设置二维条码PDF417模块大小宽度
                 变量名称			类型		变量含义				备注
        入口参数：
                ModWidth			int       范围 2~8  ，默认= 3
        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPDF417ModWidth(int ModWidth);


        /*
        函数描述：设置二维条码PDF417模块大小高度
                 变量名称			类型		变量含义				备注
        入口参数：
                ModHeight			int       范围 2~8  ，默认= 3
        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPDF417ModHeight(int ModHeight);



        /*
        函数描述：设置二维条码PDF417纠错级别
                 变量名称			类型		变量含义				备注
        入口参数：
                Level			int       范围 1~40  ，默认= 1
        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPDF417Level(int Level);



        /*
        函数描述：设置或取消标准PDF417条码
                 变量名称			类型		变量含义				备注
        入口参数：
                iStd			int       当=0，设置标准PDF417条码 当=1，设置截短PDF417条码  默认为0
        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetStdPDF417(int iStd);



        /*
        函数描述：设置PDF417条码行列数
                 变量名称			类型		变量含义				备注
        入口参数：
                iRow,iCol			int       默认iCol=0;iRow=0;  0≤ iCol ≤ 30 ; iRow=0 或 3≤iRow ≤ 90; 
                当iCol=0，条码列数根据有效打印范围自动调整；
                当iCol≠0，设置条码列数为iCol列；
                当iRow=0，条码行数根据有效打印范围自动调整；
                当iRow≠0，设置条码行数为iRown行；

        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPDF417RowCol(int iRow, int iCol);



        /*
        函数描述：打印载PDF417条码
                 变量名称			类型		变量含义				备注
        入口参数：
                pCode  ,条码数据存放指针	unsigned char* 
                iLen, 条码数据长度    int   0<iLen<256

        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int PrintPDF417Code(byte[] pCode, int iLen);



        /*
        函数描述：设置QR条码的模块大小
                 变量名称			类型		变量含义				备注
        入口参数：
                ModWidth			int       范围 2~16  ，默认= 3
        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetQRModSize(int ModWidth);



        /*
        函数描述：设置QR条码的纠错等级
                 变量名称			类型		变量含义				备注
        入口参数：
                Level			int       范围 48~51  ，默认= 48
        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetQRLevel(int Level);



        /*
        函数描述：打印QR条码
                 变量名称			类型		变量含义				备注
        入口参数：
                pCode  ,条码数据存放指针	unsigned char* 
                iLen, 条码数据长度    int   0<iLen<256

        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int PrintQRCode(byte[] pCode, int iLen);
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        // 该命令将绝对打印位置设定在 毫米。命令：ESC $  @16
        public static extern int SetAbsPrnPos(float fPos);



        /*
        函数描述：打印PDF417或QR条码
                 变量名称			类型		变量含义				备注
        入口参数：
                iType  ,二维码类型   int   iType=10 PDF417 	1 ≤ 个数 ≤ 255	0 ≤ 数据值范围 ≤ 255 
                                           iType=11 QRCODE 	1 ≤ 个数 ≤ 928	0 < 数据值范围 ≤ 255
                pCode  ,条码数据存放指针	unsigned char* 
                iLen, 条码数据长度    int   0<iLen<256

        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Print2DCode(int iType, String pCode, int iLen);




        /*
        函数描述：打印DataMatrix二维条码
                 变量名称			类型		变量含义				备注
        入口参数：
                pCode  ,条码数据存放指针	unsigned char* 
                iLen, 条码数据长度    int   0<iLen<256

        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int PrintDataMatrixCode(byte[] pCode, int iLen, int iDotWidth);





        /*
        函数描述：打印PDF417二维条码
                 变量名称			类型		变量含义				备注
        入口参数：
                pCode  ,条码数据存放指针	unsigned char* 
                iLen, 条码数据长度    int   0<iLen<256

        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int PrintPDF417(byte[] pCode, int iLen, int iModSize, int iDotWidth);




        /*
        函数描述：初始化打印机机
                 变量名称			类型		变量含义				备注
        入口参数：  无


        返回值：					int								0:成功
                                                                    1:失败					

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int InitPrinter();



        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int PrintRaster24Bmp(String szBmpFile);




        //////2016-12-13 新定义一批通用（或标准）函数，以后此推广给客户，除客户特殊需求，另外增加函数 


        /////     以下函数暂不支持  （调试中）


        /*
        函数描述：打开端口。


        参数

        lpName

        [in] 指向以 null 结尾的打印机名称或端口名称。

        当参数nParam的值为POS_COM_DTR_DSR、POS_COM_RTS_CTS、POS_COM_XON_XOFF或POS_COM_NO_HANDSHAKE 时， “COM1”，“COM2”，“COM3”，“COM4”等表示串口；

        当参数nParam的值为POS_OPEN_PARALLEL_PORT时，“LPT1”，“LPT2”等表示并口；

        当参数nParam的值为POS_OPEN_USB_PORT时，“VUSB001”、“VUSB002”、“VUSB003”、“VUSB004”等表示USB端口。

        当参数nParam的值为POS_OPEN_PRINTNAME时，表示打开指定的打印机(注：要求安装windows打印机驱动程序才能正常使用)。

        当参数nParam的值为POS_OPEN_NETPORT时，表示打开指定的网络接口(打印机默认工作端口号为9100)，如“192.168.10.251”表示网络接口IP地址。


        nComBaudrate

        [in] 指定串口的波特率（bps）。 

        可以为以下值之一：

        9600，19200，38400，115200等。

        具体的值与打印机的型号有关


        nParam

        [in] 指定串口的流控制（握手）方式、或表示通讯方式。请参考参数lpName的说明。

        可以为以下值之一：

        Flag Value Meaning 
        POS_COM_DTR_DSR 0x00 流控制为DTR/DST  
        POS_COM_RTS_CTS 0x01 流控制为RTS/CTS 
        POS_COM_XON_XOFF 0x02 流控制为XON/OFF 
        POS_COM_NO_HANDSHAKE 0x03 无握手 
        POS_OPEN_PARALLEL_PORT 0x12 打开并口通讯端口 
        POS_OPEN_USB_PORT 0x13 打开USB通讯端口 
        POS_OPEN_PRINTNAME 0X14 打开打印机驱动程序 
        POS_OPEN_NETPORT 0x15 打开网络接口 

        其中前两项也统称为硬件流控制，一般选用 RTS/CTS 方式。



        返回值

        如果函数调用成功，返回一个已打开的端口句柄。

        如果函数调用失败，返回值为 INVALID_HANDLE_VALUE （-1）。



        备注

        1．如果打开的是并口（LPT1，LPT2）、USB或网络接口，那么后面的参数 nComBaudrate将被忽略，可以设置为0，并且参数nParam必需指定为POS_OPEN_PARALLEL_PORT。同样，打开USB端口或打印机驱动程序，nParam必需指定为相应的接口参数。

        2．如果由 lpName 指定的通讯端口被其他程序占用，那么返回值为 INVALID_HANDLE_VALUE。

        3．如果参数出错，也返回INVALID_HANDLE_VALUE。

        4．如果通讯端口已经打开，则会尝试关闭已经打开的端口，然后再去打开。

        5．如果参数nParam指定打开USB端口,且有多台USB打印机同时使用，则需要使用配置工具软体给每台打印机指定名称区别开。 

        6．另请参考 POS_Close，POS_Reset。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static UIntPtr POS_Open(

       String lpName,

       int nComBaudrate,

       int nParam

       );



        /*
        描述

        关闭已经打开的并口或串口，USB端口，网络接口或打印机。


        参数

        无。


        返回值　

        如果函数调用成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_Close();


        /*
        描述

        复位打印机，把打印缓冲区中的数据清除，字符和行高的设置被清除，打印模式被恢复到上电时的缺省模式。


        参数

        无



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE



        备注

        函数成功执行后，则：

        1．接收缓冲区中的指令保留。

        2．宏定义保留。

        3．Flash 中的位图保留。

        4．Flash 中的数据保留。

        5．DIP开关的设置不进行再次检测。

        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_Reset();



        /*
        描述

        设置打印机的打印模式。



        参数

        nPrintMode

        [in] 指定打印模式。

        可以为以下值之一：

        Flag Value Meaning 
        POS_PRINT_MODE_STANDARD 0x00 标准模式（行模式） 
        POS_PRINT_MODE_PAGE 0x01 页模式 (部分打印机本身硬件支持)
        POS_PRINT_MODE_BLACK_MARK_LABEL 0x02 黑标记标签模式 (部分打印机本身硬件支持)
        POS_PRINT_MODE_WHITE_MARK_LABEL 0x03 白标记标签模式 (部分打印机本身硬件支持)
        POS_PRINT_MODE_VIRTUAL_PAGE 0x04 虚拟页模式（动态库软件仿真）




        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1、部分打印机型号不支持黑标记标签模式和白标记标签模式，请参考相关用户手册。

        2、另请参考 POS_CutPaper。


        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_SetMode(

     int nPrintMode

     );


        /*


        描述

        设置打印机的移动单位。



        参数

        nHorizontalMU

        [in] 把水平方向上的移动单位设置为 25.4 / nHorizontalMU 毫米。

        可以为0到255。


        nVerticalMU

        [in] 把垂直方向上的移动单位设置为 25.4 / nVerticalMU 毫米。

        可以为0到255。




        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_SetMotionUnit(

int nHorizontalMU,

int nVerticalMU

);



        /*


        描述

        选择国际字符集和代码页。



        参数

        nCharSet

        [in] 指定国际字符集。不同的国际字符集对0x23到0x7E的ASCII码值对应的符号定义是不同的。

        可以为以下列表中所列值之一。

        Value Meaning 
        0x00 U.S.A 
        0x01 France 
        0x02 Germany 
        0x03 U.K. 
        0x04 Denmark I 
        0x05 Sweden 
        0x06 Italy 
        0x07 Spain I 
        0x08 Japan 
        0x09 Nonway 
        0x0A Denmark II 
        0x0B Spain II 
        0x0C Latin America 
        0x0D Korea 

        nCodePage

        [in] 指定字符的代码页。不同的代码页对0x80到0xFF的ASCII码值对应的符号定义是不同的。

        可以为以下列表中所列值之一。

        Value Meaning 
        0x00 PC437 [U.S.A. Standard Europe 
        0x01 Reserved 
        0x02 PC850 [Multilingual] 
        0x03 PC860 [Portuguese] 
        0x04 PC863 [Canadian-French] 
        0x05 PC865 [Nordic] 
        0x12 PC852 
        0x13 PC858 



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_PARAMETER

        POS_ERROR_INVALID_HANDLE



        备注

        1、有些打印机可能不支持所有字符集或代码页，详细信息请参考打印机配置样张或附带的用户手册。

        2、参数nCodePage值的可取范围为0到255，除表格中所列的其他值保留以后使用。



        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_SetCharSetAndCodePage(

int nCharSet,

int nCodePage

);


        /*


        描述

        向前走纸。



        参数

        无。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE



        备注

        1．如果在标准打印模式（行模式）下打印文本，则打印缓冲区中的数据，且打印位置自动移动到下一行的行首。

        2．如果在标准打印模式（行模式）下打印位图，则在指定的位置打印位图，且打印位置自动移动到下一行的行首。

        3．如果在页模式或标签模式下，则把需要打印的数据设置在指定的位置，同时把打印位置移动到下一个行首，但是并不立即进纸并打印，而是一直到调用 POS_PL_Print 函数时才打印。

        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_FeedLine();


        /*
        描述

        设置字符的行高。



        参数

        nDistance

        [in] 指定行高点数。

        可以为 0 到 255。每点的距离与打印头分辨率相关。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1、如果把行高设置为0，则打印机使用内部的默认行高值，即1/6英寸。如果打印头纵向分辨率为180dpi 则相当于 31 点高。

        2、如果行高被设置为小于当前的字符高度，则打印机将使用当前字符高度为行高。

        3、另请参考 POS_SetRightSpacing。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_SetLineSpacing(

     int nDistance

     );



        /*

        描述

        设置字符的右间距（相邻两个字符的间隙距离）。



        参数

        nDistance

        [in] 指定右间距的点数。

        可以为 0 到 255。每点的距离与打印头分辨率相关。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1、字符右间距的设置在标准模式和页模式或标签模式是独立的。

        2、如果字符放大，则字符右间距同倍放大。

        3、另请参考 POS_SetLineSpacing。


        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_SetRightSpacing(

int nDistance

);



        /*
        描述

        预下载一幅位图到打印机的 RAM 中，同时指定此位图的 ID 号。



        参数

        pszPath

        [in] 指向以 null 结尾的表示位图路径及其文件名的字符串。


        nID

        [in] 指定将要下载的位图的 ID 号。

        可以为 0 到 7。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PATH

        POS_ERROR_INVALID_PARAMETER

        POS_ERROR_NOT_BITMAP

        POS_ERROR_NOT_MONO_BITMAP

        POS_ERROR_BEYOND_AREA



        备注

        1．将要下载的位图大小不能超过 900 平方毫米 (大约为 240点 × 240 点)。

        2．位图必须是单色的。

        3．一般打印机内部的可用 RAM 空间为 8K 字节。

        4．位图的 ID 号不要求是连续的。

        5．另请参考POS_PreDownloadBmpsToFlash、POS_S_PrintBmpInRAM、POS_PL_PrintBmpInRAM。


        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_PreDownloadBmpToRAM(

String pszPath

);


        /*
        　

        描述

        预下载一幅或若干幅位图到打印机的 Flash 中。



        参数

        pszPaths

        [in] 指向包含若干位图的文件路径及其名称的字符串数组。


        nCount

        [in] 指定将要下载的位图幅数。

        可以为1 到 255。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS.

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_PATH

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER

        POS_ERROR_NOT_BITMAP

        POS_ERROR_NOT_MONO_BITMAP

        POS_ERROR_BEYONG_AREA



        备注

        1．每幅位图的数据大小不能超过 8K 字节（大约为 256 点 × 256 点）。

        2．位图必须为单色位图。

        3．下载到 Flash 中的位图的图号与位图的个数和排列顺序相关。位图的下载顺序和文件名在数组中的顺序一致，是连续的。如：第一个位图的图号为 1，第二个为 2，以此类推。

        4．每次下载都会把上次下载到 Flash 中的位图都清除。

        5．关电后不会被清除。

        6．打印机内部Flash 的可使用空间与具体打印机型号有关。

        7．另请参考POS_PreDownloadBmpToRAM、POS_S_PrintBmpInFlash。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int POS_PreDownloadBmpsToFlash(

char*[] pszPaths,

int nCount

);



        /*

        描述

        通过串口查询打印机当前的状态。此函数是非实时的。



        参数

        pszStatus

        [out] 指向返回的状态数据的缓冲区，缓冲区大小为 1 个字节。

        返回的各状态位意义如下表所示：

        Bit Status Meaning 
        0，1 0/1 容纸器中有纸 / 纸将用尽 
        2，3 0/1 打印头处有纸 / 无纸 
        4，5 0/1 钱箱连接器引脚 3 的电平为低 / 高（表示打开或关闭） 
        6，7 0 保留（固定为0） 

        nTimeouts

        [in] 设置查询状态时大约的超时时间（毫秒）。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE



        备注

        1．此函数在并口通讯中无效。

        2．部分型号的打印机在上盖打开或打印头抬起、缺纸、Feed键按下等情况下，不能返回打印机的状态。

        3．另请参考 POS_RTQueryStatus。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_QueryStatus(

ref byte pszStatus,

int nTimeouts

);


        /*

        描述

        通过串口返回当前打印机的状态。此函数是实时的。



        参数

        pszStatus

        [out] 指向接收返回状态的缓冲区，缓冲区大小为 1 个字节。 

        返回的各状态位意义如下表所示：

        Bit Status Meaning 
        0 0/1 钱箱连接器引脚 3 的电平为低/高（表示打开或关闭） 
        1 0/1 打印机联机/脱机 
        2 0/1 上盖关闭/打开 
        3 0/1 没有/正在由Feed键按下而进纸 
        4 0/1 打印机没有/有出错 
        5 0/1 切刀没有/有出错 
        6 0/1 有纸/纸将尽（纸将尽传感器探测） 
        7 0/1 有纸/纸用尽（纸传感器探测） 



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE



        备注

        1．此函数在并口通讯中无效。

        2．部分型号的打印机在上盖打开或打印头抬起、缺纸、Feed键按下等情况下，不能返回打印机的状态。

        3．另请参考 POS_QueryStatus。

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_RTQueryStatus(

ref byte pszStatus

);



        /*

        描述

        通过网络接口查询返回当前打印机的状态。



        参数

        ipAddress

        [in] 设备IP地址。如“192.168.10.251”。

        pszStatus

        [out] 指向接收返回状态的缓冲区，缓冲区大小为 1 个字节。 

        返回的各状态位意义如下表所示：

        Bit Status Meaning 
        0 0/1 钱箱连接器引脚 3 的电平为低/高（表示打开或关闭） 
        1 0/1 打印机联机/脱机 
        2 0/1 上盖关闭/打开 
        3 0/1 没有/正在由Feed键按下而进纸 
        4 0/1 打印机没有/有出错 
        5 0/1 切刀没有/有出错 
        6 0/1 有纸/纸将尽（纸将尽传感器探测） 
        7 0/1 有纸/纸用尽（纸传感器探测） 



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE


        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]

        public static extern int POS_NETQueryStatus(

     String ipAddress,

     ref char pszStatus

     );



        /*

        描述

        往钱箱引脚发送脉冲以打开钱箱。



        参数

        nID

        [in] 指定钱箱的引脚。

        可以为以下值之一：

        Value Meaning 
        0x00 钱箱连接器引脚2 
        0x01 钱箱连接器引脚5 

        nOnTimes

        [in] 指定往钱箱发送的高电平脉冲保持时间，即 nOnTimes × 2 毫秒。

        可以为1 到 255。


        nOffTimes

        [in] 指定往钱箱发送的低电平脉冲保持时间，即 nOffTimes × 2 毫秒。

        可以为1 到 255。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS.

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1．如果参数 nOffTimes 的值小于 nOnTimes, 则往钱箱发送的低电平脉冲的保持时间为nOnTimes × 2 毫秒。

        2．请参考钱箱供应商提供的相关资料。



        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_KickOutDrawer(

     int nID,

     int nOnTimes,

     int nOffTimes

     );



        /*
        描述

        切纸。



        参数

        nMode

        [in] 指定切纸模式。

        可以为以下值之一：

        Flag Value Meaning 
        POS_CUT_MODE_FULL 0x00 全切 
        POS_CUT_MODE_PARTIAL 0x01 半切 
        POS_CUT_MODE_ALL 0x02 不区别半/全切刀类弄，直接切纸

        nDistance

        [in] 指定进纸长度的点数。

        可以为 0 到 255。每点的距离与打印头分辨率相关。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1．如果指定为全切，则参数 nDistance 忽略。

        2．如果指定为半切，则打印机走纸 nDistance 点，然后切纸。

        3．另请参考 POS_SetMode。


        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_CutPaper(

int nMode,

int nDistance

);


        /*


        描述

        新建一个打印作业。



        参数

        无



        返回值

        如果函数成功，为TRUE；否则为FALSE。



        备注

        用户可以在多次作业开始时调用一次该函数，也可以每次作业调用此功能。




        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool POS_StartDoc();



        /*


        描述

        结束一个打印作业。


        参数

        无


        返回值

        如果函数成功，为TRUE；否则为FALSE。


        备注

        用户可以在多次作业结束时调用一次该函数，也可以每次作业调用此功能。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool POS_EndDoc();




        /*
        　

        描述

        开始把发往打印机（端口）的数据保存到指定的文件。



        参数

        lpFileName

        [in] 保存数据的文件名称，是null结尾的字符串。可以是绝对路径，也可以是相对路径。


        bToPrinter

        [in] 

        TRUE ：指定是否在保存数据到文件的同时，把数据也发送到打印机（端口）。 

        FALSE ：指定是否在保存数据到文件的同时，不把数据也发送到打印机（端口）。



        返回值

        无。



        备注

        1． 如果指定的文件存在，则以追加方式不断把数据保存到此文件；如果指定的文件不存在，则会创建，然后再以追加方式保存数据。

        2． 另请参考POS_EndSaveFile。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void POS_BeginSaveFile(

String lpFileName,

bool bToPrinter

);



        /*

        描述

        结束保存数据到文件的操作。


        参数

        无。 


        返回值

        无。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void POS_EndSaveFile();



        /*

        描述

        设置标准模式下的打印区域宽度。



        参数

        nWidth

        [in] 指定打印区域的宽度。

        可以为 0 到 65535点。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1．此函数只在行首有效。

        2．由于不同的打印机型号有不同的打印头宽度（或内部缓冲区的宽度），所以可打印区域的宽度是不同的。

        3．如果打印区域的宽度设置小于一个字符宽度，则当打印机接受打印一个字符时，会自动往右或左扩展到一个字符的宽度，当打印位图时，打印机会以同样的方式来扩展并打印。

        4．如果打印区域的宽度加上左边距宽度大于可打印区域宽度，则打印机使用的打印区域宽度大小是可打印区域宽度和左边距宽度之差。

        5．另请参考POS_PL_SetArea和POS_SetMotionUnit。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_S_SetAreaWidth(

int nWidth

);


        /*


        描述

        把将要打印的字符串数据发送到打印缓冲区中，并指定X 方向（水平）上的绝对起始点位置，指定每个字符宽度和高度方向上的放大倍数、类型和风格。



        参数

        pszString

        [in] 指向以 null 结尾的字符串缓冲区。


        nOrgx

        [in] 指定 X 方向（水平）的起始点位置离左边界的点数。

        可以为 0 到 65535。


        nWidthTimes

        [in] 指定字符的宽度方向上的放大倍数。

        可以为 1到 6。


        nHeightTimes

        [in] 指定字符高度方向上的放大倍数。

        可以为 1 到 6。


        nFontType

        [in] 指定字符的字体类型。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_FONT_TYPE_STANDARD 0x00 标准 ASCII 
        POS_FONT_TYPE_COMPRESSED 0x01 压缩 ASCII  
        POS_FONT_TYPE_UDC 0x02 用户自定义字符 
        POS_FONT_TYPE_CHINESE 0x03 标准 “宋体” 


        nFontStyle

        [in] 指定字符的字体风格。

        可以为以下列表中的一个或若干个。

        Flag Value Meaning 
        POS_FONT_STYLE_NORMAL 0x00 正常 
        POS_FONT_STYLE_BOLD 0x08 加粗 
        POS_FONT_STYLE_THIN_UNDERLINE 0x80 1点粗的下划线 
        POS_FONT_STYLE_THICK_UNDERLINE 0x100 2点粗的下划线 
        POS_FONT_STYLE_UPSIDEDOWN 0x200 倒置（只在行首有效） 
        POS_FONT_STYLE_REVERSE 0x400 反显（黑底白字） 
        POS_FONT_STYLE_SMOOTH 0x800 平滑处理（用于放大时） 
        POS_FONT_STYLE_CLOCKWISE_90 0x1000 每个字符顺时针旋转 90 度 



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1．在打印机中，一般内部已有字体类型如下表所示，但是不同机型会有所不同，可以参考打印机的测试样张。

        Font Type Size (W * H) 
        Standard ASCII 12 * 24 / 13 * 24 
        Compressed ASCII 9 * 17 
        标准宋体 24 * 24 

        2．如果字符风格（nFontStyle）设置为“反显（POS_FONT_STYLE_REVERSE）”，或“顺时针旋转90度（POS_FONT_STYLE_CLOCKWISE_90）”，那么“细下划线（POS_FONT_STYLE_THIN_UNDERLINE）” 和“粗下划线（POS_FONT_STYLE_THICK_UNDERLINE）”的功能将无效。

        3．如果在汉字字符模式下，则一次调用时，标准ASCII字符（除部分符号和全角下的符号）和汉字可以混合打印；如果在ASCII字符模式下，则只可以打印标准 ASCII 字符和压缩的ASCII字符。

        4．此函数并不立即打印传入的字符串，而是一直到调用函数 POS_FeedLine 时才进行实际的进纸并打印的动作。但是，如果传入的字符串已经大于可打印宽度时，那么会自动进纸并打印。

        5．另请参考 POS_PL_TextOut。



        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_S_TextOut(

String pszString,

int nOrgx,

int nWidthTimes,

int nHeightTimes,

int nFontType,

int nFontStyle

);



        /*

        描述

        下载并打印位图



        参数

        pszPath

        [in] 指向以null 结尾的包含位图文件路径及其名称的字符串。


        nOrgx

        [in] 指定将要打印的位图和左边界的距离点数。

        可以为 0到 65535 点。


        nMode

        [in] 指定位图的打印模式。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_BITMAP_MODE_8SINGLE_DENSITY 0x00 8点单密度 
        POS_BITMAP_MODE_8DOUBLE_DENSITY 0x01 8点双密度 
        POS_BITMAP_MODE_24SINGLE_DENSITY 0x20 24点单密度 
        POS_BITMAP_MODE_24DOUBLE_DENSITY 0x21 24点双密度 


        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PATH

        POS_ERROR_INVALID_PARAMETER

        POS_ERROR_NOT_BITMAP

        POS_ERROR_NOT_MONO_BITMAP

        POS_ERROR_BEYOND_AREA



        备注

        1、位图不可以大于 8K 字节。

        2、位图必须是单色的。

        3、打印结束后，行高被设置为 31 点高。

        4、另请参考 POS_PreDownloadBmpToRAM，POS_PreDownloadBmpsToFlash，POS_PL_DownloadBmpAndPrint， POS_SetLineSpacing。



        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_S_DownloadAndPrintBmp(

String pszPath,

int nOrgx,

int nMode

);




        /*

        描述

        打印已经下载到 RAM 中的位图。



        参数

        nID

        [in] 指定位图的 ID 号。

        可以为 0 到 7。


        nOrgx

        [in] 指定将要打印的位图和左边界的距离点数。

        可以为 0到 65535 点。


        nMode

        [in] 指定位图的打印模式。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_BITMAP_PRINT_NORMAL 0x00 正常 
        POS_BITMAP_PRINT_DOUBLE_WIDTH 0x01 倍宽 
        POS_BITMAP_PRINT_DOUBLE_HEIGHT 0x02 倍高 
        POS_BITMAP_PRINT_QUADRUPLE 0x03 倍宽且倍高 



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        参考函数 POS_PreDownloadBmpToRAM，POS_PL_PrintBmpInRAM。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_S_PrintBmpInRAM(

int nOrgx,

int nMode

);



        /*

        描述

        打印已经下载到 Flash 中的位图。



        参数

        nID

        [in] 指定位图的 ID 号。

        可以为 1 到 255。


        nOrgx

        [in] 指定将要打印的位图和左边界的距离点数。

        可以为 0到 65535 点。


        nMode

        [in] 指定位图的打印模式。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_BITMAP_PRINT_NORMAL 0x00 正常 
        POS_BITMAP_PRINT_DOUBLE_WIDTH 0x01 倍宽 
        POS_BITMAP_PRINT_DOUBLE_HEIGHT 0x02 倍高 
        POS_BITMAP_PRINT_QUADRUPLE 0x03 倍宽且倍高 


        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        参考函数 POS_PreDownloadBmpsToFlash。

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_S_PrintBmpInFlash(

int nID,

int nOrgx,

int nMode

);


        /*

        描述

        设置并打印条码。



        参数

        pszInfoBuffer

        [in] 指向以 null 结尾的字符串。每个字符允许的范围和格式与具体条码类型有关。


        nOrgx

        [in] 指定将要打印的条码的水平起始点与左边界的距离点数。

        可以为 0 到65535。


        nType

        [in] 指定条码的类型。

        可以为以下列表中所列值之一。另请参考“附录 B 条码说明”。

        Flag Value Meaning 
        POS_BARCODE_TYPE_UPC_A 0x41 UPC-A 
        POS_BARCODE_TYPE_UPC_E 0x42 UPC-C 
        POS_BARCODE_TYPE_JAN13 0x43 JAN13(EAN13) 
        POS_BARCODE_TYPE_JAN8 0x44 JAN8(EAN8) 
        POS_BARCODE_TYPE_CODE39 0x45 CODE39 
        POS_BARCODE_TYPE_ITF 0x46 INTERLEAVED 2 OF 5 
        POS_BARCODE_TYPE_CODEBAR 0x47 CODEBAR 
        POS_BARCODE_TYPE_CODE93 0x48 25 
        POS_BARCODE_TYPE_CODE128 0x49 CODE 128 


        nWidthX

        [in] 指定条码的基本元素宽度。

        可以为以下列表中所列值（n）之一。

        n 单基本模块宽度
        （连续型） 双基本模块宽度（离散型） 
        窄元素宽度 宽元素宽度 
        2 0．25mm 0．25mm 0．625mm 
        3 0．375mm 0．375mm 1．0mm 
        4 0．5mm 0．5mm 1．25mm 
        5 0．625mm 0．625mm 1．625mm 
        6 0．75mm 0．75mm 1.875mm 


        nHeight

        [in] 指定条码的高度点数。

        可以为 1 到 255 。默认值为162 点。


        nHriFontType

        [in] 指定 HRI（Human Readable Interpretation）字符的字体类型。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_FONT_TYPE_STANDARD 0x00 标准ASCII 
        POS_FONT_TYPE_COMPRESSED 0x01 压缩ASCII 


        nHriFontPosition

        [in] 指定HRI（Human Readable Interpretation）字符的位置。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_HRI_POSITION_NONE  0x00 不打印 
        POS_HRI_POSITION_ABOVE 0x01 只在条码上方打印 
        POS_HRI_POSITION_BELOW 0x02 只在条码下方打印 
        POS_HRI_POSITION_BOTH  0x03 条码上、下方都打印 


        nBytesToPrint

        [in] 指定由参数 pszInfoBuffer指向的字符串个数，即将要发送给打印机的字符总数。具体值与条码类型有关。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        另请参考POS_PL_SetBarcode 和 “附录 B 条码说明”。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_S_SetBarcode(

String pszInfoBuffer,

int nOrgx,

int nType,

int nWidthX,

int nHeight,

int nHriFontType,

int nHriFontPosition,

int nBytesToPrint

);




        /*

        描述

        设置页面的打印区域。



        参数

        nOrgx

        [in] 指定区域的 X （水平）方向的起始点和左边界的距离。

        可以为 0 到 65535。


        nOrgy

        [in] 指定区域的 Y （垂直）方向的起始点和上边界（当前打印头位置）的距离点数。

        可以为 0 到 65535。


        nWidth

        [in] 指定打印区域的宽度（水平方向）。

        可以为 0 到 65535。


        nHeight

        [in] 指定打印区域的高度（垂直方向）。

        可以为 0 到 65535。


        nDirection

        [in] 指定打印区域的方向（原点位置）。

        可以为以下列表中所列值之一。

        Flag Value Starting Position 
        POS_AREA_LEFT_TO_RIGHT  0 左上角 
        POS_AREA_BOTTOM_TO_TOP 1 左下角 
        POS_AREA_RIGHT_TO_LEFT 2 右下角 
        POS_AREA_TOP_TO_BOTTOM 3 右上角 



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1．由于不同的打印机型号有不同的打印头宽度（或内部缓冲区的宽度），所以可打印区域的宽度是不同的。同样，可打印区域的高度也有可能不同，一般大约是 128 × 8 点，如果参数 nWidth 和 nHeight 所指定的值超过此限制。那么可打印区域自动设置为实际的可打印区域。

        2．如果打印区域的宽度或高度设置为0，则打印机停止指令的处理，而将随后接受到的数据都当作普通的字符数据。

        3．如果打印区域的起始点超出打印机的可打印区域，则打印机停止指令的处理，而将随后接受到的数据都当作普通的字符数据。



        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_PL_SetArea(

int nOrgx,

int nOrgy,

int nWidth,

int nHeight,

int nDirection

);



        /*

        描述

        把将要打印的字符串数据发送到打印缓冲区中，并指定X 方向（水平）上的绝对起始点位置，指定每个字符宽度和高度方向上的放大倍数、类型和风格。



        参数

        pszString

        [in] 指向以 null 结尾的字符串缓冲区。


        nOrgx

        [in] 指定 X 方向（水平）的起始点位置离左边界的点数。

        可以为 0 到 65535。


        nOrgy

        [in] 指定 Y 方向（垂直）的起始点位置离上边界的点数。

        可以为 0 到 65535。


        nWidthTimes

        [in] 指定字符的宽度方向上的放大倍数。

        可以为 1到 6。


        nHeightTimes

        [in] 指定字符高度方向上的放大倍数。

        可以为 1 到 6。


        nFontType

        [in] 指定字符的字体类型。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_FONT_TYPE_STANDARD 0x00 标准 ASCII 
        POS_FONT_TYPE_COMPRESSED 0x01 压缩 ASCII  
        POS_FONT_TYPE_UDC 0x02 用户自定义字符 
        POS_FONT_TYPE_CHINESE 0x03 标准 “宋体” 

        nFontStyle

        [in] 指定字符的字体风格。

        可以为以下列表中的一个或若干个。

        Flag Value Meaning 
        POS_FONT_STYLE_NORMAL 0x00 正常 
        POS_FONT_STYLE_BOLD 0x08 加粗 
        POS_FONT_STYLE_THIN_UNDERLINE 0x80 1点粗的下划线 
        POS_FONT_STYLE_THICK_UNDERLINE 0x100 2点粗的下划线 
        POS_FONT_STYLE_REVERSE 0x400 反显（黑底白字） 
        POS_FONT_STYLE_SMOOTH 0x800 平滑处理（用于放大时） 



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1、在打印机中，一般内部已有字体类型如下表所示，但是不同机型会有所不同，可以参考打印机的测试样张中

        Font Type Size (W * H) 
        Standard ASCII 12 * 24 / 13 * 24 
        Compressed ASCII  9 * 17 
        标准宋体 24 * 24 

        2、如果字符风格（nFontStyle）设置为“反显（POS_FONT_STYLE_REVERSE）”那么“细下划线（POS_FONT_STYLE_THIN_UNDERLINE）” 和“粗下划线POS_FONT_STYLE_THICK_UNDERLINE）”的功能将无效。

        3、如果在汉字字符模式下，则一次调用时标准ASCII字符（除部分符号）和汉字可以混合打印；如果在ASCII字符模式下，则只可以打印标准 ASCII 字符和压缩的ASCII字符。

        4、此函数并不立即打印传入的字符串，而是一直到调用函数 POS_PL_Print 时才进行实际的进纸并打印的动作。

        5、另请参考 POS_S_TextOut。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_PL_TextOut(

String pszString,

int nOrgx,

int nOrgy,

int nWidthTimes,

int nHeightTimes,

int nFontType,

int nFontStyle

);



        /*
        描述

        下载位图到打印缓冲区中。



        参数

        pszPath

        [in] 指向以null 结尾的包含位图文件路径及其名称的字符串。


        nOrgx

        [in] 指定将要打印的位图和左边界的距离点数。

        可以为 0到 65535 点。


        nOrgy

        [in] 指定将要打印的位图和上边界的距离点数。

        可以为 0到 65535 点。


        nMode

        [in] 指定位图的打印模式。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_BITMAP_MODE_8SINGLE_DENSITY 0x00 8点单密度 
        POS_BITMAP_MODE_8DOUBLE_DENSITY 0x01 8点双密度 
        POS_BITMAP_MODE_24SINGLE_DENSITY 0x20 24点单密度 
        POS_BITMAP_MODE_24DOUBLE_DENSITY 0x21 24点双密度 


        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PATH

        POS_ERROR_INVALID_PARAMETER

        POS_ERROR_NOT_BITMAP

        POS_ERROR_NOT_MONO_BITMAP

        POS_ERROR_BEYOND_AREA



        备注

        1．位图不可以大于 8K 字节。

        2．位图必须是单色的。

        3．打印结束后，行高被设置为 31 点高。

        4．此函数并不立即进纸并打印位图，而是一直等到调用函数 POS_PL_Print 时才打印。

        5．另请参考 POS_PreDownloadBmpToRAM，POS_PreDownloadBmpsToFlash，POS_S_DownloadAndPrintBmp， POS_SetLineSpacing。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_PL_DownloadAndPrintBmp(

String pszPath,

int nOrgx,

int nOrgy,

int nMode

);


        /*

        描述

        打印已经下载到 RAM 中的位图。



        参数

        nID

        [in] 指定位图的 ID 号。

        可以为 0 到 7。


        nOrgx

        [in] 指定将要打印的位图和左边界的距离点数。

        可以为 0到 65535 点。


        nOrgy

        [in] 指定将要打印的位图和上边界的距离点数。

        可以为 0到 65535 点。


        nMode

        [in] 指定位图的打印模式。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_BITMAP_PRINT_NORMAL 0x00 正常 
        POS_BITMAP_PRINT_DOUBLE_WIDTH 0x01 倍宽 
        POS_BITMAP_PRINT_DOUBLE_HEIGHT 0x02 倍高 
        POS_BITMAP_PRINT_QUADRUPLE 0x03 倍宽且倍高 


        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1、此函数并不立即进纸并打印位图，而是一直等到调用函数 POS_PL_Print 时才打印。

        2、参考函数 POS_PreDownloadBmpToRAM，POS_S_PrintBmpInRAM。

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]

        public static extern int POS_PL_PrintBmpInRAM(

int nOrgx,

int nOrgy,

int nMode

);



        /*

        描述

        设置条码。



        参数

        pszInfoBuffer

        [in] 指向以 null 结尾的字符串。每个字符允许的范围和格式与具体条码类型有关。


        nOrgx

        [in] 指定将要打印的条码的起始点与左边界的距离点数。

        可以为 0 到65535。


        nOrgy

        [in] 指定将要打印的条码的起始点与上边界的距离点数。

        可以为 0 到65535。


        nType

        [in] 指定条码的类型。

        可以为以下列表中所列值之一。另请参考“附录 B 条码说明”。

        Flag Value Meaning 
        POS_BARCODE_TYPE_UPC_A 0x41 UPC-A 
        POS_BARCODE_TYPE_UPC_E 0x42 UPC-C 
        POS_BARCODE_TYPE_JAN13 0x43 JAN13(EAN13) 
        POS_BARCODE_TYPE_JAN8 0x44 JAN8(EAN8) 
        POS_BARCODE_TYPE_CODE39 0x45 CODE39 
        POS_BARCODE_TYPE_ITF 0x46 INTERLEAVED 2 OF 5 
        POS_BARCODE_TYPE_CODEBAR 0x47 CODEBAR 
        POS_BARCODE_TYPE_CODE93 0x48 25 
        POS_BARCODE_TYPE_CODE128 0x49 CODE 128 

        nWidthX

        [in] 指定条码的基本元素宽度。

        可以为以下列表中所列值（n）之一。

        n 单基本模块宽度
        （连续型） 双基本模块宽度（离散型） 
        窄元素宽度 宽元素宽度 
        2 0．25mm 0．25mm 0．625mm 
        3 0．375mm 0．375mm 1．0mm 
        4 0．5mm 0．5mm 1．25mm 
        5 0．625mm 0．625mm 1．625mm 
        6 0．75mm 0．75mm 1.875mm 


        nHeight

        [in] 指定条码的高度点数。

        可以为 1 到 255 。


        nHriFontType

        [in] 指定 HRI（Human Readable Interpretation）字符的字体类型。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_FONT_TYPE_STANDARD 0x00 标准ASCII 
        POS_FONT_TYPE_COMPRESSED 0x01 压缩ASCII 

        nHriFontPosition

        [in] 指定HRI（Human Readable Interpretation）字符的位置。

        可以为以下列表中所列值之一。

        Flag Value Meaning 
        POS_HRI_POSITION_NONE  0x00 不打印 
        POS_HRI_POSITION_ABOVE 0x01 只在条码上方打印 
        POS_HRI_POSITION_BELOW 0x02 只在条码下方打印 
        POS_HRI_POSITION_BOTH  0x03 条码上、下方都打印 


        nBytesToPrint

        [in] 指定由参数 pszInfoBuffer指向的字符串个数，即将要发送给打印机的字符总数。具体值与条码类型有关。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1、此函数并不立即打印条码，而是一直到调用函数 POS_PL_Print时才打印。

        2、另请参考 POS_S_SetBarcode 和 “附录 B 条码说明”。

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_PL_SetBarcode(

String pszInfoBuffer,

int nOrgx,

int nOrgy,

int nType,

int nWidthX,

int nHeight,

int nHriFontType,

int nHriFontPosition,

int nBytesToPrint

);




        /*


        描述

        打印页或标签缓冲区中的数据。



        参数

        无。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE



        备注

        1．如果函数成功，则将进纸并打印票面，但是页缓冲区或标签缓冲区内容还是被保留着，可以再次调用此函数继续打印页缓冲区或标签缓冲区中的票面。

        2．可以调用 POS_PL_Clear 来清除页缓冲区或标签缓冲区中的数据。

        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]

        public static extern int POS_PL_Print();


        /*

        描述

        清除票面和标签的打印缓冲区中的数据。



        参数

        无。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：
        POS_FAIL
        POS_ERROR_INVALID_HANDLE



        备注

        1．如果函数成功，则打印机内部的当前页缓冲区被清除。

        2．另请参考 POS_PL_Print，POS_SetMode。


        */

        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_PL_Clear();



        /*


        描述

        发送数据到端口或文件。



        参数

        hPort

        [in] 端口或文件句柄。


        pszData

        [in] 指向将要发送的数据缓冲区。


        nBytesToWrite

        [in] 指定将要发送的数据的字节数。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1．此函数仅用来调试。

        2．另请参考 POS_ReadFile。

        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_WriteFile(

UIntPtr hPort,

String pszData,

int nBytesToWrite

);




        /*

        描述

        从串口，或USB端口或文件读数据到指定的缓冲区。



        参数

        hPort

        [in] 端口或文件句柄。


        pszData

        [in] 指向将要读取的数据缓冲区。


        nBytesToWrite

        [in] 指定将要读取的数据的字节数。



        返回值

        如果函数成功，则返回值为 POS_SUCCESS。

        如果函数失败，则返回值为以下值之一：

        POS_FAIL

        POS_ERROR_INVALID_HANDLE

        POS_ERROR_INVALID_PARAMETER



        备注

        1．此函数仅用来调试。

        2．此函数不支持网络接口。

        3．另请参考 POS_WriteFile。


        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_ReadFile(

UIntPtr hPort,

String pszData,

int nBytesToRead,

int nTimeouts

);



        /*


        描述

        改变dll内部的端口或文件句柄。



        参数

        hNewHandle

        [in] 用来替换dll内部的句柄。



        返回值

        如果函数成功，则返回dll内部的句柄，并使用hNewHandle替换内部句柄。

        如果函数失败，则返回 INVALID_HANDLE_VALUE（-1）。



        备注

        1．此函数仅用来调试。

        2．另请参考 POS_WriteFile，POS_ReadFile。


        */


        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr POS_SetHandle
         (

     UIntPtr hNewHandle

     );



        /*

        描述

        获取当前 dll 的发布版本号。




        参数

        pnMajor

        [out] 主版本号。


        pnMinor

        [out] 次版本号。




        返回值

        如果函数成功，则返回此 dll 的建立日期。




        备注

        此函数一般用作调试或测试使用。



        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_GetSWVersionInfo(

ref int pnMajor,

ref int pnMinor

);


        /*

        描述

        获取当前 dll 的发布版本号。




        参数

        pnMajor

        [out] 主版本号。


        pnMinor

        [out] 次版本号。




        返回值

        如果函数成功，则返回此 dll 的建立日期。




        备注

        此函数一般用作调试或测试使用。



        */
        [DllImport("YkPosdll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int POS_GetHWVersionInfo(

       ref int pnMajor,

       ref int pnMinor

       );
    }
}

