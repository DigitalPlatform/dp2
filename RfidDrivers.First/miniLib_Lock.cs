using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RFIDLIB
{
    class miniLib_Lock
    {
        [DllImport("MiniBookcaseLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Mini_Connect(string port, int baud, string frame, ref UIntPtr hd);

        [DllImport("MiniBookcaseLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Mini_Disconnect(UIntPtr hd);

        [DllImport("MiniBookcaseLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Mini_OpenDoor(UIntPtr hd, Byte addr, Byte index);

        [DllImport("MiniBookcaseLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Mini_GetDoorStatus(UIntPtr hd, Byte addr, Byte index, ref Byte sta);




        [DllImport("MiniBookcaseLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Mini_OpenLight(UIntPtr hd);

        [DllImport("MiniBookcaseLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Mini_CloseLight(UIntPtr hd);

        [DllImport("MiniBookcaseLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Mini_OpenSterilamp(UIntPtr hd);

        [DllImport("MiniBookcaseLockLib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Mini_CloseSterilamp(UIntPtr hd);

    }

}
