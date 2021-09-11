using System.IO;
using NAudio.Wave;

namespace ZargoEngine.Media.Codecs
{
    // this codes base writen by discord: 𝓗𝓾𝓜𝓜𝓮𝓡#6619
    public class WavCodec : WaveFileReader
    {
        public WavCodec(string waveFile) : base(waveFile) { }
        public WavCodec(Stream inputStream) : base(inputStream) { }
    }
}
