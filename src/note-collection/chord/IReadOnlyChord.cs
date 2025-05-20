namespace SineVita.Muguet
{
    public interface IReadOnlyChord
    {
        IReadOnlyList<IReadOnlyPitch> Notes { get; }
        PitchInterval Range { get; }
        Pitch Root { get; }
    }
}