using System.Globalization;
using SineVita.Muguet;
using SineVita.Muguet.Nelumbo;

namespace SineVita.Basil.Muguet.Nelumbo
{
    public class BasilMuguetNelumbo : BasilMuguet
    {
        public BasilMuguetNelumbo() : base() {}

        public void StartTest() {

            // * starting chord progression
            var chords = new List<List<int>> {
                new List<int> {0, 4, 7, 11}, // C Major
                new List<int> {5, 9, 12, 16}, // F major
                new List<int> {5, 9, 14}, // D minor
                new List<int> {4, 8, 11, 14}, // E minor
                new List<int> {4, 9, 12}, // A minor
                new List<int> {7, 11, 14}, // G Major
                new List<int> {0, 4, 7, 11, 16}, // G Major
            };

            // * Create Lantern List
            var lanterns = new List<Lantern>();
            int i = 0;
            foreach (List<int> chord in chords) {
                i++;
                chord.Sort();
                var pitchList = new List<Pitch>();
                foreach (int index in chord) {
                    var pitch = new MidiPitch(index);
                    pitchList.Add(pitch);
                }
                Log($"Chord {i}");
                LogPitchList(pitchList);
                lanterns.Add(new Lantern(pitchList));
            }

            // * Create Sultra
            var sultra = new Sultra(lanterns);


            // * TEST PHASE

        }

        public static void LogLantern(Lantern lantern) {
            Log("Lantern:");
            foreach (Lotus lotus in lantern.Lotuses) {
                LogPitch(lotus.Pitch);
            }
        }
    }
}