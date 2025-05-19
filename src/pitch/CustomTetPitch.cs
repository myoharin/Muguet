using System.Numerics;
using System.Text.Json;
namespace SineVita.Muguet {
        public class CustomTetPitch : Pitch, IIncrementOperators<CustomTetPitch> {
        // * Global Memory Hash
        private static Dictionary<int,CustomTetScale> _globalScales = new();
        
        // * Properties
        public int PitchIndex { get; set; } // * Only Actual Value
        private int _customTetScaleHash { get; set; }

        // * Derived Gets
        public int Base { get { return this.Scale.Base; } }
        public int TuningIndex { get { return this.Scale.TuningIndex; } }
        public float TuningFrequency { get { return this.Scale.TuningFrequency; } }
        public CustomTetScale Scale { get {
            return (CustomTetScale)_globalScales[this._customTetScaleHash].Clone();
        } }

        // * Constructors
        public CustomTetPitch(int baseValue, int tuningIndex, float tuningFrequency, int pitchIndex, int centOffsets = 0)
            : base(centOffsets) {

            PitchIndex = pitchIndex;

            var newScale = new CustomTetScale(baseValue, tuningIndex, tuningFrequency);
            _customTetScaleHash = newScale.GetHashCode();
            if (!_globalScales.ContainsKey(_customTetScaleHash)) {
                _globalScales.TryAdd(_customTetScaleHash, newScale);
            }

        }
        public CustomTetPitch(int baseValue, int tuningIndex, float tuningFrequency, double frequency)
            : base(0) {    

            double cacheIndex = baseValue * Math.Log2(frequency / tuningFrequency) + tuningIndex;
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / baseValue * 1200.0);

            var newScale = new CustomTetScale(baseValue, tuningIndex, tuningFrequency);
            _customTetScaleHash = newScale.GetHashCode();
            if (!_globalScales.ContainsKey(_customTetScaleHash)) {
                _globalScales.TryAdd(_customTetScaleHash, newScale);
            }
        }
        
        public CustomTetPitch(CustomTetScale scale, int pitchIndex, int centOffsets = 0) {
            PitchIndex = pitchIndex;
            CentOffsets = centOffsets;

            _customTetScaleHash = scale.GetHashCode();
            if (!_globalScales.ContainsKey(_customTetScaleHash)) {
                _globalScales.TryAdd(_customTetScaleHash, scale);
            }
        }
        public CustomTetPitch(CustomTetScale scale, double frequency) {
            double cacheIndex = scale.Base * Math.Log2(frequency / scale.TuningFrequency) + scale.TuningIndex;
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / scale.Base * 1200.0);

            _customTetScaleHash = scale.GetHashCode();
            if (!_globalScales.ContainsKey(_customTetScaleHash)) {
                _globalScales.TryAdd(_customTetScaleHash, scale);
            }
        }
        
        private CustomTetPitch(int customTetScaleHash, int pitchIndex, int centOffsets = 0) {
            PitchIndex = pitchIndex;
            CentOffsets = centOffsets;
            _customTetScaleHash = customTetScaleHash;
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
                $"\"Type\": \"{GetType().ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public static new CustomTetPitch FromJson(string jsonString) {
            var rootElement = JsonDocument.Parse(jsonString).RootElement;

            return new CustomTetPitch(
                rootElement.GetProperty("Base").GetInt32(),
                rootElement.GetProperty("TuningIndex").GetInt32(),
                rootElement.GetProperty("TuningFrequency").GetSingle(),
                rootElement.GetProperty("PitchIndex").GetInt32(),
                rootElement.GetProperty("CentOffsets").GetInt32()
            );
        }
        public override object Clone() {
            return new CustomTetPitch(_customTetScaleHash, PitchIndex);
        }

        // * TET increment system
        public void Up(int upBy = 1) {
            this.PitchIndex += upBy;
        }
        public void Down(int downBy = 1) {
            this.PitchIndex -= downBy;
        }
        
        public static CustomTetPitch operator ++(CustomTetPitch pitch) {
            pitch.Up();
            return pitch;
        }
        public static CustomTetPitch operator --(CustomTetPitch pitch) {
            pitch.Down();
            return pitch;
        }
        
        public void Increment(int upBy = 1) {
            this.PitchIndex += upBy;
        }
        public override void Increment(PitchInterval interval) {
            if (interval is CustomTetPitchInterval customTetInterval) {
                if (customTetInterval.Base == Scale.Base) {
                    PitchIndex += customTetInterval.PitchIntervalIndex;
                    return;
                }
            }
            var tunedInterval = new CustomTetPitchInterval(Scale.Base, interval.FrequencyRatio);
            PitchIndex += tunedInterval.PitchIntervalIndex;
        }

        public void Decrement(int downBy = 1) {
            this.PitchIndex -= downBy;
        }
        public override void Decrement(PitchInterval interval) {
            if (interval is CustomTetPitchInterval customTetInterval) {
                if (customTetInterval.Base == Scale.Base) {
                    PitchIndex -= customTetInterval.PitchIntervalIndex;
                    return;
                }
            }
            var tunedInterval = new CustomTetPitchInterval(Scale.Base, interval.FrequencyRatio);
            PitchIndex -= tunedInterval.PitchIntervalIndex;
        }

            // arithmetic operations with int
        public static CustomTetPitch operator +(CustomTetPitch pitch, int upBy) {
            pitch.Increment(upBy);
            return pitch;
        }
        public static CustomTetPitch operator +(int upBy, CustomTetPitch pitch) {
            pitch.Increment(upBy);
            return pitch;
        }

        public static CustomTetPitch operator -(CustomTetPitch pitch, int downBy) {
            pitch.Decrement(downBy);
            return pitch;
        }
        public static PitchInterval operator -(CustomTetPitch upperPitch, CustomTetPitch basePitch) { // TODO implemented very poorly
            if (upperPitch.Scale == basePitch.Scale) {
                return new CustomTetPitchInterval(
                    upperPitch.Scale.Base, 
                    upperPitch.PitchIndex - basePitch.PitchIndex, 
                    upperPitch.CentOffsets - basePitch.CentOffsets
                );
            }
            else {
                return PitchInterval.CreateCustomTetPitchInterval(basePitch, upperPitch, false);
            }
        }
        
        // * To Pitch Index
        public static float ToPitchIndex(double frequency, CustomTetScale scale, bool round = true) {
            return ToPitchIndex(frequency, scale.Base, scale.TuningIndex, scale.TuningFrequency, round);
        }
        public static float ToPitchIndex(double frequency, int baseValue, int tuningIndex, float tuningFrequency, bool round = true) {
            if (round) {return (float)Math.Round(baseValue * Math.Log2(frequency / tuningFrequency) + tuningIndex);}
            else {return (float)(baseValue * Math.Log2(frequency / tuningFrequency) + tuningIndex);}
        }
        public float ToPitchIndex(double? frequency = null, bool round = true) {
            var scale = Scale;
            return ToPitchIndex(frequency??this.Frequency, scale.Base, scale.TuningIndex, scale.TuningFrequency, round);
        }
        
    
    }
 
}