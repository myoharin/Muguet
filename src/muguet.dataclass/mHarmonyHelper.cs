using System.Collections.Generic;
using System;
using SineVita.Basil;
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
            int GcdRecursive(int a, int b)
            {
                if (b == 0)
                {
                    return a;
                }
                return GcdRecursive(b, a % b);
            }

            int gcd = GcdRecursive(numerator, denominator);
            int reducedNumerator = numerator / gcd;
            int reducedDenominator = denominator / gcd;

            return (reducedNumerator, reducedDenominator);
        }
        public static Pitch[] SortPitchArray(Pitch[] pitchArray)
        {
            int n = pitchArray.Length;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (pitchArray[j].Frequency > pitchArray[j + 1].Frequency)
                    {
                        // Swap
                        var temp = pitchArray[j];
                        pitchArray[j] = pitchArray[j + 1];
                        pitchArray[j + 1] = temp;
                    }
                }
            }
            return pitchArray;
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

        public static List<Pitch> PACToPOA(List<Pitch> PAC, Pitch Origin){PAC.Insert(0, Origin);return PAC;} //DONE
        public static List<Pitch> POAToPAC(List<Pitch> POA){POA.RemoveAt(0); return POA;} //DONE

            //number of nodes excludes the origin. Return tuple returns (n0, n1) where n1 > n0. numberOfNodes disregard the origin
        public static List<Tuple<int, int>> CalculatePSCOrder(int numberOfNodes){ //DONE
            int listLength = numberOfNodes;
            List<Tuple<int, int>> PSCOrder = new List<Tuple<int, int>>();
            for (int i = 0; i < numberOfNodes;  i++){
                Tuple<int, int> appendedTuple = new Tuple<int, int>(0, i);
                PSCOrder.Add(appendedTuple);
            }
            return PSCOrder;
            }
        public static List<Tuple<int, int>> CalculateWINAOrder(int numberOfNodes){ //DONE
            int listLength = (int)Math.Floor(numberOfNodes * (numberOfNodes + 1) * 0.5f);
            List<Tuple<int, int>> WINAOrder = new List<Tuple<int, int>>();
            for (int a = 1; a < numberOfNodes + 1; a++){
                for (int b = 0; b < a; b++){
                    Tuple<int, int> appendedTuple = new Tuple<int, int>(b, a);
                    WINAOrder.Add(appendedTuple);
                }
            }
            return WINAOrder;
            }
        public static List<Tuple<int, int>> CalculateSINAOrder(int numberOfNodes){ //DONE
            int listLength = numberOfNodes;
            List<Tuple<int, int>> SINAORDER = new List<Tuple<int, int>>();
            for (int i = 0; i < numberOfNodes;  i++){
                Tuple<int, int> appendedTuple = new Tuple<int, int>(i, i+1);
                SINAORDER.Add(appendedTuple);
            }
            return SINAORDER;
            }
        public static List<Tuple<int, int>> CalculatePSINAOrder(int numberOfNodes){ //DONE
            int listLength = numberOfNodes;
            List<Tuple<int, int>> PSINAOrder = new List<Tuple<int, int>>();
            for (int i = 1; i < numberOfNodes+1;  i++){
                Tuple<int, int> appendedTuple = new Tuple<int, int>(0, i);
                PSINAOrder.Add(appendedTuple);
            }
            return PSINAOrder;
            }

        public static Tuple<int, int> CalculatePSCIndexResult(int index){return new Tuple<int, int>(0, index);} //DONE
        public static Tuple<int, int> CalculateWINAIndexResult(int index){ //DONE
            int n1 = (int)Math.Floor(Math.Sqrt(2 * index + 0.25) + 0.5);
            int n0 = index - (int)Math.Floor(n1 * (n1 - 1) * 0.5f);
            return new Tuple<int, int>(n0, n1);}
        public static Tuple<int, int> CalculateSINAIndexResult(int index){return new Tuple<int, int>(index, index+1);} //DONE
        public static Tuple<int, int> CalculatePSINAIndexResult(int index){return new Tuple<int, int>(0, index+1);} //DONE

        public static int CalculatePSCSize(int pulseCount) {return pulseCount;}
        public static int CalculateWINASize(int pulseCount) {return CalculateWINAIndex(0, pulseCount + 1);}
        public static int CalculateSINASize(int pulseCount) {return pulseCount + 1;}
        public static int CalculatePSINASize(int pulseCount) {return pulseCount + 1;}

        public static int CalculatePSCIndex(int n0, int n1){ //DONE
            if (n0 != 0) {return -1;}
            else {return n1;}
        }
        public static int CalculateWINAIndex(int n0, int n1){ //DONE
            return (int)Math.Floor(n1 * (n1 - 1) * 0.5f) + n0;} 
        public static int CalculateSINAIndex(int n0, int n1){ //DONE
            if (n0+1 != n1) {return -1;} 
            else {return n0;}
        }
        public static int CalculatePSINAIndex(int n0, int n1){ //DONE
            if (n0 != 0) {return -1;}
            else {return n1-1;}
        }

            // Efficienct INA to INA functions
        public static List<PitchInterval> WINAToSINA(List<PitchInterval> WINA){ //DONE
            int index;
            int nodeCount = (int)Math.Floor(Math.Sqrt(2 * WINA.Count + 0.25) - 0.5); // not including the origin
            List<PitchInterval> SINA = new List<PitchInterval>();
            for (int i = 1; i < nodeCount+1; i++) {
                index = (int)Math.Floor(i * (i + 1) * 0.5f) - 1;
                SINA.Add(WINA[index]);
            }

            return SINA;}
        public static List<PitchInterval> WINAToPSINA(List<PitchInterval> WINA){ //DONE
            int index;
            int nodeCount = (int)Math.Floor(Math.Sqrt(2 * WINA.Count + 0.25) - 0.5); // not including the origin  
            List<PitchInterval> PSINA = new List<PitchInterval>();
            for (int i = 0; i < nodeCount; i++) {
                index = (int)Math.Floor(i * (i + 1) * 0.5f);
                PSINA.Add(WINA[index]);
            }
            return PSINA;}

    }
}