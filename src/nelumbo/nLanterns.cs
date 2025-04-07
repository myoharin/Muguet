using System;
using System.Numerics;
using System.Collections.Generic;
using Caprifolium;
using SineVita.Muguet;
using System.Security.Cryptography.X509Certificates;
namespace SineVita.Muguet.Nelumbo {
    public class Lantern {

        // ! NEED TO ISOLATE SELF NODE LIST FROM LONICERA AND MIGRATE, MAYBE ENTIRELY TO KNAUTIA

        // * Variables
        private Lonicera<Lotus,LotusDyad> _lonicera { get; set; }
        private Knautia<Lotus, LotusTriad> _knautia { get; set; }

        // * Derived Gets
        public IReadOnlyList<Lotus> Lotuses { get {return _lonicera.Nodes;} }
        public IReadOnlyList<LotusDyad> LotusDyads { get {return _lonicera.Links;} }
        public LotusDyad GetDyad(int index1, int index2) {
            return _lonicera.GetValue(index1, index2);
        }
        public LotusTriad? GetTriad(int index1, int index2, int index3) {
            return _knautia.GetLink(new List<int>(){index1, index2, index3});
        }
    
        // * Statics
        private static Func<Lotus, Lotus, LotusDyad> _loniceraGrowthFunction = (pitch1, pitch2) => {
            return new LotusDyad(pitch1, pitch2);
        };
        private static Func<Lotus[], LotusTriad> _knautiaGrowthFunction = (lotuses) => {
            return new LotusTriad(lotuses[0], lotuses[1], lotuses[2]);
        };

        // * Constructor
        public Lantern() {
            _lonicera = new Lonicera<Lotus, LotusDyad>(_loniceraGrowthFunction, true);
            _knautia = new Knautia<Lotus, LotusTriad>(3, _knautiaGrowthFunction);
        }
        public Lantern(List<Lotus> lotuses, bool bloom = true) {
            _lonicera = new Lonicera<Lotus, LotusDyad>(_loniceraGrowthFunction, true);
            _knautia = new Knautia<Lotus, LotusTriad>(3, _knautiaGrowthFunction);
            foreach (var lotus in lotuses) {
                Add(lotus.Pitch, replantAll: false, bloom: false);
            }
            if (bloom) {Bloom();}
            
        }
        public Lantern(List<Pitch> pitches, bool bloom = true) {
            _lonicera = new Lonicera<Lotus, LotusDyad>(_loniceraGrowthFunction, true);
            _knautia = new Knautia<Lotus, LotusTriad>(3, _knautiaGrowthFunction);
            foreach (var pitch in pitches) {
                Add(pitch, replantAll: false, bloom: false);
            }
            if (bloom) {Bloom();}
        }

        // * Caprifolium Add Remove Delete
        public void Remove(Lotus lotus) {
            _lonicera.Remove(lotus);
            _knautia.Remove(lotus);
        }
        public void RemoveAt(int i) {
            _lonicera.RemoveAt(i);
            _knautia.RemoveAt(i);
        }
        public void Add(Pitch pitch, bool replantAll = true, bool bloom = true) {
            // * find index to insert.
            int index = _lonicera.NodeCount;
            for (int i = 0; i < _lonicera.NodeCount; i++) {
                if (_lonicera.Nodes[i].Pitch > pitch) {
                    index = i;
                }
            }

            // * Caprifolium.Insert
            if (index == 0) {
                var newLotus = _lonicera[0];
                newLotus.IsRoot = false;

                _lonicera.MutateNode(0, newLotus);
                _knautia.MutateNode(0, newLotus);        
            }
            _lonicera.Insert(index, new Lotus(pitch, index == 0));
            _knautia.Insert(index, new Lotus(pitch, index == 0));
            
            // * Parameter Settings

            if (replantAll) {
                for (int i = 0 ; i < _lonicera.NodeCount ; i++) { // replant all
                    var newLotus = _lonicera[i];
                    newLotus.Replant(newLotus.IsRoot);

                    _lonicera.MutateNode(i, newLotus);
                    _knautia.MutateNode(i, newLotus);
                } 
            }
            if (bloom) {Bloom();}
        }

        // * Bloom
        public void Bloom() { // ! MAY HAVE LAMDA ERRORS & ADD KNAUTIA SUPPORT
            _lonicera.GrowAll(); // all dyads generated

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
            _lonicera.GrowAll();
            // * Fertilize with Flowers to Fruits
            for (int i = 0; i < _lonicera.NodeCount; i++) {CrossFlowerFertilization(i);}
            _lonicera.GrowAll();

        }

              

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
            if (!Scale.DiatonicScaleTypes.Contains(type)) {throw new NotImplementedException($"Scale {type} is not implemented.");}

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
    public class LotusThread {  // ! NOT DONE
        public PitchInterval Interval { get; set; }

        public bool IsNegative { get {return Interval.IsNegative;} }
        public bool IsPositive { get {return Interval.IsPositive;} }

        public bool IsDiesisStep { get {return Interval < new MidiPitchInterval(3); } }
        public bool IsThirdsStep { get {
            return new MidiPitchInterval(2) < Interval && Interval < new MidiPitchInterval(5); 
        } }
        public bool IsBalancedStep { get {
            return new MidiPitchInterval(4) < Interval && Interval < new MidiPitchInterval(8); 
        } }
        public bool IsWideStep { get {
            return new MidiPitchInterval(7) < Interval && Interval < new MidiPitchInterval(12); 
        } }
        public bool IsMassiveStep { get {
            return new MidiPitchInterval(11) < Interval;
        } }
    



        // * Constructors
        public LotusThread(Lotus masterLotus, Lotus slaveLotus) { // ! NOT DONE
            Interval = PitchInterval.CreateInterval(masterLotus.Pitch, slaveLotus.Pitch, false);

        }
    }

    
}