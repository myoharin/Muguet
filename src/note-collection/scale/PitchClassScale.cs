namespace SineVita.Muguet
{
    public class PitchClassScale : Scale
    {
        // has all the diatonic scales
        // all within 1 octave 

        private List<PitchClass> _pitchClasses;

        // * Constructor
        public PitchClassScale(List<PitchClass>? pitchClasses = null) {
            _pitchClasses = pitchClasses ?? new List<PitchClass>();
        }
        public PitchClassScale(List<Pitch> pitches) {
            _pitchClasses = pitches.Select(x => new PitchClass(x)).ToList();
        }
        public List<PitchClass> PitchClasses {
            get => _pitchClasses;
            set => SetPitchClasses(value, 0);
        }

        // * Setter
        public void SetPitchClasses(List<PitchClass> pitchClasses, int rootIndex) {
            var rootReduced = pitchClasses[0].OctaveReduced(Pitch.Empty);
            _pitchClasses = pitchClasses
                .OrderBy(pitchClass => pitchClass.OctaveReduced(rootReduced))
                .ToList();
        }

        // * Transformation
        public bool Contains(Pitch pitch) => _pitchClasses.Any(pitchClass => pitchClass.Equals(pitch));
        public bool Contains(PitchClass pitchClass) {
            foreach (var pitchClassI in _pitchClasses)
                if (pitchClassI.Equals(pitchClass))
                    return true;

            return false;
        }

        // * Override
        public override List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch) {
            if (PitchClasses.Count == 0) // Empty
                return new List<Pitch>();

            // get a base list
            List<Pitch> rootList = new();
            foreach (var p in PitchClasses) rootList.Add(p.OctaveReduced(rootPitch));
            rootList.Sort();

            // assemble a return list
            List<Pitch> returnList = new();
            var i = 0;
            Pitch evaluatedPitch;
            do {
                evaluatedPitch = (Pitch)rootList[i % rootList.Count].Clone();
                for (var _ = 0; _ < Math.Floor(i / (float)rootList.Count); _++)
                    evaluatedPitch.Increment(PitchInterval.Octave);

                if (evaluatedPitch < topPitch) returnList.Add(evaluatedPitch);
            } while (evaluatedPitch < topPitch);

            return returnList;
        }
        public override object Clone() {
            return new PitchClassScale(PitchClasses);
        }

        public override bool Equals(Scale? other) {
            throw new NotImplementedException();
        }
        
    }

}