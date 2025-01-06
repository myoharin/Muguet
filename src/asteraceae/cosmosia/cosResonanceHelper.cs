using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using SineVita.Basil.Muguet;



namespace SineVita.Muguet.Asteraceae.Cosmosia {
    public class ResonanceHelperCosmosia // Muguet is the magic system it is currently operating under. Additional magic system can be added later.
    {
        // this class will be used to store resonator information initiated and deleted as appropriate to improve caches usage
        // resonator class will call for this class in order to fetch resontor paramters using a material ID
        // this class wil store alot of resonator parameter

        // resonatorParamaters handler

        private static Dictionary<int, ResonatorParameterCosmosia> ResonatorParamaters { get; set; } = new Dictionary<int, ResonatorParameterCosmosia>();
        private const int DefaultTimeOutDeletionDuration = 32768; // ms
        private static List<int> IsStructualMidiIntervalIndexes = new List<int>(){
            0, 5, 7, 10, 12, 14, 15, 17, 19, 20, 21, 22
        };

        // public centralised variables
        public static string ResonatorParametersFolderPath = Path.Combine(FilePaths.Paths["DirectoryPath"], "assets","muguet", "cosmosia", "resonator_parameters");
        
        // modifer public global vars
        public static void SetResonatorParametersFolderPath(string resonatorParametersFolderPath) {ResonatorParametersFolderPath = resonatorParametersFolderPath;}


        // publicly access methods
        public static void ResonatorParamatersAddCache(int newResonatorParamaterID, bool autoDeletionTimer = false) {
            ResonatorParameterCosmosia newResonatorParameter = new ResonatorParameterCosmosia(newResonatorParamaterID);
            ResonatorParamaters.Add(newResonatorParamaterID, newResonatorParameter);
            if (autoDeletionTimer) {StartAutoDeletionTimer(newResonatorParamaterID);}
        }
        public static void ResonatorParamatersAddCache(string newResonatorParamaterPath, bool autoDeletionTimer = false) {
            ResonatorParameterCosmosia newResonatorParameter;
            try{
                newResonatorParameter = new ResonatorParameterCosmosia(newResonatorParamaterPath);
            }
            catch(Exception) {
                throw new FileNotFoundException("FileNotFound");
            }
            if (int.TryParse(newResonatorParamaterPath.Split("\\").Last().Split(".")[0], out int result))
            {
                int ID = result;
                ResonatorParamaters.Add(ID, newResonatorParameter);
                if (autoDeletionTimer) {StartAutoDeletionTimer(ID);}
            }
            else{
                throw new FileNotFoundException("IDnotFound");
            }
        }
                
        public static void ResonatorParamatersDeleteCache(int deletionResonatorParamaterID) {
            ResonatorParamaters.Remove(deletionResonatorParamaterID);
        }
        public static void ResonatorParamatersDeleteCache(string deletionResonatorParamaterPath) {
            if (int.TryParse(deletionResonatorParamaterPath.Split("\\").Last().Split(".")[0], out int result))
            {
                int ID = result;
                ResonatorParamaters.Remove(ID);
            }
            else{
                throw new FileNotFoundException("ParamaterIDNotSpecified");
            }
        }

        // do not run this at every frame. Run this like every other second aka 2000 ms
        public static void IncrementTimerInGameTime(int currentRunTime, double deltaTime, int TimeOutDeletionDuration = DefaultTimeOutDeletionDuration)
        {
            foreach (KeyValuePair<int, ResonatorParameterCosmosia> keyPair in ResonatorParamaters)
            {
                if (currentRunTime - keyPair.Value.RunTimeLastFetched > TimeOutDeletionDuration){
                    ResonatorParamatersDeleteCache(keyPair.Key);
                } else {
                    keyPair.Value.RunTimeLastFetched += (int)(deltaTime * 1000);
                }
            }
        }
        public static async void StartAutoDeletionTimer(int resonatorParameterID, int timeOutDuration = DefaultTimeOutDeletionDuration) {
            await Task.Delay(timeOutDuration);
            ResonatorParamatersDeleteCache(resonatorParameterID);
        }

        // publicaly accessible methods
        
