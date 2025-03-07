using System.Security.Cryptography.X509Certificates;

namespace SineVita.Muguet {
    public enum ScaleType {
        Uncatagorized = 0,
        // * Standard Diatonic Modes
        Ionian,
        Dorian,
        Phrygian,
        Lydian,
        Mixolydian,
        Aeolian,
        Locrian,
        // * Other Octave Constraint Ones
        WholeTone,
        HarmonicMinor,
        MelodicMinor,
    }

    public class Scale {
        // * Statics
        public ScaleType Type { get; set; }
        public static readonly IReadOnlyList<ScaleType> DiatonicScaleTypes = new List<ScaleType> {
            ScaleType.Ionian,
            ScaleType.Dorian,
            ScaleType.Phrygian,
            ScaleType.Lydian,
            ScaleType.Mixolydian,
            ScaleType.Aeolian,
            ScaleType.Locrian
        }.AsReadOnly();
        
    }

    public class PitchClassScale : Scale {

    }

    public class ChordReferencedScale : Scale {
        private Chord _referenceChord;
        public Chord ReferenceChord {
            get { return _referenceChord; }
            set {
                if (value.Notes.Count == 0) {_referenceChord = value;}
                if (value.Range >= _repetitionInterval) {
                    var noteList = new List<Pitch>();
                    var repitionMarker = value.Root.IncrementPitch(RepetitionInterval);
                    foreach(var note in value.Notes) {
                        while (note > repitionMarker) {
                            note.Decrement(_repetitionInterval);
                        }
                        noteList.Add(note);
                    }
                    _referenceChord = new Chord(noteList); // * Autosort
                }
                else {
                    _referenceChord = value;
                }
                
            }
        }
        private PitchInterval _repetitionInterval;
        public PitchInterval RepetitionInterval {
            get { return _repetitionInterval; }
            set {
                if (value.IsNegative) {value.Invert();}
                _repetitionInterval = value;
                ReferenceChord = _referenceChord; // re-set this to trigger range check
            }
        }

        // * Constructor
        public ChordReferencedScale(Chord chord, PitchInterval repetitionInterval, ScaleType? type) {
            Type = type??0;
            RepetitionInterval = repetitionInterval;
            ReferenceChord = chord;
            if (_referenceChord == null) {_referenceChord = Chord.Empty();}
            if (_repetitionInterval == null) {_repetitionInterval = PitchInterval.Octave;}
        }
        public ChordReferencedScale(List<Pitch> notes, PitchInterval repetitionInterval, ScaleType? type)
            : this(new Chord(notes), repetitionInterval, type) {}
    
    }
}