// Copyright (c) 2009, Tom Lokovic
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Midi
{
    /// <summary>
    /// A MIDI output device.
    /// </summary>
    /// <remarks>
    /// <para>Each instance of this class describes a MIDI output device installed on the system.
    /// You cannot create your own instances, but instead must go through the
    /// <see cref="InstalledDevices"/> property to find which devices are available.  You may wish
    /// to examine the <see cref="DeviceBase.Name"/> property of each one and present the user with
    /// a choice of which device to use.
    /// </para>
    /// <para>Open an output device with <see cref="Open"/> and close it with <see cref="Close"/>.
    /// While it is open, you may send MIDI messages with functions such as
    /// <see cref="SendNoteOn"/>, <see cref="SendNoteOff"/> and <see cref="SendProgramChange"/>.
    /// All notes may be silenced on the device by calling <see cref="SilenceAllNotes"/>.</para>
    /// <para>Note that the above methods send their messages immediately.  If you wish to arrange
    /// for a message to be sent at a specific future time, you'll need to instantiate some subclass
    /// of <see cref="Message"/> (eg <see cref="NoteOnMessage"/>) and then pass it to
    /// <see cref="Clock.Schedule(Midi.Message)">Clock.Schedule</see>.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    /// <seealso cref="Clock"/>
    /// <seealso cref="InputDevice"/>
    public class OutputDevice : DeviceBase
    {
        #region Public Methods and Properties

        /// <summary>
        /// Refresh the list of input devices
        /// </summary>
        public static void UpdateInstalledDevices()
        {
            lock (staticLock)
            {
                installedDevices = null;
            }
        }

        /// <summary>
        /// List of devices installed on this system.
        /// </summary>
        public static ReadOnlyCollection<OutputDevice> InstalledDevices
        {
            get
            {
                lock (staticLock)
                {
                    if (installedDevices == null)
                    {
                        installedDevices = MakeDeviceList();
                    }
                    return new ReadOnlyCollection<OutputDevice>(installedDevices);
                }
            }
        }

        /// <summary>
        /// True if this device is open.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                lock (this)
                {
                    return isOpen;
                }
            }
        }

        /// <summary>
        /// Opens this output device.
        /// </summary>
        /// <exception cref="InvalidOperationException">The device is already open.</exception>
        /// <exception cref="DeviceException">The device cannot be opened.</exception>
        public void Open()
        {
            lock (this)
            {
                CheckNotOpen();
                CheckReturnCode(Win32API.midiOutOpen(out handle, deviceId, null, (UIntPtr)0));
                isOpen = true;
            }
        }

        /// <summary>
        /// Closes this output device.
        /// </summary>
        /// <exception cref="InvalidOperationException">The device is not open.</exception>
        /// <exception cref="DeviceException">The device cannot be closed.</exception>
        public void Close()
        {
            lock (this)
            {
                CheckOpen();
                CheckReturnCode(Win32API.midiOutClose(handle));
                isOpen = false;
            }
        }

        /// <summary>
        /// Silences all notes on this output device.
        /// </summary>
        /// <exception cref="InvalidOperationException">The device is not open.</exception>
        /// <exception cref="DeviceException">The message cannot be sent.</exception>
        public void SilenceAllNotes()
        {
            lock (this)
            {
                CheckOpen();
                CheckReturnCode(Win32API.midiOutReset(handle));
            }
        }

        /// <summary>
        /// Sends a Note On message to this MIDI output device.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="pitch">The pitch.</param>
        /// <param name="velocity">The velocity 0..127.</param>
        /// <exception cref="ArgumentOutOfRangeException">channel, pitch, or velocity is
        /// out-of-range.</exception>
        /// <exception cref="InvalidOperationException">The device is not open.</exception>
        /// <exception cref="DeviceException">The message cannot be sent.</exception>
        public void SendNoteOn(Channel channel, Pitch pitch, int velocity)
        {
            lock (this)
            {
                CheckOpen();
                CheckReturnCode(Win32API.midiOutShortMsg(handle, ShortMsg.EncodeNoteOn(channel,
                    pitch, velocity)));
            }
        }

        /// <summary>
        /// Sends a Note Off message to this MIDI output device.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="pitch">The pitch.</param>
        /// <param name="velocity">The velocity 0..127.</param>
        /// <exception cref="ArgumentOutOfRangeException">channel, note, or velocity is
        /// out-of-range.</exception>
        /// <exception cref="InvalidOperationException">The device is not open.</exception>
        /// <exception cref="DeviceException">The message cannot be sent.</exception>
        public void SendNoteOff(Channel channel, Pitch pitch, int velocity)
        {
            lock (this)
            {
                CheckOpen();
                CheckReturnCode(Win32API.midiOutShortMsg(handle, ShortMsg.EncodeNoteOff(channel,
                    pitch, velocity)));
            }
        }

        /// <summary>
        /// Sends a Note On message to Channel10 of this MIDI output device.
        /// </summary>
        /// <param name="percussion">The percussion.</param>
        /// <param name="velocity">The velocity 0..127.</param>
        /// <remarks>This is simply shorthand for a Note On message on Channel10 with a
        /// percussion-specific note, so there is no corresponding message to receive from an input
        /// device.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">percussion or velocity is out-of-range.
        /// </exception>
        /// <exception cref="InvalidOperationException">The device is not open.</exception>
        /// <exception cref="DeviceException">The message cannot be sent.</exception>
        public void SendPercussion(Percussion percussion, int velocity)
        {
            lock (this)
            {
                CheckOpen();
                CheckReturnCode(Win32API.midiOutShortMsg(handle, ShortMsg.EncodeNoteOn(
                    Channel.Channel10, (Pitch)percussion,
                    velocity)));
            }
        }

        /// <summary>
        /// Sends a Control Change message to this MIDI output device.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="control">The control.</param>
        /// <param name="value">The new value 0..127.</param>
        /// <exception cref="ArgumentOutOfRangeException">channel, control, or value is
        /// out-of-range.</exception>
        /// <exception cref="InvalidOperationException">The device is not open.</exception>
        /// <exception cref="DeviceException">The message cannot be sent.</exception>
        public void SendControlChange(Channel channel, Control control, int value)
        {
            lock (this)
            {
                CheckOpen();
                CheckReturnCode(Win32API.midiOutShortMsg(handle, ShortMsg.EncodeControlChange(
                    channel, control, value)));
            }
        }

        /// <summary>
        /// Sends a Pitch Bend message to this MIDI output device.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="value">The pitch bend value, 0..16383, 8192 is centered.</param>
        /// <exception cref="ArgumentOutOfRangeException">channel or value is out-of-range.
        /// </exception>
        /// <exception cref="InvalidOperationException">The device is not open.</exception>
        /// <exception cref="DeviceException">The message cannot be sent.</exception>
        public void SendPitchBend(Channel channel, int value)
        {
            lock (this)
            {
                CheckOpen();
                CheckReturnCode(Win32API.midiOutShortMsg(handle, ShortMsg.EncodePitchBend(channel,
                    value)));
            }
        }

        /// <summary>
        /// Sends a Program Change message to this MIDI output device.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="instrument">The instrument.</param>
        /// <exception cref="ArgumentOutOfRangeException">channel or instrument is out-of-range.
        /// </exception>
        /// <exception cref="InvalidOperationException">The device is not open.</exception>
        /// <exception cref="DeviceException">The message cannot be sent.</exception>
        /// <remarks>
        /// A Program Change message is used to switch among instrument settings, generally
        /// instrument voices.  An instrument conforming to General Midi 1 will have the
        /// instruments described in the <see cref="Instrument"/> enum; other instruments
        /// may have different instrument sets.
        /// </remarks>
        public void SendProgramChange(Channel channel, Instrument instrument)
        {
            lock (this)
            {
                CheckOpen();
                CheckReturnCode(Win32API.midiOutShortMsg(handle, ShortMsg.EncodeProgramChange(
                    channel, instrument)));
            }
        }

