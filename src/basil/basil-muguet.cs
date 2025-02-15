using SineVita.Muguet;
namespace SineVita.Basil.Muguet
{
    public class BasilMuguet : Basil
    {
        public Random r = new Random();
        public static void LogPitch(Pitch pitch) {
            Log("Midi Index: " + HarmonyHelper.HtzToMidi(pitch.Frequency) + " | Note Name: " + pitch.NoteName);
        }
        public static void LogPitchList(List<Pitch> list) {
            foreach (Pitch pitch in list) {
                LogPitch(pitch);
            }
        }
    }
}