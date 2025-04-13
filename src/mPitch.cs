using System;
using System.Text.Json;

namespace SineVita.Muguet {
    
    public enum PitchType {
        CustomeToneEuqal,
        TwelveToneEqual,
        Float,
        Compound
    }

    public abstract class Pitch : IComparable, ICloneable {
        // * Properties
        public PitchType Type { get; set; }
        public int CentOffsets { get; set; }

        // * Derived Gets
        public string NoteName { get { return HarmonyHelper.HtzToNoteName(Frequency); } }
        public double Frequency { get { return GetFrequency(); } }

        // * statics
        public static FloatPitch New(double frequency) {return new FloatPitch(frequency);}
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
                case PitchType.CustomeToneEuqal:
                    return new CustomTetPitch(
                        rootElement.GetProperty("Base").GetInt32(),
                        rootElement.GetProperty("TuningIndex").GetInt32(),
                        rootElement.GetProperty("TuningFrequency").GetSingle(),
                        rootElement.GetProperty("PitchIndex").GetInt32(),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchType.TwelveToneEqual:
                    return new MidiPitch(
                        rootElement.GetProperty("PitchIndex").GetInt32(),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchType.Compound:
                    var pitch = FromJson(rootElement.GetProperty("Pitch").GetString() ?? Pitch.Empty.ToJson());
                    var centOffsets = rootElement.GetProperty("CentOffsets").GetInt32();
                    string? intervalStr = rootElement.GetProperty("Interval").GetString();
                    CompoundPitchInterval? interval = intervalStr != null ? (CompoundPitchInterval)(PitchInterval.FromJson(intervalStr)) : null;
                    return (interval == null) ? new CompoundPitch(pitch, centOffsets:centOffsets) : new CompoundPitch(pitch, interval, centOffsets);
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
        public FloatPitch Incremented(PitchInterval pitchInterval) {
            return new FloatPitch((float)(Frequency * pitchInterval.FrequencyRatio));
        }
        public FloatPitch Decremented(PitchInterval pitchInterval) {
            return new FloatPitch((float)(Frequency / pitchInterval.FrequencyRatio));
        }
        public PitchInterval CreateInterval(Pitch pitch2, bool absoluteInterval = false, PitchIntervalType targetType = PitchIntervalType.Float) {
            return PitchInterval.CreateInterval(this, pitch2, absoluteInterval, targetType);
        }

        public virtual void Increment(PitchInterval interval) {
            
        } // ! NOT DONE FIX MIXED SIGNATURE
        public virtual void Decrement(PitchInterval interval) {
            
        } // ! NOT DONE FIX MIXED SIGNATURE

        // * Interfaces
        public int CompareTo(object? obj) {
            if (obj == null) return 1; // Null is considered less than any object
            if (obj is Pitch otherPitch) {
                return Frequency.CompareTo(otherPitch.Frequency); // Compare by Frequency
            }
            throw new ArgumentException("Object is not a Pitch");
        }
        public abstract object Clone();
     
        public override bool Equals(object? obj) {
            if (obj == null || GetType() != obj.GetType()) return false;
            Pitch other = (Pitch)obj;
            return Math.Abs(Frequency - other.Frequency) < 0.0001;
        }
        public override int GetHashCode() {
            return Frequency.GetHashCode();
        }
        public static bool operator ==(Pitch left, Pitch right) {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        public static bool operator !=(Pitch left, Pitch right) {
            return !(left == right);
        }
        public static bool operator <(Pitch left, Pitch right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(Pitch left, Pitch right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(Pitch left, Pitch right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(Pitch left, Pitch right) {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
        
            // arithmetic operations
        public static Pitch operator +(Pitch pitch, PitchInterval pitchInterval) {
            pitch.Increment(pitchInterval);
            return pitch;
        }
        public static Pitch operator +(PitchInterval pitchInterval, Pitch pitch) {
            pitch.Increment(pitchInterval);
            return pitch;
        }

        public static Pitch operator -(Pitch pitch, PitchInterval pitchInterval) {
            pitch.Decrement(pitchInterval);
            return pitch;
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
        public override object Clone() {
            return new FloatPitch(_frequency);
        }
    }

    public class CompoundPitch : Pitch {
        // * Properties
        public CompoundPitchInterval Interval { get; set; }
        public Pitch Pitch { get; set; }

        // * Constructor
        public CompoundPitch(Pitch pitch, List<PitchInterval>? intervals = null, int centOffsets = 0)
            : base(PitchType.Compound, centOffsets) {
            Pitch = pitch;

            Interval = intervals != null ? new CompoundPitchInterval(intervals) : new();
        }
        public CompoundPitch(Pitch pitch, PitchInterval interval, int centOffsets = 0)
            : base(PitchType.Compound, centOffsets) {
            Pitch = pitch;
            Interval = new CompoundPitchInterval(interval);
        }
        
        // * Overrides
        public override double GetFrequency() {
            double origin = Math.Pow(2, CentOffsets / 1200.0) * Pitch.GetFrequency();
            origin *= Interval.GetFrequencyRatio();
            return origin;
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"Pitch\": {Pitch.ToJson()},",
                $"\"Interval\": {Interval.ToJson()},",
                
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new CompoundPitch(Pitch, Interval, CentOffsets);
        }


        public override void Increment(PitchInterval interval) { // ! NOT DONE

        }
        public override void Decrement(PitchInterval interval) { // ! NOT DONE
             
        }
    
    }
    
    public class CustomTetPitch : Pitch { // ! Implement global memory to store same copy of the same CustomTET
        public int Base { get; set; }
        public int TuningIndex { get; set; }
        public float TuningFrequency { get; set; }
        public int PitchIndex { get; set; }

        public CustomTetPitch(int baseValue, int tuningIndex, float tuningFrequency, int pitchIndex, int centOffsets = 0)
            : base(PitchType.CustomeToneEuqal, centOffsets) {
            Base = baseValue;
            TuningIndex = tuningIndex;
            TuningFrequency = tuningFrequency;
            PitchIndex = pitchIndex;
        }
        public CustomTetPitch(float frequency, int baseValue, int tuningIndex, float tuningFrequency)
            : base(PitchType.CustomeToneEuqal, 0) {
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
        public override object Clone() {
            return new CustomTetPitch(Base, TuningIndex, TuningFrequency, PitchIndex);
        }

        // * TET increment system
        public void Up(int upBy = 1) {
            PitchIndex += upBy;
        }
        public void Down(int downBy = 1) {
            PitchIndex -= downBy;
        }
        
        public static CustomTetPitch operator ++(CustomTetPitch pitch) {
            pitch.Up();
            return pitch;
        }
        public static CustomTetPitch operator --(CustomTetPitch pitch) {
            pitch.Down();
            return pitch;
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

    public class MidiPitch : Pitch {
        // * Constants
        public const int Base = 12;
        public const double TuningFrequency = 440;
        public const int TuningIndex = 69;

        // * Variable
        public int PitchIndex { get; set; }

        public MidiPitch(int midiValue, int centOffsets = 0)
            : base(PitchType.TwelveToneEqual, centOffsets) {PitchIndex = midiValue;}
        public MidiPitch(float frequency)
            : base(PitchType.TwelveToneEqual) {
            double cacheIndex = Base * Math.Log2(frequency / TuningFrequency) + TuningIndex;
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / Base * 1200.0);
        }

        // * To Pitch Index
        public float ToIndex(double? frequency = null, bool round = true) {
            return ToIndex(frequency??Frequency, round);
        }
        public static float ToIndex(double frequency, bool round = true) {
            if (round) {return (float)Math.Round(Base * Math.Log2(frequency / TuningFrequency) + TuningIndex);}
            else {return (float)(Base * Math.Log2(frequency / TuningFrequency) + TuningIndex);}
        }

        // * TET increment system
        public void Up(int upBy = 1) {
            PitchIndex += upBy;
        }
        public void Down(int downBy = 1) {
            PitchIndex -= downBy;
        }
   
        public static MidiPitch operator ++(MidiPitch pitch) {
            pitch.Up();
            return pitch;
        }
        public static MidiPitch operator --(MidiPitch pitch) {
            pitch.Down();
            return pitch;
        }


        // * Overrides
        public override double GetFrequency() {
            return (float)Math.Pow(2, CentOffsets / 1200.0) * TuningFrequency * (float)Math.Pow(2, (PitchIndex - TuningIndex) / (double)Base);
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"PitchIndex\": {PitchIndex},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new MidiPitch(PitchIndex);
        }
    }

}