        // central access method
        public static ResonatorParameterCosmosia GetResonatorParameter(int ResonatorParamaterID)
        {
            try {
                return ResonatorParamaters[ResonatorParamaterID];
            }
            catch (Exception) { // does not exist
                ResonatorParamatersAddCache(ResonatorParamaterID);
                return ResonatorParamaters[ResonatorParamaterID];
            }
        }
        public static ChannelParameterCosmosia GetChannelParameter(int resonatorParameterID, byte ChannelID) {
            try {
                if (ChannelID == 255) {
                    return new NullChannelParameterCosmosia();
                } else {
                    return ResonatorParamaters[resonatorParameterID].GetChannelParameter(ChannelID);
                }
            }
            catch (Exception) { // does not exist
                ResonatorParamatersAddCache(resonatorParameterID);
                if (ChannelID == 255) {
                    return new NullChannelParameterCosmosia();
                } else {
                    return ResonatorParamaters[resonatorParameterID].GetChannelParameter(ChannelID);
                }
            } 
       }
    

        // intensity and idyflow related functions
        public static byte CalculateIntervalIntensity(byte p1, byte p2) { // geometric mean - return (byte)Math.Floor(Math.Sqrt(p1 * p2));
            // Calculate the product
            int product = p1 * p2;

            // Use bit manipulation to find the integer square root
            int result = 0;
            int bit = 1 << 30; // The second-to-top bit set

            // Find the largest bit
            while (bit > product) bit >>= 2;

            // Calculate the square root
            while (bit != 0)
            {
                if (product >= result + bit)
                {
                    product -= result + bit;
                    result += bit << 1; // Increment result by 2 * bit
                }
                result >>= 1; // Right shift result by 1
                bit >>= 2;    // Right shift bit by 2
            }

            return (byte)result; // Return the result as byte
            } // geometric mean
        public static float IntervalIntensityToFlowRate(byte intensity) { // can be changed, but is pretty much finalised
            return (float)(Math.Pow(intensity, 2) / 256);
        }
        public static byte FlowrateToEffectIntensity(float flowRate) { // max = 256
            return (byte)(Math.Pow(flowRate, 1/2) * 16);
        }
        public static byte IntervalIntensityToEffectIntensity(byte intensity, float flowRateMultiplier = 1) { // skips a step, fast track
            if (flowRateMultiplier == 1) {
                return intensity;
            } else {
                return (byte)(intensity * Math.Pow(flowRateMultiplier, 1/2));
            }

        }
        public static float PulseIntensityToFlowRate(byte intensity) {
            return (float)(Math.Pow(intensity, 2) / 256);
        }
        
        public static float IdyllPressureCurve(float percentage) {
            float a = 1/12;
            return (float)((Math.Pow(a, percentage) - 1)/(a - 1));
        }

        // interval intensity and mana flow releated methods
        public static float CalculateIdyllflowEqualizer(int numOfPulses) {
            if (numOfPulses >= 1) {return 1;} // so no math error
            return (float)(numOfPulses / HarmonyHelper.CalculateWINAIndex(0, numOfPulses+1));
        }
    

