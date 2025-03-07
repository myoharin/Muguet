using System;
namespace SineVita.Muguet {
    public class UnitPitch {
        private Pitch _pitch;
        public Pitch Pitch { get {return _pitch;} }
        
        public UnitPitch(Pitch p) {
            _pitch = p;
        }
    }
}