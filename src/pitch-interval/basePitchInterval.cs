using System.Numerics;
using System.Text.Json;

namespace SineVita.Muguet {

    public abstract class PitchInterval : 
            IComparable, 
            ICloneable, 
            IEquatable<PitchInterval> // made redunant later
            // INumber<PitchInterval>,
            // ISignedNumber<PitchInterval>,

            // IUnsignedNumber<PitchInterval>,
            // IMinMaxValue<PitchInterval>,

            // IAdditiveIdentity<PitchInterval,PitchInterval>,
            // IMultiplicativeIdentity<PitchInterval,PitchInterval>


            // LINK https://learn.microsoft.com/en-us/dotnet/standard/generics/math
        {
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
        public virtual bool IsNegative { get {return FrequencyRatio < 1;} }
        public virtual bool IsPositive { get {return FrequencyRatio > 1;} }
        public virtual bool IsUnison { get {return (FrequencyRatio - 1.0) < 0.001;} }
        
        public virtual double FrequencyRatio { get {return GetFrequencyRatio();} }
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
        public static PitchInterval FromJson(string jsonString) { // ! NEED TO MAKE PITCH INTERVAL TYPE REDUNANT
            var jsonDocument = JsonDocument.Parse(jsonString);
            var rootElement = jsonDocument.RootElement;
            PitchIntervalType type = Enum.Parse<PitchIntervalType>(rootElement.GetProperty("Type").GetString() ??  throw new ArgumentException("Invalid JsonString"));
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
        public abstract double GetFrequencyRatio();
        public abstract string ToJson();

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
        public int CompareTo(object? obj) { // ! NOT DONE - account for numerics comparison as well
            if (obj == null) return 1; // Null is considered less than any object
            if (obj is PitchInterval otherPitch) {
                return this.FrequencyRatio.CompareTo(otherPitch.FrequencyRatio); // Compare by Frequency
            }
            throw new ArgumentException("Object is not a Pitch");
        }
        public abstract object Clone();
            
        public override bool Equals(object? obj) { // ! NOT DONE - account for numerics comparison as well
            if (obj == null || GetType() != obj.GetType()) return false;
            PitchInterval other = (PitchInterval)obj;
            return Math.Abs(FrequencyRatio - other.FrequencyRatio) < 0.0001;
        }
        public bool Equals(PitchInterval? other) {
            if (other == null || GetType() != other.GetType()) {return false;}
            return Math.Abs(FrequencyRatio - other.FrequencyRatio) < 0.0001;
        }

        public override int GetHashCode() {
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
    
        public static bool operator ==(double left, PitchInterval right) {
            return Math.Abs(left - right.FrequencyRatio) < 0.001;
        }
        public static bool operator !=(double left, PitchInterval right) {
            return !(left == right);
        }
        public static bool operator <(double left, PitchInterval right) {
            return left.CompareTo(right) < 0;
        }
        public static bool operator <=(double left, PitchInterval right) {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >(double left, PitchInterval right) {
            return left.CompareTo(right) > 0;
        }
        public static bool operator >=(double left, PitchInterval right) {
            return left.CompareTo(right) >= 0;
        }
    
        public static bool operator ==(PitchInterval left, double right) {
            return Math.Abs(left.FrequencyRatio - right) < 0.001;
        }
        public static bool operator !=(PitchInterval left, double right) {
            return !(left == right);
        }
        public static bool operator <(PitchInterval left, double right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(PitchInterval left, double right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(PitchInterval left, double right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(PitchInterval left, double right) {
            return !(left is null) && left.CompareTo(right) >= 0;
        }
    
        // ! NOT DONE
        // TODO ADD Arythmetic Operators bewteen Intervals
        // + * are synonymous | - / are synonymous

    }
}