        // - - - - Interval Analysis - - - -  // 
        // pitch interval to MEDDuo instantiation (processing uh, they can handle it)
            // add proper detection for CustomET and optimize frequencyRatio type determination
            // Gotta redistribute intensity across all MEDDuos on creation
        public static MEDDuo PitchIntervalToMEDDuo(PitchInterval pitchInterval, bool isN2R, byte intensity, int resonatorParameterID){ // NOT DONE, but can holds its own ground
            // data validation
            if (pitchInterval.FrequencyRatio < 1) {return new MEDDuoNull();}
            // reroute if midi
            if (pitchInterval is MidiPitchInterval midiPitchInterval) {
                return MidiPitchIntervalToMEDDuo(midiPitchInterval, isN2R, intensity, resonatorParameterID);
            }

            //rushed solution
            return MidiPitchIntervalToMEDDuo(new MidiPitchInterval(pitchInterval.FrequencyRatio), isN2R, intensity, resonatorParameterID);
        }
        public static MEDDuo MidiPitchIntervalToMEDDuo(MidiPitchInterval pitchInterval, bool isN2R, byte intensity, int resonatorParameterID){ // DONE - return list of possible channels(channelID, grade) or List(channelID, degree)
            // data validation
            if (pitchInterval.PitchIntervalIndex < 0) {return new MEDDuoNull();}
            
            int calculateDegree(int num, int Base) {
                return (int)Math.Floor(num / (float)Base);
            }

            // working capital
            int outFlowEffectID;
            int overFlowEffectID;
            Tuple<byte, int, bool> returnTuple;
            
            if (isN2R) // byte channelID, int degree
            {
                bool isFifth = false;
                bool isForuth = false;
                bool isOctave = false;

                if (pitchInterval.PitchIntervalIndex % 12 == 0 && pitchInterval.PitchIntervalIndex > 0) {isOctave = true;}
                if (pitchInterval.PitchIntervalIndex % 7 == 0) {isFifth = true;}
                if (pitchInterval.PitchIntervalIndex % 5 == 0) {isForuth = true;}

                if (pitchInterval.PitchIntervalIndex == 0) {isFifth = false; isForuth = false;}

                if (!isFifth && !isForuth && isOctave) {returnTuple = new Tuple<byte, int, bool>(8, calculateDegree(pitchInterval.PitchIntervalIndex, 12), false);}
                else if (isFifth && !isForuth && !isOctave) {returnTuple = new Tuple<byte, int, bool>(9, calculateDegree(pitchInterval.PitchIntervalIndex, 7), false);}
                else if (!isFifth && isForuth && !isOctave) {returnTuple = new Tuple<byte, int, bool>(10, calculateDegree(pitchInterval.PitchIntervalIndex, 5), false);}
                else if (isFifth && isForuth && !isOctave) {returnTuple = new Tuple<byte, int, bool>(11, calculateDegree(pitchInterval.PitchIntervalIndex, 35), false);}
                else if (isFifth && !isForuth && isOctave) {returnTuple = new Tuple<byte, int, bool>(12, calculateDegree(pitchInterval.PitchIntervalIndex, 84), false);}
                else if (!isFifth && isForuth && isOctave) {returnTuple = new Tuple<byte, int, bool>(13, calculateDegree(pitchInterval.PitchIntervalIndex, 60), false);}
                else {return new MEDDuoNull();} 
                outFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OutflowEffect;
                overFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OverflowEffect;
                N2RMagicalEffectDataCosmosia outflow = new N2RMagicalEffectDataCosmosia(outFlowEffectID, intensity, returnTuple.Item2);
                N2RMagicalEffectDataCosmosia overflow = new N2RMagicalEffectDataCosmosia(overFlowEffectID, intensity, returnTuple.Item2);
                return new MEDDuo(outflow, overflow);
            }
            else // byte channelID, int grade, bool type
            { // can only be one of them
                int midiInterval = pitchInterval.PitchIntervalIndex % 12;
                int grade = calculateDegree(pitchInterval.PitchIntervalIndex, 12);

                if (midiInterval == 0 && grade >= 0) {returnTuple = new Tuple<byte, int, bool>(0, grade, false);}
                else if (midiInterval == 1) {returnTuple = new Tuple<byte, int, bool>(1, grade, false);}
                else if (midiInterval == 2) {returnTuple = new Tuple<byte, int, bool>(1, grade, true);}
                else if (midiInterval == 3) {returnTuple =new Tuple<byte, int, bool>(2, grade, false);}
                else if (midiInterval == 9) {returnTuple =new Tuple<byte, int, bool>(2, grade, true);}
                else if (midiInterval == 8) {returnTuple =new Tuple<byte, int, bool>(3, grade, false);}
                else if (midiInterval == 4) {returnTuple =new Tuple<byte, int, bool>(3, grade, true);}
                else if (midiInterval == 5) {returnTuple =new Tuple<byte, int, bool>(4, grade, false);}
                else if (midiInterval == 7) {returnTuple =new Tuple<byte, int, bool>(5, grade, false);}
                else if (midiInterval == 6) {returnTuple =new Tuple<byte, int, bool>(6, grade, false);}
                else if (midiInterval == 10) {returnTuple =new Tuple<byte, int, bool>(7, grade, true);}
                else if (midiInterval == 11) {returnTuple =new Tuple<byte, int, bool>(7, grade, false);}
                else {return new MEDDuoNull();}

                outFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OutflowEffect;
                overFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OverflowEffect;
                N2NMagicalEffectDataCosmosia outflow = new N2NMagicalEffectDataCosmosia(outFlowEffectID, intensity, returnTuple.Item2, returnTuple.Item3);
                N2NMagicalEffectDataCosmosia overflow = new N2NMagicalEffectDataCosmosia(overFlowEffectID, intensity, returnTuple.Item2, returnTuple.Item3);
                return new MEDDuo(outflow, overflow);
            }
 
        }

