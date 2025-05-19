namespace SineVita.Muguet;

public partial class PitchInterval
{
    // * Interval
    private static (Pitch root, Pitch terminal) _AbsolutePitchPair(Pitch root, Pitch terminal) { // helper method
        Pitch higherPitch, lowerPitch;
        if (root.Frequency > terminal.Frequency) {
            higherPitch = root;
            lowerPitch = terminal;
        }
        else {
            higherPitch = terminal;
            lowerPitch = root;
        }

        return (lowerPitch, higherPitch);
    }

    public static PitchInterval CreateInterval(Pitch root, Pitch terminal, bool useRootType, bool absolute = false) {
        var targetType = useRootType ? root.GetType() : terminal.GetType();
        switch (targetType) {
            case null:
                return Empty;
            case Type t when t == typeof(CompoundPitchInterval):
                return CreateCompoundPitchInterval(root, terminal, true);
            case Type t when t == typeof(CustomTetPitchInterval):
                return CreateCustomTetPitchInterval(root, terminal, true);
            case Type t when t == typeof(FloatPitchInterval):
                return CreateFloatPitchInterval(root, terminal, true);
            case Type t when t == typeof(JustIntonalPitchInterval):
                return CreateJustIntonalPitchInterval(root, terminal, true);
            case Type t when t == typeof(MidiPitchInterval):
                return CreateMidiPitchInterval(root, terminal, true);
            default:
                throw new NotImplementedException($"PitchInterval type {targetType} not found.");
        }
    }

    // * Derivation
    public static CompoundPitchInterval CreateCompoundPitchInterval(Pitch root, Pitch terminal, bool absolute = false) {
        // ! NOT DONE
        (root, terminal) = absolute ? _AbsolutePitchPair(root, terminal) : (root, terminal);
        if (root is CompoundPitch compoundRoot && terminal is CompoundPitch compoundTerminal) {
            var baseDifference = CreateInterval(compoundRoot.BasePitch, compoundTerminal.BasePitch, !absolute);
            var totalDifference = new CompoundPitchInterval();

            // foreach (var interval in compoundRoot.Interval.) {

            // }
        }


        throw new NotImplementedException();
    }

    public static CustomTetPitchInterval
        CreateCustomTetPitchInterval(Pitch root, Pitch terminal, bool absolute = false) {
        throw new NotImplementedException();
    }

    public static FloatPitchInterval CreateFloatPitchInterval(Pitch root, Pitch terminal, bool absolute = false) {
        throw new NotImplementedException();
    }

    public static JustIntonalPitchInterval CreateJustIntonalPitchInterval(Pitch root, Pitch terminal,
        bool absolute = false) {
        throw new NotImplementedException();
    }

    public static MidiPitchInterval CreateMidiPitchInterval(Pitch root, Pitch terminal, bool absolute = false) {
        throw new NotImplementedException();
    }
}