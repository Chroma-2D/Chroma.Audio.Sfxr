using System;
using Chroma.Audio.Sources;

namespace Chroma.Audio.Sfxr
{
    public class SfxrWaveform : Waveform
    {
        private WaveShape _waveShape;
        private readonly Random _random = new();
        private readonly SfxrParams _localParams;

        private double _masterVolume;
        private double _envelopeVolume;
        private int _envelopeStage;
        private double _envelopeTime;
        private double _envelopeLength;
        private double _envelopeLength0;
        private double _envelopeLength1;
        private double _envelopeLength2;
        private double _envelopeOverLength0;
        private double _envelopeOverLength1;
        private double _envelopeOverLength2;
        private double _envelopeFullLength;

        private double _sustainPunch;

        private int _phase;
        private double _pos;
        private double _period;
        private double _periodTemp;
        private double _maxPeriod;

        private double _slide;
        private double _deltaSlide;
        private double _minFrequency;

        private double _vibratoPhase;
        private double _vibratoSpeed;
        private double _vibratoAmplitude;

        private double _changeAmount;
        private int _changeTime;
        private int _changeLimit;

        private double _squareDuty;
        private double _dutySweep;

        private int _repeatTime;
        private int _repeatLimit;

        private bool _phaser;
        private double _phaserOffset;
        private double _phaserDeltaOffset;
        private int _phaserInt;
        private int _phaserPos;
        private readonly double[] _phaserBuffer = new double[1024];

        private bool _filters;
        private double _lpFilterPos;
        private double _lpFilterOldPos;
        private double _lpFilterDeltaPos;
        private double _lpFilterCutoff;
        private double _lpFilterDeltaCutoff;
        private double _lpFilterDamping;
        private bool _lpFilterOn;

        private double _hpFilterPos;
        private double _hpFilterCutoff;
        private double _hpFilterDeltaCutoff;

        private readonly double[] _noiseBuffer = new double[32];

        private double _superSample;
        private double _sample;

        private byte[] _sampleBuffer;
        private int _playedSamples;

        public SfxrWaveform(SfxrParams sfxrParams) :
            base(new AudioFormat(SampleFormat.S16),
                null,
                ChannelMode.Mono)
        {
            _localParams = new SfxrParams(sfxrParams);
            SampleGenerator = StreamEffect;
            
            Reset(true);
            Synthesize();
        }

        public SfxrWaveform(string sfxrParams)
            : this(new SfxrParams(sfxrParams))
        {
        }

        public override void Play()
        {
            if (Status == PlaybackStatus.Playing)
            {
                Stop();
            }
            
            Status = PlaybackStatus.Playing;
            base.Play();
        }
        
        public override void Stop()
        {
            Status = PlaybackStatus.Stopped;
            
            _playedSamples = 0;
            Pause();
        }

        public SfxrWaveform Mutate(SfxrParams sfxrParams, double mutation)
        {
            return new(sfxrParams.MutateClone(mutation));
        }
        
        private void StreamEffect(Span<byte> data, AudioFormat format)
        {
            if (_playedSamples < _sampleBuffer.Length)
            {
                for (var i = 0; i < data.Length; i++)
                {
                    if (_playedSamples >= _sampleBuffer.Length)
                        break;

                    data[i] = _sampleBuffer[_playedSamples++];
                }
            }
            else
            {
                Stop();
            }
        }


