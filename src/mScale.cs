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

    public abstract class Scale {
        // * Transformation
        public List<Pitch> MapToRange(Pitch referencePitch, PitchInterval range, bool referencePitchIsRoot = true) {
            var otherPitch = referencePitchIsRoot ? 
                referencePitch.IncrementPitch(range) : referencePitch.DecrementPitch(range);
            var rootPitch = referencePitchIsRoot ? referencePitch : otherPitch;
            var topPitch = !referencePitchIsRoot ? referencePitch : otherPitch;
            return MapToRange(rootPitch, topPitch);
        }

        // * Abstracts Methods
        public abstract List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch);

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
        
        public static PitchClassScale MidiChromaticScale { get { // ! NOT DONE
            return null;
        } }        
    }

    public class PitchClassScale : Scale { // ! NOTE DONE
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
        public override List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch) { // ! NOT DONE
            throw new NotImplementedException();
        }
        

    }

    public class ChordReferencedScale : Scale { // ! NOT DONE
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
                            note.DecrementPitch(_repetitionInterval);
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
        public ChordReferencedScale(Chord chord, PitchInterval repetitionInterval) {
            RepetitionInterval = repetitionInterval;
            ReferenceChord = chord;
            if (_referenceChord == null) {_referenceChord = Chord.Empty();}
            if (_repetitionInterval == null) {_repetitionInterval = PitchInterval.Octave;}
        }
        public ChordReferencedScale(List<Pitch> notes, PitchInterval repetitionInterval)
            : this(new Chord(notes), repetitionInterval) {}
    

        // * Override
        public override List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch) { // ! NOT DONE
            throw new NotImplementedException();
        }
    }
}