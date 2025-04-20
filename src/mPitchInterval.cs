using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualBasic;

namespace SineVita.Muguet {

    public abstract class PitchInterval : IComparable, ICloneable{
        // * Properties
        public PitchIntervalType Type { get; init; }
        protected int _centOffsets;
        public virtual int CentOffsets {
            get {
                return _centOffsets;
            }
            set {
                _centOffsets = value;
            }
        }

        // * Derived Gets
        public bool IsNegative { get {return FrequencyRatio < 1;} }
        public bool IsPositive { get {return FrequencyRatio > 1;} }
        public bool IsUnison { get {return (FrequencyRatio - 1.0) < 0.001;} }
        
        public double FrequencyRatio { get {return GetFrequencyRatio();} }
        public int ToMidiIndex { get {
            return (int)MidiPitchInterval.ToIndex(this);
        } }
        public float ToMidiValue { get {
            return MidiPitchInterval.ToIndex(this, false);
        } }
            // ? Harmony Helper Derives
        public virtual string IntervalName { get { // can be more specific in subclass.
            return HarmonyHelper.HtzToIntervalName(FrequencyRatio);
        } }
        
        // * Statics
        public static FloatPitch New(double frequencyRatio) {
            return new FloatPitch(frequencyRatio);
        }
        public static PitchInterval Empty { get {return new FloatPitchInterval(1.0);} }
        private static readonly string[] intervalNames = new string[] {
            // Populate with actual interval names
            "R", "m2", "M2", "m3", "M3", "P4", "T1", "P5", "m6", "M6", "m7", "M7",
            "O1", "m9", "M9", "m10", "M10", "P11", "T2", "P12", "m13", "M13", "m14", "M14",
            "O2", "m16", "M16", "m17", "M17", "P18", "T3", "P19", "m20", "M20", "m21", "M21",
            "O3", "m23", "M23", "m24", "M24", "P25", "T4", "P26", "m27", "M27", "m28", "M28",
            "O4"
        };
        public static string[] IntervalNames {get { return intervalNames; }}

        public static PitchInterval Octave { get {
            return new JustIntonalPitchInterval((2,1));
        } }
        public static PitchInterval Perfect5th { get {
            return new JustIntonalPitchInterval((3,2));
        } }
        public static PitchInterval Perfect4th { get {
            return new JustIntonalPitchInterval((4,3));
        } }
        public static PitchInterval PerfectFifth { get {
            return new JustIntonalPitchInterval((3,2));
        } }
        public static PitchInterval PerfectFourth { get {
            return new JustIntonalPitchInterval((4,3));
        } }
        public static PitchInterval Unison { get {return Empty;}}

        // ! NOT DONE - Override below
        public static PitchInterval FromPitches(Pitch basePitch, Pitch upperPitch, bool absoluteInterval = false, PitchIntervalType targetType = PitchIntervalType.Float) {
            return CreateInterval(basePitch, upperPitch, absoluteInterval, targetType);
        }
        public static FloatPitchInterval CreateInterval(Pitch basePitch, Pitch upperPitch, bool absoluteInterval = false, PitchIntervalType targetType = PitchIntervalType.Float) {
            Pitch higherPitch, lowerPitch;
            if (absoluteInterval) { // * Absolute value of the interval
                if (basePitch.Frequency > upperPitch.Frequency) {
                    higherPitch = (Pitch)basePitch.Clone();
                    lowerPitch = (Pitch)upperPitch.Clone();
                }
                else {
                    higherPitch = (Pitch)upperPitch.Clone();
                    lowerPitch = (Pitch)basePitch.Clone();
                }
            } else {
                lowerPitch = (Pitch)basePitch.Clone();
                higherPitch = (Pitch)upperPitch.Clone();
            }
            
            // working capital
            var frequency = higherPitch.Frequency / lowerPitch.Frequency;          
            switch (targetType) {
            case PitchIntervalType.Float:
                return new FloatPitchInterval(frequency);
            case PitchIntervalType.JustIntonation:
                throw new NotImplementedException(); // ! NOT DONE
            case PitchIntervalType.TwelveToneEqual:
                throw new NotImplementedException(); // ! NOT DONE
            case PitchIntervalType.CustomeToneEqual:
                throw new NotImplementedException(); // ! NOT DONE
            case PitchIntervalType.Compound:
                throw new NotImplementedException(); // ! NOT DONE
            default:
                return new FloatPitchInterval(frequency); // ! temporary
                throw new ArgumentException("Unsupported PitchIntervalType");
            }
        }

