using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.ComponentModel;
using SineVita.Basil.Muguet;


namespace SineVita.Muguet.Asteraceae.Cosmosia {
    public class ResonatorCosmosia : Resonator// handles the resontor logic of objects which can qualify to be a resonator
    {   
        // Constants
        public int ResonatorParameterID { get; } //link to resonator helper to get all relevant resonator information
        public int InactivityTolerance { get; } // ms
        public byte InactivityThreshold { get; } // pulse intensity
        public float ObjectSizeMutiplier { get; } // some parameters are affected by the size of the objectz: MaxIdyllAmount
        public byte CriticalOverflowEffectIntensity{ get; }
        public bool AddPulseLowerThanOrigin { get; }

        // 1.0 Input Layer Variables
        public List<Pulse> PulseInput { get; } = new List<Pulse> (); // initiate with null, List[0] = Root | // only managed by AddPulses and DeletePulses
        public List<int> PulseInactivityDuration { get; } = new List<int> (); // ms
        public List<PitchInterval> WINA { get; } = new List<PitchInterval> (); // ACTUAL parent WINA
        public List<byte> PitchIntervalIntensity { get; } = new List<byte> ();
        public List<byte> IntervalChannelID { get; } = new List<byte> ();

        // 3.0 Layer - Resonance Helper stuff
        public float NetInflowFlowrate = 0; // per second, add up all em
        public bool IsManaDefecit = false;
        public bool IsOverflowing = false; // MaxManaAmount reached
        public bool IsCriticalOverflow = false; // Max Overflow and Outflow rate reached
        public bool IsCriticalState = false; // critical state reach, let parent handle what happens

            // use inflowGateFlowrate to calculate:
        public List<float> ProjectedOutflowSubgateFlowrate { get; } = new List<float>(); // WINA ordered
        public List<float> ProjectedOverflowSubgateFlowrate { get; } = new List<float>(); // WINA ordered

            // then total up above: calculate:
        public float NetOutflowFlowrate { get; set; } = 0; // capped if higher than Outflowlimit
        public float NetOverflowFlowrate { get; set; } = 0; // capped if higher than OverflowLimit
        
        // 4.0 End Layer Vairables
        public float IdyllAmount { get; set; } = 0;
        public List<MEDDuo> MagicalEffectDatas { get; } = new List<MEDDuo>(); // Magic Effect name | magic effect intensity | handles totaling up intensities as well // WINA ordered 
        public int CriticalOverflowDuration = 0;
    
