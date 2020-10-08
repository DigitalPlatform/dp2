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
    /// General MIDI instrument, used in Program Change messages.
    /// </summary>
    /// <remarks>
    /// <para>The MIDI protocol defines a Program Change message, which can be used to switch a
    /// device among "presets".  The General MIDI specification further standardizes those presets
    /// into the specific instruments in this enum.  General-MIDI-compliant devices will
    /// have these particular instruments; non-GM devices may have other instruments.</para>
    /// <para>MIDI instruments are one-indexed in the spec, but they're zero-indexed in code, so
    /// we have them zero-indexed here.</para>
    /// <para>This enum has extension methods, such as <see cref="InstrumentExtensionMethods.Name"/>
    /// and <see cref="InstrumentExtensionMethods.IsValid"/>, defined in
    /// <see cref="InstrumentExtensionMethods"/>.</para>
    /// </remarks>
    public enum Instrument
    {
        // Piano Family:

        /// <summary>General MIDI instrument 0 ("Acoustic Grand Piano").</summary>
        AcousticGrandPiano = 0,
        /// <summary>General MIDI instrument 1 ("Bright Acoustic Piano").</summary>
        BrightAcousticPiano = 1,
        /// <summary>General MIDI instrument 2 ("Electric Grand Piano").</summary>
        ElectricGrandPiano = 2,
        /// <summary>General MIDI instrument 3 ("Honky Tonk Piano").</summary>
        HonkyTonkPiano = 3,
        /// <summary>General MIDI instrument 4 ("Electric Piano 1").</summary>
        ElectricPiano1 = 4,
        /// <summary>General MIDI instrument 5 ("Electric Piano 2").</summary>
        ElectricPiano2 = 5,
        /// <summary>General MIDI instrument 6 ("Harpsichord").</summary>
        Harpsichord = 6,
        /// <summary>General MIDI instrument 7 ("Clavinet").</summary>
        Clavinet = 7,

        // Chromatic Percussion Family:

        /// <summary>General MIDI instrument 8 ("Celesta").</summary>
        Celesta = 8,
        /// <summary>General MIDI instrument 9 ("Glockenspiel").</summary>
        Glockenspiel = 9,
        /// <summary>General MIDI instrument 10 ("Music Box").</summary>
        MusicBox = 10,
        /// <summary>General MIDI instrument 11 ("Vibraphone").</summary>
        Vibraphone = 11,
        /// <summary>General MIDI instrument 12 ("Marimba").</summary>
        Marimba = 12,
        /// <summary>General MIDI instrument 13 ("Xylophone").</summary>
        Xylophone = 13,
        /// <summary>General MIDI instrument 14 ("Tubular Bells").</summary>
        TubularBells = 14,
        /// <summary>General MIDI instrument 15 ("Dulcimer").</summary>
        Dulcimer = 15,

        // Organ Family:

        /// <summary>General MIDI instrument 16 ("Drawbar Organ").</summary>
        DrawbarOrgan = 16,
        /// <summary>General MIDI instrument 17 ("Percussive Organ").</summary>
        PercussiveOrgan = 17,
        /// <summary>General MIDI instrument 18 ("Rock Organ").</summary>
        RockOrgan = 18,
        /// <summary>General MIDI instrument 19 ("Church Organ").</summary>
        ChurchOrgan = 19,
        /// <summary>General MIDI instrument 20 ("Reed Organ").</summary>
        ReedOrgan = 20,
        /// <summary>General MIDI instrument 21 ("Accordion").</summary>
        Accordion = 21,
        /// <summary>General MIDI instrument 22 ("Harmonica").</summary>
        Harmonica = 22,
        /// <summary>General MIDI instrument 23 ("Tango Accordion").</summary>
        TangoAccordion = 23,

        // Guitar Family:

        /// <summary>General MIDI instrument 24 ("Acoustic Guitar (nylon)").</summary>
        AcousticGuitarNylon = 24,
        /// <summary>General MIDI instrument 25 ("Acoustic Guitar (steel)").</summary>
        AcousticGuitarSteel = 25,
        /// <summary>General MIDI instrument 26 ("Electric Guitar (jazz)").</summary>
        ElectricGuitarJazz = 26,
        /// <summary>General MIDI instrument 27 ("Electric Guitar (clean)").</summary>
        ElectricGuitarClean = 27,
        /// <summary>General MIDI instrument 28 ("Electric Guitar (muted)").</summary>
        ElectricGuitarMuted = 28,
        /// <summary>General MIDI instrument 29 ("Overdriven Guitar").</summary>
        OverdrivenGuitar = 29,
        /// <summary>General MIDI instrument 30 ("Distortion Guitar").</summary>
        DistortionGuitar = 30,
        /// <summary>General MIDI instrument 31 ("Guitar Harmonics").</summary>
        GuitarHarmonics = 31,

        // Bass Family:

        /// <summary>General MIDI instrument 32 ("Acoustic Bass").</summary>
        AcousticBass = 32,
        /// <summary>General MIDI instrument 33 ("Electric Bass (finger)").</summary>
        ElectricBassFinger = 33,
        /// <summary>General MIDI instrument 34 ("Electric Bass (pick)").</summary>
        ElectricBassPick = 34,
        /// <summary>General MIDI instrument 35 ("Fretless Bass").</summary>
        FretlessBass = 35,
        /// <summary>General MIDI instrument 36 ("Slap Bass 1").</summary>
        SlapBass1 = 36,
        /// <summary>General MIDI instrument 37 ("Slap Bass 2").</summary>
        SlapBass2 = 37,
        /// <summary>General MIDI instrument 38 ("Synth Bass 1").</summary>
        SynthBass1 = 38,
        /// <summary>General MIDI instrument 39("Synth Bass 2").</summary>
        SynthBass2 = 39,

        // Strings Family:

        /// <summary>General MIDI instrument 40 ("Violin").</summary>
        Violin = 40,
        /// <summary>General MIDI instrument 41 ("Viola").</summary>
        Viola = 41,
        /// <summary>General MIDI instrument 42 ("Cello").</summary>
        Cello = 42,
        /// <summary>General MIDI instrument 43 ("Contrabass").</summary>
        Contrabass = 43,
        /// <summary>General MIDI instrument 44 ("Tremolo Strings").</summary>
        TremoloStrings = 44,
        /// <summary>General MIDI instrument 45 ("Pizzicato Strings").</summary>
        PizzicatoStrings = 45,
        /// <summary>General MIDI instrument 46 ("Orchestral Harp").</summary>
        OrchestralHarp = 46,
        /// <summary>General MIDI instrument 47 ("Timpani").</summary>
        Timpani = 47,

        // Ensemble Family:

        /// <summary>General MIDI instrument 48 ("String Ensemble 1").</summary>
        StringEnsemble1 = 48,
        /// <summary>General MIDI instrument 49 ("String Ensemble 2").</summary>
        StringEnsemble2 = 49,
        /// <summary>General MIDI instrument 50 ("Synth Strings 1").</summary>
        SynthStrings1 = 50,
        /// <summary>General MIDI instrument 51 ("Synth Strings 2").</summary>
        SynthStrings2 = 51,
        /// <summary>General MIDI instrument 52 ("Choir Aahs").</summary>
        ChoirAahs = 52,
        /// <summary>General MIDI instrument 53 ("Voice oohs").</summary>
        VoiceOohs = 53,
        /// <summary>General MIDI instrument 54 ("Synth Voice").</summary>
        SynthVoice = 54,
        /// <summary>General MIDI instrument 55 ("Orchestra Hit").</summary>
        OrchestraHit = 55,

        // Brass Family:

        /// <summary>General MIDI instrument 56 ("Trumpet").</summary>
        Trumpet = 56,
        /// <summary>General MIDI instrument 57 ("Trombone").</summary>
        Trombone = 57,
        /// <summary>General MIDI instrument 58 ("Tuba").</summary>
        Tuba = 58,
        /// <summary>General MIDI instrument 59 ("Muted Trumpet").</summary>
        MutedTrumpet = 59,
        /// <summary>General MIDI instrument 60 ("French Horn").</summary>
        FrenchHorn = 60,
        /// <summary>General MIDI instrument 61 ("Brass Section").</summary>
        BrassSection = 61,
        /// <summary>General MIDI instrument 62 ("Synth Brass 1").</summary>
        SynthBrass1 = 62,
        /// <summary>General MIDI instrument 63 ("Synth Brass 2").</summary>
        SynthBrass2 = 63,

        // Reed Family:

        /// <summary>General MIDI instrument 64 ("Soprano Sax").</summary>
        SopranoSax = 64,
        /// <summary>General MIDI instrument 65 ("Alto Sax").</summary>
        AltoSax = 65,
        /// <summary>General MIDI instrument 66 ("Tenor Sax").</summary>
        TenorSax = 66,
        /// <summary>General MIDI instrument 67 ("Baritone Sax").</summary>
        BaritoneSax = 67,
        /// <summary>General MIDI instrument 68 ("Oboe").</summary>
        Oboe = 68,
        /// <summary>General MIDI instrument 69 ("English Horn").</summary>
        EnglishHorn = 69,
        /// <summary>General MIDI instrument 70 ("Bassoon").</summary>
        Bassoon = 70,
        /// <summary>General MIDI instrument 71 ("Clarinet").</summary>
        Clarinet = 71,

        // Pipe Family:

        /// <summary>General MIDI instrument 72 ("Piccolo").</summary>
        Piccolo = 72,
        /// <summary>General MIDI instrument 73 ("Flute").</summary>
        Flute = 73,
        /// <summary>General MIDI instrument 74 ("Recorder").</summary>
        Recorder = 74,
        /// <summary>General MIDI instrument 75 ("PanFlute").</summary>
        PanFlute = 75,
        /// <summary>General MIDI instrument 76 ("Blown Bottle").</summary>
        BlownBottle = 76,
        /// <summary>General MIDI instrument 77 ("Shakuhachi").</summary>
        Shakuhachi = 77,
        /// <summary>General MIDI instrument 78 ("Whistle").</summary>
        Whistle = 78,
        /// <summary>General MIDI instrument 79 ("Ocarina").</summary>
        Ocarina = 79,

        // Synth Lead Family:

        /// <summary>General MIDI instrument 80 ("Lead 1 (square)").</summary>
        Lead1Square = 80,
        /// <summary>General MIDI instrument 81 ("Lead 2 (sawtooth)").</summary>
        Lead2Sawtooth = 81,
        /// <summary>General MIDI instrument 82 ("Lead 3 (calliope)").</summary>
        Lead3Calliope = 82,
        /// <summary>General MIDI instrument 83 ("Lead 4 (chiff)").</summary>
        Lead4Chiff = 83,
        /// <summary>General MIDI instrument 84 ("Lead 5 (charang)").</summary>
        Lead5Charang = 84,
        /// <summary>General MIDI instrument 85 ("Lead 6 (voice)").</summary>
        Lead6Voice = 85,
        /// <summary>General MIDI instrument 86 ("Lead 7 (fifths)").</summary>
        Lead7Fifths = 86,
        /// <summary>General MIDI instrument 87 ("Lead 8 (bass + lead)").</summary>
        Lead8BassPlusLead = 87,

        // Synth Pad Family:

        /// <summary>General MIDI instrument 88 ("Pad 1 (new age)").</summary>
        Pad1NewAge = 88,
        /// <summary>General MIDI instrument 89 ("Pad 2 (warm)").</summary>
        Pad2Warm = 89,
        /// <summary>General MIDI instrument 90 ("Pad 3 (polysynth)").</summary>
        Pad3Polysynth = 90,
        /// <summary>General MIDI instrument 91 ("Pad 4 (choir)").</summary>
        Pad4Choir = 91,
        /// <summary>General MIDI instrument 92 ("Pad 5 (bowed)").</summary>
        Pad5Bowed = 92,
        /// <summary>General MIDI instrument 93 ("Pad 6 (metallic)").</summary>
        Pad6Metallic = 93,
        /// <summary>General MIDI instrument 94 ("Pad 7 (halo)").</summary>
        Pad7Halo = 94,
        /// <summary>General MIDI instrument 95 ("Pad 8 (sweep)").</summary>
        Pad8Sweep = 95,

        // Synth Effects Family:

        /// <summary>General MIDI instrument 96 ("FX 1 (rain)").</summary>
        FX1Rain = 96,
        /// <summary>General MIDI instrument 97 ("FX 2 (soundtrack)").</summary>
        FX2Soundtrack = 97,
        /// <summary>General MIDI instrument 98 ("FX 3 (crystal)").</summary>
        FX3Crystal = 98,
        /// <summary>General MIDI instrument 99 ("FX 4 (atmosphere)").</summary>
        FX4Atmosphere = 99,
        /// <summary>General MIDI instrument 100 ("FX 5 (brightness)").</summary>
        FX5Brightness = 100,
        /// <summary>General MIDI instrument 101 ("FX 6 (goblins)").</summary>
        FX6Goblins = 101,
        /// <summary>General MIDI instrument 102 ("FX 7 (echoes)").</summary>
        FX7Echoes = 102,
        /// <summary>General MIDI instrument 103 ("FX 8 (sci-fi)").</summary>
        FX8SciFi = 103,

        // Ethnic Family:

        /// <summary>General MIDI instrument 104 ("Sitar").</summary>
        Sitar = 104,
        /// <summary>General MIDI instrument 105 ("Banjo").</summary>
        Banjo = 105,
        /// <summary>General MIDI instrument 106 ("Shamisen").</summary>
        Shamisen = 106,
        /// <summary>General MIDI instrument 107 ("Koto").</summary>
        Koto = 107,
        /// <summary>General MIDI instrument 108 ("Kalimba").</summary>
        Kalimba = 108,
        /// <summary>General MIDI instrument 109 ("Bagpipe").</summary>
        Bagpipe = 109,
        /// <summary>General MIDI instrument 110 ("Fiddle").</summary>
        Fiddle = 110,
        /// <summary>General MIDI instrument 111 ("Shanai").</summary>
        Shanai = 111,

        // Percussive Family:

        /// <summary>General MIDI instrument 112 ("Tinkle Bell").</summary>
        TinkleBell = 112,
        /// <summary>General MIDI instrument 113 (Agogo"").</summary>
        Agogo = 113,
        /// <summary>General MIDI instrument 114 ("Steel Drums").</summary>
        SteelDrums = 114,
        /// <summary>General MIDI instrument 115 ("Woodblock").</summary>
        Woodblock = 115,
        /// <summary>General MIDI instrument 116 ("Taiko Drum").</summary>
        TaikoDrum = 116,
        /// <summary>General MIDI instrument 117 ("Melodic Tom").</summary>
        MelodicTom = 117,
        /// <summary>General MIDI instrument 118 ("Synth Drum").</summary>
        SynthDrum = 118,
        /// <summary>General MIDI instrument 119 ("Reverse Cymbal").</summary>
        ReverseCymbal = 119,

        // Sound Effects Family:

        /// <summary>General MIDI instrument 120 ("Guitar Fret Noise").</summary>
        GuitarFretNoise = 120,
        /// <summary>General MIDI instrument 121 ("Breath Noise").</summary>
        BreathNoise = 121,
        /// <summary>General MIDI instrument 122 ("Seashore").</summary>
        Seashore = 122,
        /// <summary>General MIDI instrument 123 ("Bird Tweet").</summary>
        BirdTweet = 123,
        /// <summary>General MIDI instrument 124 ("Telephone Ring").</summary>
        TelephoneRing = 124,
        /// <summary>General MIDI instrument 125 ("Helicopter").</summary>
        Helicopter = 125,
        /// <summary>General MIDI instrument 126 ("Applause").</summary>
        Applause = 126,
        /// <summary>General MIDI instrument 127 ("Gunshot").</summary>
        Gunshot = 127
    };

    /// <summary>
    /// Extension methods for the Instrument enum.
    /// </summary>
    public static class InstrumentExtensionMethods
    {
        /// <summary>
        /// Returns true if the specified instrument is valid.
        /// </summary>
        /// <param name="instrument">The instrument to test.</param>
        public static bool IsValid(this Instrument instrument)
        {
            return (int)instrument >= 0 && (int)instrument < 128;
        }

        /// <summary>
        /// Throws an exception if instrument is not valid.
        /// </summary>
        /// <param name="instrument">The instrument to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">The instrument is out-of-range.
        /// </exception>
        public static void Validate(this Instrument instrument)
        {
            if (!instrument.IsValid())
            {
                throw new ArgumentOutOfRangeException("Instrument out of range");
            }
        }

        /// <summary>
        /// General Midi instrument names, used by GetInstrumentName().
        /// </summary>
        private static string[] InstrumentNames = new string[]
        {
            // Piano Family:
            "Acoustic Grand Piano",
            "Bright Acoustic Piano",
            "Electric Grand Piano",
            "Honky-tonk Piano",
            "Electric Piano 1",
            "Electric Piano 2",
            "Harpsichord",
            "Clavinet",

            // Chromatic Percussion Family:
            "Celesta",
            "Glockenspiel",
            "Music Box",
            "Vibraphone",
            "Marimba",
            "Xylophone",
            "Tubular Bells",
            "Dulcimer",

            // Organ Family:
            "Drawbar Organ",
            "Percussive Organ",
            "Rock Organ",
            "Church Organ",
            "Reed Organ",
            "Accordion",
            "Harmonica",
            "Tango Accordion",

            // Guitar Family:
            "Acoustic Guitar (nylon)",
            "Acoustic Guitar (steel)",
            "Electric Guitar (jazz)",
            "Electric Guitar (clean)",
            "Electric Guitar (muted)",
            "Overdriven Guitar",
            "Distortion Guitar",
            "Guitar harmonics",

            // Bass Family:
            "Acoustic Bass",
            "Electric Bass (finger)",
            "Electric Bass (pick)",
            "Fretless Bass",
            "Slap Bass 1",
            "Slap Bass 2",
            "Synth Bass 1",
            "Synth Bass 2",

            // Strings Family:
            "Violin",
            "Viola",
            "Cello",
            "Contrabass",
            "Tremolo Strings",
            "Pizzicato Strings",
            "Orchestral Harp",
            "Timpani",

            // Ensemble Family:
            "String Ensemble 1",
            "String Ensemble 2",
            "Synth Strings 1",
            "Synth Strings 2",
            "Choir Aahs",
            "Voice Oohs",
            "Synth Voice",
            "Orchestra Hit",

            // Brass Family:
            "Trumpet",
            "Trombone",
            "Tuba",
            "Muted Trumpet",
            "French Horn",
            "Brass Section",
            "Synth Brass 1",
            "Synth Brass 2",
            	
            // Reed Family:
            "Soprano Sax",
            "Alto Sax",
            "Tenor Sax",
            "Baritone Sax",
            "Oboe",
            "English Horn",
            "Bassoon",
            "Clarinet",

            // Pipe Family:
            "Piccolo",
            "Flute",
            "Recorder",
            "Pan Flute",
            "Blown Bottle",
            "Shakuhachi",
            "Whistle",
            "Ocarina",

            // Synth Lead Family:
            "Lead 1 (square)",
            "Lead 2 (sawtooth)",
            "Lead 3 (calliope)",
            "Lead 4 (chiff)",
            "Lead 5 (charang)",
            "Lead 6 (voice)",
            "Lead 7 (fifths)",
            "Lead 8 (bass + lead)",

            // Synth Pad Family:
            "Pad 1 (new age)",
            "Pad 2 (warm)",
            "Pad 3 (polysynth)",
            "Pad 4 (choir)",
            "Pad 5 (bowed)",
            "Pad 6 (metallic)",
            "Pad 7 (halo)",
            "Pad 8 (sweep)",

            // Synth Effects Family:
            "FX 1 (rain)",
            "FX 2 (soundtrack)",
            "FX 3 (crystal)",
            "FX 4 (atmosphere)",
            "FX 5 (brightness)",
            "FX 6 (goblins)",
            "FX 7 (echoes)",
            "FX 8 (sci-fi)",

            // Ethnic Family:
            "Sitar",
            "Banjo",
            "Shamisen",
            "Koto",
            "Kalimba",
            "Bag pipe",
            "Fiddle",
            "Shanai",

            // Percussive Family:
            "Tinkle Bell",
            "Agogo",
            "Steel Drums",
            "Woodblock",
            "Taiko Drum",
            "Melodic Tom",
            "Synth Drum",
            "Reverse Cymbal",

            // Sound Effects Family:
            "Guitar Fret Noise",
            "Breath Noise",
            "Seashore",
            "Bird Tweet",
            "Telephone Ring",
            "Helicopter",
            "Applause",
            "Gunshot"
        };

        /// <summary>
        /// Returns the human-readable name of a MIDI instrument.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <exception cref="ArgumentOutOfRangeException">The instrument is out-of-range.
        /// </exception>
        public static string Name(this Instrument instrument)
        {
            instrument.Validate();
            return InstrumentNames[(int)instrument];
        }
    }
}
