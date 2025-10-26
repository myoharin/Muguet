namespace SineVita.Muguet;

public partial class Chord
{
    // * IReadOnlyChord
    IReadOnlyList<IReadOnlyPitch> IReadOnlyChord.Notes => _notes.AsReadOnly();
    Chord IReadOnlyChord.ToChord() => Clone();

    void ICollection<IReadOnlyPitch>.Add(IReadOnlyPitch pitch) => this.Add(pitch.ToPitch());
    void ICollection<IReadOnlyPitch>.Clear() => _notes.Clear();
    bool ICollection<IReadOnlyPitch>.Contains(IReadOnlyPitch p) => this._notes.Contains(p.ToPitch());

    void ICollection<IReadOnlyPitch>.CopyTo(IReadOnlyPitch[] destination, int i)
    {
        List<IReadOnlyPitch> n = new();
        foreach (var p in _notes)
        {
            n.Add(p);
        }
        n.CopyTo(destination, i);
    }
    int ICollection<IReadOnlyPitch>.Count => _notes.Count();
    bool ICollection<IReadOnlyPitch>.IsReadOnly => false;
    bool ICollection<IReadOnlyPitch>.Remove(IReadOnlyPitch p) => _notes.Remove(p.ToPitch());    
    
    int IList<IReadOnlyPitch>.IndexOf(IReadOnlyPitch p) => _notes.IndexOf(p.ToPitch());
    void IList<IReadOnlyPitch>.Insert(int i, IReadOnlyPitch p) => Add(p.ToPitch());
    void IList<IReadOnlyPitch>.RemoveAt(int i) => _notes.RemoveAt(i);
    IReadOnlyPitch IList<IReadOnlyPitch>.this[int i]
    {
        get => _notes[i];
        set
        {
            _notes.RemoveAt(i);
            Add(value.ToPitch());
        }
    }

    int IReadOnlyCollection<IReadOnlyPitch>.Count => _notes.Count;
    IReadOnlyPitch IReadOnlyList<IReadOnlyPitch>.this[int i] => _notes[i];

    void System.Collections.ICollection.CopyTo(Array destination, int i)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (i < 0)
            throw new ArgumentOutOfRangeException(nameof(i));
        if (destination.Rank != 1)
            throw new ArgumentException("Multidimensional arrays are not supported.");
        if (destination.Length - i < _notes.Count)
            throw new ArgumentException("Destination array is not long enough.");

        // Copy elements to the destination array
        for (int j = 0; j < _notes.Count; j++)
        {
            destination.SetValue(_notes[j], i + j);
        }
    }

    int System.Collections.ICollection.Count => _notes.Count;
    bool System.Collections.ICollection.IsSynchronized => false;
    object System.Collections.ICollection.SyncRoot => this;

    void System.Collections.IList.Clear() => _notes.Clear();

    bool System.Collections.IList.Contains(object? value)
    {
        if (value is Pitch pitch)
            return _notes.Contains(pitch);
        return false;
    }

    int System.Collections.IList.IndexOf(object? value)
    {
        if (value is Pitch pitch)
            return _notes.IndexOf(pitch);
        return -1;
    }

    void System.Collections.IList.Insert(int index, object? value)
    {
        if (value is Pitch pitch)
            _notes.Insert(index, pitch);
    }

    bool System.Collections.IList.IsFixedSize => false;
    bool System.Collections.IList.IsReadOnly => false;
    void System.Collections.IList.Remove(object? value)
    {
        if (value is Pitch pitch)
            _notes.Remove(pitch);
    }
    void System.Collections.IList.RemoveAt(int index) => _notes.RemoveAt(index);

    object? System.Collections.IList.this[int index]
    {
        get => _notes[index];
        set
        {
            if (value is Pitch pitch)
                _notes[index] = pitch;
        }
    }

    int System.Collections.IList.Add(object? obj)
    {
        if (obj is Pitch pitch)
            return Add(pitch);
        return -1;
    }
}