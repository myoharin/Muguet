using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SineVita.Muguet.Asteraceae.Cosmosia;
using SineVita.Muguet;

namespace SineVita.Basil.Muguet
{
    class BasilMuguetCosmosia : BasilMuguet
    {
        ResonatorCosmosia Resonator = new ResonatorCosmosia(787726);
        Random r = new Random();
        public BasilMuguetCosmosia() : base() {}

        public void Test1(){
            //test started!
            Log("BasilMuguet Test_1 started");
            
            // add initial pulse
            int randomMidiValue;
            byte intensity;
            List<int> setMidiValue = new List<int>() {
                64, 78, 71, 74, 67
            };
            for (int i = 0; i < 3; i++) {
                randomMidiValue = r.Next(60, 127); //setMidiValue[i];
                intensity = (byte)r.Next(16, 64);
                Log("\n - - - Adding pulse number " + i + " with midi index " + randomMidiValue + ". - - -");
                Resonator.AddPulse(new Pulse(new MidiPitch(randomMidiValue),intensity));
            }
            AddBreaker_1();
            LogPulseList();
            AddBreaker_1();

            // run some time so it can update
            Resonator.Process(0.05);
            Resonator.Process(0.05);
            Log("Ran Resonator.Process()");
            AddBreaker_1();

            // log em
            LogResonatorWINA(Resonator);

            Log("\nLogging Idyll Amount:\n");
            for (int i = 0; i < 1 ; i++) {
                for (int j = 0; j < 40; j++) {
                    // Resonator.Process(0.02);
                    //LogResonatorIdyll(Resonator);
                }
            }
            
            //LogResonatorParameter(ResonanceHelperCosmosia.GetResonatorParameter(787726));
        }

        // * dotnet build
        // * C:\Users\tomin\OneDrive\Documents\GitHub\SineVita\prototypes\csPrototype\bin\Debug\net8.0\csPrototype.exe

        // to string functions
        public void LogPulse(Pulse pulse){
            Log("Midi Index: " + HarmonyHelper.CalculateHtzToMidi(pulse.Pitch.Frequency) + " | Intensity: " + pulse.Intensity + " | Note Name: " + HarmonyHelper.ConvertMidiToNoteName((int)HarmonyHelper.CalculateHtzToMidi(pulse.Pitch.Frequency)));
        }
        public void LogPulseList(){
            for (int i = 0; i < Resonator.PulseInput.Count; i++) {
                LogPulse(Resonator.PulseInput[i]);
            }
        }

