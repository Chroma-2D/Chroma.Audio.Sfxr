### Sfxr Playback Engine for Chroma Framework
Utilizing the flexible audio system of Chroma Engine, it is possible to create audio generators for different audio formats. This project serves as an extension for Chroma Audio System and an example on how to take advantage of the Waveform class.

### Usage
Clone the project and add it as a reference to your project or install the NuGet package `Chroma.Audio.Sfxr`.  
Once done, you can optionally initialize the content importer using `Chroma.Audio.Sfxr.Sfxr.Initialize(Game)` from your game's constructor.

#### Importing files
This satellite library supports importing both as3sfxr's parameter format and original sfxr's binary format versions 100, 101 and 102. To import using the built-in content provider after initialization, you should use:
```
Content.Load<SfxrWaveform>("Relative/Path/To/sound.sfxr", ParameterFormat.Binary);
```
You can change the parameter format to `ParameterFormat.String` if you want to load as3sfxr's parameter files.  
If you want to skip the content provider you can use `SfxrWaveform` constructor directly - it'll work just as well.

### Credits
See AUTHORS.md for information on original authors who paved the way for this project.
