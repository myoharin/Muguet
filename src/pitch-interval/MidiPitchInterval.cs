using System.Numerics;
namespace SineVita.Muguet {
    
    public class MidiPitchInterval : PitchInterval, IIncrementOperators<MidiPitchInterval> {
        public const int Base = 12;
        public int PitchIntervalIndex { get; set; }
        public MidiPitchInterval(int midiValue, int centOffsets = 0)
            : base(centOffsets) {PitchIntervalIndex = midiValue;}
        public MidiPitchInterval(double frequencyRatio) : base (0) {
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
            if (interval.GetType() == typeof(MidiPitchInterval)) {
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
                $"\"Type\": \"{GetType().ToString()}\",",
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