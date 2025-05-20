namespace SineVita.Muguet
{
    public partial class Chord : IReadOnlyChord
    {
        private List<Pitch> _notes;

        // * Constructor
        public Chord(ICollection<Pitch>? notes = null) {
            SetChord(notes is null ? new List<Pitch>() : new List<Pitch>(notes));
            if (_notes == null) _notes = new List<Pitch>();
        }
        
        // * IReadOnly
        IReadOnlyList<IReadOnlyPitch> IReadOnlyChord.Notes => _notes.AsReadOnly();
        
        // * Derived Gets
        public List<Pitch> Notes {
            get => _notes;
            set => SetChord(value);
        }

        public PitchInterval Range {
            get {
                if (Notes.Count <= 1) return PitchInterval.Unison;
                return Notes[0].CreateInterval(Notes[Notes.Count - 1]);
            }
        }

        public Pitch Root {
            get {
                if (Notes.Count == 0) throw new IndexOutOfRangeException("Root does not exist in empty chord.");
                return Notes[0];
            }
        }

        public int Add(Pitch pitch) { // return the index which it is inserted
            for (var i = 0; i < Notes.Count; i++)
                if (pitch < _notes[i]) {
                    _notes.Insert(i, pitch);
                    return i;
                }

            _notes.Add(pitch);
            return Notes.Count - 1;
        }

        public void SetChord(ICollection<Pitch> notes) {
            _notes = new List<Pitch>(notes);
            _notes.Sort();
        }
        public void SetNotes(ICollection<Pitch> notes) {
            SetChord(notes);
        }

        // * Manipulation Methods
        public void ModulateUp(PitchInterval interval) {
            Modulate(interval);
        }

        public void ModulateDown(PitchInterval interval) {
            Modulate(interval, false);
        }

        public void Modulate(PitchInterval interval, bool up = true) {
            if (!up) interval.Invert();
            for (var i = 0; i < Notes.Count; i++) _notes[i] += interval;
        }

        public Chord Modulated(PitchInterval interval, bool up = true) {
            var returnChord = (Chord)Clone();
            returnChord.Modulate(interval, up);
            return returnChord;
        }

        // * Overrides
        public override bool Equals(object? obj) {
            if (obj is Chord otherChord) {
                if (Notes.Count != otherChord.Notes.Count) return false;
                for (var i = 0; i < Notes.Count; i++)
                    if (Notes[i] != otherChord.Notes[i])
                        return false;

                return true;
            }

            return false;
        }

        public override int GetHashCode() {
            var hash = 17;
            foreach (var note in Notes) hash = hash * 31 + note.GetHashCode();
            return hash;
        }

        public object Clone() {
            return new Chord(_notes);
        }
    }
}