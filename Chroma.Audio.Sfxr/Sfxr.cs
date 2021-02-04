using System;
using System.IO;

namespace Chroma.Audio.Sfxr
{
    public static class Sfxr
    {
        public static void Initialize(Game game)
        {
            game.Content.RegisterImporter<SfxrWaveform>(
                ((path, args) =>
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
                })
            );
        }
    }
}