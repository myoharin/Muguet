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

    public abstract class Scale : ICloneable {
        // * Transformation
        public List<Pitch> MapToRange(Pitch referencePitch, PitchInterval range, bool referencePitchIsRoot = true) {
            var otherPitch = referencePitchIsRoot ? 
                referencePitch.Incremented(range) : referencePitch.Decremented(range);
            var rootPitch = referencePitchIsRoot ? referencePitch : otherPitch;
            var topPitch = !referencePitchIsRoot ? referencePitch : otherPitch;
            return MapToRange(rootPitch, topPitch);
        }

        // * Abstracts Methods
        public abstract List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch); // inclusive of range
        public abstract object Clone();

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
        
        public static PitchClassScale ChromaticScale(int initialMidiIndex = 69) {
            List<PitchClass> list = new();
            for (int i = 0; i < 12; i++) {
                list.Add(new PitchClass(new MidiPitch(initialMidiIndex+i))); // starting from certain pitch
            }
            return new PitchClassScale(list);
        }
        public static PitchClassScale ChromaticScale(MidiPitchName midiPitchName) {
            return ChromaticScale((int)midiPitchName + 60);
        }
        

        public static PitchClassScale DiatonicScaleTwelveTet(ScaleType type, MidiPitchName midiPitchName) {
            return DiatonicScaleTwelveTet(type, new MidiPitch(60+(int)midiPitchName));
        }
        public static PitchClassScale DiatonicScaleTwelveTet(ScaleType type, Pitch tonic) {
            
            List<MidiPitchInterval> lydianScaleRelativeInterval = new() {
                new MidiPitchInterval(0),
                new MidiPitchInterval(7),
                new MidiPitchInterval(2),
                new MidiPitchInterval(9),
                new MidiPitchInterval(4),
                new MidiPitchInterval(11),
                new MidiPitchInterval(6),
            };
            int flatCount = 0;
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

            for (int i = 0; i<flatCount ; i++) {
                lydianScaleRelativeInterval[6-i].Down();
            }
            
            List<Pitch> tonicGrounded = lydianScaleRelativeInterval.Select(x => tonic.Incremented(x)).ToList<Pitch>();
            return new PitchClassScale(tonicGrounded);
        }
       
    
    
    
    }

    public class PitchClassScale : Scale {
        // has all the diatonic scales
        // all within 1 octave 
        
        private List<PitchClass> _pitchClasses;
        public List<PitchClass> PitchClasses {
            get {
                return _pitchClasses;
            }
            set {
                SetPitchClasses(value, 0);
            }
        }

        // * Setter
        public void SetPitchClasses(List<PitchClass> pitchClasses, int rootIndex) {
            var rootReduced = pitchClasses[0].OctaveReduced(Pitch.Empty, true);
            _pitchClasses = pitchClasses
                .OrderBy(pitchClass => pitchClass.OctaveReduced(rootReduced, true))
                .ToList();
        }

        // * Constructor
        public PitchClassScale(List<PitchClass>? pitchClasses = null) {
            _pitchClasses = pitchClasses ?? new();
        }
        public PitchClassScale(List<Pitch> pitches) {
            _pitchClasses = pitches.Select(x => new PitchClass(x)).ToList();
            
        }

        // * Transformation
        public bool Contains(Pitch pitch) {
            foreach (var pitchClass in _pitchClasses) {
                if (pitchClass.Equals(pitch)) {
                    return true;
                }
            }
            return false;
        }
        public bool Contains(PitchClass pitchClass) {
            foreach (var pitchClassI in _pitchClasses) {
                if (pitchClassI.Equals(pitchClass)) {
                    return true;
                }
            }
            return false;
        }

        // * Override
        public override List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch) {
            if (PitchClasses.Count == 0) { // Empty
                return new();
            }

            // get base list
            List<Pitch> rootList = new();
            foreach(PitchClass p in PitchClasses) {
                rootList.Add(p.OctaveReduced(rootPitch, true));
            }
            rootList.Sort();

            // assemble return list
            List<Pitch> returnList = new();
            int i = 0;
            Pitch evaluatedPitch;
            do {
                evaluatedPitch = (Pitch)rootList[i % rootList.Count].Clone();
                for (int _ = 0; _ < Math.Floor((float)i / (float)rootList.Count); _++) {
                    evaluatedPitch.Increment(PitchInterval.Octave);
                }

                if (evaluatedPitch < topPitch) {
                    returnList.Add(evaluatedPitch);
                }

            }
            while (evaluatedPitch < topPitch);

            return returnList;
        }
        public override object Clone() {
            return new PitchClassScale(PitchClasses);
        }
    }

    public class ChordReferencedScale : Scale {
        private Chord _referenceChord;
        public Chord ReferenceChord {
            get { return _referenceChord; }
            set {
                if (value.Notes.Count == 0) {_referenceChord = value;}
                if (value.Range >= _repetitionInterval) {
                    var valueCloned = (Chord)value.Clone();
                    var noteList = new List<Pitch>();
                    var repitionMarker = valueCloned.Root.Incremented(RepetitionInterval);
                    foreach(var note in valueCloned.Notes) {
                        while (note > repitionMarker) {
                            note.Decrement(_repetitionInterval);
                        }
                        noteList.Add(note);
                    }
                    _referenceChord = new Chord(noteList); // * Autosorted
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
                var valueCloned = (PitchInterval)value.Clone();
                if (valueCloned.IsNegative) {valueCloned.Invert();}
                _repetitionInterval = valueCloned;
                ReferenceChord = _referenceChord; // re-set this to trigger range check
            }
        }

        // * Constructor
        public ChordReferencedScale(Chord chord, PitchInterval repetitionInterval) {
            RepetitionInterval = repetitionInterval;
            ReferenceChord = chord;
            if (_referenceChord == null) {_referenceChord = Chord.Empty();}
            if (_repetitionInterval == null) {_repetitionInterval = PitchInterval.Octave;}
        }
        public ChordReferencedScale(List<Pitch> notes, PitchInterval repetitionInterval)
            : this(new Chord(notes), repetitionInterval) {}
    

        // * Override
        public override List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch) {
            if (ReferenceChord.Notes.Count == 0) { // Empty
                return new();
            }

            // get base list
            List<Pitch> rootList = new();
            foreach(Pitch p in ReferenceChord.Notes) {
                rootList.Add(new PitchClass(p).Reduced(rootPitch, RepetitionInterval, true));
            }
            rootList.Sort();

            // assemble return list
            List<Pitch> returnList = new();
            int i = 0;
            Pitch evaluatedPitch;
            do {
                evaluatedPitch = (Pitch)rootList[i % rootList.Count].Clone();
                for (int _ = 0; _ < Math.Floor((float)i / (float)rootList.Count); _++) {
                    evaluatedPitch.Increment(RepetitionInterval);
                }

                if (evaluatedPitch < topPitch) {
                    returnList.Add(evaluatedPitch);
                }

            }
            while (evaluatedPitch < topPitch);

            return returnList;
        }
        public override object Clone() {
            return new ChordReferencedScale(_referenceChord, RepetitionInterval);
        }
    }

    public class CustomTetScale : Scale {
        // * Properties
        public int Base { get; set; }
        public int TuningIndex { get; set; }
        public float TuningFrequency { get; set; }

        // * Constructor
        public CustomTetScale(int baseTET, int tuningIndex, float tuningFrequency) {
            Base = baseTET;
            TuningIndex = tuningIndex;
            TuningFrequency = tuningFrequency;
        }

        public float ToIndex(double frequency, bool round = true) {
            if (round) {return (float)Math.Round(Base * Math.Log2(frequency / TuningFrequency) + TuningIndex);}
            else {return (float)(Base * Math.Log2(frequency / TuningFrequency) + TuningIndex);}
        }
        public CustomTetPitch GetPitch(int index) {
            return new CustomTetPitch(Base, TuningIndex, TuningFrequency, index);
        }

        // * Overrides
        public override List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch) {
            int rootIndex = (int)ToIndex(rootPitch.Frequency, true) - 1;
            int topIndex = (int)ToIndex(topPitch.Frequency, true) + 1;
            List<Pitch> returnList = new();
            for (int i = rootIndex; i < topIndex + 1; i++) {
                var newPitch = GetPitch(i);
                if (newPitch >= rootPitch && newPitch <= topPitch) {
                    returnList.Add((Pitch)newPitch.Clone());
                }
            }
            return returnList;
        }
        public override object Clone() {
            return new CustomTetScale(Base, TuningIndex, TuningFrequency);
        }
        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 41 + Base.GetHashCode();
                hash = hash * 31 + TuningIndex.GetHashCode();
                hash = hash * 47 + TuningFrequency.GetHashCode();
                return hash;
            }
        }
    }

}