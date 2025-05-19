using System.Text.Json;
namespace SineVita.Muguet {
    public sealed class FloatPitch : Pitch {
        // * Properties
        private double _frequency;

        // * Constructor
        public FloatPitch(double frequency) : base(0) {
            _frequency = frequency;
        }

        // * Sets
        public void SetFrequency(double frequency) { this._frequency = frequency; }

        // * Overrides
        public new int CentOffsets {
            get => 0;
            set { _centOffsets = 0; this._frequency *= Math.Pow(2, 1+value/1200d);}
        }
        public override double GetFrequency() {return _frequency;}
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"Frequency\": {_frequency},",
                $"\"Type\": {GetType().ToString()},",
                $"\"CentOffsets\":{CentOffsets}",
                "}"
            );
        }
        public static new FloatPitch FromJson(string jsonString) {
            var rootElement = JsonDocument.Parse(jsonString).RootElement;

            return new FloatPitch(rootElement.GetProperty("Frequency").GetDouble());
        }

        public override object Clone() {
            return new FloatPitch(_frequency);
        }
    
        public override void Increment(PitchInterval interval) {
            _frequency *= interval.FrequencyRatio;
        }
        public override void Decrement(PitchInterval interval) {
            _frequency /= interval.FrequencyRatio;
        }
    
    }

}