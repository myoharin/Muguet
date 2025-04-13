using System;
namespace SineVita.Muguet {
    public class PitchClass {
        public Pitch ReferencePitch { get; set; }
        
        public PitchClass(Pitch referencePitch) {
            ReferencePitch = referencePitch;
        }


        // * Reduction and Transformation
        public Pitch OctaveReduced(Pitch octaveMarker, bool markerIsRoot = true) {
            return Reduced(octaveMarker, PitchInterval.Octave, markerIsRoot);
        }

        public Pitch Reduced(Pitch octaveMarker, PitchInterval reductionInterval, bool markerIsRoot = true) {
            octaveMarker = markerIsRoot ? octaveMarker : octaveMarker.DecrementPitch(reductionInterval);
            var newPitch = ReferencePitch;
            if (newPitch < octaveMarker) {
                while (octaveMarker > newPitch) {
                    newPitch = newPitch.IncrementPitch(reductionInterval);  
                }
                return newPitch;
            }
            else if (newPitch >= octaveMarker.IncrementPitch(reductionInterval)) {
                while (newPitch >= octaveMarker.IncrementPitch(reductionInterval)) {
                    newPitch = newPitch.DecrementPitch(reductionInterval);
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