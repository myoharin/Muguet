namespace SineVita.Muguet;

public partial class PitchInterval
{
    // * Interval
    private static (Pitch root, Pitch terminal) _AbsolutePitchPair(Pitch root, Pitch terminal)
        => root.Frequency > terminal.Frequency ? (terminal, root) : (root, terminal);

    public static PitchInterval CreateInterval(Pitch root, Pitch terminal, bool useRootType, bool absolute = false) {
        var targetType = useRootType ? root.GetType() : terminal.GetType();
        switch (targetType) {
            case null:
                return Empty;
            case Type t when t == typeof(CompoundPitchInterval):
                return CreateCompoundPitchInterval(root, terminal, absolute);
            case Type t when t == typeof(CustomTetPitchInterval):
                return CreateCustomTetPitchInterval(root, terminal, absolute);
            case Type t when t == typeof(FloatPitchInterval):
                return CreateFloatPitchInterval(root, terminal, absolute);
            case Type t when t == typeof(JustIntonalPitchInterval):
                return CreateJustIntonalPitchInterval(root, terminal, null, absolute);
            case Type t when t == typeof(MidiPitchInterval):
                return CreateMidiPitchInterval(root, terminal, absolute);
            default:
                throw new NotImplementedException($"PitchInterval type {targetType} not found.");
        }
    }

    // * Derivation
    public static CompoundPitchInterval CreateCompoundPitchInterval(Pitch root, Pitch terminal, bool absolute = false) {
        (root, terminal) = absolute ? _AbsolutePitchPair(root, terminal) : (root, terminal);
        var compoundRoot = root as CompoundPitch ?? new CompoundPitch(root);
        var compoundTerminal = terminal as CompoundPitch ?? new CompoundPitch(terminal);
        
        var baseDifference = CreateInterval(compoundRoot.BasePitch, compoundTerminal.BasePitch, !absolute);
        var centDifference = compoundTerminal.CentOffsets - compoundRoot.CentOffsets;
        var returnInterval = new CompoundPitchInterval(baseDifference, centDifference);

        returnInterval.Decrement(compoundRoot.Interval);
        returnInterval.Increment(compoundTerminal.Interval);
        
        return returnInterval;
    }
    
    public static CustomTetPitchInterval CreateCustomTetPitchInterval(Pitch root, Pitch terminal, bool absolute = false) {
        if (root is CustomTetPitch customTetRoot) {
            return CreateCustomTetPitchInterval(customTetRoot, terminal, customTetRoot.Base, absolute);
        }
        else if (terminal is CustomTetPitch customTetTerminal) {
            return CreateCustomTetPitchInterval(root, customTetTerminal, customTetTerminal.Base, absolute);
        }
        else {
            return CreateCustomTetPitchInterval(root, terminal, 12, absolute);
        }
    }
    public static CustomTetPitchInterval CreateCustomTetPitchInterval(Pitch root, Pitch terminal, int baseValue, bool absolute = false) {
        (root, terminal) = absolute ? _AbsolutePitchPair(root, terminal) : (root, terminal);
        
        var customTetRoot = root as CustomTetPitch;
        var customTetTerminal = terminal as CustomTetPitch;
        if (customTetRoot is not null && customTetTerminal is not null &&
            customTetRoot.Base == baseValue && customTetTerminal.Scale == customTetRoot.Scale) {
            int centDiff = terminal.CentOffsets - root.CentOffsets;    
            var indexDiff = customTetTerminal.PitchIndex - customTetRoot.PitchIndex;
            return new CustomTetPitchInterval(baseValue, indexDiff, centDiff);
        }
        else if (customTetRoot is not null && customTetRoot.Base == baseValue) {
            customTetTerminal = new CustomTetPitch(customTetRoot.Scale, terminal.Frequency);
            return CreateCustomTetPitchInterval(customTetRoot, customTetTerminal, baseValue);
        }
        else if (customTetTerminal is not null && customTetTerminal.Base == baseValue) {
            customTetRoot = new CustomTetPitch(customTetTerminal.Scale, root.Frequency);
            return CreateCustomTetPitchInterval(customTetRoot, customTetTerminal, baseValue);
        }
        else {
            return new CustomTetPitchInterval(baseValue, (terminal / root).FrequencyRatio);
        }
    }
    
    public static FloatPitchInterval CreateFloatPitchInterval(Pitch root, Pitch terminal, bool absolute = false) {
        (root, terminal) = absolute ? _AbsolutePitchPair(root, terminal) : (root, terminal);
        double frequencyRatio = (terminal.Frequency - root.Frequency) / root.Frequency;
        return new FloatPitchInterval(frequencyRatio);
    }
    
    // TODO THIS
    public static JustIntonalPitchInterval CreateJustIntonalPitchInterval(Pitch root, Pitch terminal, ICollection<int>? primeLimits = null, bool absolute = false) {
        (root, terminal) = absolute ? _AbsolutePitchPair(root, terminal) : (root, terminal);
        double frequencyRatio = (terminal.Frequency - root.Frequency) / root.Frequency;
        return primeLimits is null
            ? new JustIntonalPitchInterval(frequencyRatio, new List<int>())
            : new JustIntonalPitchInterval(frequencyRatio, primeLimits);
    }
    
    public static MidiPitchInterval CreateMidiPitchInterval(Pitch root, Pitch terminal, bool absolute = false) {
        (root, terminal) = absolute ? _AbsolutePitchPair(root, terminal) : (root, terminal);
        var midiRoot = root as MidiPitch ?? new MidiPitch(root.Frequency);
        var midiTerminal = terminal as MidiPitch ?? new MidiPitch(terminal.Frequency);
        return new MidiPitchInterval(midiTerminal.PitchIndex - midiRoot.PitchIndex, terminal.CentOffsets - root.CentOffsets);
    }
}