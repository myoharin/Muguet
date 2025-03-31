using System.Collections.Generic;
using System;

namespace SineVita.Muguet
{
    public static class HarmonyHelper
    {
        // basic comonly used cache data for debugging stage.
        public static readonly string[] MidiPitchIntervalName = new string[] {
            // Populate with actual interval names
            "U", "m2", "M2", "m3", "M3", "P4", "T1", "P5", "m6", "M6", "m7", "M7",
            "O1", "m9", "M9", "m10", "M10", "P11", "T2", "P12", "m13", "M13", "m14", "M14",
            "O2", "m16", "M16", "m17", "M17", "P18", "T3", "P19", "m20", "M20", "m21", "M21",
            "O3", "m23", "M23", "m24", "M24", "P25", "T4", "P26", "m27", "M27", "m28", "M28",
            "O4"
        };
        public static readonly string[] IntervalNamePrefix = new string[] {
            "O", "m", "M", "m", "M", "P", "T", "P", "m", "M", "m", "M",
        };
        public static readonly int[] IntervalNameNumber = new int[] {
            -1, 2, 2, 3, 3, 4, -1, 5, 6, 6, 7, 7
        };

        // micellaneous functions
        public static (int reducedNumerator, int reducedDenominator) ReduceFraction(int numerator, int denominator)
        {
            // Nested method to find the greatest common divisor (GCD) recursively
            int GcdRecursive(int a, int b) {
                if (b == 0) {
                    return a;
                }
                return GcdRecursive(b, a % b);
            }

            int gcd = GcdRecursive(numerator, denominator);
            int reducedNumerator = numerator / gcd;
            int reducedDenominator = denominator / gcd;

            return (reducedNumerator, reducedDenominator);
        }
        
        // Midi, frequency and note name conversion methods
        public static float HtzToMidi(double htz, int rounding = 0)
        {
            return (float)Math.Round(69 + 12 * Math.Log2(htz / 440), rounding);
        }
        public static float MidiToHtz(int midi, int rounding = 3)
        {
            return (float)Math.Round(440 * Math.Pow(2, (midi - 69) / 12.0), rounding);
        }

        public static int NoteNameToMidi(string noteName) {
            int index = -100000;
            string[] junkList = { "/", "#", "b", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            string octave = noteName;

            foreach (var junk in junkList) {
                if (octave.Contains(junk)) {
                    octave = octave.Replace(junk, string.Empty);
                }
            }

            for (int i = 0; i < 12; i++) {
                if (noteName.Contains(Pitch.NoteNames[i])) {
                    index = i;
                    break;
                }
            }

            return index + int.Parse(octave) * 12 + 12;
        }
        public static string MidiToNoteName(int midiValue) // does not have lookup equiv
        {   
            int validate(int n) {
                n %= 12;
                if (n < 0) {
                    n += 12;
                }
                return n;
            }
            return $"{Pitch.NoteNames[validate(midiValue)]}{Math.Floor((double)midiValue / 12) - 1}";
        }
        
        public static float HtzToMidiInterval(double htz, int rounding = 0)
        {
            return (float)Math.Round(12 * Math.Log2(htz), rounding);
        }
        public static string HtzToNoteName(double htz, int rounding = 0) {
            return MidiToNoteName((int)HtzToMidi(htz, rounding : rounding));
        }

        public static string MidiToIntervalName(int midiValue) {
            if (midiValue >= MidiPitchIntervalName.Length) {
                int octave = (int)Math.Floor((float)midiValue / 12);
                int modValue = midiValue % 12;

                int num = modValue switch {
                    0 => octave-1,
                    6 => octave,
                    _ => IntervalNameNumber[modValue] + octave * 7
                };
                return $"{IntervalNamePrefix[modValue]}{num}";
            }
            else {
                return MidiPitchIntervalName[midiValue];
            }
        }
        public static string HtzToIntervalName(double htz, int rounding = 0) {
            return MidiToIntervalName((int)HtzToMidiInterval(htz, rounding : rounding));
        }
    
    
    
    }
}