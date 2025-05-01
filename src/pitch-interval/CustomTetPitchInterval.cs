using System.Numerics;

namespace SineVita.Muguet {
    public class CustomTetPitchInterval : PitchInterval, IIncrementOperators<CustomTetPitchInterval> {
        // * Properties
        public int Base { get; set; }
        public int PitchIntervalIndex { get; set; }

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
                $"\"Type\": \"{GetType().ToString()}\",",
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
            this.PitchIntervalIndex += upBy;
        }
        public void Down(int downBy = 1) {
            this.PitchIntervalIndex -= downBy;
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

}