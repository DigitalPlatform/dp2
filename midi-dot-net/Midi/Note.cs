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
    /// A letter and accidental, which together form an octave-independent note.
    /// </summary>
    /// <remarks>
    /// <para>This class lets you define a note by combining a letters A-G with accidentals
    /// (sharps and flats).  Examples of notes are D, B#, and Gbb.  This is the conventional
    /// way to refer to notes in an octave independent way.</para>
    /// <para>Each note unambiguously identifies a pitch (modulo octave), but each pitch has
    /// potentially many notes.  For example, the notes F, E#, D###, and Gbb all resolve to the
    /// same pitch, though the last two names are unlikely to be used in practice.</para>
    /// </remarks>
    public struct Note
    {
        /// <summary>Double-flat accidental value.</summary>
        public static int DoubleFlat = -2;

        /// <summary>Flat accidental value.</summary>
        public static int Flat = -1;

        /// <summary>Natural accidental value.</summary>
        public static int Natural = 0;

        /// <summary>Sharp accidental value.</summary>
        public static int Sharp = 1;

        /// <summary>Double-sharp accidental value.</summary>
        public static int DoubleSharp = 2;

        /// <summary>
        /// Constructs a note from a letter.
        /// </summary>
        /// <param name="letter">The letter, which must be in ['A'..'G'].</param>
        /// <exception cref="ArgumentOutOfRangeException">letter is out of range.</exception>
        public Note(char letter) : this(letter, Natural) { }

        /// <summary>
        /// Constructs a note from a string.
        /// </summary>
        /// <param name="name">The name to parse.  Must begin with a letter in ['A'..'G'],
        /// then optionally be followed by a series of '#' (sharps) or a series of 'b' (flats).
        /// </param>
        /// <exception cref="ArgumentNullException">name is null.</exception>
        /// <exception cref="ArgumentException">name cannot be parsed.</exception>
        public Note(string name)
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
            this = ParseNote(name, ref pos);
            if (name.Length > pos)
            {
                throw new ArgumentException(String.Format("unexpected character '{0}'.",
                    name[pos]));                
            }
        }

        /// <summary>
        /// Constructs a note name from a letter and accidental.
        /// </summary>
        /// <param name="letter">The letter, which must be in ['A'..'G'].</param>
        /// <param name="accidental">The accidental.  Zero means natural, positive values are
        /// sharp by that many semitones, and negative values are flat by that many semitones.
        /// Likely values are <see cref="Natural"/> (0), <see cref="Sharp"/> (1),
        /// <see cref="DoubleSharp"/> (2), <see cref="Flat"/> (-1), and <see cref="DoubleFlat"/>
        /// (-2).</param>
        /// <exception cref="ArgumentOutOfRangeException">letter is out of range.</exception>
        public Note(char letter, int accidental)
        {
            if (letter < 'A' || letter > 'G')
            {
                throw new ArgumentOutOfRangeException("letter out of range.");
            }
            this.letter = letter;
            this.accidental = accidental;
            this.positionInOctave = (LetterToNote[letter - 'A'] + accidental).PositionInOctave();
        }

        /// <summary>The letter for this note name, in ['A'..'G'].</summary>
        public char Letter { get { return letter; } }

        /// <summary>The accidental for this note name.</summary>
        /// <remarks>
        /// <para>Zero means natural, positive values are
        /// sharp by that many semitones, and negative values are flat by that many semitones.
        /// Likely values are <see cref="Natural"/> (0), <see cref="Sharp"/> (1),
        /// <see cref="DoubleSharp"/> (2), <see cref="Flat"/> (-1), and <see cref="DoubleFlat"/>
        /// (-2).</para>
        /// </remarks>
        public int Accidental { get { return accidental; } }

        /// <summary>This note's position in the octave, where octaves start at each C.</summary>
        public int PositionInOctave { get { return positionInOctave; } }

        /// <summary>
        /// ToString returns the note name.
        /// </summary>
        /// <returns>The note name with '#' for sharp and 'b' for flat.  For example, "G", "A#",
        /// "Cb", "Fbb".</returns>
        public override string ToString()
        {
            if (accidental > 0)
            {
                return new string(letter, 1) + new string('#', accidental);
            }
            else if (accidental < 0)
            {
                return new string(letter, 1) + new string('b', -accidental);
            }
            else
            {
                return new string(letter, 1);
            }
        }

        /// <summary>
        /// Parses a Note from s, starting at position pos.
        /// </summary>
        /// <param name="s">The string to parse from.</param>
        /// <param name="pos">The position to start at.  On success, advances pos to after the
        /// end of the note.</param>
        /// <returns>The note.</returns>
        /// <exception cref="ArgumentException">A note cannot be parsed.</exception>
        /// <remarks>
        /// <para>This function must find a valid letter at s[pos], and then optionally a
        /// sequence of '#' (sharps) or 'b' (flats).  It finds as many of the accidental as it can
        /// and then stops at the first character that can't be part of the accidental.</para>
        /// </remarks>
        public static Note ParseNote(string s, ref int pos)
        {
            int p = pos;
            if (s[p] < 'A' || s[p] > 'G')
            {
                throw new ArgumentException(String.Format("invalid note letter: '{0}'", s[p]));
            }
            char letter = s[p];
            p++;
            // Parse the accidental.
            int accidental = 0;
            if (s.Length > p && s[p] == '#')
            {
                while (p < s.Length && s[p] == '#')
                {
                    accidental++;
                    p++;
                }
            }
            else if (s.Length > p && s[p] == 'b')
            {
                while (p < s.Length && s[p] == 'b')
                {
                    accidental--;
                    p++;
                }
            }
            pos = p;
            return new Note(letter, accidental);
        }

        /// <summary>
        /// Returns true if this note name is enharmonic with otherNote.
        /// </summary>
        /// <param name="otherNote">Another note.</param>
        /// <returns>True if the names can refer to the same pitch.</returns>
        public bool IsEharmonicWith(Note otherNote)
        {
            return this.positionInOctave == otherNote.positionInOctave;
        }

        /// <summary>
        /// Returns the pitch for this note in the specified octave.
        /// </summary>
        /// <param name="octave">The octave, where octaves begin at each C and Middle C is the
        /// first note in octave 4.</param>
        /// <returns>The pitch with this name in the specified octave.</returns>
        public Pitch PitchInOctave(int octave)
        {
            return (Pitch)(positionInOctave + 12 * (octave + 1));
        }

        /// <summary>
        /// Returns the pitch for this note that is at or above nearPitch.
        /// </summary>
        /// <param name="nearPitch">The pitch from which the search is based.</param>
        /// <returns>The pitch for this note at or above nearPitch.</returns>
        public Pitch PitchAtOrAbove(Pitch nearPitch)
        {
            int semitoneDelta = positionInOctave - nearPitch.PositionInOctave();
            if (semitoneDelta < 0)
            {
                semitoneDelta += 12;
            }
            return nearPitch + semitoneDelta;
        }

        /// <summary>
        /// Returns the pitch for this note that is at or below nearPitch.
        /// </summary>
        /// <param name="nearPitch">The pitch from which the search is based.</param>
        /// <returns>The pitch for this note at or below nearPitch.</returns>
        public Pitch PitchAtOrBelow(Pitch nearPitch)
        {
            int semitoneDelta = positionInOctave - nearPitch.PositionInOctave();
            if (semitoneDelta > 0)
            {
                semitoneDelta -= 12;
            }
            return nearPitch + semitoneDelta;
        }

        /// <summary>
        /// Returns the number of semitones it takes to move up to the next otherNote.
        /// </summary>
        /// <param name="otherNote">The other note.</param>
        /// <returns>The number of semitones.</returns>
        public int SemitonesUpTo(Note otherNote)
        {
            int semitoneDelta = otherNote.positionInOctave - positionInOctave;
            if (semitoneDelta < 0)
            {
                semitoneDelta += 12;
            }
            return semitoneDelta;
        }

        /// <summary>
        /// Returns the number of semitones it takes to move down to the next otherNote.
        /// </summary>
        /// <param name="otherNote">The other note.</param>
        /// <returns>The number of semitones.</returns>
        public int SemitonesDownTo(Note otherNote)
        {
            int semitoneDelta = positionInOctave - otherNote.positionInOctave;
            if (semitoneDelta < 0)
            {
                semitoneDelta += 12;
            }
            return semitoneDelta;
        }

        /// <summary>Equality operator does value comparison.</summary>
        public static bool operator ==(Note a, Note b) { return a.Equals(b); }

        /// <summary>Inequality operator does value comparison.</summary>
        public static bool operator !=(Note a, Note b) { return !a.Equals(b); }

        /// <summary>
        /// Value equality for Note.
        /// </summary>
        public override bool Equals(System.Object obj)
        {
            if (!(obj is Note))
            {
                return false;
            }
            Note other = (Note)obj;
            return this.letter == other.letter && this.accidental == other.accidental;
        }

        /// <summary>
        /// Hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return this.letter.GetHashCode() + this.accidental.GetHashCode();
        }

        /// <summary>
        /// Table mapping (letter-'A') to the Note in octave -1, used to compute positionInOctave.
        /// </summary>
        private static Pitch[] LetterToNote = new Pitch[] {
            Pitch.ANeg1, Pitch.BNeg1, Pitch.CNeg1, Pitch.DNeg1, Pitch.ENeg1, Pitch.FNeg1,
            Pitch.GNeg1
        };

        private char letter;
        int accidental;
        private int positionInOctave;
    }
}
