using System;
using System.IO;

namespace Chroma.Audio.Sfxr
{
    public sealed class Sfxr
    {
        public static void Initialize(Game game)
        {
            if (game.Content.IsImporterPresent<SfxrWaveform>())
                game.Content.UnregisterImporter<SfxrWaveform>();

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