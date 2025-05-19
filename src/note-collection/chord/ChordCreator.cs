namespace SineVita.Muguet;

public partial class Chord
{
    public static Chord Empty() => new Chord(new List<Pitch>());

    public static Chord CreateMajorTriad(Pitch rootPitch) {
        return new Chord(new List<Pitch>() {
            rootPitch, 
            rootPitch.Incremented(new MidiPitchInterval(4)), 
            rootPitch.Incremented(new MidiPitchInterval(7))
        });
    }
    public static Chord CreateMinorTriad(Pitch rootPitch) {
        return new Chord(new List<Pitch>() {
            rootPitch, 
            rootPitch.Incremented(new MidiPitchInterval(3)), 
            rootPitch.Incremented(new MidiPitchInterval(7))
        });
    }
    
    
}