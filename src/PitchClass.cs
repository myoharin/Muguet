using System;
namespace SineVita.Muguet {
    public class PitchClass : // Assumes all octave equivalency
        ICloneable,
        IEquatable<PitchClass>,
        IEquatable<Pitch>
    {
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
        public bool Equals(PitchClass? other) {
            if (other == null) {return false;}
            return ReferencePitch.Equals(
                other.OctaveReduced(
                    ReferencePitch.Decremented(
                        PitchInterval.Perfect5th
            )));
        }
        public bool Equals(Pitch? other) {
            if (other == null) {return false;}
            return other.Equals(
                this.OctaveReduced(
                    other.Decremented(
                        PitchInterval.Perfect5th
            )));
        }
    
        // * Overrides
        public object Clone() {
            return new PitchClass((Pitch)ReferencePitch.Clone());
        }


    }
}