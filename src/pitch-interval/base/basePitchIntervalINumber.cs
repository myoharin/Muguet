using System.Numerics;
using System.Globalization;

namespace SineVita.Muguet {
    public abstract partial class PitchInterval
             : INumber<PitchInterval>, IUnsignedNumber<PitchInterval>, IMinMaxValue<PitchInterval>
        {
        
        public static PitchInterval MaxValue => new FloatPitchInterval(double.MaxValue);
        public static PitchInterval MinValue => new FloatPitchInterval(0);

        // * IOperators
        public static PitchInterval operator +(PitchInterval left, PitchInterval right) => left.Incremented(right);
        public static PitchInterval operator checked +(PitchInterval left, PitchInterval right) => left.Incremented(right);
        public static PitchInterval operator -(PitchInterval left, PitchInterval right) => left.Decremented(right);
        public static PitchInterval operator checked -(PitchInterval left, PitchInterval right) => left.Decremented(right);
        public static PitchInterval operator *(PitchInterval left, PitchInterval right) => left.Incremented(right);
        public static PitchInterval operator checked *(PitchInterval left, PitchInterval right) => left.Incremented(right);
        public static PitchInterval operator /(PitchInterval left, PitchInterval right) => left.Decremented(right);
        public static PitchInterval operator checked  /(PitchInterval left, PitchInterval right) => left.Decremented(right);
        public static PitchInterval operator %(PitchInterval left, PitchInterval right) {
            bool sign = IsPositive(left);
            left = Abs((PitchInterval)left.Clone());
            right = Abs(right);
            while (left >= right) {
                left.Decrement(right);
            }
            return sign ? left : left.Inverted();
        }
        
        public static PitchInterval Zero => Default;
        public static PitchInterval One => Default;
        
        // * IIdentity
        public static PitchInterval AdditiveIdentity => PitchInterval.Default;
        public static PitchInterval MultiplicativeIdentity => PitchInterval.Default;
        
        public static PitchInterval Abs(PitchInterval value) => value.IsAbsolute ? value : value.Inverted();
        public static PitchInterval Clamp(PitchInterval value, PitchInterval min, PitchInterval max) {
            if (value <= min) {
                return min;
            } else if (value >= max) {
                return max;
            } else {
                return value;
            }
        }
        public static PitchInterval Create<TOther>(TOther value) where TOther : INumber<TOther> => new FloatPitchInterval(Convert.ToDouble(value));
        public static PitchInterval CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther> => new FloatPitchInterval(Convert.ToDouble(value));
        
        public static bool IsNegative(PitchInterval value) => value.FrequencyRatio < 1d;
        public static bool IsPositive(PitchInterval value) => value.FrequencyRatio > 1d;
        
        public static string ToBase(PitchInterval value) => throw new NotImplementedException(); // TODO
        public static PitchInterval From(string value) => throw new NotImplementedException(); // TODO
        public static int Radix => 10;

        public static bool IsNegativeInfinity(PitchInterval value) => value.FrequencyRatio <= 0;
        public static bool IsPositiveInfinity(PitchInterval value) => double.IsPositiveInfinity(value.FrequencyRatio);
        public static bool IsInfinity(PitchInterval value) => value.FrequencyRatio <= 0 || double.IsPositiveInfinity(value.FrequencyRatio);
        public static bool IsNaN(PitchInterval value) => double.IsNaN(value.FrequencyRatio);
        public static bool IsFinite(PitchInterval value) => double.IsFinite(value.FrequencyRatio);
        
        public static PitchInterval Max(PitchInterval x, PitchInterval y) => x <= y ? y : x;
        public static PitchInterval Min(PitchInterval x, PitchInterval y) => x >= y ? y : x;
        
        public static PitchInterval operator ++(PitchInterval value) {
            value.Increment(new MidiPitchInterval(1));
            return value;
        }
        public static PitchInterval operator --(PitchInterval value) {
            value.Decrement(new MidiPitchInterval(1));
            return value;
        }

        public static PitchInterval operator checked ++(PitchInterval value) => ++value;
        public static PitchInterval operator checked --(PitchInterval value) => --value;
        
        public static PitchInterval operator +(PitchInterval value) => value;
        public static PitchInterval operator -(PitchInterval value) => value.Inverted();
        public static PitchInterval operator checked -(PitchInterval value) => value.Inverted();
        
        // * I-dk
        public static bool IsCanonical(PitchInterval value) => value is FloatPitchInterval;
        public static bool IsComplexNumber(PitchInterval value) => false;
        public static bool IsEvenInteger(PitchInterval value) => double.IsEvenInteger(value.FrequencyRatio);
        public static bool IsImaginaryNumber(PitchInterval value) => false;
        public static bool IsInteger(PitchInterval value) => double.IsInteger(value.FrequencyRatio);
        public static bool IsNormal(PitchInterval value) => double.IsNormal(value.FrequencyRatio);
        public static bool IsOddInteger(PitchInterval value) => double.IsOddInteger(value.FrequencyRatio);
        public static bool IsRealNumber(PitchInterval _) => true;
        public static bool IsSubnormal(PitchInterval value) => double.IsSubnormal(value.FrequencyRatio);
        public static bool IsZero(PitchInterval value) => IsNegativeInfinity(value);

        public static PitchInterval MaxMagnitude(PitchInterval x, PitchInterval y) => throw new NotImplementedException();  // TODO
        public static PitchInterval MaxMagnitudeNumber(PitchInterval x, PitchInterval y) => throw new NotImplementedException();  // TODO
        public static PitchInterval MinMagnitude(PitchInterval x, PitchInterval y) => throw new NotImplementedException(); // TODO
        public static PitchInterval MinMagnitudeNumber(PitchInterval x, PitchInterval y) => throw new NotImplementedException(); // TODO
        
        public static PitchInterval Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException(); // TODO
        public static PitchInterval Parse(string s, NumberStyles style, IFormatProvider? provider) => throw new NotImplementedException(); // TODO
        
        // * WHAT IS THIS
        public static bool TryConvertFromChecked<TOther>(TOther value, out PitchInterval result) where TOther : INumberBase<TOther> => throw new NotImplementedException(); // TODO
        public static bool TryConvertFromSaturating<TOther>(TOther value, out PitchInterval result) where TOther : INumberBase<TOther> => throw new NotImplementedException(); // TODO
        public static bool TryConvertFromTruncating<TOther>(TOther value, out PitchInterval result) where TOther : INumberBase<TOther> => throw new NotImplementedException(); // TODO
        public static bool TryConvertToChecked<TOther>(PitchInterval value, out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException(); // TODO
        public static bool TryConvertToSaturating<TOther>(PitchInterval value, out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException(); // TODO
        public static bool TryConvertToTruncating<TOther>(PitchInterval value, out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException(); // TODO
        
        public string ToString(string? format, IFormatProvider? formatProvider) {  // TODO
            throw new NotImplementedException();
        }
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? formatProvider) {  // TODO
            // Example implementation (you can adjust as needed)
            string result = ToString(format.ToString(), formatProvider);
            if (result.Length > destination.Length) {
                charsWritten = 0;
                return false;
            }
            result.AsSpan().CopyTo(destination);
            charsWritten = result.Length;
            return true;
        }
    
    }
}