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
using System.Collections.Generic;

namespace Midi
{
    /// <summary>
    /// Utility functions for encoding and decoding short messages.
    /// </summary>
    static class ShortMsg
    {
        /// <summary>
        /// Returns true if the given short message describes a Note On message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        public static bool IsNoteOn(UIntPtr dwParam1, UIntPtr dwParam2)
        {
            return ((int)dwParam1 & 0xf0) == 0x90;
        }

        /// <summary>
        /// Decodes a Note On short message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        /// <param name="channel">Filled in with the channel.</param>
        /// <param name="pitch">Filled in with the pitch.</param>
        /// <param name="velocity">Filled in with the velocity, 0.127</param>
        /// <param name="timestamp">Filled in with the timestamp in microseconds since
        /// midiInStart().</param>
        public static void DecodeNoteOn(UIntPtr dwParam1, UIntPtr dwParam2,
            out Channel channel, out Pitch pitch, out int velocity, out UInt32 timestamp)
        {
            if (!IsNoteOn(dwParam1, dwParam2))
            {
                throw new ArgumentException("Not a Note On message.");
            }
            channel = (Channel)((int)dwParam1 & 0x0f);
            pitch = (Pitch)(((int)dwParam1 & 0xff00) >> 8);
            velocity = (((int)dwParam1 & 0xff0000) >> 16);
            timestamp = (UInt32)dwParam2;
        }

        /// <summary>
        /// Encodes a Note On short message.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="pitch">The pitch.</param>
        /// <param name="velocity">The velocity 0..127.</param>
        /// <returns>A value that can be passed to midiOutShortMsg.</returns>
        /// <exception cref="ArgumentOutOfRangeException">pitch is not in MIDI range.</exception>
        public static UInt32 EncodeNoteOn(Channel channel, Pitch pitch, int velocity)
        {
            channel.Validate();
            if (!pitch.IsInMidiRange())
            {
                throw new ArgumentOutOfRangeException("Pitch out of MIDI range.");
            }
            if (velocity < 0 || velocity > 127)
            {
                throw new ArgumentOutOfRangeException("Velocity is out of range.");
            }
            return (UInt32)(0x90 | ((int)channel) | ((int)pitch << 8) | (velocity << 16));
        }


        /// <summary>
        /// Returns true if the given short message describes a Note Off message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        public static bool IsNoteOff(UIntPtr dwParam1, UIntPtr dwParam2)
        {
            return ((int)dwParam1 & 0xf0) == 0x80;
        }

        /// <summary>
        /// Decodes a Note Off short message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        /// <param name="channel">Filled in with the channel.</param>
        /// <param name="pitch">Filled in with the pitch.</param>
        /// <param name="velocity">Filled in with the velocity, 0.127</param>
        /// <param name="timestamp">Filled in with the timestamp in microseconds since
        /// midiInStart().</param>
        public static void DecodeNoteOff(UIntPtr dwParam1, UIntPtr dwParam2,
            out Channel channel, out Pitch pitch, out int velocity, out UInt32 timestamp)
        {
            if (!IsNoteOff(dwParam1, dwParam2))
            {
                throw new ArgumentException("Not a Note Off message.");
            }
            channel = (Channel)((int)dwParam1 & 0x0f);
            pitch = (Pitch)(((int)dwParam1 & 0xff00) >> 8);
            velocity = (((int)dwParam1 & 0xff0000) >> 16);
            timestamp = (UInt32)dwParam2;
        }

        /// <summary>
        /// Encodes a Note Off short message.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="pitch">The pitch.</param>
        /// <param name="velocity">The velocity 0..127.</param>
        /// <returns>A value that can be passed to midiOutShortMsg.</returns>
        public static UInt32 EncodeNoteOff(Channel channel, Pitch pitch, int velocity)
        {
            channel.Validate();
            if (!pitch.IsInMidiRange())
            {
                throw new ArgumentOutOfRangeException("Pitch out of MIDI range.");
            }
            if (velocity < 0 || velocity > 127)
            {
                throw new ArgumentOutOfRangeException("Velocity is out of range.");
            }
            return (UInt32)(0x80 | ((int)channel) | ((int)pitch << 8) | (velocity << 16));
        }

        /// <summary>
        /// Returns true if the given short message describes a Control Change message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        public static bool IsControlChange(UIntPtr dwParam1, UIntPtr dwParam2)
        {
            return ((int)dwParam1 & 0xf0) == 0xB0;
        }

