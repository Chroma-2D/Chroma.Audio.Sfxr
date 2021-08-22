using Chroma;
using Chroma.Input;
using Chroma.Audio.Sfxr;
using Chroma.Graphics;

namespace ChromaSfxrExample
{
    public class GameCore : Game
    {
        private static readonly SfxrParams _coinParams =
            new("0,,0.0736,0.4591,0.3858,0.5416,,,,,,0.5273,0.5732,,,,,,1,,,,,0.5");

        private static readonly SfxrParams _laserParams =
            new("0,,0.0359,,0.4491,0.2968,,0.2727,,,,,,0.0191,,0.5249,,,1,,,,,0.5");

        private static readonly SfxrParams _explosionParams =
            new("3,,0.3822,0.4799,0.4721,0.3917,,-0.3271,,,,-0.4969,0.8651,,,,0.5645,-0.1034,1,,,,,0.5");

        private static readonly SfxrParams _sirenParams =
            new(
                "2,0.0028,0.9527,0.1807,0.4139,0.5534,,0.0022,-0.0816,,0.9387,-0.9916,,0.8259,0.0015,0.068,-0.2339,-0.132,0.8973,-0.0149,0.0783,0.0453,,0.5");

        private readonly SfxrWaveform _coinWaveform;
        private readonly SfxrWaveform _laserWaveform;
        private readonly SfxrWaveform _explosionWaveform;
        private readonly SfxrWaveform _sirenWaveform;
        private SfxrWaveform _boomWaveform;

        public GameCore() : base(new GameStartupOptions(false))
        {
            _coinWaveform = new(_coinParams);
            _laserWaveform = new(_laserParams);
            _explosionWaveform = new(_explosionParams);
            _sirenWaveform = new(_sirenParams);
        }

        protected override void LoadContent()
        {
            _boomWaveform = Content.Load<SfxrWaveform>("Sound/boom.sfxr", ParameterFormat.Binary);
        }

        protected override void Draw(RenderContext context)
        {
            context.DrawString(
                $"[F1] Coin ({_coinWaveform.Volume}): {_coinWaveform.Status}\n" +
                $"[F2] Laser ({_laserWaveform.Volume}): {_laserWaveform.Status}\n" +
                $"[F3] Explosion ({_explosionWaveform.Volume}): {_explosionWaveform.Status}\n" +
                $"[F4] Boom ({_boomWaveform.Volume}): {_boomWaveform.Status}\n" +
                $"[F5] Siren ({_sirenWaveform.Volume}): {_sirenWaveform.Status}",
                new(16)
            );
        }

        protected override void KeyPressed(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case KeyCode.F1:
                    _coinWaveform.Play();
                    break;

                case KeyCode.F2:
                    _laserWaveform.Play();
                    break;

                case KeyCode.F3:
                    _explosionWaveform.Play();
                    break;

                case KeyCode.F4:
                    _boomWaveform.Play();
                    break;
                
                case KeyCode.F5:
                    _sirenWaveform.Play();
                    break;
            }
        }
    }
}