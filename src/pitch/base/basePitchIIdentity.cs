using System.Numerics;

namespace SineVita.Muguet {
    public abstract partial class Pitch :
        IMultiplicativeIdentity<Pitch, PitchInterval>,
        IAdditiveIdentity<Pitch, PitchInterval>
    {
        public static PitchInterval AdditiveIdentity => PitchInterval.Default;
        public static PitchInterval MultiplicativeIdentity => PitchInterval.Default;
    }
}