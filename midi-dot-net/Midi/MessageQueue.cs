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
    /// A time-sorted queue of MIDI messages.
    /// </summary>
    /// Messages can be added in any order, and can be popped off in timestamp order.
    class MessageQueue
    {
        /// <summary>
        /// Constructs an empty message queue.
        /// </summary>
        public MessageQueue() { }

        /// <summary>
        /// Adds a message to the queue.
        /// </summary>
        /// <param name="message">The message to add to the queue.</param>
        /// The message must have a valid timestamp (not MidiMessage.Now), but other than that
        /// there is no restriction on the timestamp.  For example, it is legal to add a message with
        /// a timestamp earlier than some other message which was previously removed from the queue.
        /// Such a message would become the new "earliest" message, and so would be be the first message
        /// returned by PopEarliest().
        public void AddMessage(Message message)
        {
            // If the list is empty or message is later than any message we already have, we can add this
            // as a new timeslice to the end.
            if (IsEmpty || message.Time > messages.Last.Value[0].Time)
            {
                List<Message> timeslice = new List<Message>();
                timeslice.Add(message);
                messages.AddLast(timeslice);
                return;
            }
            // We need to scan through the list to find where this should be inserted.
            LinkedListNode<List<Message>> node = messages.Last;
            while (node.Previous != null && node.Value[0].Time > message.Time) {
                node = node.Previous;
            }
            // At this point, node refers to a LinkedListNode which either has the correct
            // timestamp, or else a new timeslice needs to be added before or after node.
            if (node.Value[0].Time < message.Time) {
                List<Message> timeslice = new List<Message>();
                timeslice.Add(message);
                messages.AddAfter(node, timeslice);
            } else if (node.Value[0].Time > message.Time) {
                List<Message> timeslice = new List<Message>();
                timeslice.Add(message);
                messages.AddBefore(node, timeslice);
            } else {
                node.Value.Add(message);
            }
        }

        /// <summary>
        /// Discards all messages in the queue.
        /// </summary>
        public void Clear()
        {
            messages.Clear();
        }

        /// <summary>
        /// True if the queue is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return messages.Count == 0;
            }
        }

        /// <summary>
        /// The timestamp of the earliest messsage(s) in the queue.
        /// </summary>
        /// Throws an exception if the queue is empty.
        public float EarliestTimestamp
        {
            get
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("queue is empty");
                }
                return messages.First.Value[0].Time;
            }
        }

        /// <summary>
        /// Removes and returns the message(s) in the queue that have the earliest timestamp.
        /// </summary>
        public List<Message> PopEarliest()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("queue is empty");
            }
            List<Message> result = messages.First.Value;
            messages.RemoveFirst();
            return result;
        }

        // Linked list where each node correponds to a specific timestamp, nodes are sorted by timestamp
        // increasing, and each node contains an ordered list of messages that have been added for that
        // timestamp.
        private LinkedList<List<Message>> messages = new LinkedList<List<Message>>();
    }
}
