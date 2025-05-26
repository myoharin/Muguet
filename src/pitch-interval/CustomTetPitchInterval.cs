using System.Numerics;

namespace SineVita.Muguet {
    public sealed class CustomTetPitchInterval : PitchInterval, IIncrementOperators<CustomTetPitchInterval> {
        // * Properties
        public int Base { get; set; }
        public int PitchIntervalIndex { get; set; }
        
        public new int Radix => Base;

        // * Constructors
        public CustomTetPitchInterval(int baseValue, int pitchIntervalIndex, int centOffsets = 0)
            : base(centOffsets) {
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
                $"\"Type\": \"{GetType()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new CustomTetPitchInterval(Base, PitchIntervalIndex, CentOffsets);
        }

        public override void Increment(PitchInterval interval) {
            if (interval is CustomTetPitchInterval customInterval && customInterval.Base == this.Base) {
                this.CentOffsets += customInterval.CentOffsets;
                this.PitchIntervalIndex += customInterval.PitchIntervalIndex;
            }
            else {
                 this.Increment(new CustomTetPitchInterval(this.Base, interval.FrequencyRatio));
            }
        }
        public override void Decrement(PitchInterval interval) {
            if (interval is CustomTetPitchInterval customInterval && customInterval.Base == this.Base) {
                this.CentOffsets -= customInterval.CentOffsets;
                this.PitchIntervalIndex -= customInterval.PitchIntervalIndex;
            }
            else {
                 this.Decrement(new CustomTetPitchInterval(this.Base, interval.FrequencyRatio));
            }
        }
    
        // * TET increment system
        private void Up(int upBy = 1) {
            this.PitchIntervalIndex += upBy;
        }
        private void Down(int downBy = 1) {
            this.PitchIntervalIndex -= downBy;
        }
        
        public CustomTetPitchInterval Incremented(int upBy = 1) {
            var newInterval = (CustomTetPitchInterval)this.Clone();
            newInterval.PitchIntervalIndex += upBy;
            return newInterval;
        }
        public CustomTetPitchInterval Decremented(int downBy = 1) {
            var newInterval = (CustomTetPitchInterval)this.Clone();
            newInterval.PitchIntervalIndex -= downBy;
            return newInterval;
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
            if (round) return (float)Math.Round(baseValue * Math.Log2(frequencyRatio));

            return (float)(baseValue * Math.Log2(frequencyRatio));
        }
        public float ToPitchIndex(double frequencyRatio, bool round = true) {
            return ToPitchIndex(Base, frequencyRatio, round);
        }

    }

}