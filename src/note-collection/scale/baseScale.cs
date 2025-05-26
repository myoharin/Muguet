namespace SineVita.Muguet 
{
    public abstract class Scale :
        ICloneable,
        IEquatable<Scale>
    {
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
        
        // * Equatable
        public override bool Equals(object? obj) {
            if (obj is null) {
                return false;
            }
            else if (obj is Scale scale) {
                return Equals(scale);
            }
            return false;
        }
        
        public abstract override int GetHashCode();
        public abstract object Clone();
        public abstract bool Equals(Scale? other);

        // * Transformation
        public List<Pitch> MapToRange(Pitch referencePitch, PitchInterval range, bool referencePitchIsRoot = true) {
            var otherPitch = referencePitchIsRoot ? referencePitch.Incremented(range) : referencePitch.Decremented(range);
            var rootPitch = referencePitchIsRoot ? referencePitch : otherPitch;
            var topPitch = !referencePitchIsRoot ? referencePitch : otherPitch;
            return MapToRange(rootPitch, topPitch);
        }

        // * Abstracts Methods
        public abstract List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch); // inclusive of range

        public static PitchClassScale ChromaticScale(int initialMidiIndex = 69) {
            List<PitchClass> list = new();
            for (var i = 0; i < 12; i++)
                list.Add(new PitchClass(new MidiPitch(initialMidiIndex + i))); // starting from certain pitch
            return new PitchClassScale(list);
        }

        public static PitchClassScale ChromaticScale(MidiPitchName midiPitchName) {
            return ChromaticScale((int)midiPitchName + 60);
        }


        public static PitchClassScale DiatonicScaleTwelveTet(ScaleType type, MidiPitchName midiPitchName) {
            return DiatonicScaleTwelveTet(type, new MidiPitch(60 + (int)midiPitchName));
        }

        public static PitchClassScale DiatonicScaleTwelveTet(ScaleType type, Pitch tonic) {
            List<MidiPitchInterval> lydianScaleRelativeInterval = new() {
                new MidiPitchInterval(0),
                new MidiPitchInterval(7),
                new MidiPitchInterval(2),
                new MidiPitchInterval(9),
                new MidiPitchInterval(4),
                new MidiPitchInterval(11),
                new MidiPitchInterval(6)
            };
            var flatCount = 0;
            switch (type) {
                case ScaleType.Lydian:
                    break;
                case ScaleType.Ionian:
                    flatCount = 1;
                    break;
                case ScaleType.Mixolydian:
                    flatCount = 2;
                    break;
                case ScaleType.Dorian:
                    flatCount = 3;
                    break;
                case ScaleType.Aeolian:
                    flatCount = 4;
                    break;
                case ScaleType.Phrygian:
                    flatCount = 5;
                    break;
                case ScaleType.Locrian:
                    flatCount = 6;
                    break;
                default:
                    throw new ArgumentException($"ScaleType {type} is not a diatonic scale type.");
            }

            for (var i = 0; i < flatCount; i++) lydianScaleRelativeInterval[6 - i]--;

            var tonicGrounded = lydianScaleRelativeInterval.Select(x => tonic.Incremented(x)).ToList();
            return new PitchClassScale(tonicGrounded);
        }

        public static bool operator ==(Scale? left, Scale? right) {
            return left is null ? right is null : left.Equals(right);
        }

        public static bool operator !=(Scale? left, Scale? right) {
            return !(left == right);
        }
    }
}