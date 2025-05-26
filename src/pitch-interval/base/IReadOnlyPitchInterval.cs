using System.Text.Json;
namespace SineVita.Muguet
{
    public interface IReadOnlyPitchInterval :
        IComparable,
        IComparable<IReadOnlyPitchInterval>
        // IEquatable<IReadOnlyPitchInterval>
    {
        public double FrequencyRatio { get; }
        public bool IsAbsolute { get; }

        public bool IsUnison { get; }

        public int ToMidiIndex { get; }
        public float ToMidiValue { get; }

        public string ToJson();
        public string ToJson(bool prettyPrint) {
            var json = this.ToJson();
            return prettyPrint
                ? JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json), 
                    new JsonSerializerOptions { WriteIndented = true })
                : JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json));
        }

        public PitchInterval Inverted();
        public PitchInterval Incremented(PitchInterval interval);
        public PitchInterval Decremented(PitchInterval interval);
        
        public PitchInterval ToPitchInterval();
        public PitchInterval ToInterval => ToPitchInterval();
        public IReadOnlyPitchInterval AsReadOnly() => this;
        
        // * Comparable
        public int CompareTo(PitchInterval? other) {
            return other == null
                ? 1
                : this.FrequencyRatio.CompareTo(other.FrequencyRatio);
        }
        public new int CompareTo(object? obj) {
            if (obj == null) return 1; // Null is considered less than any object
            if (obj is PitchInterval otherPitch) {
                return this.FrequencyRatio.CompareTo(otherPitch.FrequencyRatio); // Compare by Frequency
            }
            throw new ArgumentException("Object is not a Pitch");
        }
        public object Clone();
                      
        public bool Equals(object? obj) {
            if (obj == null || GetType() != obj.GetType()) return false;
            IReadOnlyPitchInterval other = (IReadOnlyPitchInterval)obj;
            return Math.Abs(FrequencyRatio - other.FrequencyRatio) < 0.0001;
        }
        
        public static bool operator <(IReadOnlyPitchInterval? left, IReadOnlyPitchInterval? right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(IReadOnlyPitchInterval? left, IReadOnlyPitchInterval? right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(IReadOnlyPitchInterval? left, IReadOnlyPitchInterval? right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(IReadOnlyPitchInterval? left, IReadOnlyPitchInterval? right) {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    
        public static bool operator <(double left, IReadOnlyPitchInterval? right) {
            return left.CompareTo(right) < 0;
        }
        public static bool operator <=(double left, IReadOnlyPitchInterval? right) {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >(double left, IReadOnlyPitchInterval? right) {
            return left.CompareTo(right) > 0;
        }
        public static bool operator >=(double left, IReadOnlyPitchInterval? right) {
            return left.CompareTo(right) >= 0;
        }
    
        public static bool operator <(IReadOnlyPitchInterval? left, double right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(IReadOnlyPitchInterval? left, double right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(IReadOnlyPitchInterval? left, double right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(IReadOnlyPitchInterval? left, double right) {
            return !(left is null) && left.CompareTo(right) >= 0;
        }
    
    }
}