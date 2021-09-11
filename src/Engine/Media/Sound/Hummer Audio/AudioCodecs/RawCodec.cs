using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ZargoEngine.Media.Codecs
{
    // this codes base writen by discord: 𝓗𝓾𝓜𝓜𝓮𝓡#6619
    public class RawCodec : BufferedWaveProvider
    {
        public RawCodec(string rawFileName, int sampleRate = 44100, int channels = 1) : base(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels))
        {
            if (string.IsNullOrWhiteSpace(rawFileName))
            {
                throw new ArgumentException($"Provide a path to a raw audio file in command line arguments to play it.");
            }

            if (!File.Exists(rawFileName))
            {
                throw new ArgumentException($"Invalid audio path: '{rawFileName}'.");
            }

            _audioStream = new MemoryStream(File.ReadAllBytes(rawFileName));
            _audioStreamBuffer = new byte[BufferLength];
            Update();
        }

        public RawCodec(Stream inputStream, int sampleRate = 44100, int channels = 1) : base(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels))
        {
            _audioStream = inputStream;
            _audioStreamBuffer = new byte[BufferLength];
            Update();
        }

        protected async void Update()
        {
            while (_audioStream.Position != _audioStream.Length)
            {
                var writedBytes = _audioStream.Read(_audioStreamBuffer, 0, _audioStreamBuffer.Length - BufferedBytes);
                AddSamples(_audioStreamBuffer, 0, writedBytes);
                await Task.Delay(10);
            }
        }

        private Stream _audioStream;
        private byte[] _audioStreamBuffer;
    }
}