        private void Synthesize()
        {
            var samples = new int[(int)_envelopeFullLength];
            var samplesInBuffer = 0;

            var finished = false;

            for (var i = 0; i < samples.Length; i++)
            {
                if (finished)
                    break;

                // Repeats every _repeatLimit times, partially resetting the sound parameters
                if (_repeatLimit != 0)
                {
                    if (++_repeatTime >= _repeatLimit)
                    {
                        _repeatTime = 0;
                        Reset(false);
                    }
                }

                // If _changeLimit is reached, shifts the pitch
                if (_changeLimit != 0)
                {
                    if (++_changeTime >= _changeLimit)
                    {
                        _changeLimit = 0;
                        _period *= _changeAmount;
                    }
                }

                // Acccelerate and apply slide
                _slide += _deltaSlide;
                _period *= _slide;

                // Checks for frequency getting too low, and stops the sound if a minFrequency was set
                if (_period > _maxPeriod)
                {
                    _period = _maxPeriod;
                    if (_minFrequency > 0.0)
                        finished = true;
                }

                _periodTemp = _period;

                // Applies the vibrato effect
                if (_vibratoAmplitude > 0.0)
                {
                    _vibratoPhase += _vibratoSpeed;
                    _periodTemp = _period * (float)(1.0 + Math.Sin(_vibratoPhase) * _vibratoAmplitude);
                }

                _periodTemp = (int)_periodTemp;
                if (_periodTemp < 8)
                    _periodTemp = 8;

                // Sweeps the square duty
                if (_waveShape == 0)
                {
                    _squareDuty += _dutySweep;
                    if (_squareDuty < 0.0f)
                        _squareDuty = 0.0f;
                    else if (_squareDuty > 0.5f)
                        _squareDuty = 0.5f;
                }

                // Moves through the different stages of the volume envelope
                if (++_envelopeTime > _envelopeLength)
                {
                    _envelopeTime = 0;

                    switch (++_envelopeStage)
                    {
                        case 1:
                            _envelopeLength = _envelopeLength1;
                            break;
                        case 2:
                            _envelopeLength = _envelopeLength2;
                            break;
                    }
                }

                // Sets the volume based on the position in the envelope
                switch (_envelopeStage)
                {
                    case 0:
                        _envelopeVolume = _envelopeTime * _envelopeOverLength0;
                        break;
                    case 1:
                        _envelopeVolume = 1.0f + (1.0f - _envelopeTime * _envelopeOverLength1) * 2.0f * _sustainPunch;
                        break;
                    case 2:
                        _envelopeVolume = 1.0f - _envelopeTime * _envelopeOverLength2;
                        break;
                    case 3:
                        _envelopeVolume = 0.0f;
                        finished = true;
                        break;
                }

                // Moves the phaser offset
                if (_phaser)
                {
                    _phaserOffset += _phaserDeltaOffset;
                    _phaserInt = (int)_phaserOffset;
                    if (_phaserInt < 0)
                        _phaserInt = -_phaserInt;
                    else if (_phaserInt > 1023)
                        _phaserInt = 1023;
                }

                // Moves the high-pass filter cutoff
                if (_filters && _hpFilterDeltaCutoff != 0.0)
                {
                    _hpFilterCutoff *= _hpFilterDeltaCutoff;
                    if (_hpFilterCutoff < 0.00001f)
                        _hpFilterCutoff = 0.00001f;
                    else if (_hpFilterCutoff > 0.1f)
                        _hpFilterCutoff = 0.1f;
                }

                _superSample = 0.0f;
                for (var j = 0; j < 8; j++)
                {
                    // Cycles through the period
                    _phase++;
                    if (_phase >= _periodTemp)
                    {
                        _phase = _phase - (int)_periodTemp;

                        // Generates new random noise for this period
                        if (_waveShape == WaveShape.Noise)
                        {
                            for (var n = 0; n < 32; n++)
                                _noiseBuffer[n] = (float)_random.NextDouble() * 2.0f - 1.0f;
                        }
                    }

                    // Gets the sample from the oscillator
                    switch (_waveShape)
                    {
                        case WaveShape.Square: // Square wave
                        {
                            _sample = ((_phase / _periodTemp) < _squareDuty) ? 0.5f : -0.5f;
                            break;
                        }
                        case WaveShape.Sawtooth: // Saw wave
                        {
                            _sample = 1.0f - (_phase / _periodTemp) * 2.0f;
                            break;
                        }
                        case WaveShape.Sine: // Sine wave (fast and accurate approx)
                        {
                            _pos = _phase / _periodTemp;
                            _pos = _pos > 0.5f ? (_pos - 1.0f) * 6.28318531f : _pos * 6.28318531f;
                            _sample = _pos < 0
                                ? 1.27323954f * _pos + .405284735f * _pos * _pos
                                : 1.27323954f * _pos - 0.405284735f * _pos * _pos;
                            _sample = _sample < 0
                                ? .225f * (_sample * -_sample - _sample) + _sample
                                : .225f * (_sample * _sample - _sample) + _sample;

                            break;
                        }
                        case WaveShape.Noise: // Noise
                        {
                            _sample = _noiseBuffer[(_phase * 32 / (int)_periodTemp) % 32];
                            break;
                        }
                    }

                    // Applies the low and high pass filters
                    if (_filters)
                    {
                        _lpFilterOldPos = _lpFilterPos;
                        _lpFilterCutoff *= _lpFilterDeltaCutoff;
                        if (_lpFilterCutoff < 0.0f)
                            _lpFilterCutoff = 0.0f;
                        else if (_lpFilterCutoff > 0.1f)
                            _lpFilterCutoff = 0.1f;

                        if (_lpFilterOn)
                        {
                            _lpFilterDeltaPos += (_sample - _lpFilterPos) * _lpFilterCutoff;
                            _lpFilterDeltaPos *= _lpFilterDamping;
                        }
                        else
                        {
                            _lpFilterPos = _sample;
                            _lpFilterDeltaPos = 0.0f;
                        }

                        _lpFilterPos += _lpFilterDeltaPos;

                        _hpFilterPos += _lpFilterPos - _lpFilterOldPos;
                        _hpFilterPos *= 1.0f - _hpFilterCutoff;
                        _sample = _hpFilterPos;
                    }

                    // Applies the phaser effect
                    if (_phaser)
                    {
                        _phaserBuffer[_phaserPos & 1023] = _sample;
                        _sample += _phaserBuffer[(_phaserPos - _phaserInt + 1024) & 1023];
                        _phaserPos = (_phaserPos + 1) & 1023;
                    }

                    _superSample += _sample;
                }

                // Averages out the super samples and applies volumes
                _superSample = _masterVolume * _envelopeVolume * _superSample * 0.125f;

                // Clipping if too loud
                if (_superSample > 1.0f)
                    _superSample = 1.0f;
                else if (_superSample < -1.0f)
                    _superSample = -1.0f;

                samples[i] = (int)(32000.0 * _superSample);

                samplesInBuffer++;
            }

            _sampleBuffer = new byte[samplesInBuffer * 2];
            for (var i = 0; i < samplesInBuffer; i++)
            {
                if (BitConverter.IsLittleEndian)
                {
                    _sampleBuffer[i * 2] = (byte)(samples[i] & 0xff);
                    _sampleBuffer[i * 2 + 1] = (byte)(samples[i] >> 8);
                }
                else
                {
                    _sampleBuffer[i * 2] = (byte)(samples[i] >> 8);
                    _sampleBuffer[i * 2 + 1] = (byte)(samples[i] & 0xff);
                }
            }
        }

