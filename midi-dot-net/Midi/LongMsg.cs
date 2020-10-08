// Copyright (c) 2011, Justin Ryan

using System;
using System.Collections.Generic;

namespace Midi
{
    /// <summary>
    /// Utility functions for encoding and decoding short messages.
    /// </summary>
    static class LongMsg
    {
        /// <summary>
        /// Returns true if the given long message describes a SysEx message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        public static bool IsSysEx(UIntPtr dwParam1, UIntPtr dwParam2)
        {
            IntPtr newPtr = unchecked((IntPtr)(long)(ulong)dwParam1); //http://stackoverflow.com/questions/3762113/how-can-an-uintptr-object-be-converted-to-intptr-in-c
            Win32API.MIDIHDR header = (Win32API.MIDIHDR)System.Runtime.InteropServices.Marshal.PtrToStructure(newPtr, typeof(Win32API.MIDIHDR));
            return typeof(Win32API.MIDIHDR) == header.GetType();
        }

        /// <summary>
        /// Decodes a SysEx long message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        /// <param name="data">The SysEx data to send.</param>
        /// <param name="timestamp">Filled in with the timestamp in microseconds since
        /// midiInStart().</param>
        public static void DecodeSysEx(UIntPtr dwParam1, UIntPtr dwParam2, out byte[] data, out UInt32 timestamp)
        {
            //if (!IsSysEx(dwParam1, dwParam2))
            //{
            //    throw new ArgumentException("Not a SysEx message.");
            //}
            IntPtr newPtr = unchecked((IntPtr)(long)(ulong)dwParam1); //http://stackoverflow.com/questions/3762113/how-can-an-uintptr-object-be-converted-to-intptr-in-c
            Win32API.MIDIHDR header = (Win32API.MIDIHDR)System.Runtime.InteropServices.Marshal.PtrToStructure(newPtr, typeof(Win32API.MIDIHDR));
            data = new byte[header.dwBytesRecorded];
            for (int i = 0; i < header.dwBytesRecorded; i++)
            {
                //Array.Resize<byte>(ref data, data.Length + 1);
                //data[data.Length - 1] = System.Runtime.InteropServices.Marshal.ReadByte(header.lpData, i);
                data[i] = System.Runtime.InteropServices.Marshal.ReadByte(header.lpData, i);
            }
            timestamp = (UInt32)dwParam2;
        }

        /*
        /// <summary>
        /// Encodes a SysEx long message.
        /// </summary>
        /// <param name="data">SysEx Data.</param>
        /// <returns>A value that can be passed to midiOutShortMsg.</returns>
        /// <exception cref="ArgumentOutOfRangeException">pitch is not in MIDI range.</exception>
        //public static UInt32 EncodeSysEx(Byte[] data)
        //{
        //}
        */
    }
}
