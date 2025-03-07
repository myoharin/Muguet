using System.Security.Cryptography.X509Certificates;

namespace SineVita.Muguet.Petal {
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
        // * Other Ones
        WholeTone,
        HarmonicMinor,
        MelodicMinor,
    }

    public class Chord {
        private List<Pitch> _notes;

        // * Derived Gets
        public List<Pitch> Notes { 
            get {return _notes;}
            set {SetChord(value);}
        }
        public PitchInterval Range { get {
            if (Notes.Count <= 1) {return PitchInterval.Unison;}
            return Notes[0].CreateInterval(Notes[Notes.Count-1]);
        } }
        public Pitch Root { get {
            if (Notes.Count == 0) {
                throw new IndexOutOfRangeException("Root does not exist in empty chord.");
            }
            return Notes[0];
        } }


        // * Statics / Templates
        public static Chord Empty() {
            return new Chord(new List<Pitch>());
        }
        

        // * Constructor
        public Chord(List<Pitch>? notes) {
            SetChord(notes??new List<Pitch>());
            if (_notes == null) {_notes = new List<Pitch>();}
        }

        public void SetChord(List<Pitch> notes) {
            notes.Sort();
            _notes = notes;
        }
        public void SetNotes(List<Pitch> notes) {
            SetChord(notes);
        }

        // * Manipulation Methods
        public void ModulateUp(PitchInterval interval) {Modulate(interval, true);}
        public void ModulateDown(PitchInterval interval) {Modulate(interval, false);}
        public void Modulate(PitchInterval interval, bool up = true) {
            if (!up) {interval.Invert();}
            for (int i = 0; i < Notes.Count; i++) {
                _notes[i] = _notes[i].IncrementPitch(interval);
            }
        }

        public Chord Modulated(PitchInterval interval, bool up = true) {
            var returnChord = this;
            returnChord.Modulate(interval, up);
            return returnChord;
        }

    }

    public class Scale {
        public ScaleType Type { get; set; }
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
        public Scale(Chord chord, PitchInterval repetitionInterval, ScaleType? type) {
            Type = type??0;
            RepetitionInterval = repetitionInterval;
            ReferenceChord = chord;
            if (_referenceChord == null) {_referenceChord = Chord.Empty();}
            if (_repetitionInterval == null) {_repetitionInterval = PitchInterval.Octave;}
        }
        public Scale(List<Pitch> notes, PitchInterval repetitionInterval, ScaleType? type)
            : this(new Chord(notes), repetitionInterval, type) {}
    
        // * Statics
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
}