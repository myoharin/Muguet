using System;
namespace SineVita.Muguet {
    public class PitchClass : ICloneable { // Assumes all octave equivalency
        public Pitch ReferencePitch { get; set; }
        
        public PitchClass(Pitch referencePitch) {
            ReferencePitch = referencePitch;
        }


        // * Reduction and Transformation
        public Pitch OctaveReduced(Pitch octaveMarker, bool markerIsRoot = true) {
            return Reduced(octaveMarker, PitchInterval.Octave, markerIsRoot);
        }

        public Pitch Reduced(Pitch octaveMarker, PitchInterval reductionInterval, bool markerIsRoot = true) {
            octaveMarker = markerIsRoot ? octaveMarker : octaveMarker.Decremented(reductionInterval);
            var newPitch = (Pitch)ReferencePitch.Clone();
            if (newPitch < octaveMarker) {
                while (octaveMarker > newPitch) {
                    newPitch.Increment(reductionInterval);  
                }
                return newPitch;
            }
            else if (newPitch >= octaveMarker.Incremented(reductionInterval)) {
                while (newPitch >= octaveMarker.Incremented(reductionInterval)) {
                    newPitch.Decrement(reductionInterval);
                }
                return newPitch;
            }
            return newPitch;
        }

        // * Comparators
        public bool IsEqual(PitchClass other) {
            return ReferencePitch.Equals(
                other.OctaveReduced(
                    ReferencePitch.Decremented(
                        PitchInterval.Perfect5th
            )));
        }
        public bool IsEqual(Pitch other) {
            return other.Equals(
                this.OctaveReduced(
                    other.Decremented(
                        PitchInterval.Perfect5th
            )));
        }
    
        // * Overrides
        public object Clone() {
            return new PitchClass(ReferencePitch);
        }

    }
}