namespace SineVita.Muguet
{
    
    public class ChordReferencedScale : Scale
    {
        private Chord _referenceChord;
        private PitchInterval _repetitionInterval;

        // * Constructor
        public ChordReferencedScale(Chord chord, PitchInterval repetitionInterval) {
            RepetitionInterval = repetitionInterval;
            ReferenceChord = chord;
            if (_referenceChord == null) _referenceChord = Chord.Empty();
            if (_repetitionInterval == null) _repetitionInterval = PitchInterval.Octave;
        }

        public ChordReferencedScale(ICollection<Pitch> notes, PitchInterval repetitionInterval)
            : this(new Chord(notes), repetitionInterval) { }

        public Chord ReferenceChord {
            get => _referenceChord;
            set {
                if (value.Notes.Count == 0) _referenceChord = value;
                if (value.Range >= _repetitionInterval) {
                    var valueCloned = (Chord)value.Clone();
                    var noteList = new List<Pitch>();
                    var repitionMarker = valueCloned.Root.Incremented(RepetitionInterval);
                    foreach (var note in valueCloned.Notes) {
                        while (note > repitionMarker) note.Decrement(_repetitionInterval);
                        noteList.Add(note);
                    }

                    _referenceChord = new Chord(noteList); // * Autosorted
                }
                else
                    _referenceChord = value;
            }
        }

        public PitchInterval RepetitionInterval {
            get => _repetitionInterval;
            set {
                _repetitionInterval = PitchInterval.Abs((PitchInterval)value.Clone());
                ReferenceChord = _referenceChord; // re-set this to trigger range check
            }
        }


        // * Override
        public override List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch) {
            if (ReferenceChord.Notes.Count == 0) // Empty
                return new List<Pitch>();

            // get base list
            List<Pitch> rootList = new();
            foreach (var p in ReferenceChord.Notes)
                rootList.Add(new PitchClass(p).Reduced(rootPitch, RepetitionInterval));
            rootList.Sort();

            // assemble return list
            List<Pitch> returnList = new();
            var i = 0;
            Pitch evaluatedPitch;
            do {
                evaluatedPitch = (Pitch)rootList[i % rootList.Count].Clone();
                for (var _ = 0; _ < Math.Floor(i / (float)rootList.Count); _++)
                    evaluatedPitch.Increment(RepetitionInterval);

                if (evaluatedPitch < topPitch) returnList.Add(evaluatedPitch);
            } while (evaluatedPitch < topPitch);

            return returnList;
        }
        public override object Clone() {
            return new ChordReferencedScale(_referenceChord, RepetitionInterval);
        }
        public override bool Equals(Scale? other) {
            throw new NotImplementedException();
        }
        public override int GetHashCode() {
            return 71 * ReferenceChord.GetHashCode() + 13 * RepetitionInterval.GetHashCode();
        }
    }
}