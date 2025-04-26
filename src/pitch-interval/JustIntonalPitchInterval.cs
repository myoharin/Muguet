namespace SineVita.Muguet {
    public class JustIntonalPitchInterval : PitchInterval { 
        // * Properties
        public (int Numerator, int Denominator) _ratio;
        public (int Numerator, int Denominator) Ratio { // strictly maintained as coprimes
            get {
                return _ratio;
            }
            set {
                _ratio = value;
                var lcmNum = lcm(_ratio.Numerator, _ratio.Denominator);
                while (lcmNum != 1) {
                    _ratio =  (_ratio.Numerator / lcmNum, _ratio.Numerator / lcmNum);
                    lcmNum = lcm(_ratio.Numerator, _ratio.Denominator);
                }
            }
        }

        private const int _defaultRatioEstimateToleranceCent = 3;

        // * Statics
        private static int lcm(int a, int b) {
            a = Math.Abs(a);
            b = Math.Abs(b);
            while (a != b) {
                if (a < b) {b -= a;}
                if (a > b) {a -= b;}
            }
            return a;
        }
        public static (int Numerator, int Denominator) EstimateRatio(double ratio, double toleranceRatio) { // ! NOT DONE
            throw new NotImplementedException();
        }
        // TODO This function can have many different varation, need to provide a range as welll
        // TODO OK just get rid of all the functions below that allows raw frequencyRatio, or idk.
        // TODO there needs to be a parameterless converter either way

        // * Constructor
        public JustIntonalPitchInterval((int, int) justRatio, int centOffsets = 0)
            : base(PitchIntervalType.JustIntonation, centOffsets) {
            Ratio = justRatio;
        }

        // * Overrides
        public override void Invert() {
            CentOffsets *= -1;
            Ratio = (Ratio.Denominator, Ratio.Numerator);
        }
        public override double GetFrequencyRatio()  {
            return Math.Pow(2, CentOffsets / 1200.0) * Ratio.Numerator / Ratio.Denominator;
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"Ratio\": {Ratio},",
                $"\"Type\": \"{Type.ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public override object Clone() {
            return new JustIntonalPitchInterval(Ratio, CentOffsets);
        }

        public void Increment(PitchInterval interval, double toleranceRatio) { // ! NOT DONE
            if (interval is JustIntonalPitchInterval justInterval) {
                CentOffsets += justInterval.CentOffsets;
                Increment(justInterval.Ratio);
            } 
            else {
                throw new NotImplementedException();
            }
        }
        public void Increment(PitchInterval interval, int tolerenceCents) {
            Increment(interval, Math.Pow(2,tolerenceCents/1200d));
        }
        public override void Increment(PitchInterval interval) {
            Increment(interval, _defaultRatioEstimateToleranceCent);
        }
        public void Increment(double ratio) { // ! NOT DONE
            throw new NotImplementedException();
        }
        public void Increment((int Numerator, int Denominator) ratio) {
            var newRatio = (ratio.Numerator * _ratio.Numerator, ratio.Denominator * _ratio.Denominator);
            _ratio = newRatio; // Automatically reduced
        }
        
        public void Decrement(PitchInterval interval, double toleranceRatio) { // ! NOT DONE
            if (interval is JustIntonalPitchInterval justInterval) {
                CentOffsets += justInterval.CentOffsets;
                Decrement(justInterval.Ratio);
            } 
            else {
                throw new NotImplementedException();
            }
        }
        public void Decrement(PitchInterval interval, int tolerenceCents) {
            Decrement(interval, Math.Pow(2,tolerenceCents/1200d));
        }
        public override void Decrement(PitchInterval interval) {
            Decrement(interval, _defaultRatioEstimateToleranceCent);
        }
        public void Decrement(double ratio) { // ! NOT DONE
            throw new NotImplementedException();
        }
        public void Decrement((int Numerator, int Denominator) ratio) {
            var newRatio = (ratio.Denominator * _ratio.Numerator, ratio.Numerator * _ratio.Denominator);
            _ratio = newRatio; // Automatically reduced
        }

        public JustIntonalPitchInterval Incremented(double ratio) {
            var interval = (JustIntonalPitchInterval)Clone();
            interval.Increment(ratio);
            return interval;
        }
        public JustIntonalPitchInterval Incremented((int Numerator, int Denominator) ratio) {
            var interval = (JustIntonalPitchInterval)Clone();
            interval.Increment(ratio);
            return interval;
        }
        public JustIntonalPitchInterval Decremented(double ratio) {
            var interval = (JustIntonalPitchInterval)Clone();
            interval.Decrement(ratio);
            return interval;
        }
        public JustIntonalPitchInterval Decremented((int Numerator, int Denominator) ratio) {
            var interval = (JustIntonalPitchInterval)Clone();
            interval.Decrement(ratio);
            return interval;
        }

        // * Operations
        public static JustIntonalPitchInterval operator +(JustIntonalPitchInterval interval, double ratio) {
            interval.Increment(ratio);
            return interval;
        }
        public static JustIntonalPitchInterval operator +(double ratio, JustIntonalPitchInterval interval) {
            interval.Increment(ratio);
            return interval;
        }
        public static JustIntonalPitchInterval operator -(JustIntonalPitchInterval interval, double ratio) {
            interval.Decrement(ratio);
            return interval;
        }

        public static JustIntonalPitchInterval operator +(JustIntonalPitchInterval interval, (int Numerator, int Denominator) ratio) {
            interval.Increment(ratio);
            return interval;
        }
        public static JustIntonalPitchInterval operator +((int Numerator, int Denominator) ratio, JustIntonalPitchInterval interval) {
            interval.Increment(ratio);
            return interval;
        }
        public static JustIntonalPitchInterval operator -(JustIntonalPitchInterval interval, (int Numerator, int Denominator) ratio) {
            interval.Decrement(ratio);
            return interval;
        }
    
    }

}