        // initializebasil
        public ResonatorCosmosia(int resonatorParameterID, int inactivityTolerance = 2048, byte inactivityThreshold = 16, float objectSizeMutiplier = 1.0f, bool addPulseLowerThanOrigin = false)
        {
            ResonatorParameterID = resonatorParameterID;
            InactivityTolerance = inactivityTolerance;
            InactivityThreshold = inactivityThreshold;
            ObjectSizeMutiplier = objectSizeMutiplier;
            AddPulseLowerThanOrigin = addPulseLowerThanOrigin;
        }
        // Process
        public override void Process(double deltaTime) // FINALLY DONE
        {
        // - - - Flowrate calculation - - - //
            //working capital
            NetInflowFlowrate = 0; // reset - finalised
            NetOutflowFlowrate = 0; // reset - finalised
            NetOverflowFlowrate = 0; // reset - finalised with 2 code paths
            // working capital
            Tuple<int, int> POAIndices;
            float flowRate; // transitional vairable and is never finalized, please only read in local bracketted context

            float projectedOutflowFlowRate = 0;
            float projectedOverflowFlowRate = 0;

            float outflowLimiter = 1; // mutiplies by each subgate so they don't go over the outflow limit
            float overflowLimiter = 0; // mutiplies by each subgate so they don't go over the overflow limit

            float idyllPressureCurve = 0;

            // process constants (used more than once)
            ResonatorParameterCosmosia resonatorParameter = ResonanceHelperCosmosia.GetResonatorParameter(ResonatorParameterID);

            // calculate inflow rate - DONE
            for (int i = 0; i < PulseInput.Count; i++) { // add up inflow rate
                NetInflowFlowrate += ResonanceHelperCosmosia.PulseIntensityToFlowRate(PulseInput[i].Intensity);
            }
            if (NetInflowFlowrate > resonatorParameter.InflowLimit) { // just cap it, no need for anything complicated
                NetInflowFlowrate = resonatorParameter.InflowLimit;
            } 

            // calculate projected outflow rate using overflow multiplier - Done
            for (int i = 0; i < WINA.Count; i++ ) { // WINA UPDATES
                // pulseintensity -> interval intensity -> manaflow rate -> effectIntensity
                POAIndices = HarmonyHelper.CalculateWINAIndexResult(i); // remember to pulse[POA-1] since pulse is stored as PAC
                if (POAIndices.Item1 <= 0) {
                    PitchIntervalIntensity[i] = ResonanceHelperCosmosia.CalculateIntervalIntensity(resonatorParameter.OriginIntensity, PulseInput[POAIndices.Item2-1].Intensity);
                }
                else {
                    PitchIntervalIntensity[i] = ResonanceHelperCosmosia.CalculateIntervalIntensity(PulseInput[POAIndices.Item1-1].Intensity, PulseInput[POAIndices.Item2-1].Intensity);
                }
                flowRate = ResonanceHelperCosmosia.IntervalIntensityToFlowRate(PitchIntervalIntensity[i]); // resonatorParameter.GetChannelParameter(IntervalChannelID[i]).OutflowMultiplier;
                //BasilMuguet.Log($"Outflowrate: {flowRate} | OutflowMultiplier: {resonatorParameter.GetChannelParameter(IntervalChannelID[i]).OutflowMultiplier}");
                ProjectedOutflowSubgateFlowrate[i] = flowRate;       
                projectedOutflowFlowRate += flowRate;
            }


            // outflow limiter and diverge potential flowrate to overflow to calculate its ratio
            float temptOutflowLimit;
            if (IsManaDefecit) {temptOutflowLimit = Math.Min(NetInflowFlowrate, resonatorParameter.OutflowLimit);} else {temptOutflowLimit = resonatorParameter.OutflowLimit;}
            if (projectedOutflowFlowRate > temptOutflowLimit) { // when theres not enough idyll to support outflow
                outflowLimiter = temptOutflowLimit / projectedOutflowFlowRate;
                projectedOutflowFlowRate = temptOutflowLimit;
                for (int i = 0; i < WINA.Count; i++ ) {
                    ProjectedOverflowSubgateFlowrate[i] = ProjectedOutflowSubgateFlowrate[i] * (1 - outflowLimiter) * resonatorParameter.GetChannelParameter(IntervalChannelID[i]).OverflowMultiplier;
                    ProjectedOutflowSubgateFlowrate[i] *= outflowLimiter; // limit
                    projectedOverflowFlowRate += ProjectedOverflowSubgateFlowrate[i];
                }
            }

            // apply idyllpressurecurve to outflow
            idyllPressureCurve = ResonanceHelperCosmosia.IdyllPressureCurve(IdyllAmount / resonatorParameter.MaxIdyllAmount);
            for (int i = 0; i < WINA.Count; i++ ) {
                ProjectedOutflowSubgateFlowrate[i] *= idyllPressureCurve; // finalised
            }

            NetOutflowFlowrate = projectedOutflowFlowRate * idyllPressureCurve; // finalised
            
            // update IdyllAmount according to NetInflowrate, Outflow and diverge to overflow
            IdyllAmount += (float)((NetInflowFlowrate - NetOutflowFlowrate) * deltaTime); // updated
            if (IdyllAmount > resonatorParameter.MaxIdyllAmount * ObjectSizeMutiplier) { // overflowing and must be capped
                flowRate = IdyllAmount - resonatorParameter.MaxIdyllAmount; // excess flow rate in this frame
                IdyllAmount = resonatorParameter.MaxIdyllAmount; // cap it
                IsOverflowing = true;
                // finalise projectedoverflowratio into actual number
                flowRate /= (float)deltaTime; // convert into Idyll/second 
                float multiplierA = flowRate/projectedOverflowFlowRate;
                for (int i = 0; i < WINA.Count; i++ ) {ProjectedOverflowSubgateFlowrate[i] *= multiplierA;}
                projectedOverflowFlowRate = flowRate;
            } 
            else { // not oveflowing
                IsOverflowing = false;
                if (IdyllAmount < 0) {
                    IsManaDefecit = true;
                    IdyllAmount = 0;
                }
                else{
                    IsManaDefecit = false;
                }
                projectedOverflowFlowRate = 0;
            }

            // limit overflow if needed
            if (projectedOverflowFlowRate > resonatorParameter.OverflowLimit && IsOverflowing) {
                IsCriticalOverflow = true;
                overflowLimiter = resonatorParameter.OverflowLimit / projectedOverflowFlowRate;
                projectedOverflowFlowRate = resonatorParameter.OverflowLimit;
                NetOverflowFlowrate = resonatorParameter.OverflowLimit; // finalised code path 1
                for (int i = 0; i < WINA.Count; i++ ) {
                    ProjectedOverflowSubgateFlowrate[i] *= overflowLimiter;
                }
            } 
            else {
                IsCriticalOverflow = false;
                NetOverflowFlowrate = projectedOverflowFlowRate; // finalised code path 2
            }

            // update MagicalEffectIntensity
            for (int i = 0; i < WINA.Count; i++ ) {MagicalEffectDatas[i].Outflow.Intensity = ResonanceHelperCosmosia.FlowrateToEffectIntensity(ProjectedOutflowSubgateFlowrate[i]);} // outflow
            if (IsOverflowing) { // add overflowing intensity as well!
                for (int i = 0; i < WINA.Count; i++ ) {MagicalEffectDatas[i].Overflow.Intensity = ResonanceHelperCosmosia.FlowrateToEffectIntensity(ProjectedOverflowSubgateFlowrate[i]);} // outflow
            } else {
                for (int i = 0; i < WINA.Count; i++ ) {MagicalEffectDatas[i].Overflow.Intensity = 0;}
            }

            // update critical effect intensity and processing - Done
            if (IsCriticalOverflow) {
                CriticalOverflowDuration += (int)(deltaTime * 1000);
                if (CriticalOverflowDuration > resonatorParameter.CriticalEffectDurationThreshold) {
                    IsCriticalState = true;
                }
            } else {CriticalOverflowDuration = 0;}
            
            
            // update PulseInactivity System checking - Done
            for (int i = 0; i < PulseInput.Count; i++ ) {
                if (PulseInput[i].Intensity < InactivityThreshold) {
                    PulseInactivityDuration[i] += (int)(deltaTime * 1000);
                }
                if (PulseInactivityDuration[i] > InactivityTolerance) {
                    DeletePulse(PulseInput[i].PulseID); // takes in pusle ID as arguments huh...
                }
            }
        }
        
