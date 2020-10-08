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
using System.Diagnostics;
using System.Threading;

namespace Midi
{
    /// <summary>
    /// A clock for scheduling MIDI messages in a rate-adjustable, pausable timeline.
    /// </summary>
    /// <remarks>
    /// <para>Clock is used for scheduling MIDI messages.  Though you can always send messages
    /// synchronously with the various <see cref="OutputDevice"/>.Send* methods, doing so
    /// requires your code to be "ready" at the precise moment each message needs to
    /// be sent.  In most cases, and especially in interactive programs, it's more convenient
    /// to describe messages that <i>will</i> be sent at specified points in the future, and then
    /// rely on a scheduler to make it happen.  Clock is such a scheduler.</para>
    /// <h3>Basic usage</h3>
    /// <para> In the simplest case, Clock can be used to schedule a sequence of messages which is
    /// known in its entirety ahead of time.  For example, this code snippet schedules two notes to
    /// play one after the other:</para>
    /// <code>
    /// Clock clock(120);  // beatsPerMinute=120
    /// OutputDevice outputDevice = ...;
    /// clock.Schedule(new NoteOnMessage(outputDevice, Channel.Channel1, Note.E4, 80, 0));
    /// clock.Schedule(new NoteOffMessage(outputDevice, Channel.Channel1, Note.E4, 80, 1));
    /// clock.Schedule(new NoteOnMessage(outputDevice, Channel.Channel1, Note.D4, 80, 1));
    /// clock.Schedule(new NoteOffMessage(outputDevice, Channel.Channel1, Note.D4, 80, 2));
    /// </code>
    /// <para>At this point, four messages have been scheduled, but they haven't been sent because
    /// the clock has not started.  We can start the clock with <see cref="Start"/>, pause it with
    /// <see cref="Stop"/>, and reset it with <see cref="Reset"/>.  We can change the
    /// beats-per-minute at any time, even as the sequence is playing.  And the playing happens
    /// in a background thread, so your client code can focus on arranging the notes and controlling
    /// the clock.</para>
    /// <para>You can even schedule new notes as the clock is playing.  Generally you should
    /// schedule messages for times in the future; scheduling a message with a time in the past
    /// simply causes it to play immediately, which is probably not what you wanted.</para>
    /// <h3>NoteOnOffMessage and Self-Propagating Messages</h3>
    /// <para>In the above example, we wanted to play two notes but had to schedule four messages.
    /// This case is so common that we provide a convenience class, <see cref="NoteOnOffMessage"/>,
    /// which encapsulates a Note On message and its corresponding Note Off message in a single
    /// unit.  We could rewrite the above example as follows:</para>
    /// <code>
    /// Clock clock(120);  // beatsPerMinute=120
    /// OutputDevice outputDevice = ...;
    /// clock.Schedule(new NoteOnOffMessage(outputDevice, Channel.Channel1, Note.E4, 80, 0, 1));
    /// clock.Schedule(new NoteOnOffMessage(outputDevice, Channel.Channel1, Note.D4, 80, 1, 1));
    /// </code>
    /// <para>This works because each NoteOnOffMessage, when it is actually sent, does two things:
    /// it sends the Note On message to the output device, and <i>also</i> schedules the
    /// correponding Note Off message for the appropriate time in the future.  This is an example
    /// of a <i>self-propagating message</i>: a message which, when triggered, schedules additional
    /// events for the future.</para>
    /// <para>You can design your own self-propagating messages by subclassing from
    /// <see cref="Message"/>.  For example, you could make a self-propagating MetronomeMessage
    /// which keeps a steady beat by always scheduling the <i>next</i> MetronomeMessage when it
    /// plays the current beat.  However, subclassing can be tedious, and it is usually preferable
    /// to use <see cref="CallbackMessage"/> to call-out to your own code instead.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    public class Clock
    {
        /// <summary>
        /// Constructs a midi clock with a given beats-per-minute.
        /// </summary>
        /// <param name="beatsPerMinute">The initial beats-per-minute, which can be changed later.
        /// </param>
        /// <remarks>
        /// <para>When constructed, the clock is not running, and so <see cref="Time"/> will
        /// return zero.  Call <see cref="Start"/> when you are ready for the clock to start
        /// progressing (and scheduled messages to actually trigger).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">beatsPerMinute is non-positive</exception>
        public Clock(float beatsPerMinute)
        {
            if (beatsPerMinute <= 0)
            {
                throw new ArgumentOutOfRangeException("beatsPerMinute");
            }

            this.timingLock = new object();
            this.beatsPerMinute = beatsPerMinute;
            this.millisecondsPerBeat = 60000f / beatsPerMinute;
            this.millisecondFudge = 0;
            this.stopwatch = new Stopwatch();

            this.runLock = new object();
            this.isRunning = false;
            this.thread = null;

            this.threadLock = new object();
            this.threadLock = new Object();
            this.threadShouldExit = false;
            this.threadProcessingTime = 0;
            this.threadMessageQueue = new MessageQueue();
        }

        /// <summary>
        /// This clock's current time in beats.
        /// </summary>
        /// <remarks>
        /// <para>Normally, this method polls the clock's current time, and thus changes from
        /// moment to moment as long as the clock is running.  However, when called from the
        /// scheduler thread (that is, from a <see cref="Message.SendNow">Message.SendNow</see>
        /// method or a <see cref="CallbackMessage"/>), it returns the precise time at which the
        /// message was scheduled.</para>
        /// <para>For example, suppose a callback was scheduled for time T, and the scheduler
        /// managed to call that callback at time T+delta.  In the callback, Time will
        /// return T for the duration of the callback.  In any other thread, Time would
        /// return approximately T+delta.</para>
        /// </remarks>
        public float Time
        {
            get
            {
                if (isSchedulerThread)
                {
                    return threadProcessingTime;
                }
                lock (timingLock)
                {
                    return (stopwatch.ElapsedMilliseconds + millisecondFudge) / millisecondsPerBeat;
                }
            }
        }

        /// <summary>
        /// Beats per minute property.
        /// </summary>
        /// <remarks>
        /// <para>Setting this property changes the rate at which the clock progresses.  If the
        /// clock is currently running, the new rate is effectively immediately.</para>
        /// </remarks>
        public float BeatsPerMinute
        {
            get
            {
                lock (timingLock)
                {
                    return beatsPerMinute;
                }
            }
            set
            {
                lock (timingLock)
                {
                    float newBeatsPerMinute = value;
                    float newMillisecondsPerBeat = 60000f / newBeatsPerMinute;
                    long currentMillis = stopwatch.ElapsedMilliseconds;
                    long currentFudgedMillis = currentMillis + millisecondFudge;
                    float beatTime = currentFudgedMillis / millisecondsPerBeat;
                    long newFudgedMillis = (long)(beatTime * newMillisecondsPerBeat);
                    long newMillisecondFudge = newFudgedMillis - currentMillis;
                    beatsPerMinute = newBeatsPerMinute;
                    millisecondsPerBeat = newMillisecondsPerBeat;
                    millisecondFudge = newMillisecondFudge;
                }
                // Pulse the threadlock in case the scheduler thread needs to reassess its timing based on
                // the new beatsPerMinute.
                lock (threadLock)
                {
                    Monitor.Pulse(threadLock);
                }
            }
        }

        /// <summary>
        /// Returns true if this clock is currently running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                if (isSchedulerThread)
                {
                    return true;
                }
                lock (runLock)
                {
                    return this.isRunning;
                }
            }
        }

        /// <summary>
        /// Starts or resumes the clock.
        /// </summary>
        /// <remarks>
        /// <para>This method causes the clock to start progressing at the rate given in the
        /// <see cref="BeatsPerMinute"/> property.  It may only be called when the clock is
        /// not yet rnuning.</para>
        /// <para>If this is the first time Start is called,
        /// the clock starts at time zero and progresses from there.  If the clock was previously
        /// started, stopped, and not reset, then Start effectively "unpauses" the clock, picking up
        /// at the left-off time, and resuming scheduling of any as-yet-unsent messages.</para>
        /// <para>This method creates a new thread which runs in the background and sends
        /// messages at the appropriate times.  All
        /// <see cref="Message.SendNow">Message.SendNow</see> methods and
        /// <see cref="CallbackMessage"/>s will be called in that thread.</para>
        /// <para>The scheduler thread is joined (shut down) in <see cref="Stop"/>.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Clock is already running.</exception>
        /// <seealso cref="Stop"/>
        /// <seealso cref="Reset"/>
        public void Start()
        {
            if (isSchedulerThread)
            {
                throw new InvalidOperationException("Clock already running.");
            }
            lock (runLock)
            {
                if (isRunning)
                {
                    throw new InvalidOperationException("Clock already running.");
                }

                // Start the stopwatch.
                stopwatch.Start();

                // Start the scheduler thread.  This will cause it to start invoking messages in its thread.
                threadShouldExit = false;
                thread = new Thread(new ThreadStart(this.ThreadRun));
                thread.Start();
 
                // We now consider the MidiClock to actually be running.
                isRunning = true;
            }
        }

        /// <summary>
        /// Stops the clock (but does not reset its time or discard pending events).
        /// </summary>
        /// <remarks>
        /// <para>This method stops the progression of the clock.  It may only be called when
        /// the clock is running.</para>
        /// <para>Any scheduled but as-yet-unsent messages remain in the queue.  A consecutive call
        /// to <see cref="Start"/> can re-start the progress of the clock, or <see cref="Reset"/>
        /// can discard pending messages and reset the clock to zero.</para>
        /// <para>This method waits for any in-progress messages to be processed and joins
        /// (shuts down) the scheduler thread before returning, so when it returns you can be sure
        /// that no more messages will be sent or callbacks invoked.</para>
        /// <para>It is illegal to call Stop from the scheduler thread (ie, from any
        /// <see cref="Message.SendNow">Message.SendNow</see> method or
        /// <see cref="CallbackMessage"/>.  If a callback really needs to stop the clock,
        /// consider using BeginInvoke to arrange for it to happen in another thread.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Clock is not running or Stop was invoked
        /// from the scheduler thread.</exception>
        /// <seealso cref="Start"/>
        /// <seealso cref="Reset"/>
        public void Stop()
        {
            if (isSchedulerThread)
            {
                throw new InvalidOperationException("Can't call Stop() from the scheduler thread.");
            }
            lock (runLock)
            {
                if (!isRunning)
                {
                    throw new InvalidOperationException("Clock is not running.");
                }

                // Tell the thread to stop, wait for it to terminate, then discard it.  By the time this is done, we know
                // that the scheduler will not invoke any more messages.
                lock (threadLock)
                {
                    threadShouldExit = true;
                    Monitor.Pulse(threadLock);
                }
                thread.Join();
                thread = null;

                // Stop the stopwatch.
                stopwatch.Stop();

                // The MidiClock is no longer running.
                isRunning = false;
            }
        }

        /// <summary>
        /// Resets the clock to zero and discards pending messages.
        /// </summary>
        /// <remarks>
        /// <para>This method resets the clock to zero and discards any scheduled but
        /// as-yet-unsent messages.  It may only be called when the clock is not running.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Clock is running.</exception>
        /// <seealso cref="Start"/>
        /// <seealso cref="Stop"/>
        public void Reset()
        {
            if (isSchedulerThread)
            {
                throw new InvalidOperationException("Clock is running.");
            }
            lock (runLock)
            {
                if (isRunning)
                {
                    throw new InvalidOperationException("Clock is running.");
                }
                stopwatch.Reset();
                millisecondFudge = 0;
                lock (threadLock)
                {
                    threadMessageQueue.Clear();
                    Monitor.Pulse(threadLock);
                }
            }
        }

        /// <summary>
        /// Schedules a single message based on its beatTime.
        /// </summary>
        /// <param name="message">The message to schedule.</param>
        /// <remarks>
        /// <para>This method schedules a message to be sent at the time indicated in the message's
        /// <see cref="Message.Time"/> property.  It may be called at any time, whether
        /// the clock is running or not.  The message will not be sent until the clock progresses
        /// to the specified time.  (If the clock is never started, or is paused before that time
        /// and not re-started, then the message will never be sent.)</para>
        /// <para>If a message is scheduled for a time that has already passed, then the scheduler
        /// will send the message at the first opportunity.</para>
        /// </remarks>
        public void Schedule(Message message)
        {
            lock (threadLock)
            {
                threadMessageQueue.AddMessage(message);
                Monitor.Pulse(threadLock);                    
            }
        }

        /// <summary>
        /// Schedules a collection of messages, applying an optional time delta to the scheduled
        /// beatTime.
        /// </summary>
        /// <param name="messages">The message to send</param>
        /// <param name="beatTimeDelta">The delta to apply (or zero).</param>
        public void Schedule(List<Message> messages, float beatTimeDelta)
        {
            lock (threadLock)
            {
                if (beatTimeDelta == 0)
                {
                    foreach (Message message in messages)
                    {
                        threadMessageQueue.AddMessage(message);
                    }
                }
                else
                {
                    foreach (Message message in messages)
                    {
                        threadMessageQueue.AddMessage(message.MakeTimeShiftedCopy(beatTimeDelta));
                    }
                }
                Monitor.Pulse(threadLock);
            }
        }

        /// <summary>
        /// Returns the number of milliseconds from now until the specified beat time.
        /// </summary>
        /// <param name="beatTime">The beat time.</param>
        /// <returns>The positive number of milliseconds, or 0 if beatTime is in the past.</returns>
        private long MillisecondsUntil(float beatTime)
        {
            float now = (stopwatch.ElapsedMilliseconds + millisecondFudge) / millisecondsPerBeat;
            return Math.Max(0, (long)((beatTime - now) * millisecondsPerBeat));
        }

        /// <summary>
        /// Worker thread function.
        /// </summary>
        private void ThreadRun()
        {
            isSchedulerThread = true;
            lock (threadLock)
            {
                while (true)
                {
                    if (threadShouldExit)
                    {
                        return;
                    }
                    else if (threadMessageQueue.IsEmpty)
                    {
                        Monitor.Wait(threadLock);
                    }
                    else {
                        long millisToWait = MillisecondsUntil(threadMessageQueue.EarliestTimestamp);
                        if (millisToWait > 0)
                        {
                            Monitor.Wait(threadLock, (int)millisToWait);
                        }
                        else
                        {
                            threadProcessingTime = threadMessageQueue.EarliestTimestamp;
                            List<Message> timeslice = threadMessageQueue.PopEarliest();
                            foreach (Message message in timeslice)
                            {
                                message.SendNow();
                            }
                        }
                    }
                }
            }
        }

        // The timing state is guarded by lock(timingLock).
        private object timingLock;
        private float beatsPerMinute;
        private float millisecondsPerBeat;
        private long millisecondFudge;
        private Stopwatch stopwatch;

        // Running state is guarded by lock(runLock).
        private object runLock;
        private bool isRunning;
        private Thread thread;

        // Thread state is guarded by lock(threadLock).
        private Object threadLock;
        private bool threadShouldExit;
        private float threadProcessingTime;
        private MessageQueue threadMessageQueue;

        /// <summary>
        /// Thread-local, set to true in the scheduler thread, false in all other threads.
        /// </summary>
        [ThreadStatic]
        static bool isSchedulerThread = false;
    }
}