        // log wina functions
        public static void LogResonatorWINA(ResonatorCosmosia resonator){
            Log($"Logging Interval Intensity, Origin Intensity: {ResonanceHelperCosmosia.GetResonatorParameter(787726).OriginIntensity} | Origin: {HarmonyHelper.ConvertMidiToNoteName((int)HarmonyHelper.CalculateHtzToMidi(ResonanceHelperCosmosia.GetResonatorParameter(787726).Origin.Frequency ))}");
            double frequencyRatio;
            float midiIntervalInternal;
            byte intensity;
            byte channelID;
            Tuple<int, int> POAindex = new Tuple<int, int>(0,0);
            string n0;
            string n1;
            int N0;
            int N1;
            
            for (int i = 0; i < HarmonyHelper.CalculateWINASize(resonator.PulseInput.Count); i++) {
                POAindex = HarmonyHelper.CalculateWINAIndexResult(i);
                // POA -> PAC conversion, account for 0 = Origin
                if (POAindex.Item1 == 0) {n0 = HarmonyHelper.ConvertHtzToNoteName(ResonanceHelperCosmosia.GetResonatorParameter(787726).Origin.Frequency);}
                else {n0 = HarmonyHelper.ConvertHtzToNoteName(resonator.PulseInput[POAindex.Item1-1].Pitch.Frequency);}
                n1 = HarmonyHelper.ConvertHtzToNoteName(resonator.PulseInput[POAindex.Item2-1].Pitch.Frequency);
                
                if (POAindex.Item1 == 0) {N0 = (int)HarmonyHelper.CalculateHtzToMidi(ResonanceHelperCosmosia.GetResonatorParameter(787726).Origin.Frequency);}
                else {N0 = (int)HarmonyHelper.CalculateHtzToMidi(resonator.PulseInput[POAindex.Item1-1].Pitch.Frequency);}
                N1 = (int)HarmonyHelper.CalculateHtzToMidi(resonator.PulseInput[POAindex.Item2-1].Pitch.Frequency);

                //prep value for launch
                frequencyRatio = Math.Round(resonator.WINA[i].FrequencyRatio,2);
                midiIntervalInternal = HarmonyHelper.CalculateHtzToMidiInterval(resonator.WINA[i].FrequencyRatio);
                intensity = resonator.PitchIntervalIntensity[i];
                channelID = resonator.IntervalChannelID[i];
                // launch!
                Log($"Index: {i} | n{POAindex.Item2} -> n{POAindex.Item1} | {n1} -> {n0} | MidiIntervInt: {midiIntervalInternal} | FreqRatio: {frequencyRatio} | intensity: {intensity} | ChannelID: {channelID}");
            }
        }
        // LogIdyll
        public static void LogResonatorIdyll(ResonatorCosmosia resonator, int rounding = 1){
            float getSignificantFigure(float input) {
                return (float)Math.Round(input, rounding);
            }
            Log($"IdyllAmount: {getSignificantFigure(resonator.IdyllAmount)} | NetInflow: {getSignificantFigure(resonator.NetInflowFlowrate)} | NetOutflow: {getSignificantFigure(resonator.NetOutflowFlowrate)} | NetOverflow: {getSignificantFigure(resonator.NetOverflowFlowrate)} | IsManaDefecit: {resonator.IsManaDefecit} | IsOverflow: {resonator.IsOverflowing} | IsCriticalOverflow: {resonator.IsCriticalOverflow} | IsCriticalState: {resonator.IsCriticalState}");
        }
        // Channel Parameter
        public static void LogResonatorParameter(ResonatorParameterCosmosia parameter) {
            Log($"- - - Resonator Parameter: {parameter.ResonatorParameterID} - - - ");
            Log($"OriginFrequency: {parameter.Origin.Frequency} | OriginIntensity: {parameter.OriginIntensity} | MaxIdyllAmount: {parameter.MaxIdyllAmount} | CriticalEffect: {parameter.CriticalEffect} | CriticalEffectDurationThreshold: {parameter.CriticalEffectDurationThreshold} | CriticalEffectIntensity: {parameter.CriticalEffectIntensity}");
            Log($"InflowLimit: {parameter.InflowLimit} | OutflowLimit: {parameter.OutflowLimit} | OverflowLimit: {parameter.OverflowLimit}");
            for (int i = 0; i < 14 ; i++) {
                LogChannelParameter(parameter.GetChannelParameter((byte)i));
            }
        }
        public static void LogChannelParameter(ChannelParameterCosmosia cacheParam) {
            Log($"ID: {cacheParam.ChannelID} | OutflowMultiplier: {cacheParam.OutflowMultiplier} | OverflowMultiplier: {cacheParam.OverflowMultiplier} | OutflowEffect: {cacheParam.OutflowEffect} | OverflowEffect: {cacheParam.OverflowEffect}");
        }
        // Log Midi to Shits function
        public static void LogMidiIntervalsToChannel(){
            Log("\n--- --- --- Testing MidiToChannelID - N2R --- --- ---\n");
            for (int i = 0; i < 128; i++) {
                int N2R = ResonanceHelperCosmosia.MIDIPitchIntervalToChannelID(new MidiPitchInterval(i), true);
                int N2N = ResonanceHelperCosmosia.MIDIPitchIntervalToChannelID(new MidiPitchInterval(i), false);
                bool IsStructual = ResonanceHelperCosmosia.MIDIPitchIntervalIsStructual(new MidiPitchInterval(i));
                
                if (N2R == 255) {N2R = -1;}
                Log($"Midi: {i} | IsStructual: {IsStructual} | N2R: {N2R} | N2N: {N2N}");
            }
        }
    }
}