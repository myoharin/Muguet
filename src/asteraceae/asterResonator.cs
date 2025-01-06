using System.Collections.Generic;

namespace SineVita.Muguet.Asteraceae {
    public abstract class Resonator{
        public Resonator() {}
        public virtual void Process(double deltaTime) {}
        public virtual void AddPulse(Pulse newPulse) {}
        public virtual void DeletePulse(int pulseId) {}
        public virtual void MutatePulse(int oldId, Pulse newPulse) {}
        public virtual List<MagicalEffectData> GetMagicalEffects(byte intensityThreshold = 1) {return null;}
    }
}