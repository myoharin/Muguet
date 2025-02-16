using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SineVita.Muguet.Asteraceae.Cosmosia;
using SineVita.Muguet.Asteraceae;
using SineVita.Muguet;
using System.Runtime.InteropServices;
using SineVita.Lonicera;

namespace SineVita.Basil.Muguet.Cosmosia
{
    public class BasilMuguetCosmosia : BasilMuguet
    {
        public BasilMuguetCosmosia() : base() {}

        public void StartTest(){
            ResonanceHelper.ParametersFolderPath = Path.Combine("assets", "resonator-parameters");
            ResonatorCosmosia resonator = new ResonatorCosmosia(244215);
            //test started!
            Log("BasilMuguet Test_1 started");
            int randomMidiValue;
            byte intensity;
            List<int> setMidiValue = new List<int>() {
                64, 78, 71, 74, 67
            };
            List<byte> setIntensity = new List<byte>() {
                40, 20, 40, 80, 40
            };
            for (int i = 0; i < 5; i++) {
                randomMidiValue = setMidiValue[i];
                intensity = setIntensity[i];
                randomMidiValue = r.Next(60, 127);
                intensity = (byte)r.Next(16, 64);
                Log(" - - - Adding pulse number " + i + " with midi index " + randomMidiValue + ". - - -");
                var pitch = new MidiPitch(randomMidiValue);
                var pulse = new Pulse(pitch, intensity);
                resonator.AddPulse(pulse);

                LogPulseList(resonator);

                Log("\nLogging Idyll Amount:\n");
                double delta = 0.02;
                for (int _ = 0; _ < 50; _++) {
                resonator.Process(delta);
                LogResonatorIdyll(resonator); Console.Write($"Time: {Math.Round(_ * delta,2)} | ");
            }

            }
            AddBreaker_1();
            LogPulseList(resonator);
            AddBreaker_1();

            // run some time so it can update
            resonator.Process(0.05);
            resonator.Process(0.05);
            AddBreaker_1();

            // int decision;
            // int index;
            // for (int i = 0; i < 1000; i++) {
            //     decision = r.Next(1, 10);
            //     if (decision >=3 ) {
            //         Log("Adding Pulse");
            //         resonator.AddPulse(new Pulse(new MidiPitch(r.Next(24, 96)), (byte)r.Next(0, 64)));
            //     } else if (decision == 1 && resonator.Lonicera.NodeCount-1 >= 1) {
            //         Log("Deleting Pulse");
            //         index = r.Next(1, resonator.Lonicera.NodeCount-1);
            //         resonator.DeletePulse(resonator.Lonicera.Nodes[index].PulseID);
            //     } else if (decision == 2 && resonator.Lonicera.NodeCount-1 >= 1) {
            //         Log("Mutating Psulse");
            //         index = r.Next(1, resonator.Lonicera.NodeCount-1);
            //         resonator.MutatePulse(resonator.Lonicera.Nodes[index].PulseID, new Pulse(new MidiPitch(r.Next(24, 96)), (byte)r.Next(32, 128)));
            //     }   
            //     resonator.Process(0.05);

            //     //LogPulseList(resonator);
            //     LogResonatorIdyll(resonator);
            //     //AddBreaker_1();

            // }

            // for (int frameRate = 10; frameRate < 200; frameRate++) {
            //     resonator.Resonance = 0;
            //     for (int frames = 0; frames < frameRate; frames++) {
            //         resonator.Process((double)0.9/(double)frameRate);
            //     }
            //     LogResonatorIdyll(resonator); Console.Write($"FrameRate: {frameRate} | ");
            // }

            // LogResonatorParameter(resonator.Parameter);
        }

        // to string functionns
        public static void LogPulse(Pulse? pulse, int? index = null){
            if (pulse == null) {
                Log("Midi Index: " + "NULL" + " | Intensity: " + "NULL" + " | Note Name: " + "NULL");
            }
            else if (index == null) {
               Log("Midi Index: " + HarmonyHelper.HtzToMidi(pulse.Pitch.Frequency) + " | Intensity: " + pulse.Intensity + " | Note Name: " + pulse.Pitch.NoteName);
            }
            else {
                Log($"Pulse Index: {index} | Midi Index: " + HarmonyHelper.HtzToMidi(pulse.Pitch.Frequency) + " | Intensity: " + pulse.Intensity + " | Note Name: " + pulse.Pitch.NoteName);
            }
        }
        public static void LogPulseList(ResonatorCosmosia resonator){
            for (int i = 0; i < resonator.Lonicera.NodeCount; i++) {
                LogPulse(resonator.Lonicera.Nodes[i], i);
            }
        }

        // log wina functions
        public static void LogCosmosiaChannels(ResonatorCosmosia resonator){
            Tuple<int, int> POAindex;
            string n0;
            string n1; 
            float midiIntervalInternal;
            
            var loniceraPulses = resonator.Lonicera.Nodes;
            var loniceraChannels = resonator.Lonicera.Links;
            var parameter = resonator.Parameter;
            CosmosiaPulse? pulse1;
            CosmosiaPulse? pulse2;
            CosmosiaChannel? channel;

            Log($"Logging Cosmosia Channels, Origin Intensity: {parameter.OriginIntensity} | Origin: {parameter.Origin.NoteName}");
            
            for (int i = 0; i < resonator.Lonicera.LinkCount; i++) {
                POAindex = Lonicera<int,int>.LinkToNodesIndex(i);
                pulse1 = loniceraPulses[POAindex.Item1];
                pulse2 = loniceraPulses[POAindex.Item2];
                channel = loniceraChannels[i];

                if (pulse1 != null && pulse2 != null && channel != null) {
                    if (POAindex.Item1 == 0) {n0 = parameter.Origin.NoteName;}
                    else {n0 = pulse1.Pitch.NoteName;}
                    n1 = pulse2.Pitch.NoteName;

                    midiIntervalInternal = HarmonyHelper.HtzToMidiInterval(channel.Interval.FrequencyRatio);
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
            Log($"- - - Resonator Parameter: {parameter.ResonatorParameterId} - - - ");
            Log($"OriginFrequency: {parameter.Origin.Frequency} | OriginIntensity: {parameter.OriginIntensity} | MaxIdyllAmount: {parameter.MaxIdyllAmount} | CriticalEffect: {parameter.CriticalEffect} | CriticalEffectDurationThreshold: {parameter.CriticalEffectDurationThreshold} | CriticalEffectIntensity: {parameter.CriticalEffectIntensity}");
            Log($"InflowLimit: {parameter.InflowLimit} | OutflowLimit: {parameter.OutflowLimit} | OverflowLimit: {parameter.OverflowLimit}");
            for (int i = 0; i < 14 ; i++) {
                LogChannelParameter(parameter.GetChannelParameter((byte)i));
            }
        }
        public static void LogChannelParameter(ChannelParameterCosmosia cacheParam) {
            Log($"ID: {(int)cacheParam.ChannelId} | InflowMultiplier: {cacheParam.InflowMultiplier} | OutflowMultiplier: {cacheParam.OutflowMultiplier} | OverflowMultiplier: {cacheParam.OverflowMultiplier} | InflowEffect: {cacheParam.InflowEffect} | OutflowEffect: {cacheParam.OutflowEffect} | OverflowEffect: {cacheParam.OverflowEffect}");
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