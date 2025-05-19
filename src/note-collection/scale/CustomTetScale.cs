namespace SineVita.Muguet
{
    public class CustomTetScale : Scale
        {
            // * Constructor
            public CustomTetScale(int baseTet, int tuningIndex, float tuningFrequency) {
                Base = baseTet;
                TuningIndex = tuningIndex;
                TuningFrequency = tuningFrequency;
            }
    
            // * Properties
            public int Base { get; init; }
            public int TuningIndex { get; init; }
            public float TuningFrequency { get; init; }
    
            public float ToIndex(double frequency, bool round = true) {
                if (round) return (float)Math.Round(Base * Math.Log2(frequency / TuningFrequency) + TuningIndex);
    
                return (float)(Base * Math.Log2(frequency / TuningFrequency) + TuningIndex);
            }
    
            public CustomTetPitch GetPitch(int index) {
                return new CustomTetPitch(Base, TuningIndex, TuningFrequency, index);
            }
    
            // * Overrides
            public override List<Pitch> MapToRange(Pitch rootPitch, Pitch topPitch) {
                var rootIndex = (int)ToIndex(rootPitch.Frequency) - 1;
                var topIndex = (int)ToIndex(topPitch.Frequency) + 1;
                List<Pitch> returnList = new();
                for (var i = rootIndex; i < topIndex + 1; i++) {
                    var newPitch = GetPitch(i);
                    if (newPitch >= rootPitch && newPitch <= topPitch) returnList.Add((Pitch)newPitch.Clone());
                }
    
                return returnList;
            }
    
            public override object Clone() {
                return new CustomTetScale(Base, TuningIndex, TuningFrequency);
            }
    
            public override int GetHashCode() {
                unchecked {
                    var hash = 17;
                    hash = hash * 41 + Base.GetHashCode();
                    hash = hash * 31 + TuningIndex.GetHashCode();
                    hash = hash * 47 + TuningFrequency.GetHashCode();
                    return hash;
                }
            }
    
            public override bool Equals(object? obj) {
                if (obj is CustomTetScale scale) return GetHashCode == scale.GetHashCode;
                return false;
            }
            public override bool Equals(Scale? obj) {
                if (obj is CustomTetScale scale) return GetHashCode == scale.GetHashCode;
                return false;
            }
        }
}
 