using System;
using System.Security.Permissions;
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
        public virtual int ToMidiIndex { get {
            return (int)MidiPitch.ToIndex(Frequency);
        } }

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

        // * Incrementation - base type preserved
        public Pitch Incremented(PitchInterval interval) {
            var newPitch = (Pitch)this.Clone();
            newPitch.Increment(interval);
            return newPitch;
        }
        public Pitch Decremented(PitchInterval interval) {
            var newPitch = (Pitch)this.Clone();
            newPitch.Decrement(interval);
            return newPitch;
        }
        public PitchInterval CreateInterval(Pitch pitch2, bool absoluteInterval = false, PitchIntervalType targetType = PitchIntervalType.Float) {
            return PitchInterval.CreateInterval(this, pitch2, absoluteInterval, targetType);
        }

        public abstract void Increment(PitchInterval interval);
        public abstract void Decrement(PitchInterval interval);

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
        
        public static bool operator ==(double left, Pitch right) {
            return Math.Abs(left - right.Frequency) < 0.001;
        }
        public static bool operator !=(double left, Pitch right) {
            return !(left == right);
        }
        public static bool operator <(double left, Pitch right) {
            return left.CompareTo(right.Frequency) < 0;
        }
        public static bool operator <=(double left, Pitch right) {
            return left.CompareTo(right.Frequency) <= 0;
        }
        public static bool operator >(double left, Pitch right) {
            return left.CompareTo(right.Frequency) > 0;
        }
        public static bool operator >=(double left, Pitch right) {
            return left.CompareTo(right.Frequency) >= 0;
        }

        public static bool operator ==(Pitch left, double right) {
            return Math.Abs(left.Frequency - right) < 0.001;
        }
        public static bool operator !=(Pitch left, double right) {
            return !(left == right);
        }
        public static bool operator <(Pitch left, double right) {
            return left is not null && left.Frequency.CompareTo(right) < 0;
        }
        public static bool operator <=(Pitch left, double right) {
            return left is null || left.Frequency.CompareTo(right) <= 0;
        }
        public static bool operator >(Pitch left, double right) {
            return left is not null && left.Frequency.CompareTo(right) > 0;
        }
        public static bool operator >=(Pitch left, double right) {
            return left is not null ? left.Frequency.CompareTo(right) >= 0 : false ;
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
        public static PitchInterval operator -(Pitch upperPitch, Pitch basePitch) {
            return PitchInterval.CreateInterval(basePitch, upperPitch);
        }
     }

    public class FloatPitch : Pitch {
        // * Properties
        private double _frequency;

        // * Constructor
        public FloatPitch(double frequency) : base(PitchType.Float, 0) {
            _frequency = frequency;
        }

        // * Sets
        public void SetFrequency(double frequency) { _frequency = frequency; }

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
    
        public override void Increment(PitchInterval interval) {
            _frequency *= interval.FrequencyRatio;
        }
        public override void Decrement(PitchInterval interval) {
            _frequency /= interval.FrequencyRatio;
        }
    
    }

    public class CompoundPitch : Pitch { // ! NOT DONE
        
        // * Properties
        private CompoundPitchInterval _interval;
        private Pitch _pitch;

        // * Derived GS - Handles Compound stacking eliminate recursive behaviours
        public PitchInterval Interval {
            get {
                if (_interval.Intervals.Count == 0) {
                    return PitchInterval.Unison;
                }
                else if (_interval.Intervals.Count == 1) {
                    return (PitchInterval)_interval.Intervals[0].Clone();
                }
                return _interval;
            }
            set {
                var valueCloned = (PitchInterval)value.Clone();
                if (valueCloned is CompoundPitchInterval clonedCompoundInterval) {
                    _interval = clonedCompoundInterval; // guanteed to be reduced
                }
                else {
                    _interval = new CompoundPitchInterval(valueCloned);  // base case
                }
                

            } 
        }
        public Pitch Pitch {
            get { return _pitch; }
            set {
                var valueCloned = (Pitch)value.Clone();
                if (valueCloned is CompoundPitch compoundPitch) { // compile possible additional intervals to the compounder
                    _interval.Increment(compoundPitch.Interval); // handles compound stacking there
                    Pitch = compoundPitch.Pitch; // possible recursion to reduce stack (if there is one)
                    // Pitch MUST not be a compound pitch
                }
                else {
                    _pitch = valueCloned;
                }
            }
        }

        // * Constructor
        public CompoundPitch(Pitch pitch, List<PitchInterval>? intervals = null, int centOffsets = 0)
            : base(PitchType.Compound, centOffsets) {
            Interval = intervals != null ? new CompoundPitchInterval(intervals) : new();
            Pitch = pitch;
        }
        public CompoundPitch(Pitch pitch, PitchInterval interval, int centOffsets = 0)
            : base(PitchType.Compound, centOffsets) {
            Interval = new CompoundPitchInterval(interval);
            Pitch = pitch;
        }
        
        // * Try Compress

        private void tryCompressCompoundIntervalIntoPitch() { // ! NOT DONE
            // try to compress the intervals into the pitch
        }
        private bool tryCompressIntervalIntoPitch(PitchInterval interval) { // ! NOT DONE
            // try to compress the intervals into the pitch
            return false;
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


        public override void Increment(PitchInterval interval) {
            if (!tryCompressIntervalIntoPitch(interval)) {
                _interval.Increment(interval);
            }
        }
        public override void Decrement(PitchInterval interval) {
            if (!tryCompressIntervalIntoPitch(interval.Inverted())) {
                _interval.Decrement(interval);
            }
        }
    
    }
    
    public class CustomTetPitch : Pitch {
        // * Global Mememory Hash
        private static Dictionary<int,CustomTETScale> _globalScales = new();
        
        // * Properties
        public int PitchIndex { get; set; } // * Only Actual Value
        private int _customTetScaleHash { get; set; }

        // * Derived Gets
        public int Base { get { return Scale.Base; } }
        public int TuningIndex { get { return Scale.TuningIndex; } }
        public float TuningFrequency { get { return Scale.TuningFrequency; } }
        public CustomTETScale Scale { get {
            return (CustomTETScale)_globalScales[_customTetScaleHash].Clone();
        } }

        // * Constructor
        public CustomTetPitch(int baseValue, int tuningIndex, float tuningFrequency, int pitchIndex, int centOffsets = 0)
            : base(PitchType.CustomeToneEuqal, centOffsets) {

            PitchIndex = pitchIndex;

            var newScale = new CustomTETScale(baseValue, tuningIndex, tuningFrequency);
            _customTetScaleHash = newScale.GetHashCode();
            if (!_globalScales.ContainsKey(_customTetScaleHash)) {
                _globalScales.TryAdd(_customTetScaleHash, newScale);
            }

        }
        public CustomTetPitch(float frequency, int baseValue, int tuningIndex, float tuningFrequency)
            : base(PitchType.CustomeToneEuqal, 0) {    

            double cacheIndex = baseValue * Math.Log2(frequency / tuningFrequency) + tuningIndex;
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / baseValue * 1200.0);

            var newScale = new CustomTETScale(baseValue, tuningIndex, tuningFrequency);
            _customTetScaleHash = newScale.GetHashCode();
            if (!_globalScales.ContainsKey(_customTetScaleHash)) {
                _globalScales.TryAdd(_customTetScaleHash, newScale);
            }
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


        public override void Increment(PitchInterval interval) { // ! NOT DONE
            if (interval is CustomTetPitchInterval customTetInterval) {

            }   
        }
        public override void Decrement(PitchInterval interval) {  // ! NOT DONE
            throw new NotImplementedException();
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
            var scale = Scale;
            return ToPitchIndex(frequency??Frequency, scale.Base, scale.TuningIndex, scale.TuningFrequency, round);
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
   
         public MidiPitch Incremented(int upBy = 1) {
            var newPitch = (MidiPitch)this.Clone();
            newPitch.Increment(upBy);
            return newPitch;
        }
        public MidiPitch Decremented(int downBy = 1) {
            var newPitch = (MidiPitch)this.Clone();
            newPitch.Decrement(downBy);
            return newPitch;
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
    
        public override void Increment(PitchInterval interval) {
            if (interval is MidiPitchInterval midiInterval) {
                PitchIndex += midiInterval.PitchIntervalIndex;
            }
            else {
                PitchIndex += interval.ToMidiIndex;
            }  
        }
        public void Increment(int upBy = 1) {
            PitchIndex += upBy;
        }
        public override void Decrement(PitchInterval interval) {
            if (interval is MidiPitchInterval midiInterval) {
                PitchIndex -= midiInterval.PitchIntervalIndex;
            }
            else {
                PitchIndex -= interval.ToMidiIndex;
            }  
        }
        public void Decrement(int downBy = 1) {
            PitchIndex -= downBy;
        }
    }

}