        /// <summary>
        /// Decodes a Control Change short message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        /// <param name="channel">Filled in with the channel.</param>
        /// <param name="control">Filled in with the control.</param>
        /// <param name="value">Filled in with the value, 0-127.</param>
        /// <param name="timestamp">Filled in with the timestamp in microseconds since
        /// midiInStart().</param>
        public static void DecodeControlChange(UIntPtr dwParam1, UIntPtr dwParam2,
            out Channel channel, out Control control, out int value, out UInt32 timestamp)
        {
            if (!IsControlChange(dwParam1, dwParam2))
            {
                throw new ArgumentException("Not a control message.");
            }
            channel = (Channel)((int)dwParam1 & 0x0f);
            control = (Control)(((int)dwParam1 & 0xff00) >> 8);
            value = (((int)dwParam1 & 0xff0000) >> 16);
            timestamp = (UInt32)dwParam2;
        }

        /// <summary>
        /// Encodes a Control Change short message.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="control">The control.</param>
        /// <param name="value">The new value 0..127.</param>
        /// <returns>A value that can be passed to midiOutShortMsg.</returns>
        public static UInt32 EncodeControlChange(Channel channel, Control control, int value)
        {
            channel.Validate();
            control.Validate();
            if (value < 0 || value > 127)
            {
                throw new ArgumentOutOfRangeException("Value is out of range.");
            }
            return (UInt32)(0xB0 | (int)(channel) | ((int)control << 8) | (value << 16));
        }

        /// <summary>
        /// Returns true if the given short message a Program Change message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        public static bool IsProgramChange(UIntPtr dwParam1, UIntPtr dwParam2)
        {
            return ((int)dwParam1 & 0xf0) == 0xC0;
        }

        /// <summary>
        /// Decodes a Program Change short message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        /// <param name="channel">Filled in with the channel, 0-15.</param>
        /// <param name="instrument">Filled in with the instrument, 0-127</param>
        /// <param name="timestamp">Filled in with the timestamp in microseconds since
        /// midiInStart().</param>
        public static void DecodeProgramChange(UIntPtr dwParam1, UIntPtr dwParam2,
            out Channel channel, out Instrument instrument, out UInt32 timestamp)
        {
            if (!IsProgramChange(dwParam1, dwParam2))
            {
                throw new ArgumentException("Not a program change message.");
            }
            channel = (Channel)((int)dwParam1 & 0x0f);
            instrument = (Instrument)(((int)dwParam1 & 0xff00) >> 8);
            timestamp = (UInt32)dwParam2;
        }

        /// <summary>
        /// Encodes a Program Change short message.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="instrument">The instrument.</param>
        /// <returns>A value that can be passed to midiOutShortMsg.</returns>
        public static UInt32 EncodeProgramChange(Channel channel, Instrument instrument)
        {
            channel.Validate();
            instrument.Validate();
            return (UInt32)(0xC0 | (int)(channel) | ((int)instrument << 8));
        }

        /// <summary>
        /// Returns true if the given MidiInProc params describe a Pitch Bend message.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        public static bool IsPitchBend(UIntPtr dwParam1, UIntPtr dwParam2)
        {
            return ((int)dwParam1 & 0xf0) == 0xE0;
        }

        /// <summary>
        /// Decodes a Pitch Bend message based on MidiInProc params.
        /// </summary>
        /// <param name="dwParam1">The dwParam1 arg passed to MidiInProc.</param>
        /// <param name="dwParam2">The dwParam2 arg passed to MidiInProc.</param>
        /// <param name="channel">Filled in with the channel, 0-15.</param>
        /// <param name="value">Filled in with the pitch bend value, 0..16383, 8192 is centered.
        /// </param>
        /// <param name="timestamp">Filled in with the timestamp in microseconds since
        /// midiInStart().</param>
        public static void DecodePitchBend(UIntPtr dwParam1, UIntPtr dwParam2,
                               out Channel channel, out int value, out UInt32 timestamp)
        {
            if (!IsPitchBend(dwParam1, dwParam2))
            {
                throw new ArgumentException("Not a pitch bend message.");
            }
            channel = (Channel)((int)dwParam1 & 0x0f);
            value = ((((int)dwParam1 >> 9) & 0x3f80) | (((int)dwParam1 >> 8) & 0x7f));
            timestamp = (UInt32)dwParam2;
        }

        /// <summary>
        /// Encodes a Pitch Bend short message.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="value">The pitch bend value, 0..16383, 8192 is centered.</param>
        /// <returns>A value that can be passed to midiOutShortMsg.</returns>
        public static UInt32 EncodePitchBend(Channel channel, int value)
        {
            channel.Validate();
            if (value < 0 || value > 16383)
            {
                throw new ArgumentOutOfRangeException("Value is out of range.");
            }
            return (UInt32)(0xE0 | (int)(channel) | ((value & 0x7f) << 8) |
                ((value & 0x3f80) << 9));
        }
    }
}
