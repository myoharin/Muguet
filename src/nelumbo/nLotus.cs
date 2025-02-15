using Microsoft.VisualBasic;

namespace SineVita.Muguet.Nelumbo {
    public class Lotus {
        // * Variables
        public Pitch Pitch { get; set; }

        // * lotus Properties
        public bool IsRoot { get; set; } // Root of lantern

        public bool IsStructualTonic { get; set; } // Root of a fifth relation / top of a fourth relation
        public bool IsStructualMediant { get; set; } // the minor or major third of a structual chord
        public bool IsStructualDominant { get; set; } // the minor or major third of a structual chord
        
        public bool IsStressTone { get; set;} // tension note, involved in 3rd or 6th relation
        public bool IsInStressChain { get; set; } // involved in a chain of chaining stress tones
        
        public bool IsDeflatedTone { get; set;} // 2nd relations
        public bool IsAlchemicTone { get; set;} // tritone or 7ths

        // * Constructors
        public Lotus(Pitch pitch) {
            Pitch = pitch;
        }
    }
    
    
    
    public class LotusDuet {
        // * Variables
        public PitchInterval Interval { get; set; }

        // * Derived Gets
        public bool IsFifth { get {
            return Interval.ToMidiIndex % 12 == 7;
        } }
        public bool IsFourth { get {
            return Interval.ToMidiIndex % 12 == 5;
        } }
        public bool IsOctave { get {
            return Interval.ToMidiIndex % 12 == 0 && Interval.ToMidiIndex != 0;
        } }
        public bool IsUnison { get {
            return Interval.ToMidiIndex == 0;
        } }
        
        // * Contructors
        public LotusDuet(PitchInterval interval) {
            Interval = interval;
        }
        public LotusDuet(Pitch pitch1, Pitch pitch2) {
            Interval = new PitchInterval(pitch1, pitch2);
        }
        public LotusDuet(Lotus lotus1, Lotus lotus2) {
            Interval = new PitchInterval(lotus1.Pitch, lotus2.Pitch);
        }
    }
}