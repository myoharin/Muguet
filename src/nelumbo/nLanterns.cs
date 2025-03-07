using System;
using System.Numerics;
using System.Collections.Generic;
using SineVita.Lonicera;
using SineVita.Muguet.Petal.ScalerPetal;
using System.Security.Cryptography.X509Certificates;
namespace SineVita.Muguet.Nelumbo {
    public class Lantern {
        // * Variables
        private Lonicera<Lotus,LotusDyad> _lonicera { get; set; }

        // * Derived Gets
        public IReadOnlyList<Lotus> Lotuses { get {return _lonicera.Nodes;} }
        public IReadOnlyList<LotusDyad> LotusDyads { get {return _lonicera.Links;} }
        public LotusDyad GetDyad(int index1, int index2) {
            return _lonicera.GetValue(index1, index2);
        }
    
        // * Constructor
        public Lantern() {
            _lonicera = new Lonicera<Lotus, LotusDyad>(_growthFunction, true);
        }
        public Lantern(List<Lotus> lotuses, bool bloom = true) {
            _lonicera = new Lonicera<Lotus, LotusDyad>(_growthFunction, true);
            foreach (var lotus in lotuses) {
                Add(lotus.Pitch, replantAll: false, bloom: false);
            }
            if (bloom) {Bloom();}
            
        }
        public Lantern(List<Pitch> pitches, bool bloom = true) {
            _lonicera = new Lonicera<Lotus, LotusDyad>(_growthFunction, true);
            foreach (var pitch in pitches) {
                Add(pitch, replantAll: false, bloom: false);
            }
            if (bloom) {Bloom();}
        }

        // * Lonicera Add Remove Delete
        public void Remove(Lotus lotus) {
            _lonicera.Remove(lotus);
        }
        public void RemoveAt(int i) {
            _lonicera.RemoveAt(i);
        }
        public void Add(Pitch pitch, bool replantAll = true, bool bloom = true) {
            // * Mutation Func
            Lotus replant(Lotus l) {
                l.Replant(l.IsRoot);
                return l;
            }
            Lotus uproot(Lotus l) {
                l.IsRoot = false;
                return l;
            }

            // * find index to insert.
            int index = _lonicera.NodeCount;
            for (int i = 0; i < _lonicera.NodeCount; i++) {
                if (_lonicera.Nodes[i].Pitch > pitch) {
                    index = i;
                }
            }

            // * lonicera.Insert
            if (index == 0) {
                _lonicera.MutateNode(0, uproot);          
            }
            _lonicera.Insert(index, new Lotus(pitch, index == 0));
            
            // * Parameter Settings
            if (replantAll) {
               for (int i = 0 ; i < _lonicera.NodeCount ; i++) { // replant all
                _lonicera.MutateNode(i, replant);
                } 
            }
            if (bloom) {Bloom();}
        }

        // * Bloom
        public void Bloom() {
            _lonicera.Grow(); // all dyads generated

            // * Working Captial
            int targetIndex;
            List<Lotus> otherLotuses;
            List<LotusDyad> otherDyads;
            Lotus FertilizeWithBuds(Lotus l) {
                l.FertilizeWithBuddingLantern(this, targetIndex);
                return l;
            }
            Lotus FertilizeWithFlowers(Lotus l) {
                l.FertilizeWithFloweringLantern(this, targetIndex);
                return l;
            }
            Lotus FertilizeWithFruits(Lotus l) {
                l.FertilizeWithFruitingLantern(this, targetIndex);
                return l;
            }

            void CrossBudFertilization(int i) {
                targetIndex = i;
                _lonicera.MutateNode(i, FertilizeWithBuds);
            }
            void CrossFlowerFertilization(int i) {
                targetIndex = i;
                _lonicera.MutateNode(i, FertilizeWithFlowers);
            }

            // * Fertilize with Buds to flower
            for (int i = 0; i < _lonicera.NodeCount; i++) {CrossBudFertilization(i);}
            _lonicera.Grow();
            // * Fertilize with Flowers to Fruits
            for (int i = 0; i < _lonicera.NodeCount; i++) {CrossFlowerFertilization(i);}
            _lonicera.Grow();

        }




        
        
        
        // * Statics
        private static Func<Lotus, Lotus, LotusDyad> _growthFunction = (pitch1, pitch2) => {
            return new LotusDyad(pitch1, pitch2);
        };

        // * Lantern Properties
        public List<Pitch> GetStructualTonics() {
            var returnList = new List<Pitch>();
            foreach (var lotus in _lonicera.Nodes) {
                if (lotus.IsStructualTonic) {
                    returnList.Add(lotus.Pitch);
                }
            }
            return returnList;
        }
        public List<Pitch> GetStructualMediants() {
            var returnList = new List<Pitch>();
            foreach (var lotus in _lonicera.Nodes) {
                if (lotus.IsStructualMediant) {
                    returnList.Add(lotus.Pitch);
                }
            }
            return returnList;
        }
        public List<Pitch> GetStructualDominants() {
            var returnList = new List<Pitch>();
            foreach (var lotus in _lonicera.Nodes) {
                if (lotus.IsStructualDominant) {
                    returnList.Add(lotus.Pitch);
                }
            }
            return returnList;
        }

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