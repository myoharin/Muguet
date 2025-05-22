namespace SineVita.Muguet {
    public abstract partial class PitchInterval {
        
        // * IEqualityComparerer

        public static int GetHashCode(PitchInterval interval) {
            return interval.GetHashCode();
        }
        public override int GetHashCode() {
            return FrequencyRatio.GetHashCode();
        }

        // * IEquatable
        public int CompareTo(PitchInterval? other) {
            return other == null
                ? 1
                : this.FrequencyRatio.CompareTo(other.FrequencyRatio);
        }
        public int CompareTo(object? obj) {
            if (obj == null) return 1; // Null is considered less than any object
            if (obj is PitchInterval otherPitch) {
                return this.FrequencyRatio.CompareTo(otherPitch.FrequencyRatio); // Compare by Frequency
            }
            throw new ArgumentException("Object is not a Pitch");
        }
        public abstract object Clone();
                      
        public override bool Equals(object? obj) {
            if (obj == null || GetType() != obj.GetType()) return false;
            PitchInterval other = (PitchInterval)obj;
            return Math.Abs(FrequencyRatio - other.FrequencyRatio) < 0.0001;
        }
        public bool Equals(PitchInterval? other) {
            if (other is null || GetType() != other.GetType()) {return false;}
            return Math.Abs(FrequencyRatio - other.FrequencyRatio) < 0.0001;
        }
        
        public static bool operator ==(PitchInterval? left, PitchInterval? right) {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        public static bool operator !=(PitchInterval? left, PitchInterval? right) {
            return !(left == right);
        }
        public static bool operator <(PitchInterval? left, PitchInterval? right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(PitchInterval? left, PitchInterval? right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(PitchInterval? left, PitchInterval? right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(PitchInterval? left, PitchInterval? right) {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    
        public static bool operator ==(double left, PitchInterval? right) {
            return right is not null && Math.Abs(left - right.FrequencyRatio) < 0.001;
        }
        public static bool operator !=(double left, PitchInterval? right) {
            return !(left == right);
        }
        public static bool operator <(double left, PitchInterval? right) {
            return left.CompareTo(right) < 0;
        }
        public static bool operator <=(double left, PitchInterval? right) {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >(double left, PitchInterval? right) {
            return left.CompareTo(right) > 0;
        }
        public static bool operator >=(double left, PitchInterval? right) {
            return left.CompareTo(right) >= 0;
        }
    
        public static bool operator ==(PitchInterval? left, double right) {
            return left is not null && Math.Abs(left.FrequencyRatio - right) < 0.001;
        }
        public static bool operator !=(PitchInterval? left, double right) {
            return !(left == right);
        }
        public static bool operator <(PitchInterval? left, double right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(PitchInterval? left, double right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(PitchInterval? left, double right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(PitchInterval? left, double right) {
            return !(left is null) && left.CompareTo(right) >= 0;
        }
    
    }
}