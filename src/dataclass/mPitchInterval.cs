using System;

namespace SineVita.Muguet {

    public class PitchInterval // CAN BE NEGATIVE
    {
        public double FrequencyRatio { get; set; }
        public PitchType Type { get; set; }
        public bool IsNegative { get {return FrequencyRatio < 1;} }

        // * Statics
        public static PitchInterval Empty { get {return new PitchInterval((double)1.0);} }
        public static string[] IntervalNames = new string[] {
            // Populate with actual interval names
            "R", "m2", "M2", "m3", "M3", "P4", "T1", "P5", "m6", "M6", "m7", "M7",
            "O1", "m9", "M9", "m10", "M10", "P11", "T2", "P12", "m13", "M13", "m14", "M14",
            "O2", "m16", "M16", "m17", "M17", "P18", "T3", "P19", "m20", "M20", "m21", "M21",
            "O3", "m23", "M23", "m24", "M24", "P25", "T4", "P26", "m27", "M27", "m28", "M28",
            "O4"
        };
        public static PitchInterval CreateTargetInterval(Pitch pitch1, Pitch pitch2, PitchType? targetType = null, bool absoluteInterval = false) {
            return null;
        }
        // ! NOT DONE ^^

        // * Constructor
        protected PitchInterval(double? frequencyRatio = null, PitchType type = PitchType.Float) {
            if (frequencyRatio == null && type == PitchType.Float) {
                FrequencyRatio = 1;
            }
            else {FrequencyRatio = frequencyRatio ?? UpdateFrequencyRatio();}
            Type = type;
        }
        public PitchInterval(Pitch pitch1, Pitch pitch2, bool absoluteInterval = false) { // pitch 1 is always lower than pitch 2
            // Set up higher and lower pitches
            Pitch higherPitch, lowerPitch;
            if (absoluteInterval) {
                if (pitch1.Frequency > pitch2.Frequency) {
                    higherPitch = pitch1;
                    lowerPitch = pitch2;
                }
                else {
                    higherPitch = pitch2;
                    lowerPitch = pitch1;
                }
            } 
            else {
                lowerPitch = pitch1;
                higherPitch = pitch2;
            }
            
            // working capital
            FrequencyRatio = higherPitch.Frequency / lowerPitch.Frequency;            
        }

        // * virtual methods
        public virtual double UpdateFrequencyRatio(double? newFrequencyRatio = null) {
            if (newFrequencyRatio.HasValue) {FrequencyRatio = newFrequencyRatio.Value;}
            return FrequencyRatio;
        }
    }

    public abstract class PitchIntervalBase : PitchInterval
    {
        public int CentOffsets { get; set; }
        public bool IsSynched;
         
        public PitchIntervalBase(double? frequencyRatio = null, PitchType type = PitchType.Float, int centOffsets = 0) 
            : base (frequencyRatio, type) {
            CentOffsets = centOffsets;
            IsSynched = false;
        }

        public override double UpdateFrequencyRatio(double? newFrequencyRatio = null) {
            if (newFrequencyRatio.HasValue) {FrequencyRatio = newFrequencyRatio.Value;}
            IsSynched = true;
            return FrequencyRatio;
        }
    }

    public class JustIntonalPitchInterval : PitchIntervalBase 
    {
        public (int Numerator, int Denominator) JustRatio { get; set; }

        public JustIntonalPitchInterval((int, int) justRatio, double? frequencyRatio = null, int centOffsets = 0)
            : base(frequencyRatio, PitchType.JustIntonation, centOffsets) {
            JustRatio = justRatio;
        }

        public override double UpdateFrequencyRatio(double? newFrequencyRatio = null)  {
            if (newFrequencyRatio.HasValue) {FrequencyRatio = newFrequencyRatio.Value; IsSynched = false;}
            else {FrequencyRatio = Math.Pow(2, CentOffsets / 1200.0) * JustRatio.Numerator / JustRatio.Denominator; IsSynched = true;}
            return FrequencyRatio;
        }
    }

    public class CustomTETPitchInterval : PitchIntervalBase
    {
        public int Base { get; set; }
        public int PitchIntervalIndex { get; set; }

        public CustomTETPitchInterval(int baseValue, int pitchIntervalIndex, double? frequencyRatio = null, PitchType type = PitchType.CustomeToneEuqal, int centOffsets = 0)
            : base(frequencyRatio, type, centOffsets) {
            Base = baseValue;
            PitchIntervalIndex = pitchIntervalIndex;
        }
        public CustomTETPitchInterval(int baseValue, double frequencyRatio) : this(baseValue, 0) {
            double cacheIndex = baseValue * Math.Log2(frequencyRatio);
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIntervalIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIntervalIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / baseValue * 1200.0);
            IsSynched = true;
        }


        public override double UpdateFrequencyRatio(double? newFrequencyRatio = null) {
            if (newFrequencyRatio.HasValue) {FrequencyRatio = newFrequencyRatio.Value; IsSynched = false;}
            else {FrequencyRatio = Math.Pow(2, CentOffsets / 1200.0) * Math.Pow(2, PitchIntervalIndex / (double)Base); IsSynched = true;}
            return FrequencyRatio;        
        }

        // TET increment system
        public void Up(int upBy = 1, bool updateFrequencyRatio = true) {
            PitchIntervalIndex += upBy;
            if (updateFrequencyRatio) {FrequencyRatio *= (float)Math.Pow(2, upBy/Base);}
        }
        public void Down(int downBy = 1, bool updateFrequencyRatio = true) {
            PitchIntervalIndex -= downBy;
            if (updateFrequencyRatio) {FrequencyRatio /= (float)Math.Pow(2, downBy/Base);}
        }

        // * To Pitch Index
        public static float ToPitchIndex(int baseValue, double frequencyRatio, bool round = true) {
            if (round) {return (float)Math.Round(baseValue * Math.Log2(frequencyRatio));}
            else {return (float)(baseValue * Math.Log2(frequencyRatio));}
        }
        public float ToPitchIndex(double frequencyRatio, bool round = true) {
            return ToPitchIndex(Base, frequencyRatio, round);
        }

    }

    public class MidiPitchInterval : CustomTETPitchInterval 
    {
        public MidiPitchInterval(int midiValue, double? frequencyRatio = null, int centOffsets = 0)
            : base(12, midiValue, frequencyRatio, PitchType.TwelveToneEqual, centOffsets) {}
        public MidiPitchInterval(double frequencyRatio) : base (12, frequencyRatio) {}
        public new static float ToPitchIndex(double frequencyRatio, bool round = true) {
            if (round) {return (float)Math.Round(12 * Math.Log2(frequencyRatio));}
            else {return (float)(12 * Math.Log2(frequencyRatio));}
        }
    }

}