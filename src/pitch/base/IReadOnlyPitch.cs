namespace SineVita.Muguet
{
    public interface IReadOnlyPitch :
        IComparable,
        IComparable<IReadOnlyPitch>,
        IEquatable<IReadOnlyPitch>
    {
        public double Frequency { get; }
        public string NoteName { get; }
        public int ToMidiIndex { get; }
        public float ToMidiValue { get; }
        
        public Pitch Incremented(PitchInterval i);
        public Pitch Decremented(PitchInterval i);

        public Pitch ToPitch();
        public IReadOnlyPitch AsReadOnly() => this;
        
        int IComparable<IReadOnlyPitch>.CompareTo(IReadOnlyPitch? i) => Frequency.CompareTo(i?.Frequency ?? 0);
        bool System.IEquatable<IReadOnlyPitch>.Equals(IReadOnlyPitch? i) => Frequency.Equals(i?.Frequency ?? 0);

        public static bool operator <(IReadOnlyPitch? left, IReadOnlyPitch? right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(IReadOnlyPitch? left, IReadOnlyPitch? right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(IReadOnlyPitch? left, IReadOnlyPitch? right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(IReadOnlyPitch? left, IReadOnlyPitch? right) {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    
        public static bool operator <(double left, IReadOnlyPitch? right) {
            return left.CompareTo(right) < 0;
        }
        public static bool operator <=(double left, IReadOnlyPitch? right) {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >(double left, IReadOnlyPitch? right) {
            return left.CompareTo(right) > 0;
        }
        public static bool operator >=(double left, IReadOnlyPitch? right) {
            return left.CompareTo(right) >= 0;
        }
    
        public static bool operator <(IReadOnlyPitch? left, double right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(IReadOnlyPitch? left, double right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(IReadOnlyPitch? left, double right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(IReadOnlyPitch? left, double right) {
            return !(left is null) && left.CompareTo(right) >= 0;
        }

    }
}