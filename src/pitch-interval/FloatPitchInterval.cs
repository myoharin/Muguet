namespace SineVita.Muguet {
    public sealed class FloatPitchInterval : PitchInterval {
        // * Properties
        private double _frequencyRatio;

        // * Constructor
        public FloatPitchInterval(double frequencyRatio, int centOffsets = 0) : base() {
            _frequencyRatio = frequencyRatio * Math.Pow(2, centOffsets/1200d);
            CentOffsets = 0;
        }

        // * Overrides
        public override void Invert() {
            CentOffsets *= -1;
            _frequencyRatio = 1/_frequencyRatio;
        }
        public new int CentOffsets {
            get { return 0; }
            set { _frequencyRatio *= Math.Pow(2, value/1200d);}
        }
        public override double GetFrequencyRatio() {
            if (CentOffsets != 0) {
                _frequencyRatio *= Math.Pow(2, CentOffsets/1200d);
                CentOffsets = 0;
            }
            return _frequencyRatio;
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"FrequencyRatio\": {FrequencyRatio},",
                $"\"Type\": \"{GetType()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new FloatPitchInterval(_frequencyRatio);
        }

        public override void Increment(PitchInterval interval) {
            _frequencyRatio *= interval.FrequencyRatio;
        }
        public override void Decrement(PitchInterval interval) {
            _frequencyRatio /= interval.FrequencyRatio;
        }
    }

}