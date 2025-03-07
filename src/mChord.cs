namespace SineVita.Muguet {
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
}