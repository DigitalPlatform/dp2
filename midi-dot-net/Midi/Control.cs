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
using System.Linq;
using System.Text;

namespace Midi
{
    /// <summary>
    /// MIDI Control, used in Control Change messages.
    /// </summary>
    /// <remarks>
    /// <para>In MIDI, Control Change messages are used to influence various auxiliary "controls"
    /// on a device, such as knobs, levers, and pedals.  Controls are specified with
    /// integers in [0..127].  This enum provides an incomplete list of controls, because
    /// most controls are too obscure to document effetively here.  Even for the ones listed
    /// here, the details of how the value is interpreted are arcane.  Please see the MIDI spec for
    /// details.</para>
    /// <para>The most commonly used control is SustainPedal, which is considered off when &lt; 64,
    /// on when &gt; 64.</para>
    /// <para>This enum has extension methods, such as <see cref="ControlExtensionMethods.Name"/>
    /// and <see cref="ControlExtensionMethods.IsValid"/>, defined in
    /// <see cref="ControlExtensionMethods"/>.</para>
    /// </remarks>
    public enum Control
    {
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        ModulationWheel = 1,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        DataEntryMSB = 6,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        Volume = 7,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        Pan = 10,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        Expression = 11,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        DataEntryLSB = 38,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        SustainPedal = 64,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        ReverbLevel = 91,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        TremoloLevel = 92,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        ChorusLevel = 93,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        CelesteLevel = 94,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        PhaserLevel = 95,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        NonRegisteredParameterLSB = 98,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        NonRegisteredParameterMSB = 99,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        RegisteredParameterNumberLSB = 100,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        RegisteredParameterNumberMSB = 101,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        AllControllersOff = 121,
        /// <summary>General MIDI Control--See MIDI spec for details.</summary>
        AllNotesOff = 123
    }

    /// <summary>
    /// Extension methods for the Control enum.
    /// </summary>
    /// Be sure to "using midi" if you want to use these as extension methods.
    public static class ControlExtensionMethods
    {
        /// <summary>
        /// Returns true if the specified control is valid.
        /// </summary>
        /// <param name="control">The Control to test.</param>
        public static bool IsValid(this Control control)
        {
            return (int)control >= 0 && (int)control < 128;
        }

        /// <summary>
        /// Throws an exception if control is not valid.
        /// </summary>
        /// <param name="control">The control to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">The control is out-of-range.</exception>
        public static void Validate(this Control control)
        {
            if (!control.IsValid())
            {
                throw new ArgumentOutOfRangeException("Control out of range");
            }
        }

        /// <summary>
        /// Table of control names.
        /// </summary>
        private static Dictionary<int, string> ControlNames = new Dictionary<int, string>
        {
            {1, "Modulation wheel"},
            {6, "Data Entry MSB"},
            {7, "Volume"},
            {10, "Pan"},
            {11, "Expression"},
            {38, "Data Entry LSB"},
            {64, "Sustain pedal"},
            {91, "Reverb level"},
            {92, "Tremolo level"},
            {93, "Chorus level"},
            {94, "Celeste level"},
            {95, "Phaser level"},
            {98, "Non-registered Parameter LSB"},
            {99, "Non-registered Parameter MSB"},
            {100, "Registered Parameter Number LSB"},
            {101, "Registered Parameter Number MSB"},
            {121, "All controllers off"},
            {123, "All notes off"}
        };

        /// <summary>
        /// Returns the human-readable name of a MIDI control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <exception cref="ArgumentOutOfRangeException">The control is out-of-range.</exception>
        public static string Name(this Control control)
        {
            control.Validate();
            if (ControlNames.ContainsKey((int)control))
            {
                return ControlNames[(int)control];
            }
            else
            {
                return "Other Control (see MIDI spec for details).";
            }
        }
    }
}
