// ! NOT DONE

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

}