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
            _notes = new List<Pitch>(notes);
            _notes.Sort();
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
                _notes[i] += interval;
            }
        }

        public Chord Modulated(PitchInterval interval, bool up = true) {
            var returnChord = (Chord)this.Clone();
            returnChord.Modulate(interval, up);
            return returnChord;
        }

        // * Overrides
        public override bool Equals(object? obj) {
            if (obj is Chord otherChord) {
                if (Notes.Count != otherChord.Notes.Count) {
                    return false;
                }
                for (int i = 0; i < Notes.Count; i++) {
                    if (Notes[i] != otherChord.Notes[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        public override int GetHashCode() {
            int hash = 17;
            foreach (var note in Notes) {
            hash = hash * 31 + note.GetHashCode();
            }
            return hash;
        }
        public object Clone() {
            return new Chord(_notes);
        }

    }
}