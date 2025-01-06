// magic effects class parameters are used to balance mana intensity input with each other
// aka the same intensity in two different magical effect shoudl produce energy in simular quantity
// that does not mean the ability needs to be balanced, just that some thermodynamics is conserved

// when resonator inevitable applify the energy in without the appropriate lost, there would be a system to handle that.

namespace SineVita.Muguet.Asteraceae.Cosmosia {
    // Base Classes
    public class MagicalEffectDataCosmosia : MagicalEffectData { // having a base is better than nothing
        public MagicalEffectDataCosmosia(int magicalEffectID, byte intensity)
            : base(magicalEffectID, intensity) {}
    }
    public class N2RMagicalEffectDataCosmosia : MagicalEffectDataCosmosia {
        // constants
        public int Degree { get; set; }
        public N2RMagicalEffectDataCosmosia(int magicalEffectID, byte intensity, int degree)
            : base(magicalEffectID, intensity) {
                Degree = degree;
        }
    }
    public class N2NMagicalEffectDataCosmosia : MagicalEffectDataCosmosia {
        // constants
        public int Grade { get; set; }
        public bool Type { get; set; }
        public N2NMagicalEffectDataCosmosia(int magicalEffectID, byte intensity, int grade, bool type)
            : base(magicalEffectID, intensity) {
                Grade = grade;
                Type = type; // Type for midi 0, 5, 6, 7 are always false, as they are always perfect
        }
    }

    public class NullMagicalEffectDataCosmosia : MagicalEffectDataCosmosia {
        public NullMagicalEffectDataCosmosia()
            : base(255, 0) {
        }
    }

    // actual different magical effects: 

}