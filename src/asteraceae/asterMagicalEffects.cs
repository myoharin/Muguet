// magic effects class parameters are used to balance mana intensity input with each other
// aka the same intensity in two different magical effect shoudl produce energy in simular quantity
// that does not mean the ability needs to be balanced, just that some thermodynamics is conserved

// when resonator inevitable applify the energy in without the appropriate lost, there would be a system to handle that.

// it is a backend class that helps bridge and connect to the Peppermint magic effect manifestation implementation.

namespace SineVita.Muguet.Asteraceae {
    public abstract class MagicalEffectData {
        public int MagicEffectID { get; set; }
        public byte Intensity { get; set; } 
        public MagicalEffectData(int magicalEffectID, byte intensity) {
            MagicEffectID = magicalEffectID; // -1 meanis its a null value
            Intensity = intensity;
        }
    }
}