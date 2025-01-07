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
            for (int i = 0; i < 5; i++) {
                randomMidiValue = setMidiValue[i];
                //randomMidiValue = r.Next(60, 127);
                intensity = (byte)r.Next(16, 64);
                Log("\n - - - Adding pulse number " + i + " with midi index " + randomMidiValue + ". - - -");
                Resonator.AddPulse(new Pulse(new MidiPitch(randomMidiValue),intensity));
            }
            AddBreaker_1();
            LogPulseList(Resonator);
            AddBreaker_1();

            // run some time so it can update
            Resonator.Process(0.05);
            Resonator.Process(0.05);
            Log("Ran Resonator.Process()");
            AddBreaker_1();

            // log em
            LogCosmosiaChannels(Resonator);

            Log("\nLogging Idyll Amount:\n");
            for (int _ = 0; _ < 800; _++) {
                Resonator.Process(0.02);
                LogResonatorIdyll(Resonator);
            }
            
            // LogResonatorParameter(ResonanceHelperCosmosia.GetResonatorParameter(787726));
        }

        // to string functions
        public static void LogPulse(CosmosiaPulse? pulse){
            if (pulse == null) {
                Log("Midi Index: " + "NULL" + " | Intensity: " + "NULL" + " | Note Name: " + "NULL");
            }
            else {
               Log("Midi Index: " + HarmonyHelper.CalculateHtzToMidi(pulse.Pitch.Frequency) + " | Intensity: " + pulse.Intensity + " | Note Name: " + HarmonyHelper.ConvertMidiToNoteName((int)HarmonyHelper.CalculateHtzToMidi(pulse.Pitch.Frequency)));
            }
        }
        public static void LogPulseList(ResonatorCosmosia resonator){
            for (int i = 0; i < resonator.Lonicera.NodeCount; i++) {
                LogPulse(resonator.Lonicera.Nodes[i]);
            }
        }

        // log wina functions
        public static void LogCosmosiaChannels(ResonatorCosmosia resonator){
            Log($"Logging Cosmosia Channels, Origin Intensity: {ResonanceHelperCosmosia.GetResonatorParameter(787726).OriginIntensity} | Origin: {HarmonyHelper.ConvertMidiToNoteName((int)HarmonyHelper.CalculateHtzToMidi(ResonanceHelperCosmosia.GetResonatorParameter(787726).Origin.Frequency ))}");
            Tuple<int, int> POAindex;
            string n0;
            string n1;
            int N0;
            int N1;
            float midiIntervalInternal;
            
            var loniceraPulses = resonator.Lonicera.Nodes;
            var loniceraChannels = resonator.Lonicera.Links;
            CosmosiaPulse? pulse1;
            CosmosiaPulse? pulse2;
            CosmosiaChannel? channel;
            
            for (int i = 0; i < resonator.Lonicera.LinkCount; i++) {
                POAindex = HarmonyHelper.CalculateWINAIndexResult(i);
                pulse1 = loniceraPulses[POAindex.Item1];
                pulse2 = loniceraPulses[POAindex.Item2];
                channel = loniceraChannels[i];

                if (pulse1 != null && pulse2 != null && channel != null) {
                    if (POAindex.Item1 == 0) {n0 = HarmonyHelper.ConvertHtzToNoteName(ResonanceHelperCosmosia.GetResonatorParameter(787726).Origin.Frequency);}
                    else {n0 = HarmonyHelper.ConvertHtzToNoteName(pulse1.Pitch.Frequency);}
                    n1 = HarmonyHelper.ConvertHtzToNoteName(pulse2.Pitch.Frequency);
                    
                    if (POAindex.Item1 == 0) {N0 = (int)HarmonyHelper.CalculateHtzToMidi(ResonanceHelperCosmosia.GetResonatorParameter(787726).Origin.Frequency);}
                    else {N0 = (int)HarmonyHelper.CalculateHtzToMidi(pulse1.Pitch.Frequency);}
                    N1 = (int)HarmonyHelper.CalculateHtzToMidi(pulse2.Pitch.Frequency);

                    midiIntervalInternal = HarmonyHelper.CalculateHtzToMidiInterval(channel.Interval.FrequencyRatio);
                    // launch!
                    Log($"Index: {i} | n{POAindex.Item2} -> n{POAindex.Item1} | {n1} -> {n0} | MidiIntervInt: {midiIntervalInternal} | FreqRatio: {Math.Round(channel.Interval.FrequencyRatio,2)} | intensity: {channel.Intensity} | ChannelID: {channel.ChannelId}");
                }
            }
        }
        // LogIdyll
        public static void LogResonatorIdyll(ResonatorCosmosia resonator, int rounding = 1){
            float getSignificantFigure(float input) {
                return (float)Math.Round(input, rounding);
            }
            Log($"Resonance: {getSignificantFigure(resonator.Resonance)} | NetInflow: {getSignificantFigure(resonator.NetInflow)} | NetOutflow: {getSignificantFigure(resonator.NetOutflow)} | NetOverflow: {getSignificantFigure(resonator.NetOverflow)} | State: {resonator.State}");
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
            Log($"ID: {(int)cacheParam.ChannelId} | OutflowMultiplier: {cacheParam.OutflowMultiplier} | OverflowMultiplier: {cacheParam.OverflowMultiplier} | OutflowEffect: {cacheParam.OutflowEffect} | OverflowEffect: {cacheParam.OverflowEffect}");
        }
        // Log Midi to Shits function
        public static void LogMidiIntervalsToChannel(){
            Log("\n--- --- --- Testing MidiToChannelID - N2R --- --- ---\n");
            for (int i = 0; i < 128; i++) {
                int N2R = (int)ResonanceHelperCosmosia.IntervalToChannelID(new MidiPitchInterval(i), true);
                int N2N = (int)ResonanceHelperCosmosia.IntervalToChannelID(new MidiPitchInterval(i), false);
                bool IsStructual = ResonanceHelperCosmosia.IntervalIsStructual(new MidiPitchInterval(i));
                
                if (N2R == (int)CosmosiaChannelId.Null) {N2R = -1;}
                Log($"Midi: {i} | IsStructual: {IsStructual} | N2R: {N2R} | N2N: {N2N}");
            }
        }
    }
}