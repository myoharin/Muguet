using SineVita.Muguet;
namespace SineVita.Basil.Muguet
{
    public class BasilMuguet : Basil
    {
        public Random r = new Random();


        public static void Log(Pitch pitch) {
            Log("Midi Index: " + HarmonyHelper.HtzToMidi(pitch.Frequency) + " | Note Name: " + pitch.NoteName);
        }
        public static void Log(List<Pitch> list) {
            foreach (Pitch pitch in list) {
                Log(pitch);
            }
        }
        public static void Log(Chord chord) { // ! NOT DONE

        }
        public static void Log(ChordReferenceScale scale) { // ! NOT DONE

        }
        public static void Log(PitchClassScale scale) { // ! NOT DONE

        }
    }
}