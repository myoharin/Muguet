using System;
using SineVita.Basil.Muguet;
using SineVita.Muguet.Asteraceae;
using System.Text.Json;

namespace SineVita.Muguet {
    public enum MidiPitchName {
        C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B
    }
    public enum PitchType {
        JustIntonation,
        CustomeToneEuqal,
        TwelveToneEqual,
        Float
    }

    public abstract class Pitch {
        // * Properties
        public PitchType Type { get; set; }
        public int CentOffsets { get; set; }

        // * Derived Gets
        public string NoteName { get { return HarmonyHelper.HtzToNoteName(Frequency); } }
        public double Frequency { get { return GetFrequency(); } }

        // * statics
        public static Pitch Empty { get { return new FloatPitch(256f); } }
        private static readonly string[] noteNames = new string[] {
            "C", "C#/Db", "D", "D#/Eb", "E", "F", "F#/Gb", "G", "G#/Ab", "A", "A#/Bb", "B"
        };
        public static string[] NoteNames { get { return noteNames; } }

        // * FromJson
        public static Pitch FromJson(string jsonString) {
            var jsonDocument = JsonDocument.Parse(jsonString);
            var rootElement = jsonDocument.RootElement;
            PitchType type = Enum.Parse<PitchType>(rootElement.GetProperty("Type").GetString() ?? "Float");
            switch (type) {
                case PitchType.Float:
                    return new FloatPitch(rootElement.GetProperty("Frequency").GetDouble());
                case PitchType.JustIntonation:
                    var justFrequency = rootElement.GetProperty("JustFrequency");
                    return new JustIntonalPitch(
                        (justFrequency.GetProperty("Numerator").GetInt32(), justFrequency.GetProperty("Denominator").GetInt32()),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchType.CustomeToneEuqal:
                    return new CustomTetPitch(
                        rootElement.GetProperty("Base").GetInt32(),
                        rootElement.GetProperty("TuningIndex").GetInt32(),
                        rootElement.GetProperty("TuningFrequency").GetSingle(),
                        rootElement.GetProperty("PitchIndex").GetInt32(),
                        type,
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchType.TwelveToneEqual:
                    return new MidiPitch(
                        rootElement.GetProperty("PitchIndex").GetInt32(),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                default:
                    throw new ArgumentException("Invalid pitch type");
            }
        }

        // * Constructor
        protected Pitch(PitchType pitchType = PitchType.Float, int centOffsets = 0) {
            Type = pitchType;
            CentOffsets = centOffsets;
        }

        // * virtual methods
        public virtual double GetFrequency() { return 0; }
        public virtual string ToJson() {return "";}

        // * General Methods
        public FloatPitch IncrementPitch(PitchInterval pitchInterval) {
            return new FloatPitch((float)(Frequency * pitchInterval.FrequencyRatio));
        }
        public PitchInterval CreateInterval(Pitch pitch2, bool absoluteInterval = false, PitchType targetType = PitchType.Float) {
            return PitchInterval.CreateInterval(this, pitch2, absoluteInterval, targetType);
        }
    }

    public class FloatPitch : Pitch {
        // * Properties
        private double _frequency;

        // * Constructor
        public FloatPitch(double frequency) : base(PitchType.Float, 0) {
            _frequency = frequency;
        }

        // * Overrides
        public new int CentOffsets {
            get { return 0; }
            set { _frequency *= Math.Pow(2, 1+value/1200);}
        }
        public override double GetFrequency() {return _frequency;}
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"Frequency\": {_frequency},",
                $"\"Type\": {Type.ToString()},",
                $"\"CentOffsets\":{CentOffsets}",
                "}"
            );
        }
    }

    public class JustIntonalPitch : Pitch {
        // * Properties
        public (int Numerator, int Denominator) JustFrequency { get; set; }

        // * Constructor
        public JustIntonalPitch((int, int) justFrequency, int centOffsets = 0)
            : base(PitchType.JustIntonation, centOffsets) {
            JustFrequency = justFrequency;
        }
        
        // * Overrides
        public override double GetFrequency() {
            return (float)Math.Pow(2, CentOffsets / 1200.0) * JustFrequency.Numerator / JustFrequency.Denominator;
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"JustFrequency\": {{ \"Numerator\": {JustFrequency.Numerator}, \"Denominator\": {JustFrequency.Denominator} }},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
    
    }

    public class CustomTetPitch : Pitch {
        public int Base { get; set; }
        public int TuningIndex { get; set; }
        public float TuningFrequency { get; set; }
        public int PitchIndex { get; set; }

        public CustomTetPitch(int baseValue, int tuningIndex, float tuningFrequency, int pitchIndex, PitchType pitchType = PitchType.CustomeToneEuqal, int centOffsets = 0)
            : base(pitchType, centOffsets) {
            Base = baseValue;
            TuningIndex = tuningIndex;
            TuningFrequency = tuningFrequency;
            PitchIndex = pitchIndex;
        }
        public CustomTetPitch(float frequency, int baseValue, int tuningIndex, float tuningFrequency, PitchType pitchType = PitchType.CustomeToneEuqal)
            : base(pitchType, 0) {
            Base = baseValue;
            TuningIndex = tuningIndex;
            TuningFrequency = tuningFrequency;
            
            double cacheIndex = baseValue * Math.Log2(frequency / tuningFrequency) + tuningIndex;
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / baseValue * 1200.0);
        }
        // * Overrides
        public override double GetFrequency() {
            return (float)Math.Pow(2, CentOffsets / 1200.0) * TuningFrequency * (float)Math.Pow(2, (PitchIndex - TuningIndex) / (double)Base);
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"Base\": {Base},",
                $"\"TuningIndex\": {TuningIndex},",
                $"\"TuningFrequency\": {TuningFrequency},",
                $"\"PitchIndex\": {PitchIndex},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }

        // * TET increment system
        public void Up(int upBy = 1) {
            PitchIndex += upBy;
        }
        public void Down(int downBy = 1) {
            PitchIndex -= downBy;
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

    public class MidiPitch : CustomTetPitch {
        public MidiPitch(int midiValue, int centOffsets = 0)
            : base(12, 69, 440, midiValue, PitchType.TwelveToneEqual, centOffsets) {}
        public MidiPitch(float frequency)
            : base(frequency, 12, 69, 440, PitchType.TwelveToneEqual)
         {
            double cacheIndex = Base * Math.Log2(frequency / TuningFrequency) + TuningIndex;
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / Base * 1200.0);
        }
        public static float ToPitchIndex(double frequency, bool round = true) {
            return ToPitchIndex(frequency, 12, 69, 440, round);
        }
    
        // * Overrides
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"PitchIndex\": {PitchIndex},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
    }

}
