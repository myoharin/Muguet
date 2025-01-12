using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace SineVita.Muguet.Asteraceae.Cosmosia
{
    public class ResonatorParameterCosmosia : ResonatorParameter
    {
        // basic information
        public int ResonatorParameterID { get; set; }
        public Pitch Origin { get; set; }
        public byte OriginIntensity { get; set; }
        public float MaxIdyllAmount { get; set; } // max amount before the the mana starts going out the overflow limit
        public int CriticalEffect { get; set; }
        public int CriticalEffectDurationThreshold { get; set; } // in ms
        public byte CriticalEffectIntensity { get; set; }

 
        // limit on Idyllflow. If limit reached, then it will be distributed according to their intensity ratio
        public float InflowLimit { get; set; } // caping net inflow rate (pulse intensity)
        public float OutflowLimit { get; set; } // once limit reached, excess idyll flow gets stored (interval intensity)
        public float OverflowLimit { get; set; } // once limit reached, wait some time before critical effect (interval intensity)

        public List<ChannelParameterCosmosia> ChannelParameters{ get; set; }   // 13 items for now

        public ChannelParameterCosmosia GetChannelParameter(int channelID) {
            if (channelID >= 255 || channelID >= ChannelParameters.Count) {
                return new ChannelParameterCosmosia(null);
            }
            if (ChannelParameters[channelID] == null) {
                return new ChannelParameterCosmosia(null);
            }
            return ChannelParameters[channelID];
        }
        public ChannelParameterCosmosia GetChannelParameter(CosmosiaChannelId channelId) {
            return GetChannelParameter((byte)channelId);
        }

        // class instantiation methods
        public ResonatorParameterCosmosia(int resonatorParameterID, int runTime = 0) // DONE but not tested yet, accepts ID as int
        {
            ResonatorParameterID = resonatorParameterID;
            RunTimeLastFetched = runTime;

            // Construct the JSON file path
            string jsonFilePath = Path.Combine(ResonanceHelper.ParametersFolderPath, "cosmosia" ,$"{resonatorParameterID}.json");

            // Check if the file exists
            if (File.Exists(jsonFilePath))
            {
                // Read the JSON file content
                var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true}; // Optional: makes property names case insensitive
                string jsonString = File.ReadAllText(jsonFilePath);
                ResonatorParameterCosmosia? resonatorParameter = JsonSerializer.Deserialize<ResonatorParameterCosmosia>(jsonString, options);
                
                // Check if the deserialization was successful
                if (resonatorParameter != null)
                {
                    // Initialize properties from the deserialized object
                    ResonatorParameterID = resonatorParameter.ResonatorParameterID;
                    Origin = resonatorParameter.Origin ?? new Pitch(432); // Ensure Origin is initialize
                    OriginIntensity = resonatorParameter.OriginIntensity;
                    MaxIdyllAmount = resonatorParameter.MaxIdyllAmount;
                    CriticalEffect = resonatorParameter.CriticalEffect;
                    InflowLimit = resonatorParameter.InflowLimit;
                    OutflowLimit = resonatorParameter.OutflowLimit;
                    OverflowLimit = resonatorParameter.OverflowLimit;
                    ChannelParameters = resonatorParameter.ChannelParameters ?? new List<ChannelParameterCosmosia>(); // Initialize if null
                }
            }
            else
            {
                throw new FileNotFoundException($"The specified JSON file was not found: {jsonFilePath}");
            }
        }
        public ResonatorParameterCosmosia(string paramaterPath, int runTime = 0) // DONE but not tested yet, accepts full path names
        {
            if (int.TryParse(paramaterPath.Split("\\").Last().Split(".")[0], out int result))
            {
                ResonatorParameterID = result;
            }
            else{
                throw new FileNotFoundException("ParamaterIDNotSpecified");
            }

            RunTimeLastFetched = runTime;
 
            // Check if the file exists
            if (File.Exists(paramaterPath))
            {
                // Read the JSON file content
                string jsonString = File.ReadAllText(paramaterPath);
                var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true}; // Optional: makes property names case insensitive
                var resonatorParameter = JsonSerializer.Deserialize<ResonatorParameterCosmosia>(jsonString, options);
                
                // Check if the deserialization was successful
                if (resonatorParameter != null)
                {
                    // Initialize properties from the deserialized object
                    ResonatorParameterID = resonatorParameter.ResonatorParameterID;
                    Origin = resonatorParameter.Origin ?? new Pitch(432); // Ensure Origin is initialize
                    OriginIntensity = resonatorParameter.OriginIntensity;
                    MaxIdyllAmount = resonatorParameter.MaxIdyllAmount;
                    CriticalEffect = resonatorParameter.CriticalEffect;
                    InflowLimit = resonatorParameter.InflowLimit;
                    OutflowLimit = resonatorParameter.OutflowLimit;
                    OverflowLimit = resonatorParameter.OverflowLimit;
                    ChannelParameters = resonatorParameter.ChannelParameters ?? new List<ChannelParameterCosmosia>(); // Initialize if null
                }
            }
            else
            {
                throw new FileNotFoundException($"The specified JSON file was not found: {paramaterPath }");
            }
        }
    
        // json deserialization : parameterless constructor required
        [JsonConstructor]
        public ResonatorParameterCosmosia() {}
        


    }
    public class ChannelParameterCosmosia
        {
            public CosmosiaChannelId ChannelId { get; set; }
            public float OutflowMultiplier { get; set; }
            public float OverflowMultiplier { get; set; }
            public int OutflowEffect { get; set; }
            public int OverflowEffect { get; set; }
            public bool IsNull;
            public ChannelParameterCosmosia(CosmosiaChannelId channelId, float outflowMultiplier, float overflowMultiplier, int outflowEffect, int overflowEffect)
            {
                ChannelId = channelId;
                OutflowMultiplier = outflowMultiplier;
                OverflowMultiplier = overflowMultiplier;
                OutflowEffect = outflowEffect;
                OverflowEffect = overflowEffect;
                IsNull = false;
            }
            public ChannelParameterCosmosia(bool? _null) : this((CosmosiaChannelId)255, 0, 0, -1, -1) {IsNull = true;}
            [JsonConstructor]
            public ChannelParameterCosmosia() {}
        }

}