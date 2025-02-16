using System.Text.Json;

namespace SineVita.Muguet {

    public abstract class PitchInterval {
        // * Properties
        public PitchType Type { get; set; }
        public int CentOffsets { get; set; }

        // * Derived Gets
        public bool IsNegative { get {return FrequencyRatio < 1;} }
        public double FrequencyRatio { get {return GetFrequencyRatio();} }
        public string IntervalName { get {
            return HarmonyHelper.HtzToIntervalName(FrequencyRatio);
        } }
        public float ToMidiValue { get {
            return HarmonyHelper.HtzToMidiInterval(FrequencyRatio, 3);
        } }
        public int ToMidiIndex { get {
            return (int)HarmonyHelper.HtzToMidiInterval(FrequencyRatio);
        } }

        // * Statics
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

        // ! NOT DONE
        public static FloatPitchInterval CreateInterval(Pitch basePitch, Pitch upperPitch, bool absoluteInterval = false, PitchType targetType = PitchType.Float) {
            Pitch higherPitch, lowerPitch;
            if (absoluteInterval) {
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

        // * FromJson
        public static PitchInterval FromJson(string jsonString) {
            var jsonDocument = JsonDocument.Parse(jsonString);
            var rootElement = jsonDocument.RootElement;
            PitchType type = Enum.Parse<PitchType>(rootElement.GetProperty("Type").GetString() ?? "Float");
            switch (type) {
                case PitchType.Float:
                    return new FloatPitchInterval(rootElement.GetProperty("Frequency").GetDouble());
                case PitchType.JustIntonation:
                    var justFrequency = rootElement.GetProperty("JustFrequency");
                    return new JustIntonalPitchInterval(
                        (justFrequency.GetProperty("Numerator").GetInt32(), justFrequency.GetProperty("Denominator").GetInt32()),
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchType.CustomeToneEuqal:
                    return new CustomTetPitchInterval(
                        rootElement.GetProperty("Base").GetInt32(),
                        rootElement.GetProperty("PitchIndex").GetInt32(),
                        type,
                        rootElement.GetProperty("CentOffsets").GetInt32()
                    );
                case PitchType.TwelveToneEqual:
                    return new MidiPitchInterval(
                        rootElement.GetProperty("PitchIndex").GetInt32(),
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
    }

    public class FloatPitchInterval : PitchInterval {

        // * Properties
        private double _frequencyRatio;

        // * Constructor
        public FloatPitchInterval(double frequencyRatio) : base(PitchType.Float, 0) {
            _frequencyRatio = frequencyRatio;
        }

        // * Overrides
        public new int CentOffsets {
            get { return 0; }
            set { _frequencyRatio *= Math.Pow(2, 1+value/1200);}
        }
        public override double GetFrequencyRatio() {return _frequencyRatio;}

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
        public override double GetFrequencyRatio()  {
            return Math.Pow(2, CentOffsets / 1200.0) * JustRatio.Numerator / JustRatio.Denominator;
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

        public override double GetFrequencyRatio() {
            return Math.Pow(2, CentOffsets / 1200.0) * Math.Pow(2, PitchIntervalIndex / (double)Base);    
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

    public class MidiPitchInterval : CustomTetPitchInterval {
        public MidiPitchInterval(int midiValue, int centOffsets = 0)
            : base(12, midiValue, PitchType.TwelveToneEqual, centOffsets) {}
        public MidiPitchInterval(double frequencyRatio) : base (12, frequencyRatio) {}
        public new static float ToPitchIndex(double frequencyRatio, bool round = true) {
            if (round) {return (float)Math.Round(12 * Math.Log2(frequencyRatio));}
            else {return (float)(12 * Math.Log2(frequencyRatio));}
        }
        public static float ToPitchIndex(PitchInterval interval, bool round = true) {
            if (interval.Type == PitchType.TwelveToneEqual) {
                return ((MidiPitchInterval)interval).PitchIntervalIndex;
            }
            else {return ToPitchIndex(interval.FrequencyRatio, round);}
        }
    }

}