        // * Inversion | other Abstractions
        public abstract void Invert(); 
        public PitchInterval Inverted() {
            var returnInterval = this;
            returnInterval.Invert();
            return returnInterval;
        }

        // * FromJson
        public static PitchInterval FromJson(string jsonString) {
            var jsonDocument = JsonDocument.Parse(jsonString);
            var rootElement = jsonDocument.RootElement;
            PitchIntervalType type = Enum.Parse<PitchIntervalType>(rootElement.GetProperty("Type").GetString() ?? "Float");
            switch (type) {
                case PitchIntervalType.Float:
                    return new FloatPitchInterval(rootElement.GetProperty("FrequencyRatio").GetDouble());
                case PitchIntervalType.JustIntonation:
                    var justFrequency = rootElement.GetProperty("Ratio");
                    return new JustIntonalPitchInterval(
                        (justFrequency.GetProperty("Numerator").GetInt32(), justFrequency.GetProperty("Denominator").GetInt32()),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchIntervalType.CustomeToneEqual:
                    return new CustomTetPitchInterval(
                        rootElement.GetProperty("Base").GetInt32(),
                        rootElement.GetProperty("PitchIntervalIndex").GetInt32(),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchIntervalType.TwelveToneEqual:
                    return new MidiPitchInterval(
                        rootElement.GetProperty("PitchIntervalIndex").GetInt32(),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchIntervalType.Compound:
                    var centOffsets = rootElement.GetProperty("CentOffsets").GetInt32();
                    var intervals = rootElement.GetProperty("Intervals")
                        .EnumerateArray()
                        .Select(intervalElement => PitchInterval.FromJson(intervalElement.GetRawText()))
                        .ToList();
                    return new CompoundPitchInterval(intervals, centOffsets);
                default:
                    throw new ArgumentException("Invalid pitch type");
            }
        }

        // * Constructor
        protected PitchInterval(PitchIntervalType type, int centOffsets = 0) {Type = type; CentOffsets = centOffsets;}

        // * virtual methods
        public virtual double GetFrequencyRatio() {return 1;}
        public virtual string ToJson() {return "";}

        // * Incrementation - base type preserved
        public abstract void Increment(PitchInterval interval);
        public abstract void Decrement(PitchInterval interval);

        public PitchInterval Incremented(PitchInterval interval) {
            var newInterval = (PitchInterval)this.Clone();
            newInterval.Increment(interval);
            return newInterval;
        }
        public PitchInterval Decremented(PitchInterval interval) {
            var newInterval = (PitchInterval)this.Clone();
            newInterval.Decrement(interval);
            return newInterval;
        }

        // * Interfaces
        public int CompareTo(object? obj) {
            if (obj == null) return 1; // Null is considered less than any object
            if (obj is PitchInterval otherPitch) {
                return FrequencyRatio.CompareTo(otherPitch.FrequencyRatio); // Compare by Frequency
            }
            throw new ArgumentException("Object is not a Pitch");
        }
        public abstract object Clone();
            
        public override bool Equals(object? obj) {
            if (obj == null || GetType() != obj.GetType()) return false;
            PitchInterval other = (PitchInterval)obj;
            return Math.Abs(FrequencyRatio - other.FrequencyRatio) < 0.0001;
        }
        public override int GetHashCode() { // ! NOT DONE EVERYONE NEED UNIQUE HASH CODES
            return FrequencyRatio.GetHashCode();
        }
        public static bool operator ==(PitchInterval? left, PitchInterval? right) {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        public static bool operator !=(PitchInterval left, PitchInterval right) {
            return !(left == right);
        }
        public static bool operator <(PitchInterval left, PitchInterval right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(PitchInterval left, PitchInterval right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(PitchInterval left, PitchInterval right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(PitchInterval left, PitchInterval right) {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    
        // ! NOT DONE
        // TODO ADD Arythmetic Operators bewteen Intervals

    }

    public class CompoundPitchInterval : PitchInterval {
        // * Properties
        private List<PitchInterval> _intervals;
        public List<PitchInterval> Intervals {
            get { return new(_intervals); } // already reduced when added
            set { // make sure it has no more than 1 layer when setting and adding
                List<PitchInterval> valueCloned = new(value);
                _intervals = new(); // reset to base, to unison
                foreach(var interval in valueCloned) {
                    Increment(interval); // handles all the logic
                }
            }
        }

        // * Derived Gets
        public bool ContainsIntervalType(PitchIntervalType type) {
            foreach (var interval in _intervals) {
                if (interval.Type == type) {
                    return true;
                }
            }
            return false;
        }
        public bool ContainsIntervalType(System.Type type) {
            foreach (var interval in _intervals) {
                if (interval.GetType() == type) {
                    return true;
                }
            }
            return false;
        }

        // * Constructor
        public CompoundPitchInterval(List<PitchInterval>? intervals = null, int centOffsets = 0)
            : base(PitchIntervalType.Compound, centOffsets) {
            _intervals = new();
            Intervals = intervals ?? new();
        }
        public CompoundPitchInterval(PitchInterval interval, int centOffsets = 0)
            : base(PitchIntervalType.Compound, centOffsets) {
            _intervals = new();
            Intervals = new(){interval};
        }

        // * Private Methods
        private void tryCompress() { // compress the intervals together // ! NOT DONE

        }
        
        // * Overrides
        public override double GetFrequencyRatio() {
            double origin = 1.0d;
            foreach (var interval in Intervals) {
                origin *= interval.GetFrequencyRatio();
            }
            return origin;
        }
        public override void Invert() {
            CentOffsets *= -1;
            Intervals = Intervals.Select(x => x.Inverted()).ToList();
        }  
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"Intervals\": [{string.Join(", ", Intervals.Select(interval => interval.ToJson()))}],",
                
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new CompoundPitchInterval(new List<PitchInterval>(Intervals), CentOffsets);
        }

        public override void Increment(PitchInterval interval) {
            // deal with cents
            this.CentOffsets += interval.CentOffsets;
            interval.CentOffsets = 0;

            // try compress as compound
            if (interval is CompoundPitchInterval compoundInterval) {
                foreach (var internalInterval in compoundInterval.Intervals) {
                    Increment(internalInterval);
                }
                return;
            }

            // compres as other
            foreach (var existingInterval in Intervals) {
                if (existingInterval.Type == interval.Type) {
                    switch (interval.Type) {
                        case PitchIntervalType.CustomeToneEqual:
                            if (((CustomTetPitchInterval)existingInterval).Base == 
                                ((CustomTetPitchInterval)interval).Base) {
                                existingInterval.Increment(interval);
                                return;
                            }
                            break; // bass does not match, hence continue on
                        case PitchIntervalType.JustIntonation: // very easy to
                        case PitchIntervalType.TwelveToneEqual: // very easy to
                        case PitchIntervalType.Float: // very easy to
                            existingInterval.Increment(interval);
                            return;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            
            // Compression Unsuccessful, add as new.
            Intervals.Add(interval);
        }
        public override void Decrement(PitchInterval interval) {
            // deal with cents
            this.CentOffsets += interval.CentOffsets;
            interval.CentOffsets = 0;

            // try compress as compound
            if (interval is CompoundPitchInterval compoundInterval) {
                foreach (var internalInterval in compoundInterval.Intervals) {
                    Decrement(internalInterval);
                }
                return;
            }

            // compres as other
            foreach (var existingInterval in Intervals) {
                if (existingInterval.Type == interval.Type) {
                    switch (interval.Type) {
                        case PitchIntervalType.CustomeToneEqual:
                            if (((CustomTetPitchInterval)existingInterval).Base == 
                                ((CustomTetPitchInterval)interval).Base) {
                                existingInterval.Decrement(interval);
                                return;
                            }
                            break; // bass does not match, hence continue on
                        case PitchIntervalType.JustIntonation: // very easy to
                        case PitchIntervalType.TwelveToneEqual: // very easy to
                        case PitchIntervalType.Float: // very easy to
                            existingInterval.Decrement(interval);
                            return;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            
            // Compression Unsuccessful, add as new.
            Intervals.Add(interval.Inverted());
        }

    }

    public class FloatPitchInterval : PitchInterval {
        // * Properties
        private double _frequencyRatio;

        // * Constructor
        public FloatPitchInterval(double frequencyRatio, int centOffsets = 0) : base(PitchIntervalType.Float, 0) {
            _frequencyRatio = frequencyRatio * Math.Pow(2, centOffsets/1200d);
            CentOffsets = 0;
        }

        // * Overrides
        public override void Invert() {
            CentOffsets *= -1;
            _frequencyRatio = 1/_frequencyRatio;
        }
        public new int CentOffsets {
            get { return 0; }
            set { _frequencyRatio *= Math.Pow(2, 1+value/1200);}
        }
        public override double GetFrequencyRatio() {
            if (CentOffsets != 0) {
                _frequencyRatio *= Math.Pow(2, CentOffsets/1200d);
                CentOffsets = 0;
            }
            return _frequencyRatio;
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"FrequencyRatio\": {FrequencyRatio},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new FloatPitchInterval(_frequencyRatio);
        }

        public override void Increment(PitchInterval interval) {
            _frequencyRatio *= interval.FrequencyRatio;
        }
        public override void Decrement(PitchInterval interval) {
            _frequencyRatio /= interval.FrequencyRatio;
        }
    }

    public class JustIntonalPitchInterval : PitchInterval { 
        // * Properties
        public (int Numerator, int Denominator) _ratio;
        public (int Numerator, int Denominator) Ratio { // strictly maintained as coprimes
            get {
                return _ratio;
            }
            set {
                _ratio = value;
                var lcmNum = lcm(_ratio.Numerator, _ratio.Denominator);
                while (lcmNum != 1) {
                    _ratio =  (_ratio.Numerator / lcmNum, _ratio.Numerator / lcmNum);
                    lcmNum = lcm(_ratio.Numerator, _ratio.Denominator);
                }
            }
        }

        // * Helper
        private static int lcm(int a, int b) {
            a = Math.Abs(a);
            b = Math.Abs(b);
            while (a != b) {
                if (a < b) {b -= a;}
                if (a > b) {a -= b;}
            }
            return a;
        }

        // * Constructor
        public JustIntonalPitchInterval((int, int) justRatio, int centOffsets = 0)
            : base(PitchIntervalType.JustIntonation, centOffsets) {
            Ratio = justRatio;
        }

        // * Overrides
        public override void Invert() {
            CentOffsets *= -1;
            Ratio = (Ratio.Denominator, Ratio.Numerator);
        }
        public override double GetFrequencyRatio()  {
            return Math.Pow(2, CentOffsets / 1200.0) * Ratio.Numerator / Ratio.Denominator;
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"Ratio\": {Ratio},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new JustIntonalPitchInterval(Ratio, CentOffsets);
        }
    
        public override void Increment(PitchInterval interval) { // ! NOT DONE
            // * ALL THE LOGIC
            if (interval is JustIntonalPitchInterval justInterval) {

            }   
        }
        public void Increment(double ratio) { // ! NOT DONE
            throw new NotImplementedException();
        }
        public void Increment((int Numerator, int Denominator) ratio) {
            var newRatio = (ratio.Numerator * _ratio.Numerator, ratio.Denominator * _ratio.Denominator);
            _ratio = newRatio; // Automatically reduced
        }
         
        public override void Decrement(PitchInterval interval) { // ! NOT DONE
            throw new NotImplementedException();
        }
        public void Decrement(double ratio) { // ! NOT DONE
            throw new NotImplementedException();
        }
        public void Decrement((int Numerator, int Denominator) ratio) {
            var newRatio = (ratio.Denominator * _ratio.Numerator, ratio.Numerator * _ratio.Denominator);
            _ratio = newRatio; // Automatically reduced
        }

        public JustIntonalPitchInterval Incremented(double ratio) {
            var interval = (JustIntonalPitchInterval)Clone();
            interval.Increment(ratio);
            return interval;
        }
        public JustIntonalPitchInterval Incremented((int Numerator, int Denominator) ratio) {
            var interval = (JustIntonalPitchInterval)Clone();
            interval.Increment(ratio);
            return interval;
        }
        public JustIntonalPitchInterval Decremented(double ratio) {
            var interval = (JustIntonalPitchInterval)Clone();
            interval.Decrement(ratio);
            return interval;
        }
        public JustIntonalPitchInterval Decremented((int Numerator, int Denominator) ratio) {
            var interval = (JustIntonalPitchInterval)Clone();
            interval.Decrement(ratio);
            return interval;
        }

        // * Operations
        public static JustIntonalPitchInterval operator +(JustIntonalPitchInterval interval, double ratio) {
            interval.Increment(ratio);
            return interval;
        }
        public static JustIntonalPitchInterval operator +(double ratio, JustIntonalPitchInterval interval) {
            interval.Increment(ratio);
            return interval;
        }
        public static JustIntonalPitchInterval operator -(JustIntonalPitchInterval interval, double ratio) {
            interval.Decrement(ratio);
            return interval;
        }

        public static JustIntonalPitchInterval operator +(JustIntonalPitchInterval interval, (int Numerator, int Denominator) ratio) {
            interval.Increment(ratio);
            return interval;
        }
        public static JustIntonalPitchInterval operator +((int Numerator, int Denominator) ratio, JustIntonalPitchInterval interval) {
            interval.Increment(ratio);
            return interval;
        }
        public static JustIntonalPitchInterval operator -(JustIntonalPitchInterval interval, (int Numerator, int Denominator) ratio) {
            interval.Decrement(ratio);
            return interval;
        }
    
    }

    public class CustomTetPitchInterval : PitchInterval, IIncrementOperators<CustomTetPitchInterval> {
        // * Properties
        public int Base { get; set; }
        public int PitchIntervalIndex { get; set; }

        // * Constructors
        public CustomTetPitchInterval(int baseValue, int pitchIntervalIndex, int centOffsets = 0)
            : base(PitchIntervalType.CustomeToneEqual, centOffsets) {
            Base = baseValue;
            PitchIntervalIndex = pitchIntervalIndex;
        }
        public CustomTetPitchInterval(int baseValue, double frequencyRatio) : this(baseValue, 0) {
            double cacheIndex = baseValue * Math.Log2(frequencyRatio);
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIntervalIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIntervalIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / baseValue * 1200.0);
        }

        // * Overrides
        public override void Invert() {
            CentOffsets *= -1;
            PitchIntervalIndex *= -1;
        }
        public override double GetFrequencyRatio() {
            return Math.Pow(2, CentOffsets / 1200.0) * Math.Pow(2, PitchIntervalIndex / (double)Base);    
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"Base\": {Base},",
                $"\"PitchIntervalIndex\": {PitchIntervalIndex},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new CustomTetPitchInterval(Base, PitchIntervalIndex, CentOffsets);
        }

        public override void Increment(PitchInterval interval) { // ! NOT DONE
            if (interval is MidiPitchInterval midiInterval) {

            }   
        }
        public override void Decrement(PitchInterval interval) {  // ! NOT DONE
            throw new NotImplementedException();
        }
    
        // * TET increment system
        public void Up(int upBy = 1) {
            PitchIntervalIndex += upBy;
        }
        public void Down(int downBy = 1) {
            PitchIntervalIndex -= downBy;
        }

        public static CustomTetPitchInterval operator ++(CustomTetPitchInterval interval) {
            interval.Up();
            return interval;
        }
        public static CustomTetPitchInterval operator --(CustomTetPitchInterval interval) {
            interval.Down();
            return interval;
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

    public class MidiPitchInterval : PitchInterval, IIncrementOperators<MidiPitchInterval> {
        public const int Base = 12;
        public int PitchIntervalIndex { get; set; }
        public MidiPitchInterval(int midiValue, int centOffsets = 0)
            : base(PitchIntervalType.TwelveToneEqual, centOffsets) {PitchIntervalIndex = midiValue;}
        public MidiPitchInterval(double frequencyRatio) : base (PitchIntervalType.TwelveToneEqual, 0) {
            double cacheIndex = Base * Math.Log2(frequencyRatio);
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIntervalIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIntervalIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / Base * 1200.0);
        }

        // * Derived Gets
        public int Index { get {return PitchIntervalIndex;} }
        public override string IntervalName { get {return HarmonyHelper.MidiToIntervalName(Index); } }

        // * ToIndex
        public static float ToIndex(double frequencyRatio, bool round = true) {
            if (round) {return (float)Math.Round(12 * Math.Log2(frequencyRatio));}
            else {return (float)(12 * Math.Log2(frequencyRatio));}
        }
        public static float ToIndex(PitchInterval interval, bool round = true) {
            if (interval.Type == PitchIntervalType.TwelveToneEqual) {
                return ((MidiPitchInterval)interval).PitchIntervalIndex;
            }
            else {return ToIndex(interval.FrequencyRatio, round);}
        }

        // * TET increment system
        public void Up(int upBy = 1) {
            PitchIntervalIndex += upBy;
        }
        public void Down(int downBy = 1) {
            PitchIntervalIndex -= downBy;
        }
        
        public MidiPitchInterval Incremented(int upBy = 1) {
            var newInterval = (MidiPitchInterval)this.Clone();
            newInterval.Increment(upBy);
            return newInterval;
        }
        public MidiPitchInterval Decremented(int downBy = 1) {
            var newInterval = (MidiPitchInterval)this.Clone();
            newInterval.Decrement(downBy);
            return newInterval;
        }    
        
        public static MidiPitchInterval operator ++(MidiPitchInterval interval) {
            interval.Up();
            return interval;
        }
        public static MidiPitchInterval operator --(MidiPitchInterval interval) {
            interval.Down();
            return interval;
        }

            // arithmetic operations with int
        public static MidiPitchInterval operator +(MidiPitchInterval interval, int upBy) {
            interval.Increment(upBy);
            return interval;
        }
        public static MidiPitchInterval operator +(int upBy, MidiPitchInterval interval) {
            interval.Increment(upBy);
            return interval;
        }

        public static MidiPitchInterval operator -(MidiPitchInterval interval, int downBy) {
            interval.Decrement(downBy);
            return interval;
        }
        public static MidiPitchInterval operator -(MidiPitchInterval upperInterval, MidiPitchInterval baseInterval) {
            return new MidiPitchInterval(upperInterval.PitchIntervalIndex - baseInterval.PitchIntervalIndex, upperInterval.CentOffsets - baseInterval.CentOffsets);
        }

        // * Overrides
        public override void Invert() {
            CentOffsets *= -1;
            PitchIntervalIndex *= -1;
        }
        public override double GetFrequencyRatio() {
            return Math.Pow(2, CentOffsets/1200d + PitchIntervalIndex/12d);
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"PitchIntervalIndex\": {PitchIntervalIndex},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new MidiPitchInterval(PitchIntervalIndex, CentOffsets);
        }

        public override void Increment(PitchInterval interval) {
            if (interval is MidiPitchInterval midiInterval) {
                PitchIntervalIndex += midiInterval.PitchIntervalIndex;
            }
            else {
                PitchIntervalIndex += interval.ToMidiIndex;
            }  
        }
        public void Increment(int upBy = 1) {
            PitchIntervalIndex += upBy;
        }
        public override void Decrement(PitchInterval interval) {
            if (interval is MidiPitchInterval midiInterval) {
                PitchIntervalIndex -= midiInterval.PitchIntervalIndex;
            }
            else {
                PitchIntervalIndex -= interval.ToMidiIndex;
            }  
        }
        public void Decrement(int downBy = 1) {
            PitchIntervalIndex -= downBy;
        }
    }

}