        // Necessary public functions and override 
        public override void AddPulse(Pulse newPulse){ // DONE
            // check if higher than origin
            if (!AddPulseLowerThanOrigin && ResonanceHelperCosmosia.GetResonatorParameter(ResonatorParameterID).Origin.Frequency > newPulse.Pitch.Frequency) {return;}

            // check pulse intensity vs inactivity tolerance
            if (newPulse.Intensity < InactivityThreshold) {return;}
            // check if exist already
            for (int i = 0; i < PulseInput.Count; i++) {
                if (newPulse.PulseID == PulseInput[i].PulseID) {return;}
            }
            // determine newPulse's index in pulse list accoridng to their pitch
            int findIndex(Pitch pitch) {
                for (int i = 0; i < PulseInput.Count; i++) {
                    if (PulseInput[i].Pitch.Frequency > pitch.Frequency) {
                        return i;
                    }
                }
                return -1;
            }
            int newPulseIndex;
            if(newPulse == null){return;} else {
                Pitch newPitch = newPulse.Pitch;
                newPulseIndex = findIndex(newPitch);
                if (newPulseIndex != -1){
                    PulseInput.Insert(newPulseIndex, newPulse);
                    PulseInactivityDuration.Insert(newPulseIndex, 0);
                } else {
                    PulseInput.Add(newPulse);
                    PulseInactivityDuration.Add(0);
                    newPulseIndex = PulseInput.Count - 1;
                }
            }
        
            // THEN CHANGE EVERYTHING IN WINA RELATED LIST
            // ! UPDATE USING LONICERA
            // List<Tuple<int, Tuple<int, int>>> listM = HarmonyHelper.IndexesToAddWINAAddPitch(PulseInput.Count - 1, newPulseIndex);
            // List<int> indexesToBeInserted = new List<int>();
            // foreach (Tuple<int, Tuple<int, int>> i in listM){
            //     indexesToBeInserted.Add(i.Item1);
            // }
            // _InsertItemsWINAOrderedLists(indexesToBeInserted);
        }
        public override void DeletePulse(int pulseID){ // DONE - Sort out WINA related stuff as well
            int pitchIndexToBeRemoved = -1;
            for (int i = 0; i < PulseInput.Count; i++) {
                if (PulseInput[i].PulseID == pulseID) {
                    PulseInput.RemoveAt(i);
                    PulseInactivityDuration.RemoveAt(i);
                    pitchIndexToBeRemoved = i;
                    break;
                    }
            } 
            if (pitchIndexToBeRemoved == -1){return;} // no pulses have been removed

            // THEN CHANGE EVERYTHING IN WINA RELATED LIST
            // int nodeCount = (int)Math.Floor(Math.Sqrt(2 * WINA.Count + 0.25) - 0.5);
            // List<int> customIndexDeletionList = HarmonyHelper.IndexesToDeletedWINADeletePitch(nodeCount, pitchIndexToBeRemoved);
            // _RemoveRangeWINAOrderedLists(customIndexDeletionList[0], customIndexDeletionList[1]);
            // customIndexDeletionList.RemoveRange(0, 2);
            // _RemoveAtWINAOrderedLists(customIndexDeletionList);
        }
        public override void MutatePulse(Pulse newPulse) {} // NOT DONE

