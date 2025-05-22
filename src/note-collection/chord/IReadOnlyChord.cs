namespace SineVita.Muguet
{
    public interface IReadOnlyChord
    {
        public IReadOnlyList<IReadOnlyPitch> Notes { get; }
        public PitchInterval Range { get; }
        public Pitch Root { get; }
        
        public Chord ToChord();
        public IReadOnlyChord AsReadOnly() => this;
        
        // * Derives
        public IReadOnlyPitch this[int index] => Notes[index];
    }
}