using System;
using System.Security.Permissions;
using System.Numerics;
using System.Text.Json;

namespace SineVita.Muguet {

    public abstract class Pitch : 
            IComparable, 
            ICloneable, 
            IEquatable<Pitch>  // made redunant later
            // ! NOT DONE
            
            // INumber<Pitch>,
            // IUnsignedNumber<Pitch>,
            // IMinMaxValue<Pitch>,

            // IAdditiveIdentity<Pitch,PitchInterval>,
            // IMultiplicativeIdentity<Pitch,PitchInterval>


            // LINK https://learn.microsoft.com/en-us/dotnet/standard/generics/math
        {
        // * Properties
        protected int _centOffsets;
        public virtual int CentOffsets { 
            get {
                return _centOffsets;
            }
            set {
                _centOffsets = value;
            }
         }

        // * Derived Gets
        public string NoteName { get { return HarmonyHelper.HtzToNoteName(this.Frequency); } }
        public double Frequency { get { return this.GetFrequency(); } }
        public virtual int ToMidiIndex { get {
            return (int)MidiPitch.ToIndex(Frequency);
        } }

        // * statics
        public static FloatPitch New(double frequency) {return new FloatPitch(frequency);}
        public static Pitch Empty { get { return new FloatPitch(256f); } }
        private static readonly string[] noteNames = new string[] {
            "C", "C#/Db", "D", "D#/Eb", "E", "F", "F#/Gb", "G", "G#/Ab", "A", "A#/Bb", "B"
        };
        public static string[] NoteNames { get { return noteNames; } }

        // * FromJson
        public static Pitch FromJson(string jsonString) {
            var jsonDocument = JsonDocument.Parse(jsonString);
            var rootElement = jsonDocument.RootElement;
            var type = System.Type.GetType(rootElement.GetProperty("Type").GetString() ?? throw new ArgumentException("Invalid JsonString"));
            switch (type) {
                case Type t when t == typeof(FloatPitch):
                    return FloatPitch.FromJson(jsonString);
                case Type t when t == typeof(CustomTetPitch):
                    return CustomTetPitch.FromJson(jsonString);
                case Type t when t == typeof(MidiPitch):
                    return MidiPitch.FromJson(jsonString);
                case Type t when t == typeof(CompoundPitch):
                    return CompoundPitch.FromJson(jsonString);
                default:
                    throw new ArgumentException("Invalid pitch type");
            }
        }

        // * Constructor
        protected Pitch(int centOffsets = 0) {
            CentOffsets = centOffsets;
        }

        // * virtual methods
        public virtual double GetFrequency() { return 0; }
        public virtual string ToJson() {return "";}

        // * Incrementation - base type preserved
        public Pitch Incremented(PitchInterval interval) {
            var newPitch = (Pitch)this.Clone();
            newPitch.Increment(interval);
            return newPitch;
        }
        public Pitch Decremented(PitchInterval interval) {
            var newPitch = (Pitch)this.Clone();
            newPitch.Decrement(interval);
            return newPitch;
        }
        public PitchInterval CreateInterval(Pitch pitch2, bool absoluteInterval = false) {
            return PitchInterval.CreateInterval(this, pitch2, absoluteInterval);
        }

        public abstract void Increment(PitchInterval interval);
        public abstract void Decrement(PitchInterval interval);

        // * Interfaces
        public int CompareTo(object? obj) { // ! NOT DONE - account for numerics comparison as well
            if (obj == null) return 1; // Null is considered less than any object
            if (obj is Pitch otherPitch) {
                return Frequency.CompareTo(otherPitch.Frequency); // Compare by Frequency
            }
            throw new ArgumentException("Object is not a Pitch");
        }
        public abstract object Clone();

        public override bool Equals(object? obj) { // ! NOT DONE - account for numerics comparison as well
            if (obj == null || GetType() != obj.GetType()) {return false;}
            Pitch other = (Pitch)obj;
            return Math.Abs(Frequency - other.Frequency) < 0.0001;
        }
        public bool Equals(Pitch? other) {
            if (other == null || GetType() != other.GetType()) {return false;}
            return Math.Abs(Frequency - other.Frequency) < 0.0001;
        }
        
        
        public override int GetHashCode() {
            return Frequency.GetHashCode();
        }
        public static bool operator ==(Pitch? left, Pitch? right) {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        public static bool operator !=(Pitch left, Pitch right) {
            return !(left == right);
        }
        public static bool operator <(Pitch left, Pitch right) {
            return left is not null && left.CompareTo(right) < 0;
        }
        public static bool operator <=(Pitch left, Pitch right) {
            return left is null || left.CompareTo(right) <= 0;
        }
        public static bool operator >(Pitch left, Pitch right) {
            return left is not null && left.CompareTo(right) > 0;
        }
        public static bool operator >=(Pitch left, Pitch right) {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
        
        public static bool operator ==(double left, Pitch right) {
            return Math.Abs(left - right.Frequency) < 0.001;
        }
        public static bool operator !=(double left, Pitch right) {
            return !(left == right);
        }
        public static bool operator <(double left, Pitch right) {
            return left.CompareTo(right.Frequency) < 0;
        }
        public static bool operator <=(double left, Pitch right) {
            return left.CompareTo(right.Frequency) <= 0;
        }
        public static bool operator >(double left, Pitch right) {
            return left.CompareTo(right.Frequency) > 0;
        }
        public static bool operator >=(double left, Pitch right) {
            return left.CompareTo(right.Frequency) >= 0;
        }

        public static bool operator ==(Pitch left, double right) {
            return Math.Abs(left.Frequency - right) < 0.001;
        }
        public static bool operator !=(Pitch left, double right) {
            return !(left == right);
        }
        public static bool operator <(Pitch left, double right) {
            return left is not null && left.Frequency.CompareTo(right) < 0;
        }
        public static bool operator <=(Pitch left, double right) {
            return left is null || left.Frequency.CompareTo(right) <= 0;
        }
        public static bool operator >(Pitch left, double right) {
            return left is not null && left.Frequency.CompareTo(right) > 0;
        }
        public static bool operator >=(Pitch left, double right) {
            return left is not null ? left.Frequency.CompareTo(right) >= 0 : false ;
        }
        

            // arithmetic operations
        public static Pitch operator +(Pitch pitch, PitchInterval pitchInterval) {
            pitch.Increment(pitchInterval);
            return pitch;
        }
        public static Pitch operator +(PitchInterval pitchInterval, Pitch pitch) {
            pitch.Increment(pitchInterval);
            return pitch;
        }

        public static Pitch operator -(Pitch pitch, PitchInterval pitchInterval) {
            pitch.Decrement(pitchInterval);
            return pitch;
        }
        public static PitchInterval operator -(Pitch upperPitch, Pitch basePitch) {
            return PitchInterval.CreateInterval(basePitch, upperPitch);
        }
     }

}
