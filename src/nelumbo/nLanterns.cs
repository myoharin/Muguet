using System;
using System.Numerics;
using System.Collections.Generic;
using SineVita.Lonicera;
using SineVita.Muguet.Petal.ScalerPetal;
namespace SineVita.Muguet.Nelumbo {
    public class Lantern {
        // * Variables
        private List<Lotus> _lotuses { get; set; }

        // * Derived Gets
        public IReadOnlyList<Lotus> Lotuses { get {return _lotuses;} }
        public IReadOnlyList<LotusDyad> PitchIntervals { get {return ToLonicera().Links;} }
        public IReadOnlyList<LotusDyad> LotusDuets { get {return ToLonicera().Links;} }
    
        // * Constructor
        public Lantern() {
            _lotuses = new List<Lotus>();
        }
        public Lantern(List<Lotus> lotuses, bool bloom = true) {
            _lotuses = lotuses;
            if (bloom) {Bloom();}
        }
        public Lantern(List<Pitch> pitches, bool bloom = true) {
            _lotuses = new List<Lotus>();
            foreach (var pitch in pitches) {
                _lotuses.Add(new Lotus(pitch));
            }
            if (bloom) {Bloom();}
        }

        // * Lonicera
        public Lonicera<Lotus, LotusDyad> ToLonicera(bool grow = true) {
            return new Lonicera<Lotus, LotusDyad>(_growthFunction, grow, _lotuses);
        }
        
        // * Lotus Property Blooms
        public void Bloom() { // ! NOT DONE
            // main function to update the individual properties of the lotuses

        }
        
        
        // * Statics
        private static Func<Lotus, Lotus, LotusDyad> _growthFunction = (pitch1, pitch2) => {
            return new LotusDyad(pitch1, pitch2);
        };

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
                midiQuantized.Add((int)MidiPitch.ToIndex(lotus.Pitch.Frequency));
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
    
    public class LanternThread { // Used to quantitate the transition between two chords
        // * Flames
        public List<List<LotusThread>> Flames { get; set; }

        // * Duet Properties

        // * Constructor
        public LanternThread(Lantern masterLantern, Lantern slaveLantern) {
            Flames = new List<List<LotusThread>>();
            for (int i = 0; i < slaveLantern.Lotuses.Count; i++) {
                Flames.Add(new List<LotusThread>());
                for (int j = 0; j < masterLantern.Lotuses.Count; j++) {
                    var masterLotus = masterLantern.Lotuses[j]; 
                    var slaveLotus = slaveLantern.Lotuses[i];
                    Flames[i].Add(new LotusThread(masterLotus, slaveLotus));
                }
            }
        }

    }
    public class LotusThread {
        public PitchInterval Interval { get; set; }
    
        // * Constructors

        public LotusThread(Lotus masterLotus, Lotus slaveLotus) { // ! NOT DONE
            Interval = PitchInterval.CreateInterval(masterLotus.Pitch, slaveLotus.Pitch, false);

        }
    }

    
}