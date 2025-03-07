using System;
namespace SineVita.Muguet { // ! NOTE DONE
    public class PitchClass {
        private Pitch _pitch;
        public Pitch Pitch { get {return _pitch;} }
        
        public PitchClass(Pitch p) {
            _pitch = p;
        }
    }
}