        public override List<MagicalEffectData> GetMagicalEffects(byte intensityThreshold = 1) { // DONE
            List<MagicalEffectData> returnList = new List<MagicalEffectData>(); // append with all cosmosia data
            for (int i = 0; i < WINA.Count; i++){
                if (MagicalEffectDatas[i].Outflow.Intensity >= intensityThreshold) {returnList.Add(MagicalEffectDatas[i].Outflow);}
                if (MagicalEffectDatas[i].Overflow.Intensity >= intensityThreshold) {returnList.Add(MagicalEffectDatas[i].Overflow);}
            }
            return returnList;
        }

        // private functiosn
        private void _RemoveAtWINAOrderedLists(List<int> indexesToBeDeleted){
            // sort them so it is neat and predictable during the deletion process
            indexesToBeDeleted.Sort();
            // data validation
            if (!(indexesToBeDeleted[0] >= 0 && indexesToBeDeleted[indexesToBeDeleted.Count-1] < WINA.Count)){return;}
            // check for repeted indexes, clean the input first
            int lastItem = indexesToBeDeleted[0];
            int loopAdjustment = 0;
            for (int i = 1; i < indexesToBeDeleted.Count - loopAdjustment; i++){
                if (indexesToBeDeleted[i] == lastItem) {
                    indexesToBeDeleted.RemoveAt(i);
                    loopAdjustment -= 1;
                } else {lastItem = indexesToBeDeleted[i];}
            }
            
            // make em neat and tidy
            for (int i = 0; i < indexesToBeDeleted.Count; i++) {
                indexesToBeDeleted[i] -= i;

                // mass removal, singular ones
                WINA.RemoveAt(indexesToBeDeleted[i]);
                PitchIntervalIntensity.RemoveAt(indexesToBeDeleted[i]);
                IntervalChannelID.RemoveAt(indexesToBeDeleted[i]);
                ProjectedOutflowSubgateFlowrate.RemoveAt(indexesToBeDeleted[i]);
                ProjectedOverflowSubgateFlowrate.RemoveAt(indexesToBeDeleted[i]);
                MagicalEffectDatas.RemoveAt(indexesToBeDeleted[i]);
            }
        }
        private void _RemoveRangeWINAOrderedLists(int index, int count) { // item1 < item2
            // data validation
            if (index + count > WINA.Count || count == 0) {return;}
            // mass removal
            WINA.RemoveRange(index, count);
            PitchIntervalIntensity.RemoveRange(index, count);
            IntervalChannelID.RemoveRange(index, count);
            ProjectedOutflowSubgateFlowrate.RemoveRange(index, count);
            ProjectedOverflowSubgateFlowrate.RemoveRange(index, count);
            MagicalEffectDatas.RemoveRange(index, count);
            
        }