#region SysEx

        /// <summary>
        /// Sends a System Exclusive (sysex) message to this MIDI output device.
        /// </summary>
        /// <param name="data">The message to send (as byte array)</param>
        /// <exception cref="DeviceException">The message cannot be sent.</exception>
        public void SendSysEx(Byte[] data)
        {
            lock (this)
            {
                //Win32API.MMRESULT result;
                IntPtr ptr;
                UInt32 size = (UInt32)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32API.MIDIHDR));
                Win32API.MIDIHDR header = new Win32API.MIDIHDR();
                header.lpData = System.Runtime.InteropServices.Marshal.AllocHGlobal(data.Length);
                for (int i = 0; i < data.Length; i++)
                    System.Runtime.InteropServices.Marshal.WriteByte(header.lpData, i, data[i]);
                header.dwBufferLength = data.Length;
                header.dwBytesRecorded = data.Length;
                header.dwFlags = 0;

                try
                {
                    ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32API.MIDIHDR)));
                }
                catch (Exception)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(header.lpData);
                    throw;
                }

                try
                {
                    System.Runtime.InteropServices.Marshal.StructureToPtr(header, ptr, false);
                }
                catch (Exception)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(header.lpData);
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
                    throw;
                }

                //result = Win32API.midiOutPrepareHeader(handle, ptr, size);
                //if (result == 0) result = Win32API.midiOutLongMsg(handle, ptr, size);
                //if (result == 0) result = Win32API.midiOutUnprepareHeader(handle, ptr, size);
                CheckReturnCode(Win32API.midiOutPrepareHeader(handle, ptr, size));
                CheckReturnCode(Win32API.midiOutLongMsg(handle, ptr, size));
                CheckReturnCode(Win32API.midiOutUnprepareHeader(handle, ptr, size));

                System.Runtime.InteropServices.Marshal.FreeHGlobal(header.lpData);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
            }
        }

        ///// <summary>
        ///// Returns the handle to the current OutputDevice as a Strut
        ///// </summary>
        ////public static Int32 DeviceHandle(OutputDevice od)
        //public static Win32API.HMIDIOUT DeviceHandle(OutputDevice od)
        //{
        //    lock (staticLock)
        //    {
        //        //return od.handle.handle;
        //        return od.handle;
        //    }
        //}
        ///// <summary>
        ///// Returns the handle to the current OutputDevice as an Integer
        ///// </summary>
        //public static Int32 DeviceHandleHandle(OutputDevice od)
        ////public static Win32API.HMIDIOUT DeviceHandle(OutputDevice od)
        //{
        //    lock (staticLock)
        //    {
        //        //return od.handle.handle;
        //        return od.handle.handle;
        //    }
        //}

#endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Makes sure rc is MidiWin32Wrapper.MMSYSERR_NOERROR.  If not, throws an exception with an
        /// appropriate error message.
        /// </summary>
        /// <param name="rc"></param>
        private static void CheckReturnCode(Win32API.MMRESULT rc)
        {
            if (rc != Win32API.MMRESULT.MMSYSERR_NOERROR)
            {
                StringBuilder errorMsg = new StringBuilder(128);
                rc = Win32API.midiOutGetErrorText(rc, errorMsg);
                if (rc != Win32API.MMRESULT.MMSYSERR_NOERROR)
                {
                    throw new DeviceException("no error details");
                }
                throw new DeviceException(errorMsg.ToString());
            }
        }

        /// <summary>
        /// Throws a MidiDeviceException if this device is not open.
        /// </summary>
        private void CheckOpen()
        {
            if (!isOpen)
            {
                throw new InvalidOperationException("device not open");
            }
        }

        /// <summary>
        /// Throws a MidiDeviceException if this device is open.
        /// </summary>
        private void CheckNotOpen()
        {
            if (isOpen)
            {
                throw new InvalidOperationException("device open");
            }
        }

        /// <summary>
        /// Private Constructor, only called by the getter for the InstalledDevices property.
        /// </summary>
        /// <param name="deviceId">Position of this device in the list of all devices.</param>
        /// <param name="caps">Win32 Struct with device metadata</param>
        private OutputDevice(UIntPtr deviceId, Win32API.MIDIOUTCAPS caps)
            : base(caps.szPname)
        {
            this.deviceId = deviceId;
            this.caps = caps;
            this.isOpen = false;
        }

        /// <summary>
        /// Private method for constructing the array of MidiOutputDevices by calling the Win32 api.
        /// </summary>
        /// <returns></returns>
        private static OutputDevice[] MakeDeviceList()
        {
            uint outDevs = Win32API.midiOutGetNumDevs();
            OutputDevice[] result = new OutputDevice[outDevs];
            for (uint deviceId = 0; deviceId < outDevs; deviceId++)
            {
                Win32API.MIDIOUTCAPS caps = new Win32API.MIDIOUTCAPS();
                Win32API.midiOutGetDevCaps((UIntPtr)deviceId, out caps);
                result[deviceId] = new OutputDevice((UIntPtr)deviceId, caps);
            }
            return result;
        }

        #endregion

        #region Private Fields

        // Access to the global state is guarded by lock(staticLock).
        private static Object staticLock = new Object();
        private static OutputDevice[] installedDevices = null;

        // The fields initialized in the constructor never change after construction,
        // so they don't need to be guarded by a lock.
        private UIntPtr deviceId;
        private Win32API.MIDIOUTCAPS caps;

        // Access to the Open/Close state is guarded by lock(this).
        private bool isOpen;
        private Win32API.HMIDIOUT handle;

        #endregion
    }
}
