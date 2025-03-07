using System.Security.Cryptography;
using System.Text.Json;

namespace SineVita.Muguet {

    public abstract class PitchInterval : IComparable, ICloneable{
        // * Properties
        public PitchType Type { get; set; }
        public int CentOffsets { get; set; }

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
        public string IntervalName { get {
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
        public static FloatPitchInterval CreateInterval(Pitch basePitch, Pitch upperPitch, bool absoluteInterval = false, PitchType targetType = PitchType.Float) {
            Pitch higherPitch, lowerPitch;
            if (absoluteInterval) { // * Absolute value of the interval
                if (basePitch.Frequency > upperPitch.Frequency) {
                    higherPitch = basePitch;
                    lowerPitch = upperPitch;
                }
                else {
                    higherPitch = upperPitch;
                    lowerPitch = basePitch;
                }
            } else {
                lowerPitch = basePitch;
                higherPitch = upperPitch;
            }
            
            // working capital
            var frequency = higherPitch.Frequency / lowerPitch.Frequency;          
            switch (targetType) {
            case PitchType.Float:
                return new FloatPitchInterval(frequency);
            case PitchType.JustIntonation:
                throw new NotImplementedException(); // ! NOT DONE
            case PitchType.TwelveToneEqual:
                throw new NotImplementedException(); // ! NOT DONE
            case PitchType.CustomeToneEuqal:
                throw new NotImplementedException(); // ! NOT DONE
            default:
                throw new ArgumentException("Unsupported PitchType");
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
            PitchType type = Enum.Parse<PitchType>(rootElement.GetProperty("Type").GetString() ?? "Float");
            switch (type) {
                case PitchType.Float:
                    return new FloatPitchInterval(rootElement.GetProperty("FrequencyRatio").GetDouble());
                case PitchType.JustIntonation:
                    var justFrequency = rootElement.GetProperty("JustRatio");
                    return new JustIntonalPitchInterval(
                        (justFrequency.GetProperty("Numerator").GetInt32(), justFrequency.GetProperty("Denominator").GetInt32()),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchType.CustomeToneEuqal:
                    return new CustomTetPitchInterval(
                        rootElement.GetProperty("Base").GetInt32(),
                        rootElement.GetProperty("PitchIntervalIndex").GetInt32(),
                        type,
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchType.TwelveToneEqual:
                    return new MidiPitchInterval(
                        rootElement.GetProperty("PitchIntervalIndex").GetInt32(),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                default:
                    throw new ArgumentException("Invalid pitch type");
            }
        }

        // * Constructor
        protected PitchInterval(PitchType type = PitchType.Float, int centOffsets = 0) {Type = type; CentOffsets = centOffsets;}

        // * virtual methods
        public virtual double GetFrequencyRatio() {return 1;}
        public virtual string ToJson() {return "";}

        // * Interfaces
        public int CompareTo(object? obj) {
            if (obj == null) return 1; // Null is considered less than any object
            if (obj is PitchInterval otherPitch) {
                return FrequencyRatio.CompareTo(otherPitch.FrequencyRatio); // Compare by Frequency
            }
            throw new ArgumentException("Object is not a Pitch");
        }
        public object Clone() {
            var newThis = this;
            return newThis;
        }
            
        public override bool Equals(object? obj) {
            if (obj == null || GetType() != obj.GetType()) return false;
            PitchInterval other = (PitchInterval)obj;
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
    }

    public class FloatPitchInterval : PitchInterval {
        // * Properties
        private double _frequencyRatio;

        // * Constructor
        public FloatPitchInterval(double frequencyRatio) : base(PitchType.Float, 0) {
            _frequencyRatio = frequencyRatio;
        }

        // * Overrides
        public override void Invert() {
            _frequencyRatio = 1/_frequencyRatio;
        }
        public new int CentOffsets {
            get { return 0; }
            set { _frequencyRatio *= Math.Pow(2, 1+value/1200);}
        }
        public override double GetFrequencyRatio() {return _frequencyRatio;}
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"FrequencyRatio\": {FrequencyRatio},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
    }

    public class JustIntonalPitchInterval : PitchInterval {
        // * Properties
        public (int Numerator, int Denominator) JustRatio { get; set; }

        // * Constructor
        public JustIntonalPitchInterval((int, int) justRatio, double? frequencyRatio = null, int centOffsets = 0)
            : base(PitchType.JustIntonation, centOffsets) {
            JustRatio = justRatio;
        }

        // * Overrides
        public override void Invert() {
            JustRatio = (JustRatio.Denominator, JustRatio.Numerator);
        }
        public override double GetFrequencyRatio()  {
            return Math.Pow(2, CentOffsets / 1200.0) * JustRatio.Numerator / JustRatio.Denominator;
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"JustRatio\": {JustRatio},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
    }

    public class CustomTetPitchInterval : PitchInterval {
        // * Properties
        public int Base { get; set; }
        public int PitchIntervalIndex { get; set; }

        // * Constructors
        public CustomTetPitchInterval(int baseValue, int pitchIntervalIndex, PitchType type = PitchType.CustomeToneEuqal, int centOffsets = 0)
            : base(type, centOffsets) {
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

        // * TET increment system
        public void Up(int upBy = 1) {
            PitchIntervalIndex += upBy;
        }
        public void Down(int downBy = 1) {
            PitchIntervalIndex -= downBy;
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

    public class MidiPitchInterval : PitchInterval {
        public const int Base = 12;
        public int PitchIntervalIndex { get; set; }
        public MidiPitchInterval(int midiValue, int centOffsets = 0)
            : base(PitchType.TwelveToneEqual, centOffsets) {PitchIntervalIndex = midiValue;}
        public MidiPitchInterval(double frequencyRatio, bool round = true) : base (PitchType.TwelveToneEqual, 0) {
            double cacheIndex = Base * Math.Log2(frequencyRatio);
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIntervalIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIntervalIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / Base * 1200.0);
        }

        // * ToIndex
        public static float ToIndex(double frequencyRatio, bool round = true) {
            if (round) {return (float)Math.Round(12 * Math.Log2(frequencyRatio));}
            else {return (float)(12 * Math.Log2(frequencyRatio));}
        }
        public static float ToIndex(PitchInterval interval, bool round = true) {
            if (interval.Type == PitchType.TwelveToneEqual) {
                return ((MidiPitchInterval)interval).PitchIntervalIndex;
            }
            else {return ToIndex(interval.FrequencyRatio, round);}
        }

        // * Overrides
        public override void Invert() {
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
    }

}