        private void _InsertItemsWINAOrderedLists(List<int> indexesToBeInserted) {// DONE
        // sort them so it is neat and predictable
            indexesToBeInserted.Sort();
            // check for repeted indexes, clean the input first
            int lastItem = indexesToBeInserted[0];
            int loopAdjustment = 0;
            for (int i = 1; i < indexesToBeInserted.Count - loopAdjustment; i++){
                if (indexesToBeInserted[i] == lastItem) {
                    indexesToBeInserted.RemoveAt(i);
                    loopAdjustment -= 1;
                } else {lastItem = indexesToBeInserted[i];}
            }
            
                // actual calcualtions
            Tuple<int, int> POAIndices;
            bool N2R;
            bool rootIsStructual;
            byte channelID;

            for (int i = 0; i < indexesToBeInserted.Count; i++) {
                POAIndices = HarmonyHelper.CalculateWINAIndexResult(indexesToBeInserted[i]);
                BasilMuguet.Log($"{indexesToBeInserted[i]}: n{POAIndices.Item2} -> n{POAIndices.Item1}");
                    //will be calculated during _process so no need to calculate here
                ProjectedOutflowSubgateFlowrate.Insert(indexesToBeInserted[i], 0);
                ProjectedOverflowSubgateFlowrate.Insert(indexesToBeInserted[i], 0);
                PitchIntervalIntensity.Insert(indexesToBeInserted[i], 0);
                if (POAIndices.Item1 == 0) {
                    N2R = true;
                    WINA.Insert(indexesToBeInserted[i], PitchInterval.CreateTargetInterval(ResonanceHelperCosmosia.GetResonatorParameter(ResonatorParameterID).Origin, PulseInput[POAIndices.Item2-1].Pitch));
                    byte effectIntensity = 0; // calculate at run time
                    MagicalEffectDatas.Insert(indexesToBeInserted[i], ResonanceHelperCosmosia.PitchIntervalToMEDDuo(WINA[indexesToBeInserted[i]], N2R, effectIntensity, ResonatorParameterID));
                } 
                else {
                    // PitchInterval.CreateTargetInterval(PulseInput[POAIndices.Item1-1].Pitch, PulseInput[POAIndices.Item2-1].Pitch)
                    // HarmonyHelper.CreatePitchInterval(PulseInput[POAIndices.Item1-1].Pitch, PulseInput[POAIndices.Item2-1].Pitch)
                    N2R = false;
                    WINA.Insert(indexesToBeInserted[i], PitchInterval.CreateTargetInterval(PulseInput[POAIndices.Item1-1].Pitch, PulseInput[POAIndices.Item2-1].Pitch));
                    byte effectIntensity = 0; // calculate at run time
                    rootIsStructual = ResonanceHelperCosmosia.PitchIntervalIsStructual(PitchInterval.CreateTargetInterval(ResonanceHelperCosmosia.GetResonatorParameter(ResonatorParameterID).Origin, PulseInput[POAIndices.Item1-1].Pitch));
                    if (rootIsStructual) {
                        MagicalEffectDatas.Insert(indexesToBeInserted[i], ResonanceHelperCosmosia.PitchIntervalToMEDDuo(WINA[indexesToBeInserted[i]], N2R, effectIntensity, ResonatorParameterID));
                    }
                    else {
                        MagicalEffectDatas.Insert(indexesToBeInserted[i], new MEDDuoNull());
                    }
                }

                BasilMuguet.Log($"FreqRatio is: {WINA[i].FrequencyRatio}");
                channelID = ResonanceHelperCosmosia.PitchIntervalToChannelID(WINA[i], N2R);
                if (channelID == 255) {
                    IntervalChannelID.Insert(indexesToBeInserted[i], channelID); // keep null ID here
                } else {
                    IntervalChannelID.Insert(indexesToBeInserted[i], channelID);
                }
                
            }
        }
    }
    public class MEDDuo
    {
        public MagicalEffectDataCosmosia Outflow;
        public MagicalEffectDataCosmosia Overflow;
        public MEDDuo (MagicalEffectDataCosmosia outflow, MagicalEffectDataCosmosia overflow)
        {
            Outflow = outflow;
            Overflow = overflow;
        }

        public void UpdateIntensity(byte intensity){
            Outflow.Intensity = intensity;
            Overflow.Intensity = intensity;

            // Pulse Intensity(dB)(byte)
            // Interval Intensity (byte) (geometric mean)
            // SubgateflowRate (Idyll/second) (float)
            // EffectIntensity (byte)
        }

        public MagicalEffectDataCosmosia GetMagicalEffect(bool isOverflow) {
            if (isOverflow) {return Overflow;} else {return Outflow;}
        }
    }
    public class MEDDuoNull : MEDDuo {
        public MEDDuoNull() : base(new NullMagicalEffectDataCosmosia(), new NullMagicalEffectDataCosmosia()) { }
    }
}