using System;
namespace SineVita.Muguet { // ! NOTE DONE
    public class PitchClass {
        public Pitch ReferencePitch { get; set; }
        
        public PitchClass(Pitch referencePitch) {
            ReferencePitch = referencePitch;
        }


        // * Reduction and Transformation
        public Pitch OctaveReduced(Pitch octaveMarker, bool markerIsRoot = true) {
            octaveMarker = markerIsRoot ? octaveMarker : octaveMarker.DecrementPitch(PitchInterval.Octave);
            var newPitch = ReferencePitch;
            if (newPitch < octaveMarker) {
                while (octaveMarker > newPitch) {
                    newPitch = newPitch.IncrementPitch(PitchInterval.Octave);  
                }
                return newPitch;
            }
            else if (newPitch >= octaveMarker.IncrementPitch(PitchInterval.Octave)) {
                while (newPitch >= octaveMarker.IncrementPitch(PitchInterval.Octave)) {
                    newPitch = newPitch.DecrementPitch(PitchInterval.Octave);
                }
                return newPitch;
            }
            return newPitch;
        }

        // * Comparators
        public bool IsEqual(PitchClass other) {
            return ReferencePitch.Equals(
                other.OctaveReduced(
                    ReferencePitch.DecrementPitch(
                        PitchInterval.Perfect5th
            )));
        }
        public bool IsEqual(Pitch other) {
            return other.Equals(
                this.OctaveReduced(
                    other.DecrementPitch(
                        PitchInterval.Perfect5th
            )));
        }
    }
}