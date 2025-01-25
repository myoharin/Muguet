using System;
using SineVita.Basil.Muguet;
using SineVita.Muguet.Asteraceae;

namespace SineVita.Muguet {
    public enum PitchType {
        JustIntonation,
        CustomeToneEuqal,
        TwelveToneEqual,
        Float
    }
    public class Pitch {
        public float Frequency { get; set; }
        public PitchType Type { get; set; }

        // * Derived Gets
        public string NoteName { get {
            return HarmonyHelper.HtzToNoteName(Frequency);
        } }

        // * statics
        public static Pitch Empty { get {return new Pitch((float)256.0f);} }
        public static string[] NoteNames = new string[] {
            "C", "C#/Db", "D", "D#/Eb", "E", "F", "F#/Gb", "G", "G#/Ab", "A", "A#/Bb", "B"
        };

        // * Constructor
        public Pitch(float frequency) {
            Frequency = frequency;
            Type = PitchType.Float;
        }
        protected Pitch(float? frequency = null, PitchType pitchType = PitchType.Float) {
            if (frequency == null && pitchType == PitchType.Float) {
                Frequency = 256;
            }
            else {Frequency = frequency ?? (float)UpdateFrequency();}
            
            Type = pitchType;
        }

        // * virtual methods
        public virtual double UpdateFrequency(float? newFrequency = null) {
            if (newFrequency.HasValue) {Frequency = newFrequency.Value;}
            return Frequency;
        }
        public Pitch ReturnIncrementedPitch(PitchInterval pitchInterval) {
            return new Pitch((float)(this.Frequency * pitchInterval.FrequencyRatio), PitchType.Float);
        }
        public PitchInterval CreateInterval(Pitch pitch2, bool absoluteInterval = false) {
            return new PitchInterval(this, pitch2, absoluteInterval);
        }
        // NOT DONE
        // - add virtual metohds for each pitch intervals
    }

    public abstract class PitchBase : Pitch {
        public int CentOffsets { get; set; }
        public bool IsSynched { get; set; }
        public PitchBase(float? frequency, PitchType type, int centOffsets = 0) : base(frequency, type) {
            IsSynched = false;
            CentOffsets = centOffsets;
        }
    }

    public class JustIntonalPitch : PitchBase {
        public (int Numerator, int Denominator) JustFrequency { get; set; }

        public JustIntonalPitch((int, int) justFrequency, float? frequency = null, int centOffsets = 0)
            : base(frequency, PitchType.JustIntonation, centOffsets) {
            JustFrequency = justFrequency;
        }

        public override double UpdateFrequency(float? newFrequency = null) {
            if (newFrequency.HasValue) {
                Frequency = newFrequency.Value;
                IsSynched = false;
            }
            else {
                Frequency = (float)Math.Pow(2, CentOffsets / 1200.0) * JustFrequency.Numerator / JustFrequency.Denominator;
                IsSynched = true;
            }
            return Frequency;
        }
    }

    public class CustomTETPitch : PitchBase {
        public int Base { get; set; }
        public int TuningIndex { get; set; }
        public float TuningFrequency { get; set; }
        public int PitchIndex { get; set; }

        public CustomTETPitch(int baseValue, int tuningIndex, float tuningFrequency, int pitchIndex, float? frequency = null, PitchType pitchType = PitchType.CustomeToneEuqal, int centOffsets = 0)
            : base(frequency, pitchType, centOffsets) {
            Base = baseValue;
            TuningIndex = tuningIndex;
            TuningFrequency = tuningFrequency;
            PitchIndex = pitchIndex;
            if (frequency.HasValue) {UpdateFrequency(frequency);}
            else {UpdateFrequency();}
        }
        public CustomTETPitch(float frequency, int baseValue, int tuningIndex, float tuningFrequency, PitchType pitchType = PitchType.CustomeToneEuqal)
            : base(frequency, pitchType, 0) {
            Base = baseValue;
            TuningIndex = tuningIndex;
            TuningFrequency = tuningFrequency;
            
            double cacheIndex = baseValue * Math.Log2(frequency / tuningFrequency) + tuningIndex;
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / baseValue * 1200.0);
            IsSynched = true;
        }

        public override double UpdateFrequency(float? newFrequency = null) {
            if (newFrequency.HasValue) {Frequency = newFrequency.Value; IsSynched = false;}   
            else {Frequency = (float)Math.Pow(2, CentOffsets / 1200.0) * TuningFrequency * (float)Math.Pow(2, (PitchIndex - TuningIndex) / (double)Base); IsSynched = true;}
            return Frequency;
        }
            
        // TET increment system
        public void Up(int upBy = 1) {
            PitchIndex += upBy;
            Frequency *= (float)Math.Pow(2, upBy/Base);
        }
        public void Down(int downBy = 1) {
            PitchIndex -= downBy;
            Frequency /= (float)Math.Pow(2, downBy/Base);
        }
    
        // * To Pitch Index
        public static float ToPitchIndex(double frequency, int baseValue, int tuningIndex, float tuningFrequency, bool round = true) {
            if (round) {return (float)Math.Round(baseValue * Math.Log2(frequency / tuningFrequency) + tuningIndex);}
            else {return (float)(baseValue * Math.Log2(frequency / tuningFrequency) + tuningIndex);}
        }
        public float ToPitchIndex(double? frequency = null, bool round = true) {
            return ToPitchIndex(frequency??Frequency, Base, TuningIndex, TuningFrequency, round);
        }
    }

    public class MidiPitch : CustomTETPitch {
        public MidiPitch(int midiValue, float? frequency = null, int centOffsets = 0)
            : base(12, 69, 440, midiValue, frequency, PitchType.TwelveToneEqual, centOffsets) {}
        public MidiPitch(float frequency)
            : base(12, 69, 440, 1, frequency, PitchType.TwelveToneEqual, 0)
         {
            double cacheIndex = Base * Math.Log2(frequency / TuningFrequency) + TuningIndex;
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / Base * 1200.0);
            IsSynched = true;
        }
        public static float ToPitchIndex(double frequency, bool round = true) {
            return ToPitchIndex(frequency, 12, 69, 440, round);
        }
    }

}