        private void Reset(bool totalReset)
        {
            var p = _localParams;

            _period = 100.0f / (p.StartFrequency * p.StartFrequency + 0.001f);
            _maxPeriod = 100.0f / (p.MinFrequency * p.MinFrequency + 0.001f);

            _slide = 1.0f - p.Slide * p.Slide * p.Slide * 0.01f;
            _deltaSlide = -p.DeltaSlide * p.DeltaSlide * p.DeltaSlide * 0.000001f;

            if (p.WaveShape == WaveShape.Square)
            {
                _squareDuty = 0.5f - p.SquareDuty * 0.5f;
                _dutySweep = -p.DutySweep * 0.00005f;
            }

            if (p.ChangeAmount > 0.0f)
                _changeAmount = 1.0f - p.ChangeAmount * p.ChangeAmount * 0.9f;
            else
                _changeAmount = 1.0f + p.ChangeAmount * p.ChangeAmount * 10.0f;

            _changeTime = 0;

            if (Math.Abs(p.ChangeSpeed - 1.0) < 0.01f)
                _changeLimit = 0;
            else
                _changeLimit = (int)((1.0f - p.ChangeSpeed) * (1.0f - p.ChangeSpeed) * 20000 + 32);

            if (totalReset)
            {
                _masterVolume = p.MasterVolume * p.MasterVolume;

                _waveShape = p.WaveShape;

                if (p.SustainTime < 0.01f)
                    p.SustainTime = 0.01f;

                var totalTime = p.AttackTime + p.SustainTime + p.DecayTime;
                if (totalTime < 0.18)
                {
                    var multiplier = 0.18 / totalTime;
                    p.AttackTime *= multiplier;
                    p.SustainTime *= multiplier;
                    p.DecayTime *= multiplier;
                }

                _sustainPunch = p.SustainPunch;

                _phase = 0;

                _minFrequency = p.MinFrequency;

                _filters = Math.Abs(p.LowPassCutoff - 1.0) > 0.01f || p.HighPassCutoff != 0.0;

                _lpFilterPos = 0.0f;
                _lpFilterDeltaPos = 0.0f;
                _lpFilterCutoff = p.LowPassCutoff * p.LowPassCutoff * p.LowPassCutoff * 0.1f;
                _lpFilterDeltaCutoff = 1.0f + p.LowPassCutoffSweep * 0.0001f;
                _lpFilterDamping = 5.0f / (1.0f + p.LowPassResonance * p.LowPassResonance * 20.0f) *
                                   (0.01f + _lpFilterCutoff);
                if (_lpFilterDamping > 0.8) _lpFilterDamping = 0.8f;
                _lpFilterDamping = 1.0f - _lpFilterDamping;
                _lpFilterOn = Math.Abs(p.LowPassCutoff - 1.0f) > 0.01f;

                _hpFilterPos = 0.0f;
                _hpFilterCutoff = p.HighPassCutoff * p.HighPassCutoff * 0.1f;
                _hpFilterDeltaCutoff = 1.0f + p.HighPassCutoffSweep * 0.0003f;

                _vibratoPhase = 0.0f;
                _vibratoSpeed = p.VibratoSpeed * p.VibratoSpeed * 0.01f;
                _vibratoAmplitude = p.VibratoDepth * 0.5f;

                _envelopeVolume = 0.0f;
                _envelopeStage = 0;
                _envelopeTime = 0;
                _envelopeLength0 = p.AttackTime * p.AttackTime * 100000.0f;
                _envelopeLength1 = p.SustainTime * p.SustainTime * 100000.0f;
                _envelopeLength2 = p.DecayTime * p.DecayTime * 100000.0f + 10;
                _envelopeLength = _envelopeLength0;
                _envelopeFullLength = _envelopeLength0 + _envelopeLength1 + _envelopeLength2;

                _envelopeOverLength0 = 1.0f / _envelopeLength0;
                _envelopeOverLength1 = 1.0f / _envelopeLength1;
                _envelopeOverLength2 = 1.0f / _envelopeLength2;

                _phaser = p.PhaserOffset != 0.0f || p.PhaserSweep != 0.0f;

                _phaserOffset = p.PhaserOffset * p.PhaserOffset * 1020.0f;
                if (p.PhaserOffset < 0.0)
                    _phaserOffset = -_phaserOffset;
                _phaserDeltaOffset = p.PhaserSweep * p.PhaserSweep * p.PhaserSweep * 0.2f;
                _phaserPos = 0;

                for (var i = 0; i < 1024; i++)
                    _phaserBuffer[i] = 0;

                for (var i = 0; i < 32; i++)
                    _noiseBuffer[i] = (float)_random.NextDouble() * 2.0f - 1.0f;

                _repeatTime = 0;

                if (p.RepeatSpeed == 0.0)
                    _repeatLimit = 0;
                else
                    _repeatLimit = (int)((1.0f - p.RepeatSpeed) * (1.0f - p.RepeatSpeed) * 20000) + 32;
            }
        }
    }
}