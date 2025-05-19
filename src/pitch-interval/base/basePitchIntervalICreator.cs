namespace SineVita.Muguet {
    public partial class PitchInterval {
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
          Type targetType = useRootType ? root.GetType() : terminal.GetType();
          switch (targetType) {
              case Type t when t == typeof(CompoundPitchInterval):
                return CompoundPitchInterval.New(root, terminal, true);
              case Type t when t == typeof(CustomTetPitchInterval):
                return CustomTetPitchInterval.New(root, terminal, true);
              case Type t when t == typeof(FloatPitchInterval):
                return FloatPitchInterval.New(root, terminal, true);
              case Type t when t == typeof(JustIntonalPitchInterval):
                return JustIntonalPitchInterval.New(root, terminal, true);
              case Type t when t == typeof(MidiPitchInterval):
                return MidiPitchInterval.New(root, terminal, true);
              default:
                throw new NotImplementedException($"PitchInterval type {targetType} not found.");
          }
        }
        
        // * Derivation
        public static CompoundPitchInterval CreateCompoundPitchInterval(Pitch root, Pitch terminal, bool absolute = false) { // ! NOT DONE
          (root, terminal) = absolute ? _AbsolutePitchPair(root, terminal) : (root, terminal);
            if (root is CompoundPitch compoundRoot && terminal is CompoundPitch compoundTerminal) {
              var baseDifference = PitchInterval.CreateInterval(compoundRoot.BasePitch, compoundTerminal.BasePitch, !absolute);
              var totalDifference = new CompoundPitchInterval();

              // foreach (var interval in compoundRoot.Interval.) {

              // }






            }



          throw new NotImplementedException();
        }
      }  
}

