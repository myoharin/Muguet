using System.Security.Cryptography;
using Microsoft.VisualBasic;
using Caprifolium;

namespace SineVita.Muguet.Nelumbo {
    public enum LotusStage {
        Budding = 0,
        Flowering = 1,
        Fruiting = 2
    }
    public class Lotus {
        // * Variables
        public Pitch Pitch { get; set; }

        // * lotus Properties
        public LotusStage Stage { get; set; }

            // independant - Budding Stage
        public bool IsRoot { get; set; } = false; // Root of lantern

            // depends on other pitch - Flowering Stage - fertilize with buds
        public bool IsStructualTonic { get; set; } = false; // Root of a fifth relation / top of a fourth relation
        public bool IsStructualDominant { get; set; } = false; // the minor or major third of a structual chord
        
        public bool IsStressTone { get; set;} = false; // tension note, involved in 3rd or 6th relation
        public bool IsInStressChain { get; set; } = false; // involved in a chain of chaining stress tones
        
        public bool IsDeflatedTone { get; set;} = false; // 2nd relations
        public bool IsAlchemicTone { get; set;} = false; // tritone or 7ths

            // depends on other developed lotus - Fruiting Stage - fertilize with flowers
        public bool IsStructualMediant { get; set; } = false; // the minor or major third of a structual chord
        

        // * Constructors
        public Lotus(Pitch pitch, bool isRoot) {
            IsRoot = isRoot;
            Stage = LotusStage.Budding;
            Pitch = pitch;
        }

        // * Methods 

        private void validateLanternFertilizer(Lantern lantern, LotusStage stage) {
            List<Lotus> lotuses = new List<Lotus>(lantern.Lotuses);
            List<LotusDyad> dyads = new List<LotusDyad>(lantern.LotusDyads);
            // * Data Validation
            if (lotuses.Count != dyads.Count ) {
                throw new ArgumentException("Lists Length do not match.");
            }
            for (int i = 0; i < dyads.Count ; i++) {
                if ((int)dyads[i].Stage < (int)stage || (int)lotuses[i].Stage < (int)stage) {
                    throw new ArgumentException($"Fertilizer aren't {stage} at index {i}.");
                }
            }
        }

        private static List<Lotus> extractRelatedLotusFromLantern(Lantern lantern, int index) {
            var lotuses = new List<Lotus>(lantern.Lotuses);
            lotuses.RemoveAt(index);
            return lotuses;
        }
        private static List<LotusDyad> extractRelatedDyadsFromLantern(Lantern lantern, int index) {
            var dyads = new List<LotusDyad>();
            for (int j = 0; j < lantern.Lotuses.Count; j++) {
                if (index != j) {
                    dyads.Add(lantern.GetDyad(index, j));
                    // _lonicera.GetValue(i, j)
                }
            }
            return dyads;
        }

