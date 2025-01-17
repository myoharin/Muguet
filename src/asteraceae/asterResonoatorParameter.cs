namespace SineVita.Muguet.Asteraceae {
    public abstract class ResonatorParameter{
        // Meta data
        public int RunTimeLastFetched { get; set; } = 0; // milliseconds
        public AsterGenus Genus { get; set; }
        public ResonatorParameter(AsterGenus genus) {
            Genus = genus;
        }
    }
}