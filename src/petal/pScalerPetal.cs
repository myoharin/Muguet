namespace SineVita.Muguet.Petal.ScalerPetal {
    public enum ScaleType {
        // * Standard Diatonic Modes
        Ionian,
        Dorian,
        Phrygian,
        Lydian,
        Mixolydian,
        Aeolian,
        Locrian,
        // * Other Ones
        WholeTone,
        HarmonicMinor,
        MelodicMinor,
    }

    public class Scale {
        public ScaleType Type { get; set; }
        public Pitch Root { get; set; }
        public Scale(ScaleType type, Pitch root) {
            Type = type;
            Root = root;
        }
    
        // * Statics
        public static readonly IReadOnlyList<ScaleType> DiatonicScales = new List<ScaleType> {
            ScaleType.Ionian,
            ScaleType.Dorian,
            ScaleType.Phrygian,
            ScaleType.Lydian,
            ScaleType.Mixolydian,
            ScaleType.Aeolian,
            ScaleType.Locrian
        }.AsReadOnly();
    }
}