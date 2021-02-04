using System;
using System.IO;

namespace Chroma.Audio.Sfxr
{
    public class SfxrParams
    {
        public WaveShape WaveShape { get; set; }

        public double MasterVolume { get; set; } // sound_vol
        public double AttackTime { get; set; } // p_env_attack
        public double SustainTime { get; set; } // p_env_sustain
        public double SustainPunch { get; set; } // p_env_punch
        public double DecayTime { get; set; } // p_env_decay
        public double StartFrequency { get; set; } // p_base_freq
        public double MinFrequency { get; set; } // p_freq_limit
        public double Slide { get; set; } // p_freq_ramp
        public double DeltaSlide { get; set; } // p_freq_dramp
        public double VibratoDepth { get; set; } // p_vib_strength
        public double VibratoSpeed { get; set; } // p_vib_speed
        public double ChangeAmount { get; set; } // p_arp_mod
        public double ChangeSpeed { get; set; } // p_arp_speed
        public double SquareDuty { get; set; } // p_duty
        public double DutySweep { get; set; } // p_duty_ramp
        public double RepeatSpeed { get; set; } // p_repeat_speed
        public double PhaserOffset { get; set; } // p_pha_offset
        public double PhaserSweep { get; set; } // p_pha_ramp

        public double LowPassCutoff { get; set; } // p_lpf_freq
        public double LowPassCutoffSweep { get; set; } // p_lpf_ramp
        public double LowPassResonance { get; set; } // p_lpf_resonance

        public double HighPassCutoff { get; set; } // p_hpf_freq
        public double HighPassCutoffSweep { get; set; } // p_hpf_ramp

        public SfxrParams()
        {
        }

        public SfxrParams(string s)
        {
            FromString(s);
        }

        public SfxrParams(Stream s, ParameterFormat format)
        {
            FromStream(s, format);
        }

        public SfxrParams(SfxrParams p)
        {
            WaveShape = p.WaveShape;

            MasterVolume = p.MasterVolume;

            AttackTime = p.AttackTime;
            SustainTime = p.SustainTime;
            SustainPunch = p.SustainPunch;
            DecayTime = p.DecayTime;

            StartFrequency = p.StartFrequency;
            MinFrequency = p.MinFrequency;

            Slide = p.Slide;
            DeltaSlide = p.DeltaSlide;

            VibratoDepth = p.VibratoDepth;
            VibratoSpeed = p.VibratoSpeed;

            ChangeAmount = p.ChangeAmount;
            ChangeSpeed = p.ChangeSpeed;

            SquareDuty = p.SquareDuty;
            DutySweep = p.DutySweep;

            RepeatSpeed = p.RepeatSpeed;

            PhaserOffset = p.PhaserOffset;
            PhaserSweep = p.PhaserSweep;

            LowPassCutoff = p.LowPassCutoff;
            LowPassCutoffSweep = p.LowPassCutoffSweep;
            LowPassResonance = p.LowPassResonance;

            HighPassCutoff = p.HighPassCutoff;
            HighPassCutoffSweep = p.HighPassCutoffSweep;
        }

        public void FromStream(Stream stream, ParameterFormat format)
        {
            if (format == ParameterFormat.String)
            {
                using (var sr = new StreamReader(stream))
                {
                    var data = sr.ReadToEnd();
                    FromString(data);
                }
            }
            else if (format == ParameterFormat.Binary)
            {
                using (var br = new BinaryReader(stream))
                {
                    var version = br.ReadInt32();

                    if (version != 102 && version != 101 && version != 100)
                    {
                        throw new FormatException($"Unsupported SFXR file version '{version}'.");
                    }

                    WaveShape = (WaveShape)br.ReadInt32();

                    MasterVolume = 0.5f;

                    if (version == 102)
                        MasterVolume = br.ReadSingle();

                    StartFrequency = br.ReadSingle();
                    MinFrequency = br.ReadSingle();
                    Slide = br.ReadSingle();

                    if (version >= 101)
                        DeltaSlide = br.ReadSingle();
                    
                    SquareDuty = br.ReadSingle();
                    DutySweep = br.ReadSingle();

                    VibratoDepth = br.ReadSingle();
                    VibratoSpeed = br.ReadSingle();

                    // p_vib_delay
                    br.ReadSingle();

                    AttackTime = br.ReadSingle();
                    SustainTime = br.ReadSingle();
                    DecayTime = br.ReadSingle();
                    SustainPunch = br.ReadSingle();

                    // filter_on
                    br.ReadBoolean();

                    LowPassResonance = br.ReadSingle();
                    LowPassCutoff = br.ReadSingle();
                    LowPassCutoffSweep = br.ReadSingle();

                    HighPassCutoff = br.ReadSingle();
                    HighPassCutoffSweep = br.ReadSingle();

                    PhaserOffset = br.ReadSingle();
                    PhaserSweep = br.ReadSingle();

                    if (version >= 101)
                    {
                        ChangeSpeed = br.ReadSingle();
                        ChangeAmount = br.ReadSingle();
                    }
                }
            }
            else
            {
                throw new FormatException("Unsupported parameter format provided.");
            }
        }

