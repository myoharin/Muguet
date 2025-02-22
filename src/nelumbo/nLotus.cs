using System.Security.Cryptography;
using Microsoft.VisualBasic;

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

        private void validateFertilizers(List<Lotus> lotuses, List<LotusDyad> dyads, LotusStage stage) {
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
        public void FertilizeWithBuds(List<Lotus> lotuses, List<LotusDyad> dyads) {
            validateFertilizers(lotuses, dyads, LotusStage.Budding);
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
        public void FertilizeWithFlowers(List<Lotus> lotuses, List<LotusDyad> dyads) {
            validateFertilizers(lotuses, dyads, LotusStage.Flowering);

            // * Stage Check
            if ((int)Stage < (int)LotusStage.Flowering) {FertilizeWithBuds(lotuses, dyads);}
            if (Stage == LotusStage.Flowering) {Stage = LotusStage.Fruiting;}

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
        public void FertilizeWithFruits(List<Lotus> lotuses, List<LotusDyad> dyads) { // ! NOT NEEDED
            validateFertilizers(lotuses, dyads, LotusStage.Fruiting);

            // * Stage Check
            if ((int)Stage < (int)LotusStage.Fruiting) {FertilizeWithBuds(lotuses, dyads);}
            if (Stage == LotusStage.Fruiting) {Stage = LotusStage.Fruiting;}
            
            // * Actual Evaluation
            
        }
        
        
        // ! CHANGE all of this into "FertilizeWith[XX Lantern]"
        
        
        public void FertilizeWithExternalLantern(Lantern lantern) { // ! NOT DONE
            
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
            Interval = PitchInterval.CreateInterval(lotus1.Pitch, lotus2.Pitch);
            if ((int)lotus1.Stage >= 2 && (int)lotus2.Stage >= 2) {Stage = LotusStage.Fruiting;}
            else if ((int)lotus1.Stage >= 1 && (int)lotus2.Stage >= 1) {Stage = LotusStage.Flowering;}
            else {Stage = LotusStage.Budding;}
        }
    }
}