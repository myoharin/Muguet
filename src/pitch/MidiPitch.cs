using System.Numerics;
using System.Text.Json;
namespace SineVita.Muguet {
    public class MidiPitch : Pitch, IIncrementOperators<MidiPitch> {
        // * Constants
        public const int Base = 12;
        public const double TuningFrequency = 440;
        public const int TuningIndex = 69;

        // * Variable
        public int PitchIndex { get; set; }

        public MidiPitch(int midiValue, int centOffsets = 0)
            : base(centOffsets) {PitchIndex = midiValue;}
        public MidiPitch(double frequency)
            : base() {
            double cacheIndex = Base * Math.Log2(frequency / TuningFrequency) + TuningIndex;
            if (cacheIndex - Math.Floor(cacheIndex) < 0.5) {PitchIndex = (int)Math.Floor(cacheIndex);}            
            else {PitchIndex = (int)Math.Ceiling(cacheIndex);}
            CentOffsets = (int)Math.Round((cacheIndex - Math.Floor(cacheIndex)) / Base * 1200.0);
        }

        // * To Pitch Index
        public float ToIndex(double? frequency = null, bool round = true) {
            return ToIndex(frequency??this.Frequency, round);
        }
        public static float ToIndex(double frequency, bool round = true) {
            if (round) {return (float)Math.Round(Base * Math.Log2(frequency / TuningFrequency) + TuningIndex);}
            else {return (float)(Base * Math.Log2(frequency / TuningFrequency) + TuningIndex);}
        }

        // * TET increment system
        public void Up(int upBy = 1) {
            this.PitchIndex += upBy;
        }
        public void Down(int downBy = 1) {
            this.PitchIndex -= downBy;
        }
   
        public MidiPitch Incremented(int upBy = 1) { // derived from Increment()
            var newPitch = (MidiPitch)this.Clone();
            newPitch.Increment(upBy);
            return newPitch;
        }
        public MidiPitch Decremented(int downBy = 1) { // derived from Decrement()
            var newPitch = (MidiPitch)this.Clone();
            newPitch.Decrement(downBy);
            return newPitch;
        }    
        
        public static MidiPitch operator ++(MidiPitch pitch) {
            pitch.Up();
            return pitch;
        }
        public static MidiPitch operator --(MidiPitch pitch) {
            pitch.Down();
            return pitch;
        }

            // arithmetic operations with int
        public static MidiPitch operator +(MidiPitch pitch, int upBy) {
            pitch.Increment(upBy);
            return pitch;
        }
        public static MidiPitch operator +(int upBy, MidiPitch pitch) {
            pitch.Increment(upBy);
            return pitch;
        }

        public static MidiPitch operator -(MidiPitch pitch, int downBy) {
            pitch.Decrement(downBy);
            return pitch;
        }
        public static MidiPitchInterval operator -(MidiPitch upperPitch, MidiPitch basePitch) {
            return new MidiPitchInterval(upperPitch.PitchIndex - basePitch.PitchIndex, upperPitch.CentOffsets - basePitch.CentOffsets);
        }
        

        // * Overrides
        public override double GetFrequency() {
            return (float)Math.Pow(2, CentOffsets / 1200.0) * TuningFrequency * (float)Math.Pow(2, (PitchIndex - TuningIndex) / (double)Base);
        }
        public override string ToJson() {
            return string.Concat(
                "{",
                $"\"PitchIndex\": {PitchIndex},",
                $"\"Type\": \"{GetType().ToString()}\",",
                $"\"CentOffsets\": {CentOffsets}",
                "}"
            );
        }
        public static new MidiPitch FromJson(string jsonString) {
            var rootElement = JsonDocument.Parse(jsonString).RootElement;
            return new MidiPitch(
                rootElement.GetProperty("PitchIndex").GetInt32(),
                rootElement.GetProperty("CentOffsets").GetInt32()
            );
        }
        public override object Clone() {
            return new MidiPitch(PitchIndex);
        }
    
        public override void Increment(PitchInterval interval) {
            if (interval is MidiPitchInterval midiInterval) {
                PitchIndex += midiInterval.PitchIntervalIndex;
                CentOffsets += interval.CentOffsets;
            }
            else {
                var tunedInterval = new MidiPitch(interval.FrequencyRatio);
                PitchIndex += tunedInterval.ToMidiIndex;
                CentOffsets += tunedInterval.CentOffsets;
            }  
        }
        public void Increment(int upBy = 1) {
            this.PitchIndex += upBy;
        }
        public override void Decrement(PitchInterval interval) {
            if (interval is MidiPitchInterval midiInterval) {
                PitchIndex -= midiInterval.PitchIntervalIndex;
                CentOffsets -= interval.CentOffsets;
            }
            else {
                var tunedInterval = new MidiPitch(interval.FrequencyRatio);
                PitchIndex -= tunedInterval.ToMidiIndex;
                CentOffsets -= tunedInterval.CentOffsets;
            }  
        }
        public void Decrement(int downBy = 1) {
            this.PitchIndex -= downBy;
        }
    }

}