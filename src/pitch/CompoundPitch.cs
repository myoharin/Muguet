using System.Text.Json;
namespace SineVita.Muguet {
    public sealed class CompoundPitch : Pitch {
        
        // * Properties
        private CompoundPitchInterval _interval;
        private Pitch _basePitch;

        // * Derived GS - Handles Compound stacking eliminate recursive behaviours
        public PitchInterval Interval {
            get {
                if (this._interval.Intervals.Count == 0) {
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
                    _interval = clonedCompoundInterval; // guaranteed to be reduced
                }
                else {
                    _interval = new CompoundPitchInterval(valueCloned);  // base case
                }
                

            } 
        }
        public Pitch BasePitch {
            get { return this._basePitch; }
            set {
                var valueCloned = (Pitch)value.Clone();
                if (valueCloned is CompoundPitch compoundPitch) { // compile possible additional intervals to the compounder
                    _interval.Increment(compoundPitch.Interval); // handles compound stacking there
                    BasePitch = compoundPitch.BasePitch; // possible recursion to reduce stack (if there is one)
                    // Pitch MUST not be a compound pitch
                }
                else {
                    _basePitch = valueCloned;
                }
            }
        }

        // * Constructor
        public CompoundPitch(Pitch basePitch, List<PitchInterval>? intervals = null, int centOffsets = 0)
            : base(centOffsets) {
            _interval = new CompoundPitchInterval();
            _basePitch = Pitch.Empty;
            Interval = intervals != null ? new CompoundPitchInterval(intervals) : new();
            BasePitch = basePitch; // automate stack reduction
        }
        public CompoundPitch(Pitch basePitch, PitchInterval interval, int centOffsets = 0)
            : base(centOffsets) {
            _interval = new CompoundPitchInterval();
            _basePitch = Pitch.Empty;
            Interval = new CompoundPitchInterval(interval);
            BasePitch = basePitch; // automate stack reduction
        }
        
        // * Try Compress

        public bool TryCompress() { // TODO very inefficient
            // try to compress the intervals into the pitch
            bool updated = false;
            var updatedList = new List<PitchInterval>();
            foreach(var interval in this._interval.Intervals) {
                if (!tryCompressIntervalIntoPitch(interval)) {
                    updatedList.Add(interval);
                }
                else {
                    updated = true;
                }
            }
            _interval = new CompoundPitchInterval(updatedList);
            return updated;
        }
        
        private bool tryCompressIntervalIntoPitch(PitchInterval interval) {
            // try to compress the intervals into the pitch

            if (interval.IsUnison) {return true;} // check unison

            if (interval.CentOffsets != 0) { // cull cent offsets if that hadnt been done already
                _basePitch.CentOffsets += interval.CentOffsets;
                interval.CentOffsets = 0;
            }          

            switch (interval.GetType()) {
                case Type t when t == typeof(CustomTetPitchInterval):
                    if (_basePitch is CustomTetPitch customTetPitch &&
                        interval is CustomTetPitchInterval customTetInterval &&
                        customTetPitch.Scale.Base == customTetInterval.Base) {
                        customTetPitch.PitchIndex += customTetInterval.PitchIntervalIndex;
                        return true;
                    }
                    break;
                case Type t when t == typeof(FloatPitchInterval):
                    if (_basePitch is FloatPitch floatPitch) {
                        floatPitch.Increment(interval);
                        return true;
                    }
                    break;
                case Type t when t == typeof(MidiPitchInterval):
                    if (_basePitch is MidiPitch midiPitch) {
                        midiPitch.Increment(interval);
                        return true;
                    }
                    break;
                default:
                    return false;
            }
            return false;
        }

        // * Overrides
        public override double GetFrequency() {
            double origin = Math.Pow(2, CentOffsets / 1200.0) * BasePitch.GetFrequency();
            origin *= Interval.GetFrequencyRatio();
            return origin;
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"BasePitch\": {BasePitch.ToJson()},",
                $"\"Interval\": {Interval.ToJson()},",
                
                $"\"Type\": \"{GetType().ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public static new CompoundPitch FromJson(string jsonString) {
            var rootElement = JsonDocument.Parse(jsonString).RootElement;

            var pitch = Pitch.FromJson(rootElement.GetProperty("BasePitch").GetString() ?? Pitch.Empty.ToJson());
            var centOffsets = rootElement.GetProperty("CentOffsets").GetInt32();
            string? intervalStr = rootElement.GetProperty("Interval").GetString();
            CompoundPitchInterval? interval = intervalStr != null ? (CompoundPitchInterval)PitchInterval.FromJson(intervalStr) : null;
            return (interval == null) ? new CompoundPitch(pitch, centOffsets:centOffsets) : new CompoundPitch(pitch, interval, centOffsets);
        }
        public override object Clone() {
            return new CompoundPitch((Pitch)BasePitch.Clone(), (PitchInterval)Interval.Clone(), CentOffsets);
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
    
}