#define ENABLE_DETECT

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;

namespace DigitalPlatform.IO
{
    // http://www.aboutmycode.com/net-framework/how-to-get-elevated-process-path-in-net/
    public class ProcessUtil
    {
        public static List<string> GetProcessNameList()
        {
            List<string> results = new List<string>();

            System.Diagnostics.Process[] process_list = System.Diagnostics.Process.GetProcesses();

            foreach (Process process in process_list)
            {
                try
                {
                    results.Add(process.ProcessName);
                }
                catch
                {

                }
            }

            return results;
        }

        public static string GetExecutablePath(Process Process)
        {
            //If running on Vista or later use the new function
            if (Environment.OSVersion.Version.Major >= 6)
            {
                return GetExecutablePathAboveVista(Process.Id);
            }

            return Process.MainModule.FileName;
        }

        private static string GetExecutablePathAboveVista(int ProcessId)
        {
            var buffer = new StringBuilder(1024);
            IntPtr hprocess = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION,
                                          false, ProcessId);
            if (hprocess != IntPtr.Zero)
            {
                try
                {
                    int size = buffer.Capacity;
                    if (QueryFullProcessImageName(hprocess, 0, buffer, out size))
                    {
                        return buffer.ToString();
                    }
                }
                finally
                {
                    CloseHandle(hprocess);
                }
            }
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        [DllImport("kernel32.dll")]
        private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags,
                       StringBuilder lpExeName, out int size);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
                       bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            PROCESS_QUERY_LIMITED_INFORMATION = 0x00001000
        }
    }

    public static class DetectVirus
    {

        public static string ViruName =
#if ENABLE_DETECT
            "360"
#else
            "xxx"
#endif
            ;

        /*
信息创建时间:*2015-9-15 15:48:56
当前操作系统信息:*Microsoft Windows NT 5.1.2600 Service Pack 3
当前操作系统版本号:*5.1.2600.196608
本机 MAC 地址:*00192145CC58
是否安装了 KB2468871:*True
系统进程:
--- Devices:
1) 360Safe Anti Hacker Service
2) 360Box mini-filter driver
3) 360Safe Camera Filter Service
4) 360netmon
5) 360qpesv driver
6) 360reskit driver
7) 360SelfProtection
8) Abiosdsk
9) Microsoft ACPI Driver
10) Microsoft ACPIEC Driver
11) Microsoft Kernel Acoustic Echo Canceller
12) AFD
13) Ambfilt
14) asc3350p
15) RAS Asynchronous Media Driver
16) 标准 IDE/ESDI 硬盘控制器
17) Atdisk
18) ATM ARP Client Protocol
19) 音频存根驱动程序
20) BAPIDRV
21) Beep
22) cbidf2k
23) cd20xrnt
24) Cdaudio
25) Cdfs
26) CD-ROM Driver
27) Changer
28) 磁盘驱动器
29) dmboot
30) Logical Disk Manager Driver
31) dmload
32) Microsoft Kernel DLS Syntheiszer
33) Microsoft Kernel DRM Audio Descrambler
34) DsArk
35) EfiSystemMon
36) exFat
37) Fastfat
38) Fdc
39) Fips
40) Flpydisk
41) FltMgr
42) FsVga
43) Volume Manager Driver
44) Generic Packet Classifier
45) Microsoft 用于 High Definition Audio 的 UAA 总线驱动程序
46) Microsoft HID Class Driver
47) HookPort
48) HTTP
49) HUAWEISERSP
50) i2omgmt
51) i8042 键盘和 PS/2 鼠标端口驱动程序
52) CD 烧制筛选驱动器
53) Service for Realtek HD Audio (WDM)
54) Intel Processor Driver
55) IPv6 Windows Firewall Driver
56) IP Traffic Filter Driver
57) IP in IP Tunnel Driver
58) IP Network Address Translator
59) IPSEC driver
60) IR Enumerator Service
61) PnP ISA/EISA Bus Driver
62) Keyboard Class Driver
63) Keyboard HID Driver
64) Microsoft Kernel Wave Audio Mixer
65) KSecDD
66) lbrtfdc
67) mnmdd
68) Modem
69) Monfilt
70) Mouse Class Driver
71) Mouse HID Driver
72) MountMgr
73) WebDav Client Redirector
74) MRxSmb
75) Msfs
76) Microsoft Streaming Service Proxy
77) Microsoft Streaming Clock Proxy
78) Microsoft Streaming Quality Manager Proxy
79) Microsoft System Management BIOS Driver
80) Mup
81) NDIS System Driver
82) Remote Access NDIS TAPI Driver
83) NDIS 用户模式 I/O 协议
84) Remote Access NDIS WAN Driver
85) NDIS Proxy
86) NetBIOS Interface
87) NetBios over Tcpip
88) Npfs
89) Ntfs
90) Null
91) IPX Traffic Filter Driver
92) IPX Traffic Forwarder Driver
93) Parallel port driver
94) PartMgr
95) ParVdm
96) PCI Bus Driver
97) PCIDump
98) PCIIde
99) Pcmcia
100) PDCOMP
101) PDFRAME
102) PDRELI
103) PDRFRAME
104) perc2hib
105) WAN Miniport (PPTP)
106) 处理器驱动程序
107) QoS Packet Scheduler
108) Direct Parallel Link Driver
109) QQFrmMgr
110) QQProtect
111) Quantum DeepScanner Servers
112) qutmipc
113) Remote Access Auto Connection Driver
114) WAN Miniport (L2TP)
115) 远程访问 PPPOE 驱动程序
116) Direct Parallel
117) Rdbss
118) RDPCDD
119) Terminal Server Device Redirector Driver
120) RDPWD
121) Digital CD Audio Playback Filter Driver
122) Realtek 10/100/1000 PCI NIC Family NDIS XP Driver
123) Realtek RTL8139(A/B/C)-based PCI Fast Ethernet Adapter NT Driver
124) Realtek 10/100/1000 PCI-E NIC Family NDIS XP Driver
125) Secdrv
126) Serenum Filter Driver
127) Serial port driver
128) Sfloppy
129) Simbad
130) SiS315
131) SiS AGP winXP Filter
132) SiSide
133) SiSkp
134) Microsoft Kernel Audio Splitter
135) System Restore Filter Driver
136) Srv
137) Software Bus Driver
138) Microsoft Kernel GS Wavetable Synthesizer
139) Microsoft Kernel System Audio Device
140) TCP/IP Protocol Driver
141) TDPIPE
142) TDTCP
143) Terminal Device Driver
144) Udfs
145) Microcode Update Driver
146) Microsoft USB Generic Parent Driver
147) Microsoft USB 2.0 Enhanced Host Controller Miniport Driver
148) USB2 Enabled Hub
149) Microsoft USB Open Host Controller Miniport Driver
150) Microsoft USB PRINTER Class
151) USB 大容量存储设备
152) Microsoft USB Universal Host Controller Miniport Driver
153) VgaSave
154) VolSnap
155) Remote Access IP ARP Driver
156) Kernel Mode Driver Frameworks service
157) WDICA
158) Microsoft WINMM WDM Audio Compatibility Driver
159) Android USB Driver
160) WpdUsb
161) Windows Driver Foundation - User-mode Driver Framework Platform Driver
162) WUDFRd
--- System process:
1) winlogon.exe
2) zstatus.exe
3) Explorer.EXE
4) dfsvc.exe
5) httpd.exe
6) smss.exe
7) httpd.exe
8) svchost.exe
9) DhMachineSvc.exe
10) DhPluginMgr.exe
11) csrss.exe
12) lsass.exe
13) alg.exe
14) services.exe
15) svchost.exe
16) zhudongfangyu.exe
17) svchost.exe
18) 360Tray.exe
19) svchost.exe
20) CAJSHost.exe
21) ctfmon.exe
22) svchost.exe
23) spoolsv.exe
24) svchost.exe
25) conime.exe
26) SoftMgrLite.exe
27) dp2Circulation.exe

 * */
        public static bool DetectXXX()
        {
#if ENABLE_DETECT
            ServiceController[] devices = ServiceController.GetDevices();

            // 先检测驱动
            foreach (ServiceController controller in devices)
            {
                if (controller.DisplayName.StartsWith("360netmon", StringComparison.OrdinalIgnoreCase)
                    || controller.DisplayName.StartsWith("360SelfProtection", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // 再检测系统进程
            List<string> names = ProcessUtil.GetProcessNameList();

            foreach (string name in names)
            {
                if (name.StartsWith("360Tray", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("zhudongfangyu", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
#else
            return false;
#endif
        }

        public static bool DetectGuanjia()
        {
#if ENABLE_DETECT
            // 再检测系统进程
            List<string> names = ProcessUtil.GetProcessNameList();

            foreach (string name in names)
            {
                if (name.StartsWith("qqpctray", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("qqpcrtp", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
#else
            return false;
#endif
        }
    }

}
