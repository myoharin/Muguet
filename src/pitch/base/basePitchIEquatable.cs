namespace SineVita.Muguet {
    public abstract partial class Pitch : 
        IEquatable<Pitch> 
    {
        // * IEquatable
        public int CompareTo(object? obj) {
            if (obj == null) return 1; // Null is considered less than any object
            if (obj is Pitch otherPitch) {
                return this.Frequency.CompareTo(otherPitch.Frequency); // Compare by Frequency
            }
            else if (obj is IConvertible convertibleObj) {
                try {
                    double numericValue = convertibleObj.ToDouble(null);
                    return Frequency.CompareTo(numericValue); // Compare by numeric value
                } catch (InvalidCastException) {
                    throw new ArgumentException("Object cannot be converted to a numeric value");
                }
            }
            throw new ArgumentException("Object is not a Pitch");
        }
        public abstract object Clone();

        public override bool Equals(object? obj) { // ! NOT DONE - account for numerics comparison as well
            if (obj == null || GetType() != obj.GetType()) {return false;}
            Pitch other = (Pitch)obj;
            return Math.Abs(Frequency - other.Frequency) < 0.0001;
        }
        public bool Equals(Pitch? other) {
            if (other == null || GetType() != other.GetType()) {return false;}
            return Math.Abs(Frequency - other.Frequency) < 0.0001;
        }
        
        
        public override int GetHashCode() {
            return Frequency.GetHashCode();
        }
        public static bool operator ==(Pitch? left, Pitch? right) {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        public static bool operator !=(Pitch? left, Pitch? right) {
            return !(left == right);
        }
        public static bool operator <(Pitch? left, Pitch? right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(Pitch? left, Pitch? right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(Pitch? left, Pitch? right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(Pitch? left, Pitch? right) {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
        
        public static bool operator ==(double left, Pitch right) {
            return Math.Abs(left - right.Frequency) < 0.001;
        }
        public static bool operator !=(double left, Pitch right) {
            return !(left == right);
        }
        public static bool operator <(double left, Pitch right) {
            return left.CompareTo(right.Frequency) < 0;
        }
        public static bool operator <=(double left, Pitch right) {
            return left.CompareTo(right.Frequency) <= 0;
        }
        public static bool operator >(double left, Pitch right) {
            return left.CompareTo(right.Frequency) > 0;
        }
        public static bool operator >=(double left, Pitch right) {
            return left.CompareTo(right.Frequency) >= 0;
        }

        public static bool operator ==(Pitch left, double right) {
            return Math.Abs(left.Frequency - right) < 0.001;
        }
        public static bool operator !=(Pitch left, double right) {
            return !(left == right);
        }
        public static bool operator <(Pitch? left, double right) {
            return left is not null && left.Frequency.CompareTo(right) < 0;
        }
        public static bool operator <=(Pitch? left, double right) {
            return left is null || left.Frequency.CompareTo(right) <= 0;
        }
        public static bool operator >(Pitch? left, double right) {
            return left is not null && left.Frequency.CompareTo(right) > 0;
        }
        public static bool operator >=(Pitch? left, double right) {
            return left is not null ? left.Frequency.CompareTo(right) >= 0 : false ;
        }
        

            // arithmetic operations
        public static Pitch operator +(Pitch pitch, PitchInterval pitchInterval) {
            pitch.Increment(pitchInterval);
            return pitch;
        }
        public static Pitch operator +(PitchInterval pitchInterval, Pitch pitch) {
            pitch.Increment(pitchInterval);
            return pitch;
        }

        public static Pitch operator -(Pitch pitch, PitchInterval pitchInterval) {
            pitch.Decrement(pitchInterval);
            return pitch;
        }
        public static PitchInterval operator -(Pitch upperPitch, Pitch basePitch) {
            return PitchInterval.CreateInterval(basePitch, upperPitch, false, false);
        }
        public static PitchInterval operator /(Pitch upperPitch, Pitch basePitch) {
            return PitchInterval.CreateInterval(basePitch, upperPitch, false, false);
        }
    }
}