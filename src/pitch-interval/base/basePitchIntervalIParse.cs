using System.Globalization;

namespace SineVita.Muguet
{
    public abstract partial class PitchInterval
    {
        // * IParse
        public static PitchInterval Parse(string s, IFormatProvider? provider) { // TODO
            throw new NotImplementedException();
        }  
        public static PitchInterval Parse(ReadOnlySpan<char> s) => Parse(new string(s));
        public static PitchInterval Parse(string s) => Parse(s, null);
        public static PitchInterval Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(new string(s), provider);
        
        public static bool TryParse(string? s, IFormatProvider? provider, out PitchInterval result) { // TODO
            throw new NotImplementedException();
        }  
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out PitchInterval result) => TryParse(new string(s), provider, out result);
        public static bool TryParse(string? s, out PitchInterval result) => TryParse(s, null, out result);
        public static bool TryParse(ReadOnlySpan<char> s, out PitchInterval result) => TryParse(new string(s), null, out result);
        
        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out PitchInterval result) => throw new NotImplementedException(); // TODO
        public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out PitchInterval result) => throw new NotImplementedException(); // TODO

    }
}