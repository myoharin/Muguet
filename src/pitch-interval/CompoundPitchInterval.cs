namespace SineVita.Muguet {
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
            : base(centOffsets) {
            _intervals = new();
            Intervals = intervals ?? new();
        }
        public CompoundPitchInterval(PitchInterval interval, int centOffsets = 0)
            : base(centOffsets) {
            _intervals = new();
            Intervals = new(){interval};
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
                
                $"\"Type\": \"{GetType().ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"            );
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
                if (existingInterval.GetType == interval.GetType) {
                    switch (interval.GetType()) {
                        case Type t when t == typeof(CustomTetPitchInterval):
                            if (((CustomTetPitchInterval)existingInterval).Base == 
                                ((CustomTetPitchInterval)interval).Base) {
                                existingInterval.Increment(interval);
                                return;
                            }
                            break; // bass does not match, hence continue on
                        case Type t when t == typeof(JustIntonalPitchInterval): // very easy to
                            existingInterval.Increment(interval);
                            return;
                        case Type t when t == typeof(MidiPitchInterval): // very easy to
                            existingInterval.Increment(interval);
                            return;
                        case Type t when t == typeof(FloatPitchInterval): // very easy to
                            existingInterval.Increment(interval);
                            return;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            
            // Compression Unsuccessful, add as new.
            _intervals.Add(interval);
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
                if (existingInterval.GetType() == interval.GetType()) {
                    switch (interval.GetType()) {
                        case Type t when t == typeof(CustomTetPitchInterval):
                            if (((CustomTetPitchInterval)existingInterval).Base == 
                                ((CustomTetPitchInterval)interval).Base) {
                                existingInterval.Decrement(interval);
                                return;
                            }
                            break; // bass does not match, hence continue on
                        case Type t when t == typeof(JustIntonalPitchInterval): // very easy to
                            existingInterval.Decrement(interval);
                            return;
                        case Type t when t == typeof(MidiPitchInterval): // very easy to
                            existingInterval.Decrement(interval);
                            return;
                        case Type t when t == typeof(FloatPitchInterval): // very easy to
                            existingInterval.Decrement(interval);
                            return;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            
            // Compression Unsuccessful, add as new.
            _intervals.Add(interval.Inverted());
        }

    }

}