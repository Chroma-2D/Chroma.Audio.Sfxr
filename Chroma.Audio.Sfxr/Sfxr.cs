using System;
using System.IO;

namespace Chroma.Audio.Sfxr
{
    public sealed class Sfxr
    {
        private static bool _initialized;
        
        public static void Initialize(Game game)
        {
            if (_initialized)
                throw new InvalidOperationException("Sfxr was already initialized.");
            
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

            _initialized = true;
        }
    }
}