        public void Replant(bool isRoot) {
            Stage = LotusStage.Budding;
            IsRoot = isRoot;
            IsStructualTonic  = false;
            IsStructualDominant  = false;
            IsStressTone = false;
            IsInStressChain  = false;
            IsDeflatedTone = false;
            IsAlchemicTone = false;
            IsStructualMediant = false;

        }
        public void FertilizeWithBuddingLantern(Lantern lantern, int lotusIndex) {
            validateLanternFertilizer(lantern, LotusStage.Budding);
            List<Lotus> lotuses = extractRelatedLotusFromLantern(lantern, lotusIndex);
            List<LotusDyad> dyads = extractRelatedDyadsFromLantern(lantern, lotusIndex);


            if (Stage == LotusStage.Budding) {Stage = LotusStage.Flowering;}

            var stressTones = new List<int>() {3, 4, 8, 9};
            var deflatedTones  = new List<int>() {1,2};
            var alchemicTones = new List<int>() {6, 10, 11};

            int stressCount = 0;

            for (int i = 0; i < dyads.Count ; i++) {
                int midiAbsMod = (dyads[i].Interval.ToMidiIndex % 12 + 12 )% 12;
                if (midiAbsMod == 7) {IsStructualTonic = true;}
                if (midiAbsMod == 5) {IsStructualDominant = true;}
                if (stressTones.Contains(midiAbsMod)) {IsStressTone = true; stressCount++;}
                if (deflatedTones.Contains(midiAbsMod)) {IsDeflatedTone = true;}
                if (alchemicTones.Contains(midiAbsMod)) {IsAlchemicTone = true;}
            }

            IsInStressChain = stressCount >= 2;
        }
        public void FertilizeWithFloweringLantern(Lantern lantern, int lotusIndex) {
            // * Stage Check
            if ((int)Stage < (int)LotusStage.Flowering) {FertilizeWithBuddingLantern(lantern, lotusIndex);}
            if (Stage == LotusStage.Flowering) {Stage = LotusStage.Fruiting;}

            // * Validation
            validateLanternFertilizer(lantern, LotusStage.Budding);
            List<Lotus> lotuses = extractRelatedLotusFromLantern(lantern, lotusIndex);
            List<LotusDyad> dyads = extractRelatedDyadsFromLantern(lantern, lotusIndex);

            // * Actual Evaluation
            var stressTones = new List<int>() {3, 4, 8, 9};
            var stressTonics = new List<Lotus>();
            var stressDominants = new List<Lotus>();

            for (int i = 0; i < dyads.Count ; i++) {
                int midiAbsMod = (dyads[i].Interval.ToMidiIndex % 12 + 12 )% 12;
                if (stressTones.Contains(midiAbsMod) && lotuses[i].IsStructualTonic) {stressTonics.Add(lotuses[i]);}
                if (stressTones.Contains(midiAbsMod) && lotuses[i].IsStructualDominant) {stressDominants.Add(lotuses[i]);}
            }

            // * Check Structual Mediant
            foreach(var tonic in stressTonics)  {
                foreach(var dominant in stressDominants) {
                    int interval = PitchInterval.CreateInterval(tonic.Pitch, dominant.Pitch, true).ToMidiIndex;
                    if (interval == 7 || interval == 5) {IsStructualMediant = true;}
                    if (IsStructualMediant) {break;}
                }
                if (IsStructualMediant) {break;}
            }

        }
        public void FertilizeWithFruitingLantern(Lantern lantern, int lotusIndex) { // ! NOT NEEDED?
            // * Stage Check
            if ((int)Stage < (int)LotusStage.Fruiting) {FertilizeWithBuddingLantern(lantern, lotusIndex);}
            if (Stage == LotusStage.Fruiting) {Stage = LotusStage.Fruiting;}
            
            // * Actual Evaluation
            validateLanternFertilizer(lantern, LotusStage.Fruiting);
            List<Lotus> lotuses = extractRelatedLotusFromLantern(lantern, lotusIndex);
            List<LotusDyad> dyads = extractRelatedDyadsFromLantern(lantern, lotusIndex);
            
        }
        
        public void FertilizeWithExternalLantern(Lantern lantern) { // ! NOT DONE
            // * Actual Evaluation
            validateLanternFertilizer(lantern, LotusStage.Fruiting);
            List<Lotus> lotuses = new List<Lotus>(lantern.Lotuses);
            List<LotusDyad> dyads = new List<LotusDyad>(lantern.LotusDyads);
        }
    }
    
    
    
    public class LotusDyad {
        // * Variables
        public PitchInterval Interval { get; set; } // root - bottom | bloom - top
        public LotusStage Stage { get; set; }

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
        public LotusDyad(Lotus lotus1, Lotus lotus2) {
            // * Determine Dyad Stage
            Interval = PitchInterval.CreateInterval(lotus1.Pitch, lotus2.Pitch);
            if ((int)lotus1.Stage >= 2 && (int)lotus2.Stage >= 2) {Stage = LotusStage.Fruiting;}
            else if ((int)lotus1.Stage >= 1 && (int)lotus2.Stage >= 1) {Stage = LotusStage.Flowering;}
            else {Stage = LotusStage.Budding;}
        }
    }

    public class LotusTriad {
        public readonly PitchInterval Interval_2_1;
        public readonly PitchInterval Interval_2_0;
        public readonly PitchInterval Interval_1_0;

        public LotusTriad(Lotus lotus1, Lotus lotus2, Lotus lotus3) {
            
        }


    }
}