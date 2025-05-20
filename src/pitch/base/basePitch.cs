using System.Text.Json;

namespace SineVita.Muguet 
{
    public abstract partial class Pitch : 
            IReadOnlyPitch,
            IComparable, 
            ICloneable
        {
        // * Properties
        protected int _centOffsets;
        public int CentOffsets { 
            get => _centOffsets;
            set { _centOffsets = value;  }
         }

        // * Derived Gets
        public string NoteName { get { return HarmonyHelper.HtzToNoteName(this.Frequency); } }
        public double Frequency { get { return this.GetFrequency(); } }
        public virtual int ToMidiIndex { get {
            return (int)MidiPitch.ToIndex(Frequency);
        } }

        // * statics
        public static FloatPitch New(double frequency) => new FloatPitch(frequency);
        public static Pitch Empty => new FloatPitch(256f);
        private static readonly string[] noteNames = new string[] {
            "C", "C#/Db", "D", "D#/Eb", "E", "F", "F#/Gb", "G", "G#/Ab", "A", "A#/Bb", "B"
        };
        public static string[] NoteNames => noteNames;

        // * FromJson
        public static Pitch FromJson(string jsonString) {
            var jsonDocument = JsonDocument.Parse(jsonString);
            var rootElement = jsonDocument.RootElement;
            var type = System.Type.GetType(rootElement.GetProperty("Type").GetString() ?? throw new ArgumentException("Invalid JsonString"));
            switch (type) {
                case Type t when t == typeof(FloatPitch):
                    return FloatPitch.FromJson(jsonString);
                case Type t when t == typeof(CustomTetPitch):
                    return CustomTetPitch.FromJson(jsonString);
                case Type t when t == typeof(MidiPitch):
                    return MidiPitch.FromJson(jsonString);
                case Type t when t == typeof(CompoundPitch):
                    return CompoundPitch.FromJson(jsonString);
                default:
                    throw new ArgumentException("Invalid pitch type");
            }
        }

        // * Constructor
        protected Pitch(int centOffsets = 0) {
            CentOffsets = centOffsets;
        }

        // * virtual methods
        public virtual double GetFrequency() { return 0; }
        public virtual string ToJson() {return "";}

        // * Incrementation - base type preserved
        public Pitch Incremented(PitchInterval interval) {
            var newPitch = (Pitch)this.Clone();
            newPitch.Increment(interval);
            return newPitch;
        }
        public Pitch Decremented(PitchInterval interval) {
            var newPitch = (Pitch)this.Clone();
            newPitch.Decrement(interval);
            return newPitch;
        }
        public PitchInterval CreateInterval(Pitch pitch2, bool absoluteInterval = false) {
            return PitchInterval.CreateInterval(this, pitch2, absoluteInterval);
        }

        public abstract void Increment(PitchInterval interval);
        public abstract void Decrement(PitchInterval interval);
     }

}
