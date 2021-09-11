using System;
using NAudio.Wave;

namespace ZargoEngine.Media.Codecs
{
    // this codes base writen by discord: 𝓗𝓾𝓜𝓜𝓮𝓡#6619
    public class SineCodec : ISampleProvider, IWaveProvider
    {
        public WaveFormat WaveFormat { get; }
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public double Duration { get; set; }

        public SineCodec(WaveFormat waveFormat, double frequency = 220, double amplitude = 0.5, double duration = 60)
        {
            if (waveFormat.Channels != 1)
            {
                throw new ArgumentException("Must be a mono wave format.");
            }

            WaveFormat = waveFormat;
            Frequency = frequency;
            Amplitude = amplitude;
            Duration = duration;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var dt = 1.0 / WaveFormat.SampleRate;
            for (int i = 0; i < count; i++)
            {
                if (_time >= Duration)
                {
                    _time = Duration;
                    return i;
                }

                buffer[offset + i] = (float)(Math.Sin(Frequency * Math.PI * 2.0 * _time) * Amplitude);
                _time += dt;
            }

            return count;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return this.ToWaveProvider().Read(buffer, offset, count);
        }

        private double _time;
    }
}
