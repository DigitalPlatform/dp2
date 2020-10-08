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

namespace Midi
{
    /// <summary>
    /// Common base class for input and output devices.
    /// </summary>
    /// This base class exists mainly so that input and output devices can both go into the same
    /// kinds of MidiMessages.
    public class DeviceBase
    {
        /// <summary>
        /// Protected constructor.
        /// </summary>
        /// <param name="name">The name of this device.</param>
        protected DeviceBase(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// The name of this device.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }
        private string name;
    }

    /// <summary>
    /// Exception thrown when an operation on a MIDI device cannot be satisfied.
    /// </summary>
    public class DeviceException : System.ApplicationException
    {
        /// <summary>
        /// Constructs exception with a specific error message.
        /// </summary>
        /// <param name="message"></param>
        public DeviceException(string message) { }
    }
}