        public void FromBinary(byte[] data)
        {
            using (var ms = new MemoryStream(data))
                FromStream(ms, ParameterFormat.Binary);
        }

        public void FromString(string s)
        {
            var splitStrings = s.Split(new[] {','});

            for (var i = 0; i < splitStrings.Length; i++)
                if (splitStrings[i].Length == 0)
                    splitStrings[i] = "0";

            WaveShape =
                splitStrings[0] == "0" ? WaveShape.Square
                : splitStrings[0] == "1" ? WaveShape.Sawtooth
                : splitStrings[0] == "2" ? WaveShape.Sine
                : WaveShape.Noise;

            AttackTime = float.Parse(splitStrings[1]);
            SustainTime = float.Parse(splitStrings[2]);
            SustainPunch = float.Parse(splitStrings[3]);
            DecayTime = float.Parse(splitStrings[4]);
            StartFrequency = float.Parse(splitStrings[5]);
            MinFrequency = float.Parse(splitStrings[6]);
            Slide = float.Parse(splitStrings[7]);
            DeltaSlide = float.Parse(splitStrings[8]);
            VibratoDepth = float.Parse(splitStrings[9]);
            VibratoSpeed = float.Parse(splitStrings[10]);
            ChangeAmount = float.Parse(splitStrings[11]);
            ChangeSpeed = float.Parse(splitStrings[12]);
            SquareDuty = float.Parse(splitStrings[13]);
            DutySweep = float.Parse(splitStrings[14]);
            RepeatSpeed = float.Parse(splitStrings[15]);
            PhaserOffset = float.Parse(splitStrings[16]);
            PhaserSweep = float.Parse(splitStrings[17]);
            LowPassCutoff = float.Parse(splitStrings[18]);
            LowPassCutoffSweep = float.Parse(splitStrings[19]);
            LowPassResonance = float.Parse(splitStrings[20]);
            HighPassCutoff = float.Parse(splitStrings[21]);
            HighPassCutoffSweep = float.Parse(splitStrings[22]);
            MasterVolume = float.Parse(splitStrings[23]);
        }

        public void Mutate(double mutation)
        {
            var r = new Random();

            if (r.NextDouble() < 0.5) StartFrequency += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) MinFrequency += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) Slide += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) DeltaSlide += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) SquareDuty += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) DutySweep += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) VibratoDepth += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) VibratoSpeed += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) AttackTime += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) SustainTime += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) DecayTime += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) SustainPunch += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) LowPassCutoff += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) LowPassCutoffSweep += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) LowPassResonance += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) HighPassCutoff += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) HighPassCutoffSweep += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) PhaserOffset += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) PhaserSweep += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) RepeatSpeed += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) ChangeSpeed += r.NextDouble() * mutation * 2 - mutation;
            if (r.NextDouble() < 0.5) ChangeAmount += r.NextDouble() * mutation * 2 - mutation;
        }

        public SfxrParams MutateClone(double mutation)
        {
            var p = new SfxrParams(this);
            p.Mutate(mutation);
            return p;
        }
    }
}