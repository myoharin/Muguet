namespace SineVita.Muguet
{
    public interface IReadOnlyPitch
    {
        public double Frequency { get; }
        public string NoteName { get; }
        public int ToMidiIndex { get; }
        
        public Pitch Incremented(PitchInterval interval);
        public Pitch Decremented(PitchInterval interval);
    }
}