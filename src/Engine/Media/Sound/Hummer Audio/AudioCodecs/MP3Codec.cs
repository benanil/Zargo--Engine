using System.IO;
using NAudio.FileFormats.Mp3;
using NAudio.Wave;

namespace ZargoEngine.Media.Codecs
{
    // this codes base writen by discord: 𝓗𝓾𝓜𝓜𝓮𝓡#6619
    public class MP3Codec : Mp3FileReaderBase
    {
        public MP3Codec(string mp3FileName) : base(mp3FileName, new FrameDecompressorBuilder(Mp3FrameDecompressor)) { }
        public MP3Codec(Stream inputStream) : base(inputStream, new FrameDecompressorBuilder(Mp3FrameDecompressor)) { }
        public MP3Codec(Stream inputStream, bool ownInputStream) : base(inputStream, new FrameDecompressorBuilder(Mp3FrameDecompressor), ownInputStream) { }
        private static IMp3FrameDecompressor Mp3FrameDecompressor(WaveFormat mp3Format) => new DmoMp3FrameDecompressor(mp3Format);
    }
}
