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
    /// Description of a scale's pattern as it ascends through an octave.
    /// </summary>
    /// <remarks>
    /// This class describes the general behavior of a scale as it ascends from a tonic up to
    /// the next tonic.  It is described in terms of semitones relative to the tonic; to apply it to
    /// a particular tonic, pass one of these to the constructor of <see cref="Scale"/>.
    /// </remarks>
    public class ScalePattern
    {
        #region Properties

        /// <summary>The name of the scale being described.</summary>
        public string Name { get { return name; } }

        /// <summary>The ascent of the scale.</summary>
        /// <remarks>
        /// <para>The ascent is expressed as a series of integers, each giving a semitone
        /// distance above the tonic.  It must have at least two elements, start at zero (the
        /// tonic), be monotonically increasing, and stay below 12 (the next tonic above).</para>
        /// <para>The number of elements in the ascent tells us how many notes-per-octave in the
        /// scale.  For example, a heptatonic scale will always have seven elements in the ascent.
        /// </para>
        /// </remarks>
        public int[] Ascent { get { return ascent; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a scale pattern.
        /// </summary>
        /// <param name="name">The name of the scale pattern.</param>
        /// <param name="ascent">The ascending pattern of the scale.  See the <see cref="Ascent"/>
        /// property for a detailed description and requirements.  This parameter is copied.</param>
        /// <exception cref="ArgumentNullException">name or ascent is null.</exception>
        /// <exception cref="ArgumentException">ascent is invalid.</exception>
        public ScalePattern(string name, int[] ascent)
        {
            if (name == null || ascent == null)
            {
                throw new ArgumentNullException();
            }
            // Make sure ascent is valid.
            if (!AscentIsValid(ascent))
            {
                throw new ArgumentException("ascent is invalid.");
            }
            this.name = string.Copy(name);
            this.ascent = new int[ascent.Length];
            Array.Copy(ascent, this.ascent, ascent.Length);
        }

        #endregion

        #region Operators, Equality, Hash Codes

        /// <summary>
        /// ToString returns the pattern name.
        /// </summary>
        /// <returns>The pattern's name, such as "Major" or "Melodic Minor (ascending)".</returns>
        public override string ToString() { return name; }

        /// <summary>
        /// Equality operator does value equality.
        /// </summary>
        public static bool operator ==(ScalePattern a, ScalePattern b)
        {
            return System.Object.ReferenceEquals(a, b) || a.Equals(b);
        }

        /// <summary>
        /// Inequality operator does value inequality.
        /// </summary>
        public static bool operator !=(ScalePattern a, ScalePattern b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Value equality.
        /// </summary>
        public override bool Equals(System.Object obj)
        {
            ScalePattern other = obj as ScalePattern;
            if ((Object)other == null)
            {
                return false;
            }
            if (!this.name.Equals(other.name))
            {
                return false;
            }
            if (this.ascent.Length != other.ascent.Length)
            {
                return false;
            }
            for (int i = 0; i < this.ascent.Length; ++i)
            {
                if (this.ascent[i] != other.ascent[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Hash code.
        /// </summary>
        public override int GetHashCode()
        {
            // TODO
            return 0;
        }

        #endregion

        #region Private

        /// <summary>Returns true if ascent is valid.</summary>
        private bool AscentIsValid(int[] ascent)
        {
            // Make sure it is non-empty, starts at zero, and ends before 12.
            if (ascent.Length < 2 || ascent[0] != 0 || ascent[ascent.Length-1] >= 12)
            {
                return false;
            }
            // Make sure it's monotonically increasing.
            for (int i = 1; i < ascent.Length; ++i)
            {
                if (ascent[i] <= ascent[i - 1])
                {
                    return false;
                }
            }
            return true;
        }

        private string name;
        private int[] ascent;

        #endregion
    }
    
    /// <summary>
    /// A scale based on a pattern and a tonic note.
    /// </summary>
    /// <remarks>
    /// <para>For our purposes, a scale is defined by a tonic and then the pattern that it uses to
    /// ascend up to the next tonic.  The tonic is described with a <see cref="Note"/> because it is
    /// not specific to any one octave.  The ascending pattern is provided by the
    /// <see cref="ScalePattern"/> class.
    /// </para>
    /// <para>This class comes with a collection of predefined patterns, such as
    /// <see cref="Major"/> and <see cref="Scale.HarmonicMinor"/>.</para>
    /// </remarks>
    public class Scale
    {
        #region Properties

        /// <summary>
        /// The scale's human-readable name, such as "G# Major" or "Eb Melodic Minor (ascending)".
        /// </summary>
        public string Name
        {
            get
            {
                return String.Format("{0} {1}", tonic, pattern);
            }
        }

        /// <summary>The tonic of this scale.</summary>
        public Note Tonic { get { return tonic; } }

        /// <summary>The pattern of this scale.</summary>
        public ScalePattern Pattern { get { return pattern; } }

        /// <summary>
        /// The sequence of notes in this scale.
        /// </summary>
        /// <remarks>
        /// <para>This sequence begins at the tonic and ascends, stopping before the next tonic.
        /// </para>
        /// </remarks>
        public Note[] NoteSequence { get { return noteSequence; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a scale from its tonic and its pattern.
        /// </summary>
        /// <param name="tonic">The tonic note.</param>
        /// <param name="pattern">The scale pattern.</param>
        /// <exception cref="ArgumentNullException">tonic or pattern is null.</exception>
        public Scale(Note tonic, ScalePattern pattern)
        {
            if (tonic == null || pattern == null)
            {
                throw new ArgumentNullException();
            }
            this.tonic = tonic;
            this.pattern = pattern;
            this.positionInOctaveToSequenceIndex = new int[12];
            this.noteSequence = new Note[pattern.Ascent.Length];
            int numAccidentals;
            Build(this.tonic, this.pattern, this.positionInOctaveToSequenceIndex, this.noteSequence,
                out numAccidentals);
        }

        #endregion

        #region Scale/Pitch Interaction

        /// <summary>
        /// Returns true if pitch is in this scale.
        /// </summary>
        /// <param name="pitch">The pitch to test.</param>
        /// <returns>True if pitch is in this scale.</returns>
        public bool Contains(Pitch pitch)
        {
            return this.ScaleDegree(pitch) != -1;
        }

        /// <summary>
        /// Returns the scale degree of the given pitch in this scale.
        /// </summary>
        /// <param name="pitch">The pitch to test.</param>
        /// <returns>The scale degree of pitch in this scale, where 1 is the tonic.  Returns -1
        /// if pitch is not in this scale.</returns>
        public int ScaleDegree(Pitch pitch)
        {
            int result = this.positionInOctaveToSequenceIndex[pitch.PositionInOctave()];
            return result == -1 ? -1 : result + 1;
        }

        #endregion

        #region Predefined Scale Patterns

        /// <summary>
        /// Pattern for Major scales.
        /// </summary>
        public static ScalePattern Major =
            new ScalePattern("Major", new int[] { 0, 2, 4, 5, 7, 9, 11 });

        /// <summary>
        /// Pattern for Natural Minor scales.
        /// </summary>
        public static ScalePattern NaturalMinor =
            new ScalePattern("Natural Minor", new int[] { 0, 2, 3, 5, 7, 8, 10 });

        /// <summary>
        /// Pattern for Harmonic Minor scales.
        /// </summary>
        public static ScalePattern HarmonicMinor =
            new ScalePattern("Harmonic Minor", new int[] { 0, 2, 3, 5, 7, 8, 11 });

        /// <summary>
        /// Pattern for Melodic Minor scale as it ascends.
        /// </summary>
        public static ScalePattern MelodicMinorAscending =
            new ScalePattern("Melodic Minor (ascending)",
                  new int[] { 0, 2, 3, 5, 7, 9, 11 });

        /// <summary>
        /// Pattern for Melodic Minor scale as it descends.
        /// </summary>
        public static ScalePattern MelodicMinorDescending =
            new ScalePattern("Melodic Minor (descending)",
                  new int[] { 0, 2, 3, 5, 7, 8, 10 });

        /// <summary>
        /// Pattern for Chromatic scales.
        /// </summary>
        public static ScalePattern Chromatic =
            new ScalePattern("Chromatic",
                  new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });

        /// <summary>
        /// Array of all the built-in scale patterns.
        /// </summary>
        public static ScalePattern[] Patterns = new ScalePattern[]
        {
            Major,
            NaturalMinor,
            HarmonicMinor,
            MelodicMinorAscending,
            MelodicMinorDescending,
            Chromatic
        };

        #endregion

        #region Operators, Equality, Hash Codes

        /// <summary>
        /// ToString returns the scale's human-readable name.
        /// </summary>
        /// <returns>The scale's name, such as "G# Major" or "Eb Melodic Minor (ascending)".
        /// </returns>
        public override string ToString() { return Name; }

        /// <summary>
        /// Equality operator does value equality because Scale is immutable.
        /// </summary>
        public static bool operator ==(Scale a, Scale b)
        {
            return System.Object.ReferenceEquals(a, b) || a.Equals(b);
        }

        /// <summary>
        /// Inequality operator does value inequality because Chord is immutable.
        /// </summary>
        public static bool operator !=(Scale a, Scale b)
        {
            return !(System.Object.ReferenceEquals(a, b) || a.Equals(b));
        }

        /// <summary>
        /// Value equality.
        /// </summary>
        public override bool Equals(System.Object obj)
        {
            Scale other = obj as Scale;
            if ((Object)other == null)
            {
                return false;
            }

            return base.Equals(obj) || (this.tonic == other.tonic && this.pattern == other.pattern);
        }

        /// <summary>
        /// Hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return this.tonic.GetHashCode() + this.pattern.GetHashCode();
        }

        #endregion

        #region Private

        /// <summary>
        /// Builds a scale.
        /// </summary>
        /// <param name="tonic">The tonic.</param>
        /// <param name="pattern">The scale pattern.</param>
        /// <param name="positionInOctaveToSequenceIndex">Must have 12 elements, and is filled with
        /// the 0-indexed scale position (or -1) for each position in the octave.</param>
        /// <param name="noteSequence">Must have pattern.Ascent.Length elements, and is filled with
        /// the notes for each scale degree.</param>
        /// <param name="numAccidentals">Filled with the total number of accidentals in the built
        /// scale.</param>
        private static void Build(Note tonic, ScalePattern pattern,
            int[] positionInOctaveToSequenceIndex, Note[] noteSequence, out int numAccidentals)
        {
            numAccidentals = 0;
            for (int i = 0; i < 12; ++i)
            {
                positionInOctaveToSequenceIndex[i] = -1;
            }
            Pitch tonicPitch = tonic.PitchInOctave(0);
            for (int i = 0; i < pattern.Ascent.Length; ++i)
            {
                Pitch pitch = tonicPitch + pattern.Ascent[i];
                Note note;
                if (pattern.Ascent.Length == 7)
                {
                    char letter = (char)(i + (int)(tonic.Letter));
                    if (letter > 'G')
                    {
                        letter = (char)((int)letter - 7);
                    }
                    note = pitch.NoteWithLetter(letter);
                }
                else
                {
                    note = pitch.NotePreferringSharps();
                }
                noteSequence[i] = note;
                positionInOctaveToSequenceIndex[pitch.PositionInOctave()] = i;
            }
        }

        private Note tonic;
        private ScalePattern pattern;
        private int[] positionInOctaveToSequenceIndex; // for each PositionInOctave, the 0-indexed
                                                       // position of that pitch in noteSequence,
                                                       // or -1 if it's not in the scale.
        private Note[] noteSequence; // the note sequence of the scale.

        #endregion
    }
}
