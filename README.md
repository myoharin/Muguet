# Muguet
Muguet all in one music theory base components for c# development. Supports flexible / generic interfaces and definitions of Pitches, PitchInterval, PitchClass, Scales and Chords.

## Table of Contents
- [Core Components](#core-components)
   - [IReadOnlyPitch](#1.1-ireadonlypitch)
   - [Pitch](#1.2-pitch)
     - [CompoundPitch](#compound-pitch)
     - [CustomTetPitch](#custom-tet-pitch)
     - [FloatPitch](#float-pitch)
     - [MidiPitch](#midi-pitch)
   - [IReadOnlyPitchInterval](#1.3-ireadonlyinterval)
   - [PitchInterval](#1.4-pitchinterval)
     - [CompoundPitchInterval](#compound-pitch-interval)
     - [CustomTetPitchInterval](custom-tet-pitch-interval)
     - [FloatPitchInterval](#float-pitch-interval)
     - [JustIntonalPitchInterval](just-intonal-pitch-interval)
     - [MidiPitchInterval](#midi-pitch-interval)
   - [PitchClass](#pitchclass)
   - [Scale](#scale)
   - [Chord](#chord)
- [Features](#features)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)

## 1.0 Core Components #core-components

## 1.1 IReadOnlyPitch

### _Properties_

- **Frequency**: `double`
    - Returns the frequency of the pitch in Hertz (Hz).

- **NoteName**: `string`
    - Returns the name of the note (e.g., "C#", "G", etc.) associated with the pitch.

- **ToMidiIndex**: `int`
    - Returns the pitch to its closest corresponding MIDI index.

- **ToMidiValue**: `float`
    - Returns the pitch as its corresponding MIDI value.


### _Methods_

- `Pitch` **Incremented(`PitchInterval` interval)**:
    - Returns a new `Pitch` that is incremented by the specified `PitchInterval`.
    - Parameters:
        - `PitchInterval` _interval_: The interval by which to increment the pitch.

- `Pitch` **Decremented(`PitchInterval` interval)**:
    - Returns a new `Pitch` that is decremented by the specified `PitchInterval`.
    - Parameters:
        - `interval`: The interval by which to decrement the pitch.

- `Pitch` **ToPitch()**:
    - Converts the current instance to a `Pitch` object.

- `IReadOnly` **AsReadOnly()**:
    - Returns a read-only interface of the current `Pitch` instance.
    - Returns: `IReadOnlyPitch` which provides access to pitch information without allowing modifications.

### _Comparison Operators_
- Description: Compares two `IReadOnlyPitch` or `double` instances for equality.
- Operators Supported:
  - **==**
  - **!=**
  - **<**
  - **<=**
  - **>**
  - **>=**
- Returns: `true` if the pitches are equal; otherwise, `false`.
- Returns: `true` if the pitches are equal; otherwise, `false`.


## 1.2 Pitch

Represents a musical pitch with a specific frequency.

**Multiple implementations available:**
- `FloatPitch`: Direct frequency representation in Hz as a double
- `MidiPitch`: 12-TET (12-tone equal temperament) pitch representation using MIDI index
- `CustomTetPitch`: Custom equal temperament systems (e.g., 19-TET, 31-TET)
- `CompoundPitch`: Complex pitch representations combining a base-pitch and an interval

**Implments:**
- `IReadOnlyPitch`
- `IEquatable<Pitch>`
- `ICloneable`
- `IComparable<Pitch>`

### _Instance Properties_
### _Instance Methods_
### _Operators_
### _Static Properties_
### _Static Methods_

## 1.3 IReadOnlyPitchInterval

### _Properties_

- **FrequencyRatio**: `double`
    - Returns the frequency ratio of the pitch interval as a double.

- **IsAbsolute**: `bool`
    - Indicates whether the pitch interval is an absolute ratio: FrequencyRatio >= 1.

- **IsUnison**: `bool`
    - Indicates whether the pitch interval is unison: FrequencyRatio == 1.

- **ToMidiIndex**: `int`
    - Returns the pitch interval as its corresponding interval indexed in 12 TET.

- **ToMidiValue**: `float`
    - Returns the pitch interval as its corresponding interval indexed in 12 TET.

### _Methods_

- `PitchInterval` **Incremented(`PitchInterval` interval)**:
    - Returns a new `PitchInterval` that is incremented by the specified `PitchInterval`.
    - Parameters:
        - `PitchInterval`: The interval by which to increment the pitch.


- `PitchInterval` **Decremented(`PitchInterval` interval)**:
    - Returns a new `PitchInterval` that is decremented by the specified `PitchInterval`.
    - Parameters:
        - `PitchInterval`: The interval by which to decrement the pitch.


- `PitchInterval` **ToPitchInterval()**:
    - Converts the current instance to a `PitchInterval` object.


- `IReadOnlyPitchInterval` **AsReadOnly()**: 
    - Returns a read-only version of the current `PitchInterval` instance.
    - Returns: `IReadOnlyPitchInterval` which provides access to pitch information without allowing modifications.


#### _Comparison Operators_
- Compares two `IReadOnlyPitchInterval` or `double` instances for equality.
- Operators Supported:
    - **==**
    - **!=**
    - **<**
    - **<=**
    - **>**
    - **>=**
- Returns: `true` if the pitches are equal; otherwise, `false`.
- Returns: `true` if the pitches are equal; otherwise, `false`.


## 1.4 PitchInterval

Represents the interval between two pitches as a frequency ratio.

**Multiple implementations available:**
- `FloatPitchInterval`: Direct frequency ratio representation
- `MidiPitchInterval`: 12-TET interval (semitone-based)
- `JustIntonalPitchInterval`: Rational number intervals (e.g., 3/2 for perfect fifth)
- `CustomTetPitchInterval`: Custom equal temperament intervals
- `CompoundPitchInterval`: Combination of multiple intervals

**Implements:**
- `IReadOnlyPitchInterval`
- `IEquatable<PitchInterval>`
- `ICloneable`
- `IComparable<PitchInterval>`
- `INumber`

### _Instance Properties_
- **CentOffsets**: `int`
    - How many cents to offset the pitch interval by, where 1200 cents makes an octave.
### _Derived Gets_
- **FrequencyRatio**: `double`
  - The frequency ratio of the class a double, determined by the class's implementation.
- **IsAbsolute**: `bool`
  - Returns true if `FrequencyRatio` is >= 1.
- **IsUnison**: `bool`
  - Returns true only if `FrequencyRatio` is equal to 0.
- **IntervalName** `string`
  - Returns the interval name as string internal name for the purpose of 12 TET.
  - In order and increment by 1 at each octave: 
    - U
    - m2
    - M2
    - m3
    - M3
    - P4
    - T1
    - P5
    - m6
    - M6
    - m7
    - M7
    - U1

### _Instance Methods_

- `PitchInterval` **Inverted()**:
  - Returns the inverted result of the `PitchInterval` without altering its base value.
- `PitchInterval` **Incremented(`PitchInterval` interval)**:
    - Returns the incremented result of the `PitchInterval` by `interval` without altering its base value.
- `PitchInterval` **Decremented(`PitchInterval` interval)**:
    - Returns the decremented result of the `PitchInterval` by `interval` without altering its base value.

### _Implementation Abstracts_
- `Protected Constructor` **PitchInterval(`int` centOffset = 0)**
- `double` **GetFrequencyRatio()**:
  - A function to returns the frequency ratio, and is the primary override.
- `string` **ToJson()**:
  - A function to convert current class to a json String.
- `void` **Invert()**:
  - A function to alter implementation's values which leads to FrequencyRatio = 1/FrequencyRatio.
- `void` **Increment(`PitchInterval` interval)**:
  - A function to alter the implementation's value which leads to the incrementation of the FrequencyRatio by the interval.
- `void` **Decrement(`PitchInterval` interval)**:
    - A function to alter the implementation's value which leads to the decrementation of the FrequencyRatio by the interval.
### _Operators_
### _Static Properties_
- **Unison**: `PitchInterval`
    - Returns a `FloatPitchInterval` of ratio 1.0, otherwise Unison
- **Perfect5th**: `PitchInterval`
    - Returns a `JustIntonalPitchInterval` of 3:2
- **Perfect4th**: `PitchInterval`
    - Returns a `JustIntonalPitchInterval` of 4:3
- **PerfectFifth**: `PitchInterval`
    - Returns a `JustIntonalPitchInterval` of 3:2
- **PerfectFourth**: `PitchInterval`
    - Returns a `JustIntonalPitchInterval` of 4:3

- **Empty**: `PitchInterval`
    - Returns `Unison`
- **Default**: `PitchInterval`
    - Returns `Unison`
### _Static Methods_
- `PitchInterval` **FromJson(`string` jsonString)**:
    - Takes a json file as a string and return a PitchInterval.
    - json file format is intuitive but still must be
    - Supports the all PitchInterval implementations in library.
    - Parameters:
        - `string` _jsonString_: The json file as a string.

## 1.5 IReadOnlyChord

Represents a collection of pitches with automatic sorting and manipulation.

**Implements:**
- `IReadOnlyList<IReadOnlyPitch>`
- `IList<IReadOnlyPitch>`
- `IList`

### _Properties_

- **Notes**: `IReadOnlyList<IReadOnlyPitch>`
    - Returns the notes of the chord as an immutable list.


- **Range**: `PitchInterval`
    - Returns the range of the chord, calculating the difference between the terminal the root.


- **Root**: `Pitch`
    - Returns the root / the lowest pitch of the chord.


- **Terminal**: `Pitch`
    - Returns the terminal / the highest pitch of the chord.

### _Methods_
### _Operators_

## 1.6 Chord

Represents a collection of pitches with automatic sorting and manipulation.

**Implements:**
- IReadOnlyChord

### _Instance Properties_
- **Notes**: `List<Pitch>`
    - Returns the notes of the chord as a mutable list.

### _Instance Methods_
- `Chord` **Add(`Pitch` pitch)**:
    - Adds a pitch to the chord.
    - Parameters:
        - `Pitch` _pitch_: The pitch to add.
    

- `void` **SetChord(`ICollection<Pitch>` notes)**:
    - Sets the notes of the chord.


- `void` **SetNotes(`ICollection<Pitch>` notes)**:
    - Sets the notes of the chord.


- `void` **Modulate(`PitchInterval` interval, `bool` up = `true`)**:
    - Modulates the chord by the specified interval.
    - Parameters:
        - `PitchInterval` _interval_: The interval by which to modulate the chord.
        - `bool` _up_: Indicates whether to modulate up or down.`


- `void` **ModulateUp(`PitchInterval` pitch)**:
    - Modulates the chord by the specified interval.
    - Parameters:
      - `PitchInterval` _pitch_: The interval by which to modulate the chord.


- `void` **ModulateDown(`PitchInterval` pitch)**:
  - Modulates the chord down by the specified interval.
  - Parameters:
    - `PitchInterval` _pitch_: The pitch by which to modulate the chord.


- `Chord` **Modulated(`PitchInterval` interval, `bool` up = `true`)**:
    - Parameters:
        - `PitchInterval` _interval_: The interval by which to modulate the chord.
        - `bool` _up_: Indicates whether to modulate up or down.
    - Returns: `Chord` which is a copy of the current chord with the specified interval modulated.

### _Static Properties_
-  **Empty:** `Chord`
    - Returns an empty `Chord` instance.

### _Static Methods_
- `Chord` **CreateMajorTriad(`Pitch root`)**:
    - Parameters:
        - `Pitch` _root_: The root of the triad.
    - Returns an atomic major triad `Chord` formed according to `MidiPitchIntervals`.

- `Chord` **CreateMinorTriad(`Pitch root`)**:
    - Parameters:
        - `Pitch` _root_: The root of the triad.
    - Returns an atomic minor triad `Chord` formed according to `MidiPitchIntervals`.




## 1.7 PitchClass

Represents pitch equivalence classes (octave equivalency). Treats all pitches separated by octaves as equivalent.

### _Instance Properties_
### _Instance Methods_
### _Operators_
### _Static Properties_
### _Static Methods_


## 1.8 Scale

Abstract representation of musical scales with multiple implementations:

- **ChordReferencedScale**: Scales derived from chord structures
- **CustomTetScale**: Scales in custom equal temperament systems
- **PitchClassScale**: Scale built from pitch classes

### _Instance Properties_
### _Instance Methods_
### _Operators_
### _Static Properties_
### _Static Methods_