        // directly get channel ID
        public static byte PitchIntervalToChannelID(PitchInterval pitchInterval, bool isN2R) {
            // data validation
            if (pitchInterval.FrequencyRatio < 1) {return 255;}
            
            // reroute if midi
            if (pitchInterval is MidiPitchInterval midiPitchInterval) {
                return MIDIPitchIntervalToChannelID(midiPitchInterval, isN2R);
            }

            //rushed solution
            return MIDIPitchIntervalToChannelID(new MidiPitchInterval(pitchInterval.FrequencyRatio), isN2R);
        }
        public static byte MIDIPitchIntervalToChannelID(MidiPitchInterval pitchInterval, bool isN2R) {
            //BasilMuguet.Log("pitchInterval.PitchIntervalIndex is " + pitchInterval.PitchIntervalIndex);
            // data validation
            if (pitchInterval.PitchIntervalIndex < 0) {return 255;}
            int calculateDegree(int num, int Base) {
                return (int)Math.Floor(num / (float)Base);
            }
            if (isN2R) // byte channelID
            {
                bool isFifth = false;
                bool isForuth = false;
                bool isOctave = false;
                
                if (pitchInterval.PitchIntervalIndex == 0) {return 8;}
                if (pitchInterval.PitchIntervalIndex % 12 == 0 && pitchInterval.PitchIntervalIndex > 0) {isOctave = true;}
                if (pitchInterval.PitchIntervalIndex % 7 == 0) {isFifth = true;}
                if (pitchInterval.PitchIntervalIndex % 5 == 0) {isForuth = true;}

                if (!isFifth && !isForuth && isOctave) {return 8;}
                else if (isFifth && !isForuth && !isOctave) {return 9;}
                else if (!isFifth && isForuth && !isOctave) {return 10;}
                else if (isFifth && isForuth && !isOctave) {return 11;}
                else if (isFifth && !isForuth && isOctave) {return 12;}
                else if (!isFifth && isForuth && isOctave) {return 13;}

                else {return 255;}
            }
            else // byte channelID
            { // can only be one of them
                int midiInterval = pitchInterval.PitchIntervalIndex % 12;
                int grade = calculateDegree(pitchInterval.PitchIntervalIndex, 12);

                if (midiInterval == 0 && grade >= 0) {return 0;}
                else if (midiInterval == 1) {return 1;}
                else if (midiInterval == 2) {return 1;}
                else if (midiInterval == 3) {return 2;}
                else if (midiInterval == 9) {return 2;}
                else if (midiInterval == 8) {return 3;}
                else if (midiInterval == 4) {return 3;}
                else if (midiInterval == 5) {return 4;}
                else if (midiInterval == 7) {return 5;}
                else if (midiInterval == 6) {return 6;}
                else if (midiInterval == 10) {return 7;}
                else if (midiInterval == 11) {return 7;}

                else {return 255;}
            }
 
        }
       
        // IntervalIsStrucal
        public static bool PitchIntervalIsStructual(PitchInterval pitchInterval) {
            // data validation
            if (pitchInterval.FrequencyRatio < 1) {return false;}
            // reroute if midi
            if (pitchInterval is MidiPitchInterval midiPitchInterval) {
                return MIDIPitchIntervalIsStructual(midiPitchInterval);
            }

            //rushed solution
            return MIDIPitchIntervalIsStructual(new MidiPitchInterval(pitchInterval.FrequencyRatio));
        }
        public static bool MIDIPitchIntervalIsStructual(MidiPitchInterval pitchInterval) {
            // data validation
            if (pitchInterval.PitchIntervalIndex < 0) {return false;}

            int midiIndex = pitchInterval.PitchIntervalIndex;
            if (midiIndex >= 24) {return true;}
            for (int i = 0 ; i < IsStructualMidiIntervalIndexes.Count ; i++) {
                if (midiIndex == IsStructualMidiIntervalIndexes[i]) {return true;}
            }
            return false;
        }
    

    
    }
}