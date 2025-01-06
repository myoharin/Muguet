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
        // * Helper Constants
        private static Dictionary<int, ResonatorParameterCosmosia> ResonatorParamaters { get; set; } = new Dictionary<int, ResonatorParameterCosmosia>();
        private const int DefaultTimeOutDeletionDuration = 32768; // ms
        private static List<int> _intervalIsStructual = new List<int>(){
            0, 5, 7, 10, 12, 14, 15, 17, 19, 20, 21, 22
        };
        public static string ResonatorParametersFolderPath = Path.Combine("assets", "cosmosia", "resonator_parameters");

        // * Manual Parameter Cache Management
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

        // * Auto Parameter Deletion
        public static void IncrementTimerInGameTime(int currentRunTime, double deltaTime, int TimeOutDeletionDuration = DefaultTimeOutDeletionDuration) {
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

        // * Safe Parameter Access
        public static ResonatorParameterCosmosia GetResonatorParameter(int ResonatorParamaterID) {
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
    

        // * Mana Calculation functions
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
        public static float CalculateIdyllflowEqualizer(int numOfPulses) {
            if (numOfPulses >= 1) {return 1;} // so no math error
            return (float)(numOfPulses / HarmonyHelper.CalculateWINAIndex(0, numOfPulses+1));
        }
        

        // * From Interval Methods
        public static MEDDuo IntervalToMEDDuo(PitchInterval interval, bool isN2R, byte intensity, int resonatorParameterID){ // DONE - return list of possible channels(channelID, grade) or List(channelID, degree)
            
            int midiIndex = (int)MidiPitchInterval.ToPitchIndex(interval);
            
            // data validation
            if (midiIndex < 0) {return new MEDDuo();}
            
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

                if (midiIndex % 12 == 0 && midiIndex > 0) {isOctave = true;}
                if (midiIndex % 7 == 0) {isFifth = true;}
                if (midiIndex % 5 == 0) {isForuth = true;}

                if (midiIndex == 0) {isFifth = false; isForuth = false;}

                if (!isFifth && !isForuth && isOctave) {returnTuple = new Tuple<byte, int, bool>(8, calculateDegree(midiIndex, 12), false);}
                else if (isFifth && !isForuth && !isOctave) {returnTuple = new Tuple<byte, int, bool>(9, calculateDegree(midiIndex, 7), false);}
                else if (!isFifth && isForuth && !isOctave) {returnTuple = new Tuple<byte, int, bool>(10, calculateDegree(midiIndex, 5), false);}
                else if (isFifth && isForuth && !isOctave) {returnTuple = new Tuple<byte, int, bool>(11, calculateDegree(midiIndex, 35), false);}
                else if (isFifth && !isForuth && isOctave) {returnTuple = new Tuple<byte, int, bool>(12, calculateDegree(midiIndex, 84), false);}
                else if (!isFifth && isForuth && isOctave) {returnTuple = new Tuple<byte, int, bool>(13, calculateDegree(midiIndex, 60), false);}
                else {return new MEDDuo();} 
                outFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OutflowEffect;
                overFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OverflowEffect;
                
                var arguments = new Dictionary<AsterArgumentType, float>() {{AsterArgumentType.COS_N2R_DEGREE, returnTuple.Item2}};
    
                MagicalEffectData outflow = new MagicalEffectData(outFlowEffectID, intensity, arguments);
                MagicalEffectData overflow = new MagicalEffectData(overFlowEffectID, intensity, arguments);

                return new MEDDuo(outflow, overflow);
            }
            else // byte channelID, int grade, bool type
            { // can only be one of them
                int grade = calculateDegree(midiIndex, 12);
                midiIndex = midiIndex % 12;
                

                if (midiIndex == 0 && grade >= 0) {returnTuple = new Tuple<byte, int, bool>(0, grade, false);}
                else if (midiIndex == 1) {returnTuple = new Tuple<byte, int, bool>(1, grade, false);}
                else if (midiIndex == 2) {returnTuple = new Tuple<byte, int, bool>(1, grade, true);}
                else if (midiIndex == 3) {returnTuple =new Tuple<byte, int, bool>(2, grade, false);}
                else if (midiIndex == 9) {returnTuple =new Tuple<byte, int, bool>(2, grade, true);}
                else if (midiIndex == 8) {returnTuple =new Tuple<byte, int, bool>(3, grade, false);}
                else if (midiIndex == 4) {returnTuple =new Tuple<byte, int, bool>(3, grade, true);}
                else if (midiIndex == 5) {returnTuple =new Tuple<byte, int, bool>(4, grade, false);}
                else if (midiIndex == 7) {returnTuple =new Tuple<byte, int, bool>(5, grade, false);}
                else if (midiIndex == 6) {returnTuple =new Tuple<byte, int, bool>(6, grade, false);}
                else if (midiIndex == 10) {returnTuple =new Tuple<byte, int, bool>(7, grade, true);}
                else if (midiIndex == 11) {returnTuple =new Tuple<byte, int, bool>(7, grade, false);}
                else {return new MEDDuo();}

                outFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OutflowEffect;
                overFlowEffectID = GetResonatorParameter(resonatorParameterID).GetChannelParameter(returnTuple.Item1).OverflowEffect;
                
                var arguments = new Dictionary<AsterArgumentType, float>() {
                    {AsterArgumentType.COS_N2N_GRADE, returnTuple.Item2},
                    {AsterArgumentType.COS_N2N_TYPE, returnTuple.Item3 ? 1 : 0}
                };
    
                MagicalEffectData outflow = new MagicalEffectData(outFlowEffectID, intensity, arguments);
                MagicalEffectData overflow = new MagicalEffectData(overFlowEffectID, intensity, arguments);
                return new MEDDuo(outflow, overflow);
            }
 
        }
        public static byte IntervalToChannelID(PitchInterval interval, bool isN2R) {
            //BasilMuguet.Log("pitchInterval.PitchIntervalIndex is " + pitchInterval.PitchIntervalIndex);
            int midiIndex = (int)MidiPitchInterval.ToPitchIndex(interval);
            // data validation
            if (midiIndex < 0) {return 255;}
            int calculateDegree(int num, int Base) {
                return (int)Math.Floor(num / (float)Base);
            }
            if (isN2R) // byte channelID
            {
                bool isFifth = false;
                bool isForuth = false;
                bool isOctave = false;
                
                if (midiIndex == 0) {return 8;}
                if (midiIndex % 12 == 0 && midiIndex > 0) {isOctave = true;}
                if (midiIndex % 7 == 0) {isFifth = true;}
                if (midiIndex % 5 == 0) {isForuth = true;}

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
                midiIndex = midiIndex % 12;
                int grade = calculateDegree(midiIndex, 12);

                if (midiIndex == 0 && grade >= 0) {return 0;}
                else if (midiIndex == 1) {return 1;}
                else if (midiIndex == 2) {return 1;}
                else if (midiIndex == 3) {return 2;}
                else if (midiIndex == 9) {return 2;}
                else if (midiIndex == 8) {return 3;}
                else if (midiIndex == 4) {return 3;}
                else if (midiIndex == 5) {return 4;}
                else if (midiIndex == 7) {return 5;}
                else if (midiIndex == 6) {return 6;}
                else if (midiIndex == 10) {return 7;}
                else if (midiIndex == 11) {return 7;}

                else {return 255;}
            }
 
        }
        public static bool IntervalIsStructual(PitchInterval interval) {
            // convert
            int midiIndex = (int)MidiPitchInterval.ToPitchIndex(interval);
            if (midiIndex >= 24) {return true;}
            return _intervalIsStructual.Contains(midiIndex);
        }
    

    
    }
}