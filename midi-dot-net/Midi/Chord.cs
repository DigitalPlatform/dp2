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
    /// Description of a chord's pattern starting at the root note.
    /// </summary>
    /// <remarks>
    /// This class describes the ascending sequence of notes included in a chord, starting with
    /// the root note.  It is described in terms of semitones relative to root and letters
    /// relative to the root.  To apply it to particular tonic, pass one of these to the
    /// constructor of <see cref="Chord"/>.
    /// </remarks>
    public class ChordPattern
    {
        #region Properties

        /// <summary>
        /// The name of the chord pattern.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Abbreviation for this chord pattern.
        /// </summary>
        /// <remarks>
        /// This is the string used in the abbreviated name for a chord, placed immediately
        /// after the tonic and before the slashed inversion (if there is one).  For example,
        /// for minor chords the abbreviation is "m", as in "Am".
        /// </remarks>
        public string Abbreviation { get { return abbreviation; } }

        /// <summary>
        /// The ascending note sequence of the chord, in semitones-above-the-root.
        /// </summary>
        /// <remarks>
        /// <para>This sequence starts at zero (for the root) and is monotonically
        /// increasing, each element representing a pitch in semitones above the root.</para>
        /// </remarks>
        public int[] Ascent { get { return ascent; } }

        /// <summary>
        /// The sequence of letters in the chord.
        /// </summary>
        /// <remarks>
        /// <para>This array describes what sequence of letters appears in this chord.  Each
        /// element is a "letter offset", a positive integer that tell you how many letters to
        /// move up from the root for that note.  It must start at zero, representing the
        /// letter for the root note.</para>
        /// </remarks>
        public int[] LetterOffsets { get { return letterOffsets; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a chord pattern.
        /// </summary>
        /// <param name="name">The name of the chord pattern.</param>
        /// <param name="abbreviation">The abbreviation for the chord.  See the
        /// <see cref="Abbreviation"/> property for details.</param>
        /// <param name="ascent">Array encoding the notes in the chord.  See the
        /// <see cref="Ascent"/> property for details.</param>
        /// <param name="letterOffsets">Array encoding the sequence of letters in the chord.
        /// Must be the same length as ascent.  See the <see cref="LetterOffsets"/> property for
        /// details.</param>
        /// <exception cref="ArgumentException">ascent or letterOffsets is invalid, or they have
        /// different lengths.</exception>
        /// <exception cref="ArgumentNullException">an argument is null.</exception>
        public ChordPattern(string name, string abbreviation, int[] ascent, int[] letterOffsets)
        {
            if (name == null || abbreviation == null || ascent == null || letterOffsets == null)
            {
                throw new ArgumentNullException();
            }
            if (ascent.Length != letterOffsets.Length || !IsSequenceValid(ascent) ||
                !IsSequenceValid(letterOffsets))
            {
                throw new ArgumentException();
            }
            this.name = String.Copy(name);
            this.abbreviation = String.Copy(abbreviation);
            this.ascent = new int[ascent.Length];
            Array.Copy(ascent, this.ascent, ascent.Length);
            this.letterOffsets = new int[letterOffsets.Length];
            Array.Copy(letterOffsets, this.LetterOffsets, letterOffsets.Length);
        }

        #endregion

        #region Operators, Equality, Hash Codes

        /// <summary>
        /// ToString returns the pattern name.
        /// </summary>
        /// <returns>The pattern's name, such as "Major" or "Minor".</returns>
        public override string ToString() { return name; }

        /// <summary>
        /// Equality operator does value equality.
        /// </summary>
        public static bool operator ==(ChordPattern a, ChordPattern b)
        {
            return System.Object.ReferenceEquals(a, b) || a.Equals(b);
        }

        /// <summary>
        /// Inequality operator does value inequality.
        /// </summary>
        public static bool operator !=(ChordPattern a, ChordPattern b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Value equality.
        /// </summary>
        public override bool Equals(System.Object obj)
        {
            ChordPattern other = obj as ChordPattern;
            if ((Object)other == null)
            {
                return false;
            }
            if (!this.name.Equals(other.name))
            {
                return false;
            }
            if (!this.abbreviation.Equals(other.abbreviation))
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
            if (this.letterOffsets.Length != other.letterOffsets.Length)
            {
                return false;
            }
            for (int i = 0; i < this.letterOffsets.Length; ++i)
            {
                if (this.letterOffsets[i] != other.letterOffsets[i])
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

        /// <summary>
        /// Returns true if sequence has at least two elements, starts at zero, and is monotonically
        /// increasing.
        /// </summary>
        private bool IsSequenceValid(int[] sequence)
        {
            // Make sure it is non-empty and starts at zero.
            if (sequence.Length < 2 || sequence[0] != 0)
            {
                return false;
            }
            // Make sure it's monotonically increasing.
            for (int i = 1; i < sequence.Length; ++i)
            {
                if (sequence[i] <= sequence[i - 1])
                {
                    return false;
                }
            }
            return true;
        }

        private string name;
        private string abbreviation;
        private int[] ascent;
        private int[] letterOffsets;

        #endregion
    }

    /// <summary>
    /// A chord.
    /// </summary>
    /// <remarks>
    /// <para>A chord is defined by its root note, the sequence of semitones, the sequence of
    /// letters, and the inversion.  The root note is described with a <see cref="Note"/>
    /// because we want to be able to talk about the chord independent of any one octave.  The
    /// pattern of semitones and letters is given by the <see cref="Pattern"/> nested class.  The
    /// inversion is an integer indicating how many rotations the pattern has undergone.
    /// </para>
    /// <para>This class comes with a collection of predefined chord patterns, such as
    /// <see cref="Major"/> and <see cref="Chord.Minor"/>.</para>
    /// </remarks>
    public class Chord
    {
        #region Properties

        /// <summary>
        /// The name of this chord.
        /// </summary>
        public string Name
        {
            get
            {
                if (inversion == 0)
                {
                    return String.Format("{0}{1}", root, pattern.Abbreviation);
                }
                else
                {
                    return String.Format("{0}{1}/{2}", root, pattern.Abbreviation, noteSequence[0]);

                }
            }
        }
        
        /// <summary>The root note of this chord.</summary>
        public Note Root { get { return root; } }

        /// <summary>The bass note of this chord.</summary>
        public Note Bass { get { return noteSequence[0]; } }

        /// <summary>The pattern of this chord.</summary>
        public ChordPattern Pattern { get { return pattern; } }
        
        /// <summary>The inversion of this chord.</summary>
        public int Inversion { get { return inversion; } }

        /// <summary>
        /// The sequence of notes in this chord.
        /// </summary>
        public Note[] NoteSequence { get { return noteSequence; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a chord from its root note, pattern, and inversion.
        /// </summary>
        /// <param name="root">The root note of the chord.</param>
        /// <param name="pattern">The chord pattern.</param>
        /// <param name="inversion">The inversion, in [0..N-1] where N is the number of notes
        /// in pattern.</param>
        /// <exception cref="ArgumentNullException">pattern is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">inversion is out of range.</exception>
        public Chord(Note root, ChordPattern pattern, int inversion)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException();
            }
            if (inversion < 0 || inversion >= pattern.Ascent.Length)
            {
                throw new ArgumentOutOfRangeException("inversion out of range.");
            }
            this.root = root;
            this.pattern = pattern;
            this.inversion = inversion;
            this.positionInOctaveToContains = new bool[12];
            Note[] uninvertedSequence = new Note[pattern.Ascent.Length];
            Build(root, pattern, this.positionInOctaveToContains,
                uninvertedSequence);
            this.noteSequence = new Note[pattern.Ascent.Length];
            RotateArrayLeft(uninvertedSequence, this.noteSequence, inversion);
        }

        /// <summary>
        /// Constructs a chord from a string.
        /// </summary>
        /// <param name="name">The name to parse.  This is the same format as the Name property:
        /// a letter in ['A'..'G'], an optional series of accidentals (#'s or b's), then an
        /// optional inversion specified as a '/' followed by another note name.  If the
        /// inversion is present it must be one of the notes in the chord.</param>
        /// <exception cref="ArgumentNullException">name is null.</exception>
        /// <exception cref="ArgumentException">cannot parse a chord from name.</exception>
        public Chord(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name is null.");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("name is empty.");
            }
            int pos = 0;
            this.root = Note.ParseNote(name, ref pos);
            this.pattern = null;
            foreach (ChordPattern p in Chord.Patterns)
            {
                if (pos + p.Abbreviation.Length > name.Length)
                {
                    continue;
                }
                if (String.Compare(name, pos, p.Abbreviation, 0, p.Abbreviation.Length) != 0)
                {
                    continue;
                }
                if (pos + p.Abbreviation.Length == name.Length ||
                    name[pos+p.Abbreviation.Length] == '/')
                {
                    pos += p.Abbreviation.Length;
                    this.pattern = p;
                    break;
                }
            }
            if (this.pattern == null)
            {
                throw new ArgumentException("name does not match a known chord pattern.");
            }
            // At this point, we know the note and pattern (but not yet the inversion).  Build
            // the chord prior to inversion.
            this.positionInOctaveToContains = new bool[12];
            Note[] uninvertedSequence = new Note[pattern.Ascent.Length];
            Build(root, pattern, this.positionInOctaveToContains,
                uninvertedSequence);
            this.noteSequence = new Note[pattern.Ascent.Length];
            // Now see if there's an inversion.
            this.inversion = 0;
            if (pos < name.Length)
            {
                if (name[pos] != '/')
                {
                    throw new ArgumentException(String.Format("unexpected character '{0}' in name.",
                        name[pos]));
                }
                pos++;
                Note bass = Note.ParseNote(name, ref pos);
                if (name.Length > pos)
                {
                    throw new ArgumentException(String.Format("unexpected character '{0}' in name.",
                        name[pos]));
                }
                this.inversion = Array.IndexOf(uninvertedSequence, bass);
                if (inversion == -1)
                {
                    throw new ArgumentException("invalid bass note for inversion.");
                }
            }
            RotateArrayLeft(uninvertedSequence, this.noteSequence, inversion);            
        }

        #endregion

        #region Chord/Pitch Interaction

        /// <summary>
        /// Returns a list of chords which match the set of input pitches.
        /// </summary>
        /// <param name="pitches">Notes being analyzed.</param>
        /// <returns>A (possibly empty) list of chords.</returns>
        public static List<Chord> FindMatchingChords(List<Pitch> pitches)
        {
            Pitch[] sorted = pitches.ToArray();
            System.Array.Sort(sorted);
            int[] semitonesAboveBass = new int[sorted.Length];
            for (int i = 0; i < sorted.Length; ++i)
            {
                semitonesAboveBass[i] = sorted[i]-sorted[0];
            }

            List<Chord> result = new List<Chord>();
            foreach (ChordPattern pattern in Patterns)
            {
                int[] semitoneSequence = pattern.Ascent;
                if (semitoneSequence.Length != semitonesAboveBass.Length)
                {
                    continue;
                }
                for (int inversion = 0; inversion < semitoneSequence.Length; ++inversion)
                {
                    int[] invertedSequence = new int[semitoneSequence.Length];
                    RotateArrayLeft(semitoneSequence, invertedSequence, inversion);
                    if (inversion != 0)
                    {
                        for (int i = 0; i < semitoneSequence.Length - inversion; ++i)
                        {
                            invertedSequence[i] -= 12;
                        }
                    }
                    int[] iSemitonesAboveBass = new int[invertedSequence.Length];
                    for (int i = 0; i < invertedSequence.Length; ++i)
                    {
                        iSemitonesAboveBass[i] = invertedSequence[i] - invertedSequence[0];
                    }
                    bool equals = true;
                    for (int i = 0; i < iSemitonesAboveBass.Length; ++i)
                    {
                        if (iSemitonesAboveBass[i] != semitonesAboveBass[i])
                        {
                            equals = false;
                            break;
                        }
                    }
                    if (equals)
                    {
                        Pitch rootPitch =
                            inversion == 0 ? sorted[0] : sorted[sorted.Length - inversion];
                        Note rootNote = rootPitch.NotePreferringSharps();
                        result.Add(new Chord(rootNote, pattern, inversion));
                        if (rootPitch.NotePreferringFlats() != rootNote)
                        {
                            Note otherRootNote = rootPitch.NotePreferringFlats();
                            result.Add(new Chord(otherRootNote, pattern, inversion));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if this chord contains the specified pitch.
        /// </summary>
        /// <param name="pitch">The pitch to test.</param>
        /// <returns>True if this chord contains the pitch.</returns>
        public bool Contains(Pitch pitch)
        {
            return positionInOctaveToContains[pitch.PositionInOctave()];
        }

        #endregion

        #region Predefined Chord Patterns

        /// <summary>
        /// Pattern for Major chords.
        /// </summary>
        public static ChordPattern Major =
            new ChordPattern("Major", "", new int[] { 0, 4, 7 }, new int[] { 0, 2, 4 });

        /// <summary>
        /// Pattern for Minor chords.
        /// </summary>
        public static ChordPattern Minor =
            new ChordPattern("Minor", "m", new int[] { 0, 3, 7 }, new int[] { 0, 2, 4 });

        /// <summary>
        /// Pattern for Seventh chords.
        /// </summary>
        public static ChordPattern Seventh =
            new ChordPattern("Seventh", "7", new int[] { 0, 4, 7, 10 }, new int[] { 0, 2, 4, 6 });

        /// <summary>
        /// Pattern for Augmented chords.
        /// </summary>
        public static ChordPattern Augmented =
            new ChordPattern("Augmented", "aug", new int[] { 0, 4, 8 }, new int[] { 0, 2, 4 });

        /// <summary>
        /// Pattern for Diminished chords.
        /// </summary>
        public static ChordPattern Diminished =
            new ChordPattern("Diminished", "dim", new int[] { 0, 3, 6 }, new int[] { 0, 2, 4 });

        /// <summary>
        /// Array of all the built-in chord patterns.
        /// </summary>
        public static ChordPattern[] Patterns = new ChordPattern[]
        {
            Major,
            Minor,
            Seventh,
            Augmented,
            Diminished
        };

        #endregion

        #region Operators, Equality, Hash Codes

        /// <summary>
        /// ToString returns the chord name.
        /// </summary>
        /// <returns>The chord's name.</returns>
        public override string ToString() { return Name; }

        /// <summary>
        /// Equality operator does value equality because Chord is immutable.
        /// </summary>
        public static bool operator ==(Chord a, Chord b)
        {
            return System.Object.ReferenceEquals(a, b) || a.Equals(b);
        }

        /// <summary>
        /// Inequality operator does value inequality because Chord is immutable.
        /// </summary>
        public static bool operator !=(Chord a, Chord b)
        {
            return !(System.Object.ReferenceEquals(a, b) || a.Equals(b));
        }

        /// <summary>
        /// Value equality.
        /// </summary>
        public override bool Equals(System.Object obj)
        {
            Chord c = obj as Chord;
            if ((Object)c == null)
            {
                return false;
            }

            return base.Equals(obj) || (this.root == c.root && this.pattern == c.pattern &&
                this.inversion == c.inversion);
        }

        /// <summary>
        /// Hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return this.root.GetHashCode() + this.inversion.GetHashCode() +
                this.pattern.GetHashCode();
        }

        #endregion

        #region Private

        private static void Build(Note root, ChordPattern pattern,
            bool[] positionInOctaveToContains, Note[] noteSequence)
        {
            for (int i = 0; i < 12; ++i)
            {
                positionInOctaveToContains[i] = false;
            }
            Pitch rootPitch = root.PitchInOctave(0);
            for (int i = 0; i < pattern.Ascent.Length; ++i)
            {
                Pitch pitch = rootPitch + pattern.Ascent[i];
                char letter = (char)(pattern.LetterOffsets[i] + (int)(root.Letter));
                while (letter > 'G')
                {
                    letter = (char)((int)letter - 7);
                }
                noteSequence[i] = pitch.NoteWithLetter(letter);
                positionInOctaveToContains[pitch.PositionInOctave()] = true;
            }
        }

        /// <summary>
        /// Fills dest with a rotated version of source.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="dest">The dest array, which must have the same length and underlying type
        /// as source.</param>
        /// <param name="rotation">The number of elements to rotate to the left by.</param>
        private static void RotateArrayLeft(Array source, Array dest, int rotation)
        {
            if (source.Length != dest.Length)
            {
                throw new ArgumentException("source and dest lengths differ.");
            }
            if (rotation == 0)
            {
                source.CopyTo(dest, 0);
            }
            else
            {
                for (int i = 0; i < source.Length; ++i)
                {
                    dest.SetValue(source.GetValue((rotation + i) % source.Length), i);
                }
            }
        }

        private Note root;
        private ChordPattern pattern;
        int inversion;
        private bool[] positionInOctaveToContains; // for each PositionInOctave, true if that pitch
                                                   // is contained in this chord.
        private Note[] noteSequence; // the note sequence of the scale.

        #endregion
    }
}
