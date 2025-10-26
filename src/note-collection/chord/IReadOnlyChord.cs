using System.Collections;

namespace SineVita.Muguet
{
    public interface IReadOnlyChord :
        IReadOnlyList<IReadOnlyPitch>,
        IList<IReadOnlyPitch>,
        IList
    {
        public IReadOnlyList<IReadOnlyPitch> Notes { get; }
        public PitchInterval Range { get; }
        public Pitch Root { get; }
        public Pitch Terminal { get; }
        
        public Chord ToChord();
        public IReadOnlyChord AsReadOnly() => this;
        
        // * Derives
        public IReadOnlyPitch this[int index] => Notes[index];
        bool ICollection<IReadOnlyPitch>.IsReadOnly => true;
    }
}