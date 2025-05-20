using System.Text.Json;
namespace SineVita.Muguet
{
    public interface IReadOnlyPitchInterval
    {
        public double FrequencyRatio { get; }
        public bool IsMagnitude { get; }
        public bool IsAbsolute { get; }

        public bool IsUnison { get; }

        public int ToMidiIndex { get; }
        public float ToMidiValue { get; }

        public string ToJson();
        public string ToJson(bool prettyPrint) {
            var json = this.ToJson();
            return prettyPrint
                ? JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json), new JsonSerializerOptions { WriteIndented = true })
                : JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json));
        }

        public PitchInterval Inverted();
        public PitchInterval Incremented(PitchInterval interval);
        public PitchInterval Decremented(PitchInterval interval);
    }
}