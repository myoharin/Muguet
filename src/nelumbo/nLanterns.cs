using System;
using SineVita.Lonicera;
using SineVita.Muguet.Petal.ScalerPetal;
namespace SineVita.Muguet.Nelumbo {
    public class Lantern {
        // * Variables
        private List<Lotus> _lotuses { get; set; }

        // * Derived Gets
        public IReadOnlyList<Lotus> Lotuses { get {return _lotuses;} }
        public IReadOnlyList<LotusDuet> PitchIntervals { get {return ToLonicera().Links;} }
        public IReadOnlyList<LotusDuet> LotusDuets { get {return ToLonicera().Links;} }
    
        // * Constructor
        public Lantern() {
            _lotuses = new List<Lotus>();
        }
        public Lantern(List<Lotus> lotuses, bool bloom = true) {
            _lotuses = lotuses;
        }
        public Lantern(List<Pitch> pitches, bool bloom = true) {
            _lotuses = new List<Lotus>();
            foreach (var pitch in pitches) {
                _lotuses.Add(new Lotus(pitch));
            }
        }

        // * Lonicera
        public Lonicera<Lotus, LotusDuet> ToLonicera(bool grow = true) {
            return new Lonicera<Lotus, LotusDuet>(_growthFunction, true, _lotuses);
        }
        public void Bloom() { // ! NOT DONE
            // main function to update the individual properties of the lotuses

        }
        
        
        // * Statics
        private static Func<Lotus, Lotus, LotusDuet> _growthFunction = (pitch1, pitch2) => {
            return new LotusDuet(pitch1, pitch2);
        };
    

        // // * Lonicera Organisers
        // public bool Add(Pitch pitch) {
        //     var lotus = new Lotus(pitch);
        //     int index = 0;
        //     while (pitch.Frequency > _lonicera.Nodes[index].Pitch.Frequency && index < _lonicera.Nodes.Count) {
        //         index++;
        //     }
        //     if (index == _lonicera.Nodes.Count) {
        //         _lonicera.Add(lotus);
        //     } else {
        //         _lonicera.Insert(index, lotus, true);
        //     }
        //     return true;
        // }
        // public bool Add(double frequency) {
        //     return Add(new Pitch((float)frequency));
        // }
        // public bool Remove(Pitch pitch, int centTolerance = 20) {
        //     return Remove(pitch.Frequency, centTolerance);
        // }
        // public bool Remove(double frequency, int centTolerance = 20) { // remove lowest which matches it
        //     for (int i = 0; i < _lonicera.Nodes.Count; i++) {
        //         if (Math.Abs(_lonicera.Nodes[i].Pitch.Frequency - frequency) < centTolerance) {
        //             _lonicera.RemoveAt(i);
        //             return true;
        //         }
        //     }   
        //     return false;
        // }

        // * Lantern Properties
        public List<MidiPitchName> GetDiatonicScales(ScaleType type = ScaleType.Ionian) {
            if (!Scale.DiatonicScales.Contains(type)) {throw new NotImplementedException();}

            int rootOffSet = type switch {
                ScaleType.Lydian => 0,
                ScaleType.Ionian => 7,
                ScaleType.Mixolydian => 2,
                ScaleType.Dorian => 9,
                ScaleType.Aeolian => 4,
                ScaleType.Phrygian => 11,
                ScaleType.Locrian => 6,
                _ => 0
            };
            
            var midiQuantized =  new List<int>();
            var midiFifthCircle =  new List<int>();
            foreach (var lotus in Lotuses) {
                midiQuantized.Add((int)MidiPitch.ToPitchIndex(lotus.Pitch.Frequency));
            }
            midiQuantized.Sort();
            for (int i = 0; i < midiQuantized.Count; i++) {
                var circleValue = midiQuantized[i] % 12;
                if (circleValue < 0) {circleValue += 12;}
                if (circleValue % 2 == 1) {circleValue = (circleValue + 6) % 12;}
                midiFifthCircle.Add(circleValue);
            }
            midiFifthCircle.Sort();
            int range = midiFifthCircle[midiFifthCircle.Count - 1] - midiFifthCircle[0];
            // very much limited to diaontic scales with C = 256 as root

            // everything organised into a circle of fifth where C = 0
            var returnList = new List<MidiPitchName>();
            if (range <= 6) {
                for (int i = 0; i < 7 - range; i++) {
                    returnList.Add((MidiPitchName)((48-i+rootOffSet) % 12));
                }
            }

            return returnList;
        }
        

    }
    public class LanternDuet { // * Used to quantitate the transition between two chords
        public bool LeadingToneTension { get; set; }

        public LanternDuet(Lantern lantern1, Lantern lantern2) {
            for (int i = 0; i < lantern1.Lotuses.Count; i++) {
                for (int j = 0; j < lantern2.Lotuses.Count; j++) {
                   var lotus1 = lantern1.Lotuses[i]; 
                   var lotus2 = lantern2.Lotuses[i];




                }
            }

        }
    }
    
}