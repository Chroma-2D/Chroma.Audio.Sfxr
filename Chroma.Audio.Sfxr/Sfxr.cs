using System;
using System.IO;
using Chroma.Extensibility;

namespace Chroma.Audio.Sfxr
{
    [EntryPoint]
    internal sealed class Sfxr
    {
        internal Sfxr(Game game)
        {
            game.Content.RegisterImporter<SfxrWaveform>(
                (path, args) =>
                {
                    if (args.Length != 1)
                        throw new ArgumentException("SfxrWaveform requires a parameter format argument for import.");

                    var format = (ParameterFormat)args[0];
                    using (var fs = new FileStream(path, FileMode.Open))
                    {
                        return new SfxrWaveform(
                            new SfxrParams(fs, format)
                        );
                    }
                }
            );
